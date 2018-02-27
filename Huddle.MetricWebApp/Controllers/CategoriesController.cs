/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.MetricWebApp.SharePoint;
using Huddle.MetricWebApp.Util;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Huddle.MetricWebApp.Controllers
{
    public class CategoriesController : BaseAPIController
    {
        public async Task<HttpResponseMessage> Get()
        {
            var categories = await CategoriesService.GetItemsAsync();
            var result = categories.Select(category => category.ToJson());
            return ToJson(result);
        }
    }
}
