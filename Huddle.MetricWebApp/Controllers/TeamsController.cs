/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.MetricWebApp.Infrastructure;
using Huddle.MetricWebApp.Services;
using Huddle.MetricWebApp.Util;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Huddle.MetricWebApp.Controllers
{
    public class TeamsController : BaseAPIController
    {
        [HttpGet]
        public async Task<HttpResponseMessage> Get(string id)
        {
            var users = await new TeamsService(await AuthenticationHelper.GetGraphServiceClientAsync()).GetTeamMembersAsync(id);
            var result = users.Select(user => user.ToJson());
            return ToJson(result);
        }
    }
}
