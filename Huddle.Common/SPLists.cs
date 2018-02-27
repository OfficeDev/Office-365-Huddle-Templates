/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

namespace Huddle.Common
{
    public class SPLists
    {
        public const string ID = "ID";
        public const string Title = "Title";
        public const string State = "HuddleState";
        public const string Created = "Created";
        public const string Issue = "Issue";
        public const string Reason = "Reason";
        public const string TargetGoal = "HuddleTargetGoal";
        public const string ValueType = "HuddleValueType";

        public static class Categories
        {
            public const string ListTitle = "Categories";

            public static class Columns
            {
                public const string ID = SPLists.ID;
                public const string Title = SPLists.Title;
            }
        }

        public static class Issues
        {
            public const string Title = "Issues";

            public static class Columns
            {
                public const string ID = SPLists.ID;
                public const string Title = SPLists.Title;
                public const string State = SPLists.State;
                public const string Category = "HuddleCategory";
                public const string TeamId = "HuddleTeamId";
                public const string Created = SPLists.Created;
                public const string Owner = "HuddleOwner";
            }

            public static class States
            {
                public const string Active = "1";
            }
        }

        public static class Metrics
        {
            public const string Title = "Metrics";
            public static class Columns
            {
                public const string ID = SPLists.ID;
                public const string Title = SPLists.Title;
                public const string State = SPLists.State;
                public const string Issue = "HuddleIssue";
                public const string TargetGoal = SPLists.TargetGoal;
                public const string ValueType = SPLists.ValueType;
                public const string Created = SPLists.Created;
            }
        }

        public static class Reasons
        {
            public const string Title = "Reasons";

            public static class Columns
            {
                public const string ID = SPLists.ID;
                public const string Title = SPLists.Title;
                public const string ReasonTracking = "HuddleReasonTracking";
                public const string TrackingFrequency = "HuddleTrackingFrequency";
                public const string State = SPLists.State;
                public const string Metric = "HuddleMetric";
                public const string ValueType = SPLists.ValueType;
                public const string Created = SPLists.Created;
            }
        }

        public static class MetricValuess
        {
            public const string Title = "Metric Values";

            public static class Columns
            {
                public const string ID = SPLists.ID;
                public const string Metric = "HuddleMetric";
                public const string Value = "HuddleValue";
                public const string Date = "HuddleDate";
            }
        }

        public static class ReasonValues
        {
            public const string Title = "Reason Values";

            public static class Columns
            {
                public const string ID = SPLists.ID;
                public const string Reason = "HuddleReason";
                public const string Value = "HuddleValue";
                public const string Date = "HuddleDate";
            }
        }

        public static class MetricIdeas
        {
            public const string Title = "Metric Ideas";

            public static class Columns
            {
                public const string ID = "ID";
                public const string Metric = "HuddleMetricNullable";
                public const string TaskId = "HuddleTaskId";
                public const string TaskURL = "HuddleTaskURL";
                public const string TaskName = "HuddleTaskName";
                public const string InputDate = "HuddleInputDate";
                public const string TaskStartDate = "HuddleTaskStartDate";
                public const string TaskStatus = "HuddleTaskStatus";
                public const string TaskCompletedDate = "HuddleTaskCompletedDate";
            }
        }
    }
}
