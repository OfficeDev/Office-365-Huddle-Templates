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
using System.Web.Http;

namespace Huddle.MetricWebApp.Controllers
{
    public class IssuesController : BaseAPIController
    {
        public async Task<HttpResponseMessage> Get(int id)
        {
            var queryIssue = await IssuesService.GetItemAsync(id);
            return ToJson(queryIssue.ToJson());
        }
        
        public async Task<HttpResponseMessage> Post(JObject objData)
        {
            dynamic jsonData = objData;
            Issue toAddIssue = jsonData.issue.ToObject<Issue>();
            toAddIssue.State = 1;
            toAddIssue.MSTeamId = jsonData.teamId;
            var issue = await IssuesService.InsertItemAsync(toAddIssue);
            return ToJson(issue.ToJson());
        }
        
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpPost, Route("api/issues/editIssue")]
        public async Task<HttpResponseMessage> EditIssue(JObject objData)
        {
            dynamic jsonData = objData;
            JObject issue = jsonData.issue;
            var toEditIssue = issue.ToObject<Issue>();
            await IssuesService.UpdateItemAsync(toEditIssue);
            return ToJson(new
            {
                issueId = toEditIssue.Id
            });
        }

        [HttpDelete]
        [Route("api/issues/Delete/{id}")]
        public async Task<HttpResponseMessage> Delete(int id)
        {
            await IssuesService.DeleteIssueAndRelatedItemsAsync(id);
            return ToJson(new
            {
                issueId = id
            });
        }
    }
}
