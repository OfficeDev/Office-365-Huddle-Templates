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

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    public class ListIdeasOptions
    {
        public string Status { get; set; }

        public DateTime? From { get; set; }
    }

    public class ListIdeasDialog : HuddleDialog
    {
        private static readonly string[] ideaStatusChoices = new[] { "All ideas", "New ideas", "In progress ideas", "Completed", "Shareable ideas" };

        public ListIdeasDialog(string id, IConfiguration configuration, UserState userState)
            : base(id, configuration, userState)
        {
            this.AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] {
                SelectTeamStepAsync,
                SelectStatusStepAsync,
                ListIdeasPhase1Async,
                ListIdeasPhase2Async
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectTeamStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await UserProfileAccessor.GetAsync(stepContext.Context);
            if (userProfile.SelectedTeam == null)
                return await stepContext.BeginDialogAsync(nameof(SelectTeamDialog), cancellationToken: cancellationToken);
            return await stepContext.NextAsync(userProfile.SelectedTeam, cancellationToken);
        }

        private async Task<DialogTurnResult> SelectStatusStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var team = (Models.Team)stepContext.Result;
            if (team == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("No team was selected. Cancelled creating idea."), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            var listIdeasOptions = (ListIdeasOptions)stepContext.Options;
            if (string.IsNullOrEmpty(listIdeasOptions.Status))
            {
                var propmtOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Would you like?"),
                    Choices = ideaStatusChoices.Select(i => new Choice(i)).ToList(),
                    Style = ListStyle.HeroCard
                };
                return await stepContext.PromptAsync(nameof(ChoicePrompt), propmtOptions, cancellationToken);
            }
            return await stepContext.NextAsync(listIdeasOptions.Status, cancellationToken);
        }

        private async Task<DialogTurnResult> ListIdeasPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var listIdeasOptions = (ListIdeasOptions)stepContext.Options;
            if (string.IsNullOrEmpty(listIdeasOptions.Status))
                listIdeasOptions.Status = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ListIdeasPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse?.Token == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenResponse.Token);
            var tenantId = token.Claims.FirstOrDefault(i => i.Type == "tid").Value;

            var userProfile = await UserProfileAccessor.GetAsync(stepContext.Context);
            var team = userProfile.SelectedTeam;

            var listIdeasOptions = (ListIdeasOptions)stepContext.Options;

            var plannerService = new PlannerService(tokenResponse.Token);
            var ideaService = new IdeaService(tokenResponse.Token);

            var plan = await plannerService.GetTeamPlanAsync(team.Id, team.DisplayName);
            if (plan == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Could not found the plan."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            var bucketName = ideaService.GetBucketName(listIdeasOptions.Status);
            var ideas = await ideaService.GetAsync(plan.Id, bucketName, listIdeasOptions.From);

            var summary = ideas.Length > 0
                ? $"Getting {ideas.Length} {(ideas.Length > 1 ? "ideas" : "idea")} from Microsoft Planner, please wait..."
                : "No idea was found.";
            await stepContext.Context.SendActivityAsync(summary);

            foreach (var bucket in IdeasPlan.Buckets.All)
            {
                var bucketIdeas = ideas.Where(i => StringComparer.InvariantCultureIgnoreCase.Equals(i.Bucket, bucket)).ToArray();
                if (!bucketIdeas.Any()) continue;

                if (string.IsNullOrEmpty(bucketName))
                    await stepContext.Context.SendActivityAsync($"{bucket} ({bucketIdeas.Length + " " + (bucket.Length > 1 ? "ideas" : "idea")})");

                int pageSize = 6;
                int pageCount = (bucketIdeas.Length + pageSize - 1) / pageSize;
                for (int page = 0; page < pageCount; page++)
                {
                    var attachments = new List<Attachment>();
                    var pageIdeas = bucketIdeas.Skip(pageSize * page).Take(pageSize).ToArray();
                    foreach (var idea in pageIdeas)
                    {
                        await ideaService.GetDetailsAsync(idea);
                        var url = ideaService.GetIdeaUrl(tenantId, team.Id, plan.Id, idea.Id);
                        var owners = $"Owners: {string.Join(",", idea.Owners)}";
                        var text = $"Start Date<br/>{idea.StartDate?.DateTime.ToShortDateString()}";
                        if (!string.IsNullOrEmpty(idea.Description))
                            text += $"<br/><br/>{idea.Description.Replace("\r\n", "<br/>").Replace("\n", "<br/>")}";
                        var viewAction = new CardAction(ActionTypes.OpenUrl, "View", value: url);
                        var heroCard = new HeroCard(idea.Title, owners, text, buttons: new List<CardAction> { viewAction });
                        attachments.Add(heroCard.ToAttachment());
                    }

                    var message = MessageFactory.Carousel(attachments);
                    await stepContext.Context.SendActivityAsync(message);
                }
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
