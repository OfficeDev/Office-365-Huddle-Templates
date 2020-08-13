/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.CognitiveModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    public class MainDialog : HuddleDialog
    {
        private readonly IRecognizer _luisRecognizer;

        public MainDialog(IConfiguration configuration, IRecognizer luisRecognizer, UserState userState)
            : base(nameof(MainDialog), configuration, userState)
        {
            _luisRecognizer = luisRecognizer;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new CreateIdeaDialog(nameof(CreateIdeaDialog), configuration, userState));
            AddDialog(new ListIdeasDialog(nameof(ListIdeasDialog), configuration, userState));
            AddDialog(new SelectTeamDialog(nameof(SelectTeamDialog), configuration, userState));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var text = stepContext.Options?.ToString() ?? "I'm the Ideas Bot. You can tell me:";
            var options = new[] { "New Idea", "List Ideas" };

            var heroCard = new HeroCard
            {
                Text = text,
                Buttons = options.Select(i => new CardAction(ActionTypes.MessageBack, i, null, i, i, i, null)).ToList()
            };
            var promptOptions = new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(heroCard.ToAttachment()),
            };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<IdeasModel>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case IdeasModel.Intent.Idea_Create:
                    return await stepContext.BeginDialogAsync(nameof(CreateIdeaDialog), new CreateIdeaOptions(), cancellationToken);

                case IdeasModel.Intent.Idea_List:
                    var status = luisResult.Entities.Idea_Status?[0].FirstOrDefault();
                    var option = new ListIdeasOptions() { Status = status };
                    return await stepContext.BeginDialogAsync(nameof(ListIdeasDialog), option, cancellationToken);

                case IdeasModel.Intent.Team_Switch:
                    return await stepContext.BeginDialogAsync(nameof(SelectTeamDialog), cancellationToken: cancellationToken);

                case IdeasModel.Intent.Data_Clear:
                    var botAdapter = (BotFrameworkAdapter)stepContext.Context.Adapter;
                    await botAdapter.SignOutUserAsync(stepContext.Context, ConnectionName, null, cancellationToken);
                    await UserState.ClearStateAsync(stepContext.Context, cancellationToken);
                    await stepContext.Context.SendActivityAsync("User data and security token were cleared.");
                    return await stepContext.NextAsync(null, cancellationToken);

                case IdeasModel.Intent.None:
                    break;
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
