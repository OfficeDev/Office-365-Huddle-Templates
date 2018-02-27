/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.Common;
using Microsoft.SharePoint.Client;
using System.Linq;

namespace Huddle.MetricWebApp.SharePoint
{
    public class DataClearService
    {
        private ClientContext clientContext;

        public DataClearService(ClientContext clientContext)
        {
            this.clientContext = clientContext;
        }

        public void ClearListItems()
        {
            RemoveListItems(SPLists.ReasonValues.Title);
            RemoveListItems(SPLists.Reasons.Title);
            RemoveListItems(SPLists.MetricIdeas.Title);
            RemoveListItems(SPLists.MetricValuess.Title);
            RemoveListItems(SPLists.Issues.Title);
        }

        private void RemoveListItems(string listTitle)
        {
            var query = new CamlQuery();
            query.ViewXml = @"<View><Query></Query><ViewFields><FieldRef Name='ID'/></ViewFields></View>";
            var items = clientContext.GetItems(listTitle, query).ToArray();

            foreach (var item in items)
                item.DeleteObject();
            clientContext.ExecuteQuery();
        }
    }
}
