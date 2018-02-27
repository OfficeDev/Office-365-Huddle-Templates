/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.Common;
using Huddle.MetricWebApp.Infrastructure;
using Huddle.MetricWebApp.Models;
using Microsoft.SharePoint.Client;
using System.Linq;
using System.Threading.Tasks;

namespace Huddle.MetricWebApp.SharePoint
{
    public class CategoriesService
    {
        public static async Task<Category[]> GetItemsAsync()
        {
            var query = new CamlQuery();
            query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", string.Empty);
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var items = clientContext.GetItems(SPLists.Categories.ListTitle, query);
                var categories = items.Select(item => item.ToCategory())
                     .OrderBy(item => item.Id)
                     .ToArray();
                return categories;
            }
        }
    }
}
