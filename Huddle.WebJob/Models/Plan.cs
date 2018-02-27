/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;

namespace Huddle.WebJob.Models
{
    [Serializable]
    public class Plan
    {
        public string Id { get; set; }

        public string Title { get; set; }
    }
}
