/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SP = Microsoft.SharePoint.Client;

namespace Huddle.WebJob.Infrastructure
{
    public static class AuthenticationHelper
    {
        public static readonly string AADCertThumbprint = Constants.AADClientCertThumbprint;

        public static async Task<SP.ClientContext> GetSharePointClientAppOnlyContextAsync()
        {
            var resource = GetSharePointResourceId(Constants.BaseSPSiteUrl);

            var context = new AuthenticationContext(Constants.Authority, ADALTokenCache.Instances);
            var cert = GetX509Certificate();
            if (cert == null)
                throw new Exception("Could not find Client Assertion Certificate with thumbprint " + Constants.AADClientCertThumbprint);

            var clientCert = new ClientAssertionCertificate(Constants.AADClientId, cert);
            var result = await context.AcquireTokenAsync(resource, clientCert);

            return GetSharePointClientContext(Constants.BaseSPSiteUrl, result.AccessToken);
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

        private static ClientAssertionCertificate GetClientAssertionCertificate()
        {
            var cert = GetX509Certificate();
            if (cert == null)
                throw new Exception("Could not find Client Assertion Certificate with thumbprint " + Constants.AADClientCertThumbprint);
            return new ClientAssertionCertificate(Constants.AADClientId, cert);
        }

        private static X509Certificate2 GetX509Certificate()
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates.OfType<X509Certificate2>()
                .FirstOrDefault(i => i.Thumbprint == Constants.AADClientCertThumbprint);
        }
    }
}
