/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.Graph;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Huddle.MetricWebApp.Infrastructure
{
    public class BearerAuthenticationProvider : IAuthenticationProvider
    {
        private Task<string> getAccessToken;

        public BearerAuthenticationProvider(Task<string> getAccessToken)
        {
            this.getAccessToken = getAccessToken;
        }

        public BearerAuthenticationProvider(string accessToken)
            : this(Task.FromResult(accessToken)) { }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", await getAccessToken);
        }
    }
}
