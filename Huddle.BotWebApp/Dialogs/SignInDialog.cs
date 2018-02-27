/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Utils;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Dialogs
{
    [Serializable]
    public class SignInDialog : IDialog<GraphServiceClient>
    {
        public static readonly int SignInMinutes = 3;

        public static string DefaultSignInUrl { get; set; }

        private string signInUrl;
        private string userObjectId;

        public SignInDialog(string signInUrl)
        {
            this.signInUrl = signInUrl;
        }

        public SignInDialog() : this(DefaultSignInUrl) { }
        
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(SignInAsync);
            return Task.CompletedTask;
        }
        
        private async Task SignInAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var teamAccount = await context.GetTeamsAccountAsync();
            userObjectId = teamAccount.ObjectId;      

            var graphServiceClient = await AuthenticationHelper.GetGraphServiceClientSafeAsync(userObjectId, Permissions.Delegated);
            if (graphServiceClient != null)
            {
                context.Done(graphServiceClient);
                return;
            }

            var signinCard = new SigninCard
            {
                Text = "Authentication Required",
                Buttons = new List<CardAction>()
                {
                    new CardAction(
                        ActionTypes.OpenUrl,
                        "Sign into Office 365",
                        value: signInUrl)
                }
            };
            var message = context.MakeMessage();
            message.Attachments.Add(signinCard.ToAttachment());
            await context.PostAsync(message);

            await context.SayAsync("Hi There! Nice to meet you. Please click the Sign into Office 365 button above.");

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(SignInMinutes));
            try
            {                
                graphServiceClient = await GetGraphServiceClientAsync(activity, cancellationTokenSource.Token);                
            }
            catch (TaskCanceledException ex)
            {
                var exception = new SignTimeoutException("Sign in timeout", ex);
                context.Fail(exception);
                return;
            }
            
            context.Done(graphServiceClient);
        }

        private async Task<GraphServiceClient> GetGraphServiceClientAsync(Activity activity, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return null;

                var client = await AuthenticationHelper.GetGraphServiceClientSafeAsync(userObjectId, Permissions.Delegated);
                if (client != null) return client;

                await Task.Delay(1000 * 2, cancellationToken);
            }
        }          
    }
}
