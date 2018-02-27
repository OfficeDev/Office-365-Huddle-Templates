/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Reason } from './reason';

export class ReasonValue {
    id?: number;
    reason?: Reason;
    reasonMetricValues?: number;
    inputDate?: Date;
    isUpdated?: boolean = false;
}
