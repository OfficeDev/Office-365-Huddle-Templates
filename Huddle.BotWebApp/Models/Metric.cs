/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Newtonsoft.Json;
using System;

namespace Huddle.BotWebApp.Models
{
    [Serializable]
    public class Metric
    {
        public static readonly Metric Other = new Metric { Name = "Other" };

        [JsonProperty("id")]
        public int? Id { get; set; }

        public string Name { get; set; }
    }
}
