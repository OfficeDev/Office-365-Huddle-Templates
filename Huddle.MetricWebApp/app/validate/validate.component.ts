/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, Input } from '@angular/core';
import { AbstractControlDirective, AbstractControl } from '@angular/forms';
import { Constants } from '../shared/constants';

@Component({
    selector: 'show-errors',
    template: `
        <ul *ngIf="showErrors()" class="error-ul">
          <li style="color: red" *ngFor="let error of listOfErrors()">{{error}}</li>
        </ul>
      `,
})
export class ValidateComponent {

    private static readonly errorMessages = {
        'required': () => Constants.stringRequired,
        'pattern': () => Constants.numberRequired
    };

    @Input() private control: AbstractControlDirective | AbstractControl;

    showErrors(): boolean {
        return this.control &&
            this.control.errors &&
            (this.control.dirty || this.control.touched);
    }

    listOfErrors(): string[] {
        return Object.keys(this.control.errors)
            .map(field => this.getMessage(field, this.control.errors[field]));
    }

    private getMessage(type: string, params: any) {
        return ValidateComponent.errorMessages[type](params);
    }
}
