/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Issue } from '../shared/models/issue';

export class IssueViewModel {
    Issue?: Issue;
    IsSelected?: Boolean;
    IssueState?: string;
    Expanded?: Boolean;
}
