/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.Graph;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Services
{
    public class Array<T>
    {
        public T[] Value { get; set; }
    }

    abstract public class GraphService
    {
        protected GraphServiceClient _graphServiceClient;

        public GraphService(string token)
        {
            this._graphServiceClient = GetAuthenticatedClient(token);
        }

        private GraphServiceClient GetAuthenticatedClient(string token)
        {
            var authProvider = new DelegateAuthenticationProvider(
                requestMessage =>
                {
                    // Append the access token to the request.
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

                    // Get event times in the current time zone.
                    requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");
                    requestMessage.Headers.Add("Prefer", "HonorNonIndexedQueriesWarningMayFailRandomly");

                    return Task.CompletedTask;
                });
            return new GraphServiceClient(authProvider);
        }
    }
}
