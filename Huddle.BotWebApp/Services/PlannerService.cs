/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.Graph;
using System.Linq;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Services
{
    public class PlannerService: GraphService
    {
        public PlannerService(string token) : base(token) { }

        public async Task<PlannerPlan> GetTeamPlanAsync(string teamId, string teamName)
        {
            var plans = await _graphServiceClient.Groups[teamId].Planner.Plans
                .Request()
                .Filter($"title eq '{teamName}'") //does not work
                .GetAsync();

            return plans
                .Where(i => i.Title == teamName)
                .FirstOrDefault();
        }

        public async Task<PlannerBucket[]> GetBucketsAsync(string planId)
        {
            return await _graphServiceClient.Planner.Plans[planId].Buckets.Request().GetAllAsync();
        }

        public async Task<PlannerBucket> GetNewIdeaBucketAsync(string planId)
        {
            var buckets = await _graphServiceClient.Planner.Plans[planId].Buckets.Request()
                .Filter($"name eq '{IdeasPlan.Buckets.NewIdea}'")
                .GetAsync();
             
            return buckets
                .Where(i => i.Name == IdeasPlan.Buckets.NewIdea)
                .FirstOrDefault();
        }
    }
}
