/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Huddle.BotWebApp
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || items.Count() == 0;
        }

        public static bool IsNotNullAndEmpty<T>(this IEnumerable<T> items)
        {
            return items != null && items.Any();
        }

        public static bool In<T>(this T t, IEnumerable<T> c)
        {
            return c.Any(i => i.Equals(t));
        }

        public static bool NotIn<T>(this T t, IEnumerable<T> c)
        {
            return c.All(i => i.Equals(t));
        }

        public static bool IgnoreCaseEquals(this string s, string other)
        {
            return StringComparer.InvariantCultureIgnoreCase.Equals(s, other);
        }

        public static bool IgnoreCaseIn(this string s, IEnumerable<string> values)
        {
            return values.Any(i => i.Equals(s, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string Join(this IEnumerable<string> source, string separator)
        {
            return string.Join(separator, source);
        }
    }
}
