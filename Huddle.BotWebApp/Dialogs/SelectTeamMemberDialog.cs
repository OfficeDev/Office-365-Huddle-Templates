/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Huddle.BotWebApp.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Graph;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    [Serializable]
    public class SelectTeamMemberDialog : IDialog<TeamMember>
    {
        private Team team;
        private TeamMember[] members;
        private string text;

        public SelectTeamMemberDialog(Team team, string text)
        {
            this.team = team;
            this.text = text;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            await context.Forward(new SignInDialog(), SelectTeamMember, context.Activity, CancellationToken.None);
        }

        private async Task SelectTeamMember(IDialogContext context, IAwaitable<GraphServiceClient> result)
        {
            var graphServiceClient = await result;
            var teamService = new TeamService(graphServiceClient);
            members = await teamService.GetTeamMembersAsync(team.Id);

            await context.ChoiceAsync(text, members.Select(i => i.DisplayName).ToArray());
            context.Wait(TeamMemberSelected);
        }

        private async Task TeamMemberSelected(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            var displayName = activity.Text;
            var member = members
                .Where(i => i.DisplayName == displayName)
                .FirstOrDefault();

            if (member == null)
            {
                var text = $"Sorry, could not find user '{displayName}' in {team.DisplayName}. Please identify the idea owner.";
                await context.ChoiceAsync(text, members.Select(i => i.DisplayName).ToArray());
                context.Wait(TeamMemberSelected);
            }
            else
                context.Done(member);
        }
    }
}