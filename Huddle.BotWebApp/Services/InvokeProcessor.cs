/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Huddle.BotWebApp.SharePoint;
using Huddle.BotWebApp.Utils;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Services
{
    public class InvokeProcessor
    {
        private ConnectorClient connectorClient;

        public InvokeProcessor(ConnectorClient connectorClient)
        {
            this.connectorClient = connectorClient;
        }

        public async Task ProcessAsync(Activity activity)
        {
            Activity replyActivity;
            try
            {
                replyActivity = await ProcessCoreAsync(activity);
            }
            catch (Exception ex)
            {
                replyActivity = activity.CreateReply($"Error: " + ex.Message);
            }
            await connectorClient.Conversations.ReplyToActivityWithRetriesAsync(replyActivity);
        }

        public async Task<Activity> ProcessCoreAsync(Activity activity)
        {
            if (!activity.IsO365ConnectorCardActionQuery())
                throw new NotImplementedException("Only O365ConnectorCardAction is supported.");

            var data = activity.GetO365ConnectorCardActionQueryData();
            if (data.ActionId == "create-idea")
                return await SaveIdeaAsyncAsync(activity, data.Body);

            else
                throw new NotImplementedException($"Unknown action {data.ActionId}.");
        }

        public async Task<Activity> SaveIdeaAsyncAsync(Activity activity, string body)
        {
            var model = JsonConvert.DeserializeObject<CreateIdeaModel>(body);
            model.StartDate = new DateTime(model.StartDate.AddHours(12).Date.Ticks);

            var teamAccount = await activity.GetTeamsAccountAsync();
            var userObjectId = teamAccount.ObjectId;
            var graphServiceClient = await AuthenticationHelper.GetGraphServiceClientSafeAsync(userObjectId, Permissions.Delegated);

            var planService = new PlannerService(graphServiceClient);
            var ideaService = new IdeaService(graphServiceClient);

            var plan = await planService.GetTeamPlanAsync(model.Team);
            if (plan == null) throw new ApplicationException($"Could not found plan named '{model.Team.DisplayName}'");

            var description = $"Next Steps\r\n{model.NextSteps}\r\n\r\nAligned to Metric\r\n{model.Metric.Name}";
            var startDate = new DateTimeOffset(model.StartDate, TimeSpan.FromHours(12));
            var plannerTask = await ideaService.CreateAsync(plan.Id, model.Title, model.StartDate, model.Owner.Id, description);
            var plannerTaskUrl = ideaService.GetIdeaUrl(model.Team.Id, plan.Id, plannerTask.Id);

            var replyActivity = activity.CreateReply();
            try
            {
                var clientContext = await AuthenticationHelper.GetAppOnlySharePointClientContextAsync();
                var metricsService = new MetricsService(clientContext);
                await metricsService.CreateMetricIdeaAsync(model.Metric.Id, plannerTask, Constants.IdeasPlan.Buckets.NewIdea, plannerTaskUrl);

            }
            catch (Exception ex)
            {
                replyActivity.Text = "Failed to add item to MetricIdea list: " + ex.Message;
            }

            var card = new O365ConnectorCard("Idea created",
                sections: new O365ConnectorCardSection[] {
                    new O365ConnectorCardSection(
                        facts: new O365ConnectorCardFact[]{
                            new O365ConnectorCardFact("Title", model.Title),
                            new O365ConnectorCardFact("Next Steps", model.NextSteps.Replace("\n", "<br />")),
                            new O365ConnectorCardFact("Aligned to Metric", model.Metric.Name),
                            new O365ConnectorCardFact("Owner", model.Owner.DisplayName),
                            new O365ConnectorCardFact("Start Date", model.StartDate.AddHours(12).ToShortDateString()),
                        })
                });
            replyActivity.Attachments.Add(card.ToAttachment());
            return replyActivity;
        }
    }
}
