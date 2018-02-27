/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;

namespace Huddle.BotWebApp.Models
{
    public class CreateIdeaModel
    {
        public string Title { get; set; }

        public string NextSteps { get; set; }

        public Metric Metric { get; set; }

        public TeamMember Owner { get; set; }

        public DateTime StartDate { get; set; }

        public Team Team { get; set; }
    }
}
