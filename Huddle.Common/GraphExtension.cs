/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Huddle.Common
{
    public static class GraphExtension
    {
        public static async Task<User[]> GetAllAsync(this IGroupMembersCollectionWithReferencesRequest request)
        {
            var collectionPage = await request.GetAsync();
            return await GetAllAsync(collectionPage)
                .ContinueWith(i => i.Result.OfType<User>().ToArray());
        }

        public static async Task<PlannerTask[]> GetAllAsync(this IPlannerPlanTasksCollectionRequest request)
        {
            var collectionPage = await request.GetAsync();
            return await GetAllAsync(collectionPage);
        }

        public static async Task<PlannerTask[]> GetAllAsync(this IPlannerBucketTasksCollectionRequest request)
        {
            var collectionPage = await request.GetAsync();
            return await GetAllAsync(collectionPage);
        }

        public static async Task<PlannerBucket[]> GetAllAsync(this IPlannerPlanBucketsCollectionRequest request)
        {
            var collectionPage = await request.GetAsync();
            return await GetAllAsync(collectionPage);
        }

        private static async Task<TItem[]> GetAllAsync<TItem>(ICollectionPage<TItem> collectionPage)
        {
            var list = new List<TItem>();

            dynamic page = collectionPage;
            while (true)
            {
                list.AddRange(page.CurrentPage);
                if (page.NextPageRequest == null) break;
                page = await page.NextPageRequest.GetAsync();
            }

            return list.ToArray();
        }
    }
}
