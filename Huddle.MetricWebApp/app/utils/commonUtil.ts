/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Router } from '@angular/router';
import { WeekInputViewModel } from "../shared/models/weekInputViewModel";
import { Issue } from "../shared/models/issue";
import { Reason } from "../shared/models/reason";
import { Constants } from '../shared/constants';
declare var jQuery: any;

export class CommonUtil {

    private static readonly teamId: string = "teamId";

    public static isInMsTeam(): boolean {
        var isInIFrame = (window.location != window.parent.location) ? true : false;
        return isInIFrame;
    }

    public static getTeamId(): string {
        return this.getParam(this.teamId);
    }

    public static getParam(key: string): string {
        // This function is anonymous, is executed immediately and 
        // the return value is assigned to QueryString!
        var query_string = {};
        var query = window.location.search.substring(1);
        var vars = query.split("&");
        for (var i = 0; i < vars.length; i++) {
            var pair = vars[i].split("=");
            // If first entry with this name
            if (typeof query_string[pair[0]] === "undefined") {
                query_string[pair[0]] = decodeURIComponent(pair[1]);
                // If second entry with this name
            } else if (typeof query_string[pair[0]] === "string") {
                var arr = [query_string[pair[0]], decodeURIComponent(pair[1])];
                query_string[pair[0]] = arr;
                // If third or later entry with this name
            } else {
                query_string[pair[0]].push(decodeURIComponent(pair[1]));
            }
        }
        return query_string[key];
    }
    
    public static getDisplayValueType(valueType: string) {
        if (valueType === Constants.valueTypes.dollars.val)
            return Constants.valueTypes.dollars.text;
        else if (valueType === Constants.valueTypes.percentages.val)
            return Constants.valueTypes.percentages.text;
        return Constants.valueTypes.numbers.text;
    }
}
