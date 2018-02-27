/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;

namespace Huddle.MetricWebApp.Models
{
    public class Issue : IQueryResult
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Category Category { get; set; }

        public int Metric { get; set; }

        public int State { get; set; }

        public DateTime StartDate { get; set; }

        public string MSTeamId { get; set; }

        public string Owner { get; set; }

        public int ActiveMetricCount { get; set; }
    }
}
