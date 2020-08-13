/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

namespace Huddle.BotWebApp.Services
{
    public static class IdeasPlan
    {
        public static class Buckets
        {
            public static readonly string NewIdea = "New Idea";
            public static readonly string Shareable = "Shareable";
            public static readonly string InProgress = "In Progress";
            public static readonly string Completed = "Completed";
            public static readonly string[] All = { NewIdea, InProgress, Completed, Shareable };
        }
    }
}
