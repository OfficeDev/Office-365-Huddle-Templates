/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Huddle.WebJob.Infrastructure
{

    public class ADALTokenCache : TokenCache
    {
        public static readonly ADALTokenCache Instances = new ADALTokenCache();

        private byte[] cacheData;

        private ADALTokenCache()
        {
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
        }

        public override void Clear()
        {
            base.Clear();
            cacheData = null;
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            if (cacheData != null)
                this.Deserialize(cacheData);
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (!this.HasStateChanged) return;

            cacheData = this.Serialize();
            this.HasStateChanged = false;
        }
    }
}
