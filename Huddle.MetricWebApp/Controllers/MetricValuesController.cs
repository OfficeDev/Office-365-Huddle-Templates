/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.MetricWebApp.Models;
using Huddle.MetricWebApp.SharePoint;
using Huddle.MetricWebApp.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Huddle.MetricWebApp.Controllers
{
    public class MetricValuesController : BaseAPIController
    {
        [Route("api/metricvalues/{metricIds}/{reasonIds}/{weekStartDay}")]
        public async Task<HttpResponseMessage> Get(string metricIds, string reasonIds, DateTime weekStartDay)
        {
            var metricValues = new List<MetricValue[]>();
            var metricIdList = metricIds.Split(new char[] { '-' }).ToList();
            metricValues = await MetricValuesService.GetItemsAsync(metricIdList, weekStartDay);

            var reasonValues = new List<ReasonValue[]>();
            var reasonIdList = reasonIds.Split(new char[] { '-' }).ToList();
            reasonValues = await ReasonValuesService.GetItemsAsync(reasonIdList, weekStartDay);

            var metricValsData = metricValues.Select(mv => mv.ToJson()).ToList();
            var reasonValsData = reasonValues.Select(rv => rv.ToJson()).ToList();

            metricValsData.AddRange(reasonValsData);
            return ToJson(metricValsData);
        }

        public async Task<HttpResponseMessage> Post(JObject objData)
        {
            dynamic jsonData = objData;

            JArray metricValuesArrays = jsonData.metricValues;
            var toAddMetricValues = metricValuesArrays.Select(mvArray =>
            {
                return mvArray.Select(mv => mv.ToObject<MetricValue>()).ToList();
            }).ToList();

            JArray reasonMetricArrays = jsonData.reasonValues;
            var toAddReasonValues = reasonMetricArrays.Select(rvArray =>
            {
                return rvArray.Select(rm => rm.ToObject<ReasonValue>()).ToList();
            }).ToList();

            toAddMetricValues.ForEach(metricValueList =>
            {
                metricValueList.ForEach(async metricValue =>
                {
                    if (metricValue.Value.HasValue)
                    {
                        if (metricValue.Id == 0)
                            await MetricValuesService.InsertItemAsync(metricValue);
                        else
                            await MetricValuesService.UpdateItemAsync(metricValue);
                    }
                });
            });
            
            toAddReasonValues.ForEach(reasonValueList =>
            {
                reasonValueList.ForEach(async reasonValue =>
                {
                    if (reasonValue.Value.HasValue)
                    {
                        if (reasonValue.Id == 0)
                            await ReasonValuesService.InsertItemAsync(reasonValue);
                        else
                            await ReasonValuesService.UpdateItemAsync(reasonValue);
                    }
                });
            });

            return ToJson(new
            {
                success = true,
                metricValues = toAddMetricValues.Select(metricList => metricList.Select(metricVal => metricVal.ToJson())),
                reasonValues = toAddReasonValues.Select(reasonList => reasonList.Select(reasonVal => reasonVal.ToJson()))
            });
        }
    }
}
