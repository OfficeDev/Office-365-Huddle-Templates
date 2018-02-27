/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Huddle.BotWebApp.Services;
using Huddle.BotWebApp.SharePoint;
using Huddle.BotWebApp.Utils;
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
    public class CreateIdeaDialog : TeamDialog<Idea>
    {
        private static readonly string[] ConfirmOptions = { "Yes", "No" };

        private Idea idea = new Idea();
        private string nextSteps;
        private Metric metric;
        private Metric[] metrics;

        protected override async Task StartTeamActionAsync(IDialogContext context)
        {
            await context.SayAsync($"Hi {TeamsChannelAccount.GivenName}! What is your idea?");
            context.Wait(SetTitleAsync);
        }

        private async Task SetTitleAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;

            idea.Title = activity.GetTrimmedText();
            if (idea.Title.IsNullOrEmpty())
            {
                await context.SayAsync("Idea is required. Please input again.");
                context.Wait(SetTitleAsync);
            }
            else
            {
                await context.SayAsync("Great! What are the next steps to implement this idea?");
                context.Wait(SetNextStepsAsync);
            }
        }

        private async Task SetNextStepsAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;

            nextSteps = activity.GetTrimmedText();
            if (nextSteps.IsNullOrEmpty())
            {
                await context.SayAsync("Next steps is required. Please input again.");
                context.Wait(SetNextStepsAsync);
            }
            else
            {
                idea.Description = $"Next Steps\r\n{nextSteps}";
                await SendMetricsCardAsync(context);
                context.Wait(SetMetricAsync);
            }
        }

        private async Task SetMetricAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;

            var metricText = activity.GetTrimmedText();
            if (!metricText.IsNullOrEmpty())
            {
                int number = -1;
                if (int.TryParse(metricText, out number) && number > 0 && number <= metrics.Length)
                {
                    metric = metrics[number - 1];
                    await context.SayAsync("You selected: " + metric.Name);
                }
                else
                    metric = metrics.FirstOrDefault(i => metricText.IgnoreCaseEquals(i.Name));
            }

            if (metric == null)
            {
                await context.SayAsync("I don't see that as an active metric. Please add new metrics through the Metric Input tab in your team.");
                await SendMetricsCardAsync(context);
                context.Wait(SetMetricAsync);
            }
            else
            {
                idea.Description += $"\r\n\r\nAligned to Metric\r\n{metric.Name}";
                await context.SayAsync($"Thanks. Please identify the idea owner. Loading members of {Team.DisplayName}...");
                await context.Forward(new SelectTeamMemberDialog(Team, ""), SetOwner, activity, CancellationToken.None);
            }
        }

        private async Task SetOwner(IDialogContext context, IAwaitable<TeamMember> result)
        {
            idea.Owners = new[] { await result };

            await context.ChoiceAsync(
                "Almost done. What date will you start to implement this? <br /> (Click one of the dates below, or input one with format mm/dd/yyyy)",
                new[] { "Today", "Tomorrow" });
            context.Wait(SetStartDate);
        }

        private async Task SetStartDate(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;

            DateTime startDate;
            var commitDateStr = activity.GetTrimmedText();
            if (commitDateStr.IgnoreCaseEquals("today"))
                startDate = new DateTime(DateTime.Today.Ticks);
            else if (commitDateStr.IgnoreCaseEquals("tomorrow"))
                startDate = new DateTime(DateTime.Today.Ticks).AddDays(1);
            else if (!DateTime.TryParse(commitDateStr, out startDate))
            {
                await context.SayAsync("Invalid date, please input again. (Date format: mm/dd/yyyy)");
                context.Wait(SetStartDate);
                return;
            }

            startDate = startDate.Date.AddHours(12);
            idea.StartDate = new DateTimeOffset(startDate, TimeSpan.Zero);
            var summary = $"**Idea**: {idea.Title}<br />" +
                $"**Next Steps**: {(nextSteps.Contains("\r\n") ? "<br />" : "")}{nextSteps.Replace("\r\n", "<br />")}<br />" +
                $"**Aligned to Metric**: {metric.Name}<br />" +
                $"**Owner**: {idea.Owners.Select(i => i.DisplayName).FirstOrDefault()}<br />" +
                $"**Start Date**: {idea.StartDate?.DateTime.ToShortDateString()}";
            await context.SayAsync(summary);
            await context.ComfirmAsync("Would you like to submit this idea?");

            context.Wait(ConfirmIdeaAsync);
        }

        private async Task ConfirmIdeaAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;

            var answer = activity.GetTrimmedText();
            if (!answer.IgnoreCaseIn(ConfirmOptions))
            {
                await context.ComfirmAsync("Sorry, I don't understand. Would you like to submit this idea?");
                context.Wait(ConfirmIdeaAsync);
                return;
            }

            if (answer.IgnoreCaseEquals("No"))
            {
                await context.ComfirmAsync("Okay. Would you like an email summary of your idea? (Not functional during pilot)");
                context.Wait(ConfirmToSendIdeaByEmail);
            }
            else
                await context.Forward(new SignInDialog(), SaveIdea, context.Activity, CancellationToken.None);
        }

        private async Task ConfirmToSendIdeaByEmail(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;

            var answer = activity.Text.Trim();
            if (!answer.IgnoreCaseIn(ConfirmOptions))
            {
                await context.ComfirmAsync("Sorry, I don't understand. Would you like an email summary of your idea? (Not functional during pilot)");
                context.Wait(ConfirmToSendIdeaByEmail);
                return;
            }

            if (answer.IgnoreCaseEquals("Yes"))
                await context.SayAsync("Sorry. Sending idea through email is not implemented!");
            else
                await context.SayAsync("Create idea canceled.");
            context.Done(null as Idea);
        }

        private async Task SaveIdea(IDialogContext context, IAwaitable<GraphServiceClient> result)
        {
            var planService = new PlannerService(await result);
            var ideaService = new IdeaService(await result);

            var plan = await planService.GetTeamPlanAsync(Team);
            if (plan == null) throw new ApplicationException($"Could not found plan named '{Team.DisplayName}'");

            var plannerTask = await ideaService.CreateAsync(plan.Id, idea.Title, idea.StartDate, idea.Owners.Select(i => i.Id).FirstOrDefault(), idea.Description);
            var plannerTaskUrl = ideaService.GetIdeaUrl(Team.Id, plan.Id, plannerTask.Id);

            try
            {
                var clientContext = await AuthenticationHelper.GetAppOnlySharePointClientContextAsync();
                var metricsService = new MetricsService(clientContext);
                await metricsService.CreateMetricIdeaAsync(metric.Id, plannerTask, Constants.IdeasPlan.Buckets.NewIdea, plannerTaskUrl);
            }
            catch (Exception ex)
            {
                await context.SayAsync("Failed to add item to MetricIdea list: " + ex.Message);
            }

            await context.SayAsync("Idea created.");
            context.Done(idea);
        }

        private async Task SendMetricsCardAsync(IDialogContext context)
        {
            var clientContext = await AuthenticationHelper.GetAppOnlySharePointClientContextAsync();
            var metricsService = new MetricsService(clientContext);
            metrics = (await metricsService.GetActiveMetricsAsync(Team.Id))
                .Union(new[] { Metric.Other })
                .ToArray();

            var buttons = metrics
                .Select((m, i) => new CardAction
                {
                    Title = $"{i + 1}. {m.Name}",
                    Value = m.Name,
                    Type = ActionTypes.ImBack
                })
                .ToArray();
            var heroCard = new HeroCard(text: "What metric does this idea try to move?", buttons: buttons);

            var message = context.MakeMessage();
            message.Attachments.Add(heroCard.ToAttachment());
            await context.PostAsync(message);
        }
    }
}
