/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.MetricWebApp.Models;
using System;
using System.Linq;

namespace Huddle.MetricWebApp.Util
{
    public static class Extensions
    {
        public static object ToJson(this Category category)
        {
            return new
            {
                id = category.Id,
                name = category.Name
            };
        }

        public static object ToJson(this Microsoft.Graph.User user)
        {
            return new
            {
                id = user.Id,
                name = user.DisplayName,
                mail = user.Mail
            };
        }

        public static object ToJson(this Issue issue)
        {
            return new
            {
                id = issue.Id,
                category = (issue.Category != null ? new { id = issue.Category.Id, name = issue.Category.Name } : new { id = 0, name = string.Empty }),
                metric = issue.Metric,
                name = issue.Name,
                startDate = issue.StartDate,
                issueState = issue.State,
                owner = issue.Owner,
                activeMetricCount = issue.ActiveMetricCount
            };
        }

        public static object ToJson(this Metric metric)
        {
            return new
            {
                id = metric.Id,
                issue = metric.Issue != null ? metric.Issue.ToJson() : metric.Issue,
                name = metric.Name,
                targetGoal = metric.TargetGoal,
                valueType = metric.ValueType,
                metricState = metric.State,
                startDate = metric.StartDate,
            };
        }

        public static object ToJson(this Reason reason)
        {
            return new
            {
                id = reason.Id,
                name = reason.Name,
                metric = reason.Metric != null ? reason.Metric.ToJson() : reason.Metric,
                startDate = reason.StartDate,
                reasonState = reason.State,
                reasonTracking = reason.ReasonTracking,
                trackingFrequency = reason.TrackingFrequency,
                valueType = reason.ValueType
            };
        }

        public static object ToJson(this MetricValue metricValue)
        {
            return new
            {
                id = metricValue.Id,
                metric = metricValue.Metric.ToJson(),
                metricValues = metricValue.Value,
                inputDate = metricValue.InputDate
            };
        }

        public static object ToJson(this ReasonValue reasonValue)
        {
            return new
            {
                id = reasonValue.Id,
                reason = reasonValue.Reason.ToJson(),
                reasonMetricValues = reasonValue.Value,
                inputDate = reasonValue.InputDate
            };
        }

        public static object ToJson(this MetricValue[] issueMetrics)
        {
            return new
            {
                isMetricValue = true,
                metricValueArray = issueMetrics.Select(im => new
                {
                    id = im.Id,
                    metric = im.Metric.ToJson(),
                    metricValues = im.Value,
                    inputDate = im.InputDate
                })
            };
        }

        public static object ToJson(this ReasonValue[] reasonMetrics)
        {
            return new
            {
                isMetricValue = false,
                reasonValueArray = reasonMetrics.Select(im => new
                {
                    id = im.Id,
                    reason = im.Reason.ToJson(),
                    reasonMetricValues = im.Value,
                    inputDate = im.InputDate
                })
            };
        }

        public static TrackingFrequency? ToTrackingFrequency(this string trackingStr)
        {
            TrackingFrequency result;
            if (Enum.TryParse(trackingStr, out result))
                return result;
            return null;
        }
    }
}
