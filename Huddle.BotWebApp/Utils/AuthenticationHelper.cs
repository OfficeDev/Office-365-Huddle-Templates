/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Infrastructure;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SP = Microsoft.SharePoint.Client;

namespace Huddle.BotWebApp.Utils
{
    public enum Permissions
    {
        /// <summary>
        /// The client accesses the web API as the signed-in user.
        /// </summary>
        Delegated,
        /// <summary>
        /// The client accesses the web API directly as itself (no user context).
        /// </summary>
        /// <remarks>
        /// This type of permission requires administrator consent.
        /// </remarks>
        Application
    }

    /// <summary>
    /// A static helper class used to get access token, authentication result, authentication context and instances of service client.
    /// </summary>
    public static class AuthenticationHelper
    {
        public static async Task<GraphServiceClient> GetGraphServiceClientAsync(string userObjectId, Permissions permissions = Permissions.Delegated)
        {
            var accessToken = await GetAccessTokenAsync(userObjectId, Constants.Resources.MSGraph, permissions);
            var serviceRoot = Constants.Resources.MSGraph + "/v1.0/" + ClaimsPrincipal.Current.GetTenantId();
            return new GraphServiceClient(serviceRoot, new BearerAuthenticationProvider(accessToken));
        }

        public static async Task<GraphServiceClient> GetGraphServiceClientSafeAsync(string userObjectId, Permissions permissions = Permissions.Delegated)
        {
            try
            {
                return await GetGraphServiceClientAsync(userObjectId, permissions);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<SP.ClientContext> GetAppOnlySharePointClientContextAsync()
        {
            var resourceId = GetSharePointResourceId(Constants.BaseSPSiteUrl);
            var token = await GetAccessTokenAsync(Constants.AADTenantId, resourceId, Permissions.Application);
            return GetSharePointClientContext(Constants.BaseSPSiteUrl, token);
        }

        public static async Task<string> GetAccessTokenAsync(string userObjectId, string resource, Permissions permissions = Permissions.Delegated)
        {
            var result = await GetAuthenticationResult(userObjectId, resource, permissions);
            return result.AccessToken;
        }

        public static Task<AuthenticationResult> GetAuthenticationResult(string userObjectId, string resource, Permissions permissions)
        {
            var context = GetAuthenticationContext(userObjectId, permissions);
            var clientCredential = new ClientCredential(Constants.AADClientId, Constants.AADClientSecret);

            if (permissions == Permissions.Delegated)
            {
                var userIdentifier = new UserIdentifier(userObjectId, UserIdentifierType.UniqueId);
                return context.AcquireTokenSilentAsync(resource, clientCredential, userIdentifier);
            }
            else if (permissions == Permissions.Application)
            {
                var cert = GetX509Certificate();
                if (cert == null)
                    throw new Exception("Could not find Client Assertion Certificate with thumbprint " + Constants.AADClientCertThumbprint);

                var clientCert = new ClientAssertionCertificate(Constants.AADClientId, cert);
                return context.AcquireTokenAsync(resource, clientCert);
            }
            else
                throw new NotImplementedException();
        }

        public static AuthenticationContext GetAuthenticationContext(ClaimsIdentity claimsIdentity, Permissions permissions)
        {
            var userObjectId = claimsIdentity.GetObjectIdentifier();
            return GetAuthenticationContext(userObjectId, permissions);
        }


        private static AuthenticationContext GetAuthenticationContext(string userId, Permissions permissions)
        {
            var signedInUserID = permissions == Permissions.Delegated ? userId : Constants.AADTenantId;
            var tokenCache = ADALTokenCache.Create(signedInUserID);
            return new AuthenticationContext(Constants.Authority, tokenCache);
        }

        private static X509Certificate2 GetX509Certificate()
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates.OfType<X509Certificate2>().FirstOrDefault(i => i.Thumbprint == Constants.AADClientCertThumbprint);
            }
        }

        private static string GetSharePointResourceId(string siteCollectionUrl)
        {
            var url = new Uri(siteCollectionUrl);
            return url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
        }

        private static SP.ClientContext GetSharePointClientContext(string url, string token)
        {
            var clientContext = new SP.ClientContext(url)
            {
                AuthenticationMode = SP.ClientAuthenticationMode.Anonymous,
                FormDigestHandlingEnabled = false
            };
            clientContext.ExecutingWebRequest += (sender, args) =>
                    args.WebRequestExecutor.WebRequest.Headers["Authorization"] = "Bearer " + token;
            return clientContext;
        }
    }
}
