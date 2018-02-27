/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    [Serializable]
    public abstract class TeamDialog<TResult> : IDialog<TResult>
    {
        protected Team Team { get; private set; }

        protected TeamsChannelAccount TeamsChannelAccount { get; private set; }

        public async Task StartAsync(IDialogContext context)
        {
            TeamsChannelAccount = await context.GetTeamsAccountAsync();
            context.Wait(this.MessageReceivedAsync);
        }

        protected abstract Task StartTeamActionAsync(IDialogContext context);

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            Team = context.UserData.GetValueOrDefault<Team>(Constants.UserDataKey.TeamId);
            if (Team != null)
                await StartTeamActionAsync(context);
            else
                await context.Forward(new SelectTeamDialog(), TeamSelected, context.Activity, CancellationToken.None);
        }

        private async Task TeamSelected(IDialogContext context, IAwaitable<Team> result)
        {
            Team = await result;
            context.UserData.SetValue(Constants.UserDataKey.TeamId, Team);
            await StartTeamActionAsync(context);
        }
    }
}
