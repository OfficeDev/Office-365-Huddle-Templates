/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
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
    public class SelectTeamDialog : IDialog<Team>
    {
        private Team[] teams;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            await context.Forward(new SignInDialog(), SelectTeam, context.Activity, CancellationToken.None);
        }

        private async Task SelectTeam(IDialogContext context, IAwaitable<GraphServiceClient> result)
        {
            var graphServiceClient = await result;

            teams = await graphServiceClient.GetJoinedTeamsAsync();
            if (teams.Length == 0)
            {
                context.Fail(new Exception("Sorry. You do not belong to any team."));
            }
            else if (teams.Length == 1)
            {
                var team = teams.First();
                await context.SayAsync($"You only have one team: {team.DisplayName}. It was selected automatically.");
                context.Done(team);
            }
            else
            {
                await context.ChoiceAsync("Please select one of you teams", teams.Select(i => i.DisplayName));
                context.Wait(TeamSelected);
            }
        }

        private async Task TeamSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result as Activity;

            var teamDisplayName = activity.Text.Trim();
            var team = teams
                .Where(i => i.DisplayName == teamDisplayName)
                .FirstOrDefault();
            if (team == null)
            {
                await context.ChoiceAsync($"Could not found team '{teamDisplayName}'. Please select one of you teams", teams.Select(i => i.DisplayName));
                context.Wait(TeamSelected);
            }
            else
                context.Done(team);
        }
    }
}
