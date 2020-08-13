/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Bots;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{

    public class HuddleDialog : ComponentDialog
    {
        private const string CancelMsgText = "Cancelled.";

        protected IConfiguration Configuration;
        protected readonly string ConnectionName;
        protected readonly UserState UserState;
        protected readonly IStatePropertyAccessor<UserProfile> UserProfileAccessor;

        public HuddleDialog(string id, IConfiguration configuration, UserState userState)
            : base(id)
        {
            Configuration = configuration;
            ConnectionName = configuration["ConnectionName"];
            UserState = userState;
            UserProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            var oauthPromptSettings = new OAuthPromptSettings
            {
                ConnectionName = ConnectionName,
                Text = "Please Sign In",
                Title = "Sign In"
            };
            AddDialog(new OAuthPrompt(nameof(OAuthPrompt), oauthPromptSettings));
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null) return result;
            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text?.ToLowerInvariant();
                switch (text)
                {
                    case "cancel":
                    case "quit":
                        var cancelMessage = MessageFactory.Text(CancelMsgText, CancelMsgText, InputHints.IgnoringInput);
                        await innerDc.Context.SendActivityAsync(cancelMessage, cancellationToken);
                        await innerDc.CancelAllDialogsAsync(cancellationToken);
                        return await innerDc.BeginDialogAsync(InitialDialogId, cancellationToken: cancellationToken);
                    case "logout":
                        // The bot adapter encapsulates the authentication processes.
                        var botAdapter = (BotFrameworkAdapter)innerDc.Context.Adapter;
                        await botAdapter.SignOutUserAsync(innerDc.Context, ConnectionName, null, cancellationToken);
                        await innerDc.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."), cancellationToken);
                        await innerDc.CancelAllDialogsAsync(cancellationToken);
                        return await innerDc.BeginDialogAsync(InitialDialogId, cancellationToken: cancellationToken);
                }
            }
            return null;
        }
    }
}
