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
    public class MetricsController : BaseAPIController
    {
        public async Task<HttpResponseMessage> Get(int id)
        {
            var metricArray = await MetricsService.GetItemsAsync(id);
            var result = metricArray.Select(metric => metric.ToJson()).ToArray();
            return ToJson(result);
        }
    }
}
