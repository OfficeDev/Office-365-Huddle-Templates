/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Microsoft.Graph;
using System.Linq;
using System.Threading.Tasks;
using Team = Huddle.BotWebApp.Models.Team;

namespace Huddle.BotWebApp.Services
{
    public class TeamsService : GraphService
    {

        public TeamsService(string token) : base(token) { }

        public async Task<TeamMember[]> GetTeamMembersAsync(string teamId)
        {
            var users = await _graphServiceClient.Groups[teamId].Members.Request().GetAllAsync();
            return users
                .Cast<User>()
                .Select(i => new TeamMember { Id = i.Id, DisplayName = i.DisplayName })
                .OrderBy(i => i.DisplayName)
                .ToArray();
        }

        public async Task<Team[]> GetJoinedTeamsAsync()
        {
            var teams = await _graphServiceClient.Me.JoinedTeams.Request().GetAllAsync();
            return teams.Select(i => new Team
            {
                Id = i.Id,
                DisplayName = i.AdditionalData["displayName"] as string
            }).ToArray();
        }
    }
}
