/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Category } from './category';
import { State } from './state';
import { IssueState } from './issueState';
import { QueryResult } from './queryResult';

export class Issue implements QueryResult {
    id?: number;
    name?: string;
    category?: Category;
    metric?: string;
    targetGoal?: string;
    owner?: string;
    issueState?: IssueState;
    startDate?: Date;
    isMetricEditable?: boolean;
    isNameEditable?: boolean;
    activeMetricCount?: number;
}
