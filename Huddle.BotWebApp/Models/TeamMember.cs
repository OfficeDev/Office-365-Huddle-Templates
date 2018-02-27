/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;

namespace Huddle.BotWebApp.Models
{
    [Serializable]
    public class TeamMember
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }
    }
}
