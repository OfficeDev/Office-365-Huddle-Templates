/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Dialogs;
using Huddle.BotWebApp.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Polly;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Huddle.BotWebApp.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private ConnectorClient connectorClient;
        private InvokeProcessor invokeProcessor;

        public MessagesController()
        {
            this.connectorClient = new ConnectorClient(
                new Uri("https://smba.trafficmanager.net/amer-client-ss.msg/"),
                ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppIdKey],
                ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppPasswordKey]);
            this.connectorClient.SetRetryPolicy(
                RetryHelpers.DefaultPolicyBuilder.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) }));
            this.invokeProcessor = new InvokeProcessor(connectorClient);
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var account = await activity.GetTeamsAccountAsync();
            SignInDialog.DefaultSignInUrl = GetSignInUri(account.UserPrincipalName).ToString();

            if (activity?.Type == ActivityTypes.Message)
                await Conversation.SendAsync(activity, () => new RootDialog());
            else if (activity?.Type == ActivityTypes.Invoke)
                await invokeProcessor.ProcessAsync(activity);
            else
                HandleSystemMessage(activity);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing that the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private Uri GetSignInUri(string userPrincipalName)
        {
            var baseUri = Request.RequestUri.GetComponents(UriComponents.Scheme | UriComponents.HostAndPort, UriFormat.Unescaped);
            return new Uri(Request.RequestUri, $"/Account/SignIn?loginHint={userPrincipalName}&redirectUri=" + HttpUtility.UrlEncode("/Account/SignInCallback"));
        }
    }
}
