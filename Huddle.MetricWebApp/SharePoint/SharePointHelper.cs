/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

 using System;

namespace Huddle.MetricWebApp.SharePoint
{
    public static class SharePointHelper
    {
        private const string LookupConnectStr = ";#";
        private const string ISO8601DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

        public static string BuildSingleLookFieldValue(int id, string value)
        {
            return id + LookupConnectStr + value;
        }
        
        public static string ToISO8601DateTimeString(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToUniversalTime().ToString(ISO8601DateTimeFormat);
        }

        public static string ToISO8601DateTimeString(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString(ISO8601DateTimeFormat);
        }
    }
}
