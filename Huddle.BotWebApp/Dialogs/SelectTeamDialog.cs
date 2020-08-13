/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    public class SelectTeamDialog : HuddleDialog
    {
        public SelectTeamDialog(string id, IConfiguration configuration, UserState state)
            : base(id, configuration, state)
        {

            this.AddDialog(new TextPrompt(nameof(TextPrompt)));
            this.AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] {
               SignStepAsync,
               ShowTeamsStepAsync,
               FinalStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        protected async Task<DialogTurnResult> SignStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await UserProfileAccessor.GetAsync(stepContext.Context);
            if (userProfile.SelectedTeam != null)
            {
                var activity = MessageFactory.Text($"Your current team is **{userProfile.SelectedTeam.DisplayName}**");
                activity.TextFormat = "markdown";
                await stepContext.Context.SendActivityAsync(activity);
            }
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowTeamsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse?.Token == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            var service = new TeamsService(tokenResponse.Token);
            var teams = await service.GetJoinedTeamsAsync();
            if (teams.Length == 0)
            {
                await stepContext.Context.SendActivityAsync("Sorry, you do not belong to any team at the moment.");
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if (teams.Length == 1)
            {
                var team = teams.First();

                var userProfile = await UserProfileAccessor.GetAsync(stepContext.Context);
                userProfile.SelectedTeam = team;

                await stepContext.Context.SendActivityAsync($"You only have one team: {team.DisplayName}. It was selected automatically.");
                return await stepContext.EndDialogAsync(team, cancellationToken);
            }
            else
            {
                stepContext.Values["teams"] = teams;
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select one of you teams:"),
                    Choices = teams.Select(i => new Choice(i.DisplayName)).ToArray(),
                    Style = ListStyle.HeroCard
                };
                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var teamName = ((FoundChoice)stepContext.Result).Value;

            var teams = (Models.Team[])stepContext.Values["teams"];
            var team = teams.FirstOrDefault(i => i.DisplayName == teamName);

            var userProfile = await UserProfileAccessor.GetAsync(stepContext.Context);
            userProfile.SelectedTeam = team;

            var activity = MessageFactory.Text($"You selected **{team.DisplayName}**.");
            activity.TextFormat = "markdown";
            await stepContext.Context.SendActivityAsync(activity);

            return await stepContext.EndDialogAsync(team, cancellationToken);
        }
    }
}
