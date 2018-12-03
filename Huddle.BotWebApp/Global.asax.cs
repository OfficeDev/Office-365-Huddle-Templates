/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Autofac;
using Huddle.BotWebApp.Infrastructure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Huddle.BotWebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RegisterBotModules();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private void RegisterBotModules()
        {
            // Use an in-memory store for bot data.
            // This registers a IBotDataStore singleton that will be used throughout the app.
            var store = new InMemoryDataStore();
            Conversation.UpdateContainer(builder =>
            {
                builder.Register(c => new CachingBotDataStore(store,
                         CachingBotDataStoreConsistencyPolicy
                         .ETagBasedConsistency))
                         .As<IBotDataStore<BotData>>()
                         .AsSelf()
                         .InstancePerLifetimeScope();
                builder.RegisterModule(new ReflectionSurrogateModule());
                builder.RegisterModule<GlobalMessageHandlersBotModule>();
            });
        }
    }
}
