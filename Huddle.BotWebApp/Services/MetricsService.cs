/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Microsoft.Graph;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Huddle.BotWebApp.Services
{
    public class MetricsService : GraphService
    {
        private ISiteRequestBuilder _site;

        public MetricsService(string token, string baseSPSiteUrl) :
            base(token)
        {
            var url = new Uri(baseSPSiteUrl);
            _site = _graphServiceClient.Sites.GetByPath(url.PathAndQuery.Trim('/'), url.Host);
        }

        public async Task<Metric[]> GetActiveMetricsAsync(string teamId)
        {
            var issues = await _site.Lists["Issues"].Items.Request()
                .Select("Id")
                .Filter($"fields/HuddleTeamId eq '{teamId}'")
                .GetAllAsync();
            if (issues.Length == 0) return new Metric[0];

            var fitler = string.Join(" or ", issues.Select(i => "fields/HuddleIssueLookupId eq " + i.Id));
            var metrics = await _site.Lists["Metrics"].Items.Request()
                .Expand("fields($select=Title)")
                .Filter(fitler)
                .GetAllAsync();

            return metrics.Select(i => new Metric
            {
                Id = int.Parse(i.Id),
                Name = i.Fields.AdditionalData["Title"] as string
            }
            ).ToArray();
        }
    }
}

