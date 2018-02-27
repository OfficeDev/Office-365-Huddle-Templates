/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { QueryResult } from './queryResult';

export class User implements QueryResult {
    id?: number;
    name?: string;
    mail?: string;
}
