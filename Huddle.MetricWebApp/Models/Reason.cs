/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;

namespace Huddle.MetricWebApp.Models
{
    public enum TrackingFrequency
    {
        Daily,
        Weekly
    }

    public class Reason : IQueryResult
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Metric Metric { get; set; }

        public DateTime StartDate { get; set; }

        public string ReasonTracking { get; set; }

        public TrackingFrequency? TrackingFrequency { get; set; }

        public string ValueType { get; set; }

        public int State { get; set; }
    }
}
