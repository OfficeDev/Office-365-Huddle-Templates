/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Constants = Microsoft.Recognizers.Text.DataTypes.TimexExpression.Constants;

namespace Huddle.BotWebApp.Dialogs
{
    public class DateResolverDialog : HuddleDialog
    {
        private const string PromptMsgText = "Almost done. What date will you start to implement this? <br />(Click one of the dates below, or input one with format mm/dd/yyyy)";
        private const string RepromptMsgText = "I'm sorry, please enter a full date including Day Month and Year.";

        public DateResolverDialog(string id, IConfiguration configuration, UserState userState)
            : base(id ?? nameof(DateResolverDialog), configuration, userState)
        {
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt), DateTimePromptValidator));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var timex = (string)stepContext.Options;
            if (timex == null)
            {
                var dates = new[] { "Today", "Tomorrow" };
                var heroCard = new HeroCard
                {
                    Text = PromptMsgText,
                    Buttons = dates.Select(i => new CardAction(ActionTypes.MessageBack, i, null, i, i, i, null)).ToList()
                };
                var promptOptions = new PromptOptions
                {
                    Prompt = (Activity)MessageFactory.Attachment(heroCard.ToAttachment()),
                    RetryPrompt = MessageFactory.Text(RepromptMsgText, RepromptMsgText, InputHints.ExpectingInput)
                };
                // We were not given any date at all so prompt the user.
                return await stepContext.PromptAsync(nameof(DateTimePrompt), promptOptions, cancellationToken);
            }

            // We have a Date we just need to check it is unambiguous.
            var timexProperty = new TimexProperty(timex);
            if (!timexProperty.Types.Contains(Constants.TimexTypes.Definite))
            {
                // This is essentially a "reprompt" of the data we were given up front.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(RepromptMsgText, RepromptMsgText, InputHints.ExpectingInput),
                };
                return await stepContext.PromptAsync(nameof(DateTimePrompt), promptOptions, cancellationToken);
            }
            return await stepContext.NextAsync(new List<DateTimeResolution> { new DateTimeResolution { Timex = timex } }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var timex = ((List<DateTimeResolution>)stepContext.Result)[0].Timex;
            return await stepContext.EndDialogAsync(timex, cancellationToken);
        }

        private static Task<bool> DateTimePromptValidator(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
                // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
                var timex = promptContext.Recognized.Value[0].Timex.Split('T')[0];

                // If this is a definite Date including year, month and day we are good otherwise reprompt.
                // A better solution might be to let the user know what part is actually missing.
                var isDefinite = new TimexProperty(timex).Types.Contains(Constants.TimexTypes.Definite);

                return Task.FromResult(isDefinite);
            }
            return Task.FromResult(false);
        }
    }
}
