/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;

namespace Huddle.MetricWebApp.Models
{
    public class ReasonValue
    {
        public int Id { get; set; }

        public Reason Reason { get; set; }

        public double? Value { get; set; }

        public DateTime InputDate { get; set; }
    }
}
