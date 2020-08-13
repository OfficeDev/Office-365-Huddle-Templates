/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    public class MailIdeaSummaryDialog : HuddleDialog
    {
        public MailIdeaSummaryDialog(string id, IConfiguration configuration, UserState userState)
            : base(id, configuration, userState)
        {
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] {
                MailSummaryPhase1Async,
                MailSummaryPhase2Async
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> MailSummaryPhase1Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Okay. Would you like an email summary of your idea? (Not functional during pilot)")
            };
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> MailSummaryPhase2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var text = (bool)stepContext.Result
                ? "Sorry. Sending idea through email is not implemented!"
                : "Okay.";

            await stepContext.Context.SendActivityAsync(text);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
