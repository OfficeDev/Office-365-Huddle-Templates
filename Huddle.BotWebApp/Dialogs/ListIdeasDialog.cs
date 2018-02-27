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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    [Serializable]
    public class ListIdeasDialog : TeamDialog<Idea[]>
    {
        private static readonly string[] ideaStatusChoices = new[] { "All ideas", "New ideas", "In progress ideas", "Shareable ideas" };

        public string status { get; set; }

        public DateTime? from { get; set; }

        public ListIdeasDialog(string status, DateTime? from)
        {
            this.status = status;
            this.from = from;
        }

        protected override async Task StartTeamActionAsync(IDialogContext context)
        {
            if (status.IsNullOrEmpty())
            {
                await context.ChoiceAsync("Would you like?", ideaStatusChoices);
                context.Wait(StatusSelected);
            }
            else
                await context.Forward(new SignInDialog(), ListIdeasAsync, context.Activity, CancellationToken.None);
        }

        private async Task StatusSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;
            var s = activity.GetTrimmedText().ToLower();
            if (s.Contains("new")) status = "New";
            else if (s.Contains("in progress")) status = "In progress";
            else if (s.Contains("shareable")) status = "Shareable";
            else if (s.Contains("all")) status = null;
            else
            {
                await context.ChoiceAsync("Would you like?", ideaStatusChoices);
                context.Wait(StatusSelected);
                return;
            }

            await context.Forward(new SignInDialog(), ListIdeasAsync, context.Activity, CancellationToken.None);
        }

        private async Task ListIdeasAsync(IDialogContext context, IAwaitable<GraphServiceClient> result)
        {
            var plannerService = new PlannerService(await result);
            var ideaService = new IdeaService(await result);

            var plan = await plannerService.GetTeamPlanAsync(Team);
            var ideas = await ideaService.GetAsync(Team, plan.Id, status, from);

            var summary = ideas.Length > 0
                ? $"Getting {ideas.Length} {(ideas.Length > 1 ? "ideas" : "idea")} from Microsoft Planner, Please wait..."
                : "No idea was found.";
            await context.SayAsync(summary);

            foreach (var bucket in Constants.IdeasPlan.Buckets.All)
            {
                var bucketIdeas = ideas.Where(i => i.Bucket == bucket).ToArray();
                if (!bucketIdeas.Any()) continue;

                if (string.IsNullOrEmpty(status))
                    await context.SayAsync($"{bucket} ({bucketIdeas.Length + " " + (bucket.Length > 1 ? "ideas" : "idea")})");

                int pageSize = 6;
                int pageCount = (bucketIdeas.Length + pageSize - 1) / pageSize;
                for (int page = 0; page < pageCount; page++)
                {
                    var message = context.MakeMessage();
                    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                    var pageIdeas = bucketIdeas.Skip(pageSize * page).Take(pageSize).ToArray();
                    foreach (var idea in pageIdeas)
                    {
                        await ideaService.GetDetailsAsync(idea);
                        var url = ideaService.GetIdeaUrl(Team.Id, plan.Id, idea.Id);
                        var owners = $"Owners: {idea.Owners.Select(i => i.DisplayName).Join(", ")}";
                        var text = $"Start Date<br/>{idea.StartDate?.DateTime.ToShortDateString()}";
                        if (idea.Description.IsNotNullAndEmpty())
                            text += $"<br/><br/>{idea.Description.Replace("\r\n", "<br/>").Replace("\n", "<br/>")}";
                        var viewAction = new CardAction(ActionTypes.OpenUrl, "View", value: url);
                        var heroCard = new HeroCard(idea.Title, owners, text, buttons: new List<CardAction> { viewAction });
                        message.Attachments.Add(heroCard.ToAttachment());
                    }
                    await context.PostAsync(message);
                }
            }

            context.Done(ideas);
        }
    }
}
