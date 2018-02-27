/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.Common;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace Huddle.MetricWebApp.Services
{
    public class TeamsService
    {
        private GraphServiceClient graphServiceClient;

        public TeamsService(GraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient;
        }

        public Task<User[]> GetTeamMembersAsync(string teamId)
        {
            return graphServiceClient.Groups[teamId].Members.Request().GetAllAsync();
        }
    }
}
