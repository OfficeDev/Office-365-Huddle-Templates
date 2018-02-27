/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.MetricWebApp.Models;
using Huddle.MetricWebApp.SharePoint;
using Huddle.MetricWebApp.Util;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Huddle.MetricWebApp.Controllers
{
    public class MetricController : BaseAPIController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> Post(JObject objData)
        {
            dynamic jsonData = objData;
            JObject metric = jsonData.metric;
            var toAddMetric = metric.ToObject<Metric>();
            var result = await MetricsService.InsertItemAsync(toAddMetric);
            return ToJson(result.ToJson());
        }

        [HttpGet, Route("api/metrics/GetMetricById/{id}")]
        public async Task<HttpResponseMessage> GetMetricById(int id)
        {
            var metric = await MetricsService.GetMetricById(id);
            return ToJson(metric.ToJson());

        }

        [HttpDelete]
        [Route("api/metric/Delete/{id}")]
        public async Task<HttpResponseMessage> Delete(int id)
        {
            await MetricsService.DeleteMetricAndRelatedItemsAsync(id);
            return ToJson(new
            {
                issueId = id
            });
        }
    }
}
