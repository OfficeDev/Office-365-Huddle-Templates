/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { NgModule } from '@angular/core';
import { APP_BASE_HREF } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { AppComponent } from './app.component';
import { Ng2Bs3ModalModule } from 'ng2-bs3-modal/ng2-bs3-modal';
import { CookieModule } from 'ngx-cookie';
import { HttpModule } from '@angular/http';
import { routing } from './app.routing';
import { NguiAutoCompleteModule } from '@ngui/auto-complete';
import { UiSwitchModule } from 'ngx-ui-switch';
import { IssueListComponent } from './issueList/issueList.component';
import { HeaderComponent } from './header/header.component';
import { AddIssueComponent } from './issue/addIssue.component';
import { EditIssueComponent } from './issue/editIssue.component';
import { AddMetricComponent } from './metric/addMetric.component';
import { EditMetricComponent } from './metric/editMetric.component';
import { WeekSelectorComponent } from './issue/weekSelector.component';
import { WeekInputComponent } from './issue/weekInput.component';
import { MetricListComponent } from './metric/metricList.component';
import { ReasonListComponent } from './reason/reasonList.component';
import { NewReasonComponent } from './reason/newReason.component';
import { EditReasonComponent } from './reason/editReason.component';
import { CookieService } from "./services/cookie.service";
import { DataService } from "./services/data.service";
import { IssueService } from "./services/issue.service";
import { ReasonService } from "./services/reason.service";
import { MetricValueService } from "./services/metricValue.service";
import { QueryService } from "./services/query.service";
import { MetricService } from './services/metric.service';
import { WeekSelectorService } from './services/weekSelector.service';
import { ValidateComponent } from './validate/validate.component';
import { ConfirmComponent } from './confirm/confirm.component';
import { CommonConfirmComponent } from './confirm/common.confirm.component';

@NgModule({
    imports: [
        BrowserModule,
        ReactiveFormsModule,
        FormsModule,
        HttpModule,
        routing,
        Ng2Bs3ModalModule,
        NguiAutoCompleteModule,
        UiSwitchModule,
        CookieModule.forRoot()],
    declarations: [
        AppComponent,
        IssueListComponent,
        HeaderComponent,
        AddIssueComponent,
        EditIssueComponent,
        WeekInputComponent,
        WeekSelectorComponent,
        ReasonListComponent,
        NewReasonComponent,
        EditReasonComponent,
        MetricListComponent,
        AddMetricComponent,
        EditMetricComponent,
        ValidateComponent,
        ConfirmComponent,
        CommonConfirmComponent
    ],
    providers: [
        { provide: APP_BASE_HREF, useValue: '/' },
        CookieService,
        DataService,
        IssueService,
        ReasonService,
        MetricValueService,
        QueryService,
        MetricService,
        WeekSelectorService
    ],
    bootstrap: [AppComponent]

})
export class AppModule { } 
