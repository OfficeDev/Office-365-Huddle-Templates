/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.Common;
using Huddle.MetricWebApp.Models;
using Huddle.MetricWebApp.Util;
using Microsoft.SharePoint.Client;
using System;

namespace Huddle.MetricWebApp.SharePoint
{
    public static class ClientContextExtensions
    {
        public static ListItemCollection GetItems(this ClientContext clientContext, string listTitle, CamlQuery query)
        {
            var web = clientContext.Site.RootWeb;
            var list = web.Lists.GetByTitle(listTitle);
            var items = list.GetItems(query);
            clientContext.Load(items);
            clientContext.ExecuteQuery();
            return items;
        }

        public static bool IgnoreCaseEquals(this string s, string other)
        {
            return StringComparer.InvariantCultureIgnoreCase.Equals(s, other);
        }

        public static System.DateTime UTCToLocalDateTime(this System.DateTime UTCTime, int timeZoneBias)
        {
            var time = UTCTime;
            if (UTCTime.Kind == DateTimeKind.Utc)
                time = new DateTime(UTCTime.Ticks);
            return time.AddMinutes(-1 * timeZoneBias);
        }

        public static string GetFieldValueStr(this ListItem item, string fieldName)
        {
            if (item[fieldName] == null) return string.Empty;
            return item[fieldName].ToString();
        }

        public static int GetFieldValueInt(this ListItem item, string fieldName)
        {
            if (item[fieldName] == null) return 0;
            return Convert.ToInt32(item[fieldName]);
        }

        public static FieldLookupValue GetFieldValueLookup(this ListItem item, string fieldName)
        {
            var result = new FieldLookupValue();
            if (item[fieldName] == null)
                result = new FieldLookupValue() { LookupId = 0 };
            else
                result = item[fieldName] as FieldLookupValue;
            return result;
        }

        public static string GetFieldValueUser(this ListItem item, string fieldName)
        {
            var result = string.Empty;
            if (item[fieldName] == null) return result;

            var userVal = item[fieldName] as FieldUserValue;
            if (userVal == null) return result;

            if (string.IsNullOrEmpty(userVal.LookupValue)) return userVal.Email ?? result;
            return userVal.LookupValue;
        }

        public static Category ToCategory(this ListItem item)
        {
            return new Category()
            {
                Id = item.GetFieldValueInt(SPLists.Categories.Columns.ID),
                Name = item.GetFieldValueStr(SPLists.Categories.Columns.Title),
            };
        }

        public static Issue ToIssue(this ListItem item)
        {
            var category = item.GetFieldValueLookup(SPLists.Issues.Columns.Category);
            return new Issue()
            {
                Id = item.GetFieldValueInt(SPLists.Issues.Columns.ID),
                Category = new Category() { Id = category.LookupId, Name = category.LookupValue },
                Name = item.GetFieldValueStr(SPLists.Issues.Columns.Title),
                StartDate = (DateTime)item[SPLists.Issues.Columns.Created],
                State = int.Parse(item[SPLists.Issues.Columns.State].ToString()),
                Owner = item.GetFieldValueUser(SPLists.Issues.Columns.Owner)
            };
        }

        public static Metric ToMetric(this ListItem item)
        {
            var metric = item.GetFieldValueLookup(SPLists.Metrics.Columns.Issue);
            return new Metric()
            {
                Id = item.GetFieldValueInt(SPLists.Metrics.Columns.ID),
                Issue = new Issue() { Id = metric.LookupId, Name = metric.LookupValue },
                Name = item.GetFieldValueStr(SPLists.Metrics.Columns.Title),
                TargetGoal = item.GetFieldValueStr(SPLists.Metrics.Columns.TargetGoal),
                ValueType = item.GetFieldValueStr(SPLists.Metrics.Columns.ValueType),
                State = int.Parse(item[SPLists.Metrics.Columns.State].ToString()),
                StartDate = (DateTime)item[SPLists.Metrics.Columns.Created]
            };
        }

        public static Reason ToReason(this ListItem item)
        {
            var metric = item.GetFieldValueLookup(SPLists.Reasons.Columns.Metric);
            return new Reason()
            {
                Id = item.GetFieldValueInt(SPLists.Reasons.Columns.ID),
                Metric = new Metric() { Id = metric.LookupId, Name = metric.LookupValue },
                Name = item.GetFieldValueStr(SPLists.Reasons.Columns.Title),
                StartDate = (DateTime)item[SPLists.Reasons.Columns.Created],
                State = int.Parse(item[SPLists.Reasons.Columns.State].ToString()),
                ValueType = item.GetFieldValueStr(SPLists.Reasons.Columns.ValueType),
                ReasonTracking = item.GetFieldValueStr(SPLists.Reasons.Columns.ReasonTracking),
                TrackingFrequency = item.GetFieldValueStr(SPLists.Reasons.Columns.TrackingFrequency).ToTrackingFrequency()
            };
        }

        public static MetricValue ToMetricValue(this ListItem item)
        {
            var metric = item[SPLists.MetricValuess.Columns.Metric] as FieldLookupValue;
            return new MetricValue()
            {
                Id = (int)item[SPLists.MetricValuess.Columns.ID],
                Metric = new Metric() { Id = metric.LookupId, Name = metric.LookupValue },
                InputDate = (DateTime)item[SPLists.MetricValuess.Columns.Date],
                Value = item.ToMetricValues(SPLists.MetricValuess.Columns.Value)
            };
        }

        public static ReasonValue ToReasonValue(this ListItem item)
        {
            var reason = item[SPLists.ReasonValues.Columns.Reason] as FieldLookupValue;
            return new ReasonValue()
            {
                Id = (int)item[SPLists.ReasonValues.Columns.ID],
                Reason = new Reason() { Id = reason.LookupId, Name = reason.LookupValue },
                InputDate = (DateTime)item[SPLists.ReasonValues.Columns.Date],
                Value = item.ToMetricValues(SPLists.ReasonValues.Columns.Value)
            };
        }

        private static double ToMetricValues(this ListItem item, string metricField)
        {
            double metricValue = Convert.ToDouble(item[metricField]);
            return metricValue;
        }
    }
}
