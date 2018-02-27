/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System.Collections.Generic;

namespace Huddle.MetricWebApp.Models
{
    public interface IQueryResult { }

    public class QueryData<T> where T : IQueryResult
    {
        public List<T> items;
    }
}
