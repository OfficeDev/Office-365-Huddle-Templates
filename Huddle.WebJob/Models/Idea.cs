/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;
using Newtonsoft.Json;

namespace Huddle.WebJob.Models
{
    [Serializable]
    public class Idea
    {
        public string Id { get; set; }

        [JsonProperty("@odata.etag")]
        public string Etag { get; set; }

        public string BucketId { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string BucketName { get; set; }

        public DateTimeOffset? CreatedDateTime { get; set; }

        public DateTimeOffset? StartDateTime { get; set; }

        public static string GetIdeaUrl(string groupId, string planId, string taskId)
        {
            return $"https://tasks.office.com/{Constants.AADTenantId}/EN-US/Home/Planner#/plantaskboard?groupId={groupId}&planId={planId}&taskId={taskId}";
        }
    }
}
