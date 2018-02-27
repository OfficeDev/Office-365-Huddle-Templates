/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Injectable, Inject } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs/Rx';
import { DataService } from '../services/data.service';
import { Category } from '../shared/models/category';
import { Constants } from '../shared/constants';
import { ModelConverter } from '../utils/modelConverter';
import { DateHelper } from '../utils/dateHelper';
import { HandleError } from '../shared/handleError';

@Injectable()
export class CategoryService {

    constructor(private dataService: DataService) { }

    public getCategories(): Observable<Category[]> {
        let activeObject: ReplaySubject<Category[]> = new ReplaySubject(1);
        this.dataService.getArray<Category>(Constants.webAPI.categoryUrl)
            .subscribe((category) => {
                let result: Category[] = [];
                category.forEach(function (category, index) {
                    result.push(category);
                }, this);
                activeObject.next(category);
            },
            (error) => {
                HandleError.handleError(error);
            });
        return activeObject;
    }
}
