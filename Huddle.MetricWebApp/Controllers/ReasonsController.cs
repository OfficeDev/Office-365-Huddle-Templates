/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.MetricWebApp.Models;
using Huddle.MetricWebApp.SharePoint;
using Huddle.MetricWebApp.Util;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Huddle.MetricWebApp.Controllers
{
    public class ReasonsController : BaseAPIController
    {
        [HttpGet]
        public async Task<HttpResponseMessage> Get(int id)
        {
            var reason = await ReasonsService.GetReasonById(id);
            return ToJson(reason.ToJson());
        }
        
        [Route("api/reasons")]
        [HttpPost]
        public async Task<HttpResponseMessage> Post(JObject objData)
        {
            dynamic jsonData = objData;
            JObject reason = jsonData.reason;
            var toAddReason = reason.ToObject<Reason>();
            await ReasonsService.InsertItemAsync(toAddReason);
            return ToJson(new
            {
                issueId = toAddReason.Id
            });
        }
        
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpGet, Route("api/reasons/reasonlist/{metricId}")]
        public async Task<HttpResponseMessage> GetReasonsByMetricList(int metricId)
        {
            var reasonList = await ReasonsService.GetReasonsByMetricIdAsync(metricId);
            var result = reasonList.Select(reason => reason.ToJson()).ToArray();
            return ToJson(result);
        }

        [HttpPost, Route("api/reasons/updatereasonstatus/{id}")]
        public async Task<HttpResponseMessage> UpdateReasonStatus(int id)
        {
            await ReasonsService.UpdateReasonStatus(id);
            return ToJson(new
            {
                issueId = id
            });
        }

        [HttpPost, Route("api/reasons/editReason")]
        public async Task<HttpResponseMessage> EditReason(JObject objData)
        {
            dynamic jsonData = objData;
            JObject reason = jsonData.reason;
            var toEditReason = reason.ToObject<Reason>();
            await ReasonsService.UpdateItemAsync(toEditReason);
            return ToJson(new
            {
                issueId = toEditReason.Id
            });
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete(int id)
        {
            await ReasonsService.DeleteReasonAndReasonValuesAsync(id);
            return ToJson(new
            {
                issueId = id
            });
        }
    }
}
