/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.SharePoint.Client;

namespace Huddle.BotWebApp.SharePoint
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
    }
}
