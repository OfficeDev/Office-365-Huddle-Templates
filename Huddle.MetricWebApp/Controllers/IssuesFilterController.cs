/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.MetricWebApp.SharePoint;
using Huddle.MetricWebApp.Util;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Huddle.MetricWebApp.Controllers
{
    public class IssuesFilterController : BaseAPIController
    {
        [Route("api/issuesfilter/{state}/{teamId}")]
        public async Task<HttpResponseMessage> Get(int state, string teamId)
        {
            var issueList = (await IssuesService.GetItemsAsync(state, teamId)).ToList();
            await MetricsService.CalcMetricCount(issueList);
            var result = issueList.Select(issue => issue.ToJson()).ToArray();
            return ToJson(result);
        }
    }
}
