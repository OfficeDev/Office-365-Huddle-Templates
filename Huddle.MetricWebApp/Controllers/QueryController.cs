/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.MetricWebApp.Models;
using Huddle.MetricWebApp.SharePoint;
using Huddle.MetricWebApp.Util;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Huddle.MetricWebApp.Controllers
{
    public class QueryController : BaseAPIController
    {
        [Route("api/itemsquery/{state}/{key}/{teamId}")]
        public async Task<HttpResponseMessage> Get(int state, string teamId, string key)
        {
            var resultList = await QueryService.QueryItemsAsync(state, teamId, key);
            var dataResult = resultList.Select(item =>
            {
                var type = item.GetType().Name;
                if (type == "Issue") return (item as Issue).ToJson();
                if (type == "Metric") return (item as Metric).ToJson();
                return (item as Reason).ToJson();
            }).ToArray();
            return ToJson(dataResult);
        }
    }
}
