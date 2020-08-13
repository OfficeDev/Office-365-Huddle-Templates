/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    public static class Extensions
    {
        public static async Task<DialogTurnResult> ShowChoices(this WaterfallStepContext stepContext, string dialogId, string title, string text, IEnumerable<string> options, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Text = text,
                Buttons = options.Select(i => new CardAction(ActionTypes.MessageBack, i, null, i, i, i, null)).ToList()
            };
            return await stepContext.PromptAsync(dialogId,
                new PromptOptions
                {
                    Prompt = (Activity)MessageFactory.Attachment(heroCard.ToAttachment()),
                },
                cancellationToken);
        }
    }
}
