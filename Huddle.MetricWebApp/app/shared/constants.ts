/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

export class Constants {

    static readonly suggestCharNum = 3;

    static teamId: string = ""; //for dev
    static deleteConfirmMessage: string = "Are you sure you want to delete this item?";
    static issueListName = "issues";
    static metricListName = "metrics";
    static reasonListName = "reasons";
    static numberRequired: string = 'This field must be a valid number';
    static stringRequired: string = 'This field is required';

    static valueTypes = class {
        static dollars = {
            text: '$',
            val: "Dollars"
        };
        static numbers = {
            text: '',
            val: "Numbers"
        };
        static percentages = {
            text: '%',
            val: "Percentages"
        }
    }

    static route = class {
        public static readonly addIssue = 'add-issue';
        public static readonly metricIssue = 'metric-issue';
    }

    static issueState = class {
        public static readonly unselectedIssueState = -1;
        public static readonly defaultIssueState = 1;
    }

    static webAPI = class {
        public static readonly userUrl = '/api/user';
        public static readonly teamsUrl = '/api/teams';
        public static readonly issuesUrl = '/api/issues';
        public static readonly issueEditUrl = '/api/issues/editIssue';
        public static readonly issueDeleteUrl = '/api/issues/delete';
        public static readonly categoriesUrl = '/api/categories';
        public static readonly reasonsUrl = '/api/reasons';
        public static readonly reasonStatusUrl = '/api/reasons/updatereasonstatus';
        public static readonly reasonsListUrl = 'api/reasons/reasonlist';
        public static readonly reasonEditUrl = 'api/reasons/editReason';
        public static readonly metricValuesUrl = '/api/metricvalues';
        public static readonly issuesFilterUrl = '/api/issuesFilter';
        public static readonly queryUrl = '/api/itemsquery';
        public static readonly metricsUrl = '/api/metrics';
        public static readonly metricUrl = '/api/metric';
        public static readonly metricEditUrl = '/api/metricedit';
        public static readonly categoryUrl = '/api/category';
    }
}
