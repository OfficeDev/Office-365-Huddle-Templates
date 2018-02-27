/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Huddle.Common;
using Microsoft.Graph;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Services
{
    public class IdeaService
    {
        private GraphServiceClient graphServiceClient;
        private PlannerService plannerService;
        private TeamService teamService;

        public IdeaService(GraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient;
            this.plannerService = new PlannerService(graphServiceClient);
            this.teamService = new TeamService(graphServiceClient);
        }

        public async Task<Idea[]> GetAsync(Team team, string planId, string status, DateTime? from)
        {
            var bucketName = GetBucketName(status);
            var buckets = await plannerService.GetBucketsAsync(planId);
            var bucketDict = buckets.ToDictionary(b => b.Id);
            var bucket = buckets.FirstOrDefault(i => i.Name == bucketName);

            var members = await teamService.GetTeamMembersAsync(team.Id);
            var membersDict = members.ToDictionary(m => m.Id);

            var tasks = await graphServiceClient.Planner.Plans[planId].Tasks.Request().GetAllAsync();
            if (bucket != null)
                tasks = tasks.Where(i => i.BucketId == bucket.Id).ToArray();

            return tasks.Select(i => new Idea
            {
                Id = i.Id,
                Bucket = bucketDict[i.BucketId].Name,
                Title = i.Title,
                StartDate = i.StartDateTime,
                Owners = i.Assignments
                    .Select(a => membersDict[a.Key])
                    .ToArray()
            }).ToArray();
        }

        public async Task<PlannerTask> CreateAsync(string planId, string title, DateTimeOffset? startDate, string ownerId, string description)
        {
            var newIdearBucket = await plannerService.GetNewIdeaBucketAsync(planId);
            if (newIdearBucket == null) throw new ApplicationException("Could not found New Idea bucket.");

            var plannerTask = new PlannerTask
            {
                PlanId = planId,
                BucketId = newIdearBucket.Id,
                Title = title,
                StartDateTime = startDate,
                Assignments = new PlannerAssignments()
            };
            plannerTask.Assignments.AddAssignee(ownerId);
            plannerTask = await graphServiceClient.Planner.Tasks.Request().AddAsync(plannerTask);

            var planerTaskDetails = new PlannerTaskDetails { Description = description };
            var plannerTaskRequestBuilder = graphServiceClient.Planner.Tasks[plannerTask.Id];

            PlannerTaskDetails details = null;

            int count = 1;
            while (true)
            {
                try
                {
                    details = await plannerTaskRequestBuilder.Details.Request().GetAsync();
                    break;
                }
                catch (Exception ex)
                {
                    if (count < 6)
                        await Task.Delay(1000);
                    else
                        throw new Exception("Task created. But failed to create its details. ", ex);
                }
                count++;
            }

            details = await plannerTaskRequestBuilder.Details
                .Request(new[] { new HeaderOption("If-Match", details.GetEtag()) })
                .UpdateAsync(planerTaskDetails);

            return plannerTask;
        }

        public async Task GetDetailsAsync(Idea idea)
        {
            var details = await graphServiceClient.Planner.Tasks[idea.Id].Details.Request().GetAsync();
            if (details != null)
                idea.Description = details.Description;
        }

        public string GetIdeaUrl(string groupId, string planId, string taskId)
        {
            return $"https://tasks.office.com/{Constants.AADTenantId}/EN-US/Home/Planner#/plantaskboard?groupId={groupId}&planId={planId}&taskId={taskId}";
        }

        private string GetNextStepsFromDetails(PlannerTaskDetails details)
        {
            return Regex.Match(
                details.Description,
                "(?<=Next Steps(\r\n)+).*(?=(\r\n)+Aligned to Metric)",
                RegexOptions.Compiled | RegexOptions.Multiline).Value;
        }

        private string GetAlignedToMetricFromDetails(PlannerTaskDetails details)
        {
            return Regex.Match(
                details.Description,
                "(?<=Aligned to Metric\r\n).*",
                RegexOptions.Compiled | RegexOptions.Multiline).Value;
        }

        private string GetBucketName(string status)
        {
            if (status.IsNullOrEmpty()) return null;

            switch (status.ToLower())
            {
                case "new":
                    return Constants.IdeasPlan.Buckets.NewIdea;
                case "in progress":
                    return Constants.IdeasPlan.Buckets.InProgress;
                default:
                    return status;
            }
        }
    }
}
