/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Huddle.BotWebApp.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    public class CreateIdeaOptions
    {
        public string Title { get; set; }
        public string NextSteps { get; set; }
        public string Metric { get; set; }
        public string Owner { get; set; }
        public DateTime? StartDate { get; set; }
    }

    public class CreateIdeaDialog : HuddleDialog
    {
        public CreateIdeaDialog(string id, IConfiguration configuration, UserState userState)
            : base(id, configuration, userState)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog(nameof(DateResolverDialog), configuration, userState));
            AddDialog(new SelectTeamDialog(nameof(SelectTeamDialog), configuration, userState));
            AddDialog(new MailIdeaSummaryDialog(nameof(MailIdeaSummaryDialog), configuration, userState));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] {
                SelectTeamStepAsync,
                TitleStepAsync,
                NextStepsStepAsync,
                MetricPhase1Async,
                MetricPhase2Async,
                OwnerPhase1Async,
                OwnerPhase2Async,
                StartDateStepAsync,
                ConfirmStepAsync,
                CreateIdeaPhase1Async,
                CreateIdeaPhase2Async
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectTeamStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await UserProfileAccessor.GetAsync(stepContext.Context);
            if (userProfile.SelectedTeam != null)
                return await stepContext.NextAsync(userProfile.SelectedTeam, cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(SelectTeamDialog), cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> TitleStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var team = (Models.Team)stepContext.Result;
            if (team == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("No team was selected. Cancelled creating idea."), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            var createIdeaOptions = (CreateIdeaOptions)stepContext.Options;
            if (createIdeaOptions.Title != null)
                return await stepContext.NextAsync(createIdeaOptions.Title, cancellationToken);

            var member = await TeamsInfo.GetMemberAsync(stepContext.Context, stepContext.Context.Activity.From.Id, cancellationToken);
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text($"Hi @{member.GivenName}! What is your idea?")
            };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> NextStepsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var createIdeaOptions = (CreateIdeaOptions)stepContext.Options;
            createIdeaOptions.Title = (string)stepContext.Result;

            if (createIdeaOptions.NextSteps != null)
                return await stepContext.NextAsync(createIdeaOptions.NextSteps, cancellationToken);

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Great! What are the next steps to implement this idea?")
            };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> MetricPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var createIdeaOptions = (CreateIdeaOptions)stepContext.Options;
            createIdeaOptions.NextSteps = (string)stepContext.Result;

            if (createIdeaOptions.Metric != null)
                return await stepContext.NextAsync(createIdeaOptions.Metric, cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> MetricPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var createIdeaOptions = (CreateIdeaOptions)stepContext.Options;
            if (createIdeaOptions.Metric != null)
                return await stepContext.NextAsync(createIdeaOptions.Metric, cancellationToken);

            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(WaterfallDialog), createIdeaOptions, cancellationToken);
            }

            var userProfile = await UserProfileAccessor.GetAsync(stepContext.Context);
            var service = new MetricsService(tokenResponse.Token, Configuration["BaseSPSiteUrl"]);
            var metrics = (await service.GetActiveMetricsAsync(userProfile.SelectedTeam.Id)).ToList();
            metrics.Add(new Metric { Id = 0, Name = "Other" });
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("What metric does this idea try to move?"),
                Choices = metrics.Select(i => new Choice(i.Name)).ToArray()
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> OwnerPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var createIdeaOptions = (CreateIdeaOptions)stepContext.Options;
            if (stepContext.Result is FoundChoice)
                createIdeaOptions.Metric = ((FoundChoice)stepContext.Result).Value;

            if (createIdeaOptions.Owner != null)
                return await stepContext.NextAsync(createIdeaOptions.Owner, cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> OwnerPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var createIdeaOptions = (CreateIdeaOptions)stepContext.Options;
            if (createIdeaOptions.Owner != null)
                return await stepContext.NextAsync(createIdeaOptions.Owner, cancellationToken);

            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(WaterfallDialog), createIdeaOptions, cancellationToken);
            }

            var userProfile = await UserProfileAccessor.GetAsync(stepContext.Context);
            var service = new TeamsService(tokenResponse.Token);
            var members = await service.GetTeamMembersAsync(userProfile.SelectedTeam.Id);
            stepContext.Values["members"] = members;

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Thanks. Please identify the idea owner."),
                Choices = members.Select(i => new Choice(i.DisplayName)).ToArray(),
                Style = ListStyle.HeroCard
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> StartDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var createIdeaOptions = (CreateIdeaOptions)stepContext.Options;
            if (stepContext.Result is FoundChoice)
            {
                var members = (TeamMember[])stepContext.Values["members"];
                var owner = ((FoundChoice)stepContext.Result).Value;
                createIdeaOptions.Owner = members
                    .Where(i => i.DisplayName == owner)
                    .Select(i => i.Id)
                    .FirstOrDefault();
            }

            if (createIdeaOptions.StartDate != null)
                return await stepContext.NextAsync(createIdeaOptions.StartDate, cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), createIdeaOptions.StartDate, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dateStr = (string)stepContext.Result;
            var createIdeaOptions = (CreateIdeaOptions)stepContext.Options;
            createIdeaOptions.StartDate = DateTime.Parse(dateStr);

            var summary = $"**Idea**: {createIdeaOptions.Title}<br />" +
                $"**Next Steps**: {(createIdeaOptions.NextSteps.Contains("\r\n") ? "<br />" : "")}{createIdeaOptions.NextSteps.Replace("\r\n", "<br />")}<br />" +
                $"**Aligned to Metric**: {createIdeaOptions.Metric}<br />" +
                $"**Owner**: {createIdeaOptions.Owner}<br />" +
                $"**Start Date**: {createIdeaOptions.StartDate.Value.ToShortDateString()}";
            var activity = MessageFactory.Text(summary);
            activity.TextFormat = "markdown";
            await stepContext.Context.SendActivityAsync(activity, cancellationToken);

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Would you like to submit this idea?")
            };
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> CreateIdeaPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
                return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);

            var createIdeaOptions = (CreateIdeaOptions)stepContext.Options;
            return await stepContext.ReplaceDialogAsync(nameof(MailIdeaSummaryDialog), createIdeaOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> CreateIdeaPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            var userProfile = await base.UserProfileAccessor.GetAsync(stepContext.Context);
            var team = userProfile.SelectedTeam;

            var createIdeaOptions = (CreateIdeaOptions)stepContext.Options;
            var planService = new PlannerService(tokenResponse.Token);
            var plan = await planService.GetTeamPlanAsync(team.Id, team.DisplayName);

            if (plan == null)
            {
                var message = $"Failed to create the idea: could not find plan named '{team.DisplayName}'";
                await stepContext.Context.SendActivityAsync(message);
            }
            else
            {
                var description = $"Next Steps\r\n{createIdeaOptions.NextSteps}" +
                    $"\r\n\r\nAligned to Metric\r\n{createIdeaOptions.Metric}";
                var ideaService = new IdeaService(tokenResponse.Token);
                try
                {
                    await ideaService.CreateAsync(plan.Id,
                         createIdeaOptions.Title,
                         new DateTimeOffset(createIdeaOptions.StartDate.Value, TimeSpan.Zero),
                         createIdeaOptions.Owner,
                         description
                    );
                    await stepContext.Context.SendActivityAsync("Idea created.");
                }
                catch (Exception ex)
                {
                    var message = $"Failed to create the idea: {ex.Message}";
                    await stepContext.Context.SendActivityAsync(message);
                }
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
