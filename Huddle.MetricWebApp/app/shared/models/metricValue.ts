/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Metric } from './metric';

export class MetricValue {
    id?: number;
    metric?: Metric;
    metricValues?: number;
    inputDate?: Date;
    isUpdated?: boolean = false;
}
