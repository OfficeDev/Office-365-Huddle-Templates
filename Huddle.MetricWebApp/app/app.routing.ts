/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { IssueListComponent } from './issueList/issueList.component';

const appRoutes: Routes = [
    { path: '', redirectTo: 'home', pathMatch: 'full' },
    { path: 'home', component: IssueListComponent },
    { path: 'issueList', component: IssueListComponent },
];

export const routing: ModuleWithProviders =
    RouterModule.forRoot(appRoutes);
