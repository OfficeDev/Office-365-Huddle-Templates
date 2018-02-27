/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Huddle.Common;
using Microsoft.Graph;
using System.Linq;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Services
{
    public class PlannerService
    {
        private GraphServiceClient graphServiceClient;

        public PlannerService(GraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient;
        }

        public async Task<PlannerPlan> GetTeamPlanAsync(Team team)
        {
            var plans = await graphServiceClient.Groups[team.Id].Planner.Plans
                .Request()
                .Filter($"title eq '{team.DisplayName}'") //does not work
                .GetAsync();

            return plans
                .Where(i => i.Title == team.DisplayName)
                .FirstOrDefault();
        }

        public async Task<PlannerBucket[]> GetBucketsAsync(string planId)
        {
            return await graphServiceClient.Planner.Plans[planId].Buckets.Request().GetAllAsync();
        }

        public async Task<PlannerBucket> GetNewIdeaBucketAsync(string planId)
        {
            var buckets = await graphServiceClient.Planner.Plans[planId].Buckets.Request()
                .Filter($"name eq '{Constants.IdeasPlan.Buckets.NewIdea}'")
                .GetAsync();

            return buckets
                .Where(i => i.Name == Constants.IdeasPlan.Buckets.NewIdea)
                .FirstOrDefault();
        }
    }
}
