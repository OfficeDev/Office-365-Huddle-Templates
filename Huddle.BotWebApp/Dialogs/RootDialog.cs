/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Infrastructure;
using Huddle.BotWebApp.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    [Serializable]

    public class RootDialog : LuisDialog<object>
    {
        public RootDialog() :
          base(new LuisService(new LuisModelAttribute(Constants.LuisAppId, Constants.LuisAPIKey, domain: Constants.LuisAPIDomain)))
        { }

        #region Create Idea

        [LuisIntent("Idea.Create")]
        public async Task CreateIdeaAsync(IDialogContext context, LuisResult result)
        {
            await context.Forward(new CreateIdeaDialog(), IdeaCreated, context.Activity, CancellationToken.None);
        }

        private async Task IdeaCreated(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                await result;
            }
            catch (ActionCancelledException)
            {
                await context.SayAsync($"Idea canceled.");
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, "Create idea failed: ");
            }
            context.Wait(MessageReceived);
        }

        #endregion

        #region List Idea

        [LuisIntent("Idea.List")]
        public async Task ListIdeaAsync(IDialogContext context, LuisResult result)
        {
            string status = null;
            DateTime? from = null;

            EntityRecommendation statusEntityRecommendation;
            if (result.TryFindEntity("Idea.Status", out statusEntityRecommendation))
                status = (statusEntityRecommendation.Resolution["values"] as IEnumerable<object>).OfType<string>().FirstOrDefault();

            await context.Forward(new ListIdeasDialog(status, from), IdeaListed, context.Activity, CancellationToken.None);
        }

        private async Task IdeaListed(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                await result;
            }
            catch (ActionCancelledException)
            {
                await context.SayAsync($"List ideas canceled.");
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, "List ideas failed: ");
            }
            context.Wait(MessageReceived);
        }

        #endregion

        #region Switch Team

        [LuisIntent("Team.Switch")]
        public async Task SwitchTeam(IDialogContext context, LuisResult result)
        {
            var team = context.UserData.GetValueOrDefault<Team>(Constants.UserDataKey.TeamId);
            if (team != null)
                await context.SayAsync($"You current team is **{team.DisplayName}**");
            await context.Forward(new SelectTeamDialog(), TeamSelected, context.Activity, CancellationToken.None);
        }

        private async Task TeamSelected(IDialogContext context, IAwaitable<Team> result)
        {
            Team team = null;
            try
            {
                team = await result;
            }
            catch (ActionCancelledException)
            {
                await context.SayAsync($"Switch team canceled.");
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, "Switch team failed: ");
            }

            if (team != null)
            {
                context.UserData.SetValue(Constants.UserDataKey.TeamId, team);
                await context.SayAsync($"Done. You current team is **{team.DisplayName}**");
            }
            context.Wait(MessageReceived);
        }

        #endregion

        #region Clear Data

        [LuisIntent("Data.Clear")]
        public async Task ClearDataAsync(IDialogContext context, LuisResult result)
        {
            context.UserData.Clear();
            context.ConversationData.Clear();

            var user = await context.GetTeamsAccountAsync();

            var tokenCache = ADALTokenCache.Create(user.ObjectId);
            tokenCache.Clear();

            await context.SayAsync("User & Conversation data were cleared. Token cache was cleared");
            context.Wait(MessageReceived);
        }

        #endregion

        #region None

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.ChoiceAsync("I'm the Ideas Bot. You can tell me:", new[] { "New Idea", "List Ideas" });
            context.Wait(MessageReceived);
        }

        #endregion

        #region Private Methods

        private async Task HandleExceptionAsync(IDialogContext context, Exception ex, string messagePrefix)
        {
            if (ex is SignTimeoutException)
                await context.SayAsync($"Sorry I timed out - please ask me to do something again.");
            else
                await context.SayAsync(messagePrefix + ex.Message);
        }

        #endregion
    }
}