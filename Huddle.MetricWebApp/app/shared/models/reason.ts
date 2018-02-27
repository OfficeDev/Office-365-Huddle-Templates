/*
* Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

import { Metric } from './metric';
import { State } from './state';
import { QueryResult } from './queryResult';

export class Reason implements QueryResult {
    id?: number;
    name?: string;
    metric?: Metric;
    reasonState?: State;
    startDate?: Date;
    isEditable?: boolean;
    reasonTracking?: string;
    valueType: string;
    trackingFrequency: string;
}