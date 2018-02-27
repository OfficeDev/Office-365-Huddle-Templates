/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Huddle.BotWebApp
{
    public static class BotExtension
    {
        public static async Task<TeamsChannelAccount> GetTeamsAccountAsync(this IActivity activity)
        {
            // The result returned by the following code is lack of information
            // return activity.From.AsTeamsChannelAccount();
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            var members = await connector.Conversations.GetConversationMembersAsync(activity.Conversation.Id);
            var member = members
                .Where(i => i.Id == activity.From.Id)
                .AsTeamsChannelAccounts()
                .FirstOrDefault();
            return member;
        }

        public static string GetTrimmedText(this IMessageActivity activity)
        {
            if (activity.Text == null) return null;
            return activity.RemoveRecipientMention().Trim().Replace(" \n\n ", "\r\n");
        }

        public static Task<TeamsChannelAccount> GetTeamsAccountAsync(this IDialogContext context)
        {
            return context.Activity.GetTeamsAccountAsync();
        }

        public static async Task ComfirmAsync(this IDialogContext context, string text = "")
        {
            var message = context.MakeMessage();
            message.AddHeroCard(text, new[] { "Yes", "No" });
            await context.PostAsync(message);
        }

        public static async Task ChoiceAsync(this IDialogContext context, string text, IEnumerable<string> options)
        {
            var message = context.MakeMessage();
            message.AddHeroCard(text, options);
            await context.PostAsync(message);
        }
    }
}
