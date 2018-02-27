/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Web.Hosting;
using System.Web.Security;

namespace Huddle.BotWebApp.Infrastructure
{

    public class ADALTokenCache : TokenCache
    {
        private static ConcurrentDictionary<string, ADALTokenCache> Instances = new ConcurrentDictionary<string, ADALTokenCache>();

        public static ADALTokenCache Create(string signedInUserId)
        {
            return Instances.GetOrAdd(signedInUserId, id => new ADALTokenCache(id));
        }

        private string userId;
        private string filePath;
        private byte[] cacheData;
        private DateTime cacheDateTime = DateTime.MinValue;

        private ADALTokenCache(string signedInUserId)
        {
            this.userId = signedInUserId;
            this.filePath = HostingEnvironment.MapPath("~/App_Data/TokenCache/" + signedInUserId);
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
        }

        public override void Clear()
        {
            base.Clear();
            ClearCacheData();
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            if (cacheData == null || cacheDateTime < GetCacheFileLastWriteTime())
            {
                ReadCacheData();
                if (cacheData != null)
                    this.Deserialize(MachineKey.Unprotect(cacheData, "ADALCache"));
            }
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (!this.HasStateChanged) return;

            cacheData = MachineKey.Protect(this.Serialize(), "ADALCache");
            WriteCacheData();
            this.HasStateChanged = false;
        }

        #region Read and write cache data from file

        private void ReadCacheData()
        {
            try
            {
                cacheDateTime = File.GetLastWriteTimeUtc(filePath);
                cacheData = File.ReadAllBytes(filePath);
            }
            catch { }
        }

        private void WriteCacheData()
        {
            lock (filePath)
            {
                File.WriteAllBytes(filePath, cacheData);
                cacheDateTime = GetCacheFileLastWriteTime();
            }
        }

        private void ClearCacheData()
        {
            File.Delete(filePath);
            cacheDateTime = DateTime.MinValue;
        }

        private DateTime GetCacheFileLastWriteTime()
        {
            try
            {
                return File.GetLastWriteTimeUtc(filePath);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        #endregion
    }
}
