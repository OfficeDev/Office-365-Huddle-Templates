/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Injectable, Inject } from '@angular/core';
import { CookieService as NGCookieService, CookieOptions } from 'ngx-cookie';

@Injectable()
export class CookieService {

    constructor(private cookieService: NGCookieService) { }

    public put(key: string, value: string): void {
        this.cookieService.put(key, value, { expires: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000) });
    }

    public get(key: string): string {
        return this.cookieService.get(key);
    }
}
