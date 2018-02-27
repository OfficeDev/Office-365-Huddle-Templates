/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.WebJob.Models;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Huddle.WebJob.Services
{
    class PlannerService
    {
        private TeamsService teamService = null;
        private MetricIdeaService metricIdeaService = null;

        public PlannerService()
        {
            teamService = new TeamsService();
        }

        public PlannerService(TeamsService teamService)
        {
            this.teamService = teamService;
        }

        public PlannerService(ClientContext spClientContext) : this()
        {
            metricIdeaService = new MetricIdeaService(spClientContext);
        }

        public async Task<IEnumerable<Idea>> MoveShareableIdeas()
        {
            var movedIdeas = new List<Idea>();
            var globalTeam = await teamService.GetGlobalTeamAsync();
            LogService.LogInfo($"The global team is: {globalTeam?.DisplayName}.");
            var globalPlan = await GetTeamPlanAsync(globalTeam);
            var globalBucket = await GetShareableBucketAsync(globalPlan);
            if (globalBucket == null)
            {
                LogService.LogInfo("Failed to get the global bucket.");
                return movedIdeas;
            }

            var nonGlobalTeams = await teamService.GetNonGlobalTeamsAsync();
            foreach (var team in nonGlobalTeams)
            {
                var operation = $"Move shareable ideas in team {team.DisplayName}";
                LogService.LogOperationStarted(operation);
                var plan = await GetTeamPlanAsync(team);
                if (plan != null)
                {
                    var ideas = (await GetIdeasInShareableBucketAsync(plan));
                    foreach (var idea in ideas)
                    {
                        var updatedIdea = await MoveIdea(idea, globalBucket.Id, globalPlan.Id);
                        LogService.LogInfo($"Moved idea {updatedIdea.Title}.");
                        movedIdeas.Add(updatedIdea);
                        if (metricIdeaService != null)
                        {
                            updatedIdea.Url = Idea.GetIdeaUrl(globalTeam.Id, globalPlan.Id, updatedIdea.Id);
                            metricIdeaService.UpdateIdeaInMetricIdeaList(idea, updatedIdea);
                        }
                    }
                }
                else
                    LogService.LogInfo($"Failed to get the plan of the team.");
                LogService.LogOperationEnded(operation);
            }
            return movedIdeas;
        }

        public async Task<IEnumerable<Idea>> ReoveObsoleteIdeas()
        {
            var removedIdeas = new List<Idea>();
            var globalTeam = await teamService.GetGlobalTeamAsync();
            var globalPlan = await GetTeamPlanAsync(globalTeam);
            var globalBucket = await GetShareableBucketAsync(globalPlan);
            var lifeSpan = 0d;
            var gotLifeSpan = double.TryParse(Constants.GlobalTaskLifeSpan, out lifeSpan);

            LogService.LogInfo($"Remove ideas more than {lifeSpan} days on board in bucket: {globalBucket?.Name}, team: { globalTeam?.DisplayName}.");
            if (globalBucket == null || !gotLifeSpan)
                return removedIdeas;

            var ideas = (await GetIdeasInBucketAsync(globalBucket));
            foreach (var idea in ideas)
            {
                if (!idea.CreatedDateTime.HasValue || (DateTime.UtcNow - idea.CreatedDateTime.Value.UtcDateTime).TotalDays <= lifeSpan)
                    continue;

                await DeleteIdea(idea);
                removedIdeas.Add(idea);
                if (metricIdeaService != null)
                    metricIdeaService.DeleteIdeaInMetricIdeaList(idea);
            }
            return removedIdeas;
        }

        public async Task<IEnumerable<Idea>> GetIdeasInTeamAsync(Team team)
        {
            var plan = await GetTeamPlanAsync(team);
            var buckets = await GetBucketsInPlanAsync(plan);
            var ideas = await GetIdeasInPlanAsync(plan);
            foreach (var idea in ideas)
            {
                var bucket = buckets.Where(b => b.Id == idea.BucketId).FirstOrDefault();
                idea.BucketName = bucket?.Name;
            }
            return ideas;
        }

        private async Task<Plan> GetTeamPlanAsync(Team team)
        {
            if (team == null) return null;

            var parameters = new JObject();
            parameters.Add("groupId", team.Id);
            var plans = await HttpHelper.Request<Array<Plan>>(Constants.LogicAppUrls.ListPlans, parameters);
            return plans == null ? null : plans.Value.Where(i => i.Title == team.DisplayName).FirstOrDefault();
        }

        private async Task<IEnumerable<Bucket>> GetBucketsInPlanAsync(Plan plan)
        {
            if (plan == null) return new Bucket[0];

            var parameters = new JObject();
            parameters.Add("planId", plan.Id);
            var buckets = await HttpHelper.Request<Array<Bucket>>(Constants.LogicAppUrls.ListBuckets, parameters);
            return buckets == null ? new Bucket[0] : buckets.Value;
        }

        private async Task<Bucket> GetShareableBucketAsync(Plan plan)
        {
            return (await GetBucketsInPlanAsync(plan)).Where(i => i.Name == Constants.ShareableBucket).FirstOrDefault();
        }

        private async Task<IEnumerable<Idea>> GetIdeasInShareableBucketAsync(Plan plan)
        {
            var bucket = await GetShareableBucketAsync(plan);
            if (bucket == null) return new Idea[0];

            return await GetIdeasInBucketAsync(bucket);
        }

        private async Task<IEnumerable<Idea>> GetIdeasInPlanAsync(Plan plan)
        {
            if (plan == null) return new Idea[0];

            var parameters = new JObject();
            parameters.Add("planId", plan.Id);
            var ideas = await HttpHelper.Request<Array<Idea>>(Constants.LogicAppUrls.ListPlanTasks, parameters);
            return ideas == null ? new Idea[0] : ideas.Value;
        }

        private async Task<IEnumerable<Idea>> GetIdeasInBucketAsync(Bucket bucket)
        {
            if (bucket == null) return new Idea[0];

            var parameters = new JObject();
            parameters.Add("bucketId", bucket.Id);
            var ideas = await HttpHelper.Request<Array<Idea>>(Constants.LogicAppUrls.ListBucketTasks, parameters);
            return ideas == null ? new Idea[0] : ideas.Value;
        }

        private async Task<Idea> MoveIdea(Idea idea, string bucketId, string planId)
        {
            var parameters = new JObject();
            parameters.Add("taskId", idea.Id);
            var detail = await HttpHelper.Request<IdeaDetail>(Constants.LogicAppUrls.GetTaskDetails, parameters);

            // Copy
            parameters = new JObject();
            parameters.Add("bucketId", bucketId);
            parameters.Add("planId", planId);
            parameters.Add("title", idea.Title);
            parameters.Add("description", detail.Description);
            parameters.Add("startDateTime", idea.StartDateTime);
            var newIdea = await HttpHelper.Request<Idea>(Constants.LogicAppUrls.CreateTask, parameters);
            LogService.LogInfo($"Created idea {newIdea.Title} (ID: {newIdea.Id}) in global team.");

            // Delete
            await DeleteIdea(idea);

            return newIdea;
        }

        private async Task DeleteIdea(Idea idea)
        {
            var parameters = new JObject();
            parameters.Add("id", idea.Id);
            parameters.Add("etag", idea.Etag);
            await HttpHelper.Request<Idea>(Constants.LogicAppUrls.DeleteTask, parameters);
            LogService.LogInfo($"Deleted idea {idea.Title} (ID: {idea.Id}).");
        }
    }
}
