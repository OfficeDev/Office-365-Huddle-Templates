/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Observable } from "rxjs/Observable";

export class HandleError {

    public static handleError(error: Response) {
        alert(error.toString());
    }
}
