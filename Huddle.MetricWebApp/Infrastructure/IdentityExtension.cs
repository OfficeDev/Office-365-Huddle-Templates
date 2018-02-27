/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System.Security.Claims;
using System.Security.Principal;

namespace Huddle.MetricWebApp.Infrastructure
{
    public static class IdentityExtension
    {
        static readonly string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";

        static readonly string ObjectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public static string GetTenantId(this ClaimsIdentity identity)
        {
            return identity.FindFirst(TenantId)?.Value;
        }

        public static string GetObjectIdentifier(this ClaimsIdentity identity)
        {
            return identity.FindFirst(ObjectIdentifier)?.Value;
        }

        public static string GetTenantId(this IPrincipal user)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            if (claimsIdentity == null) return null;
            return GetTenantId(claimsIdentity);
        }

        public static string GetObjectIdentifier(this IPrincipal user)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            if (claimsIdentity == null) return null;
            return GetObjectIdentifier(claimsIdentity);
        }
    }
}
