/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.MetricWebApp.Models;
using Huddle.MetricWebApp.SharePoint;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Huddle.MetricWebApp.Controllers
{
    public class MetricEditController : BaseAPIController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> EditMetric(JObject objData)
        {
            dynamic jsonData = objData;
            JObject metric = jsonData.metric;
            var toEditMetric = metric.ToObject<Metric>();
            await MetricsService.UpdateItemAsync(toEditMetric);
            return ToJson(new
            {
                issueId = toEditMetric.Id
            });
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Post(int id)
        {
            await MetricsService.UpdateMetricStatus(id);
            return ToJson(new
            {
                issueId = id
            });
        }
    }
}
