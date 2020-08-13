/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Services
{
    public class IdeaService : GraphService
    {
        private PlannerService _plannerService;

        public IdeaService(string token) : base(token)
        {
            this._plannerService = new PlannerService(token);
        }

        public async Task<Idea[]> GetAsync(string planId, string bucketName, DateTime? from)
        {
            var buckets = await _plannerService.GetBucketsAsync(planId);
            var bucketDict = buckets.ToDictionary(b => b.Id);
            var bucket = buckets.FirstOrDefault(i => StringComparer.InvariantCultureIgnoreCase.Equals(i.Name, bucketName));

            var tasks = await _graphServiceClient.Planner.Plans[planId].Tasks.Request().GetAllAsync();
            if (bucket != null)
                tasks = tasks.Where(i => i.BucketId == bucket.Id).ToArray();

            var userDict = new Dictionary<string, string>();
            var userIds = tasks.SelectMany(i => i.Assignments.Select(j => j.Key)).Distinct().ToArray();
            if (userIds.Length > 0)
            {
                var filter = string.Join(" or ", userIds.Select(i => $"id eq '{i}'"));
                var users = await _graphServiceClient.Users.Request().Filter(filter).Select("id, displayName").GetAllAsync();
                userDict = users.ToDictionary(i => i.Id, i => i.DisplayName);
            }

            return tasks.Select(i => new Idea
            {
                Id = i.Id,
                Bucket = bucketDict[i.BucketId].Name,
                Title = i.Title,
                StartDate = i.StartDateTime,
                Owners = i.Assignments
                    .Select(a => userDict[a.Key])
                    .ToArray()
            }).ToArray();
        }

        public async Task<PlannerTask> CreateAsync(string planId, string title, DateTimeOffset? startDate, string ownerId, string description)
        {
            var newIdeaBucket = await _plannerService.GetNewIdeaBucketAsync(planId);
            if (newIdeaBucket == null) throw new ApplicationException("Could not find New Idea bucket.");

            var plannerTask = new PlannerTask
            {
                PlanId = planId,
                BucketId = newIdeaBucket.Id,
                Title = title,
                StartDateTime = startDate,
                Assignments = new PlannerAssignments()
            };
            plannerTask.Assignments.AddAssignee(ownerId);
            plannerTask = await _graphServiceClient.Planner.Tasks.Request().AddAsync(plannerTask);

            var planerTaskDetails = new PlannerTaskDetails { Description = description };
            var plannerTaskRequestBuilder = _graphServiceClient.Planner.Tasks[plannerTask.Id];

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
            var details = await _graphServiceClient.Planner.Tasks[idea.Id].Details.Request().GetAsync();
            if (details != null)
                idea.Description = details.Description;
        }

        public string GetIdeaUrl(string tenantId, string groupId, string planId, string taskId)
        {
            return $"https://tasks.office.com/{tenantId}/EN-US/Home/Planner#/plantaskboard?groupId={groupId}&planId={planId}&taskId={taskId}";
        }

        public string GetBucketName(string status)
        {
            if (string.IsNullOrEmpty(status)) return null;

            var statusLower = status.ToLower();
            if (statusLower.Contains("new"))
                return IdeasPlan.Buckets.NewIdea;
            if (statusLower.Contains("in progress"))
                return IdeasPlan.Buckets.InProgress;
            if (statusLower.Contains("shareable"))
                return IdeasPlan.Buckets.Shareable;
            if (statusLower.Contains("completed"))
                return IdeasPlan.Buckets.Completed;
            return null;
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

    }
}
