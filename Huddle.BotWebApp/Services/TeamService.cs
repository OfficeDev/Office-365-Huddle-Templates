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
    public class TeamService
    {
        private GraphServiceClient graphServiceClient;

        public TeamService(GraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient;
        }

        public async Task<TeamMember[]> GetTeamMembersAsync(string teamId)
        {
            var users = await graphServiceClient.Groups[teamId].Members.Request().GetAllAsync();
            return users
                .Cast<User>()
                .Select(i => new TeamMember { Id = i.Id, DisplayName = i.DisplayName })
                .OrderBy(i => i.DisplayName)
                .ToArray();
        }
    }
}
