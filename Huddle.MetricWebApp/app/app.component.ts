/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, AfterViewChecked } from "@angular/core"

@Component({
    selector: "app-root",
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})

export class AppComponent implements OnInit {
    isIssueListVisible: Boolean = true;

    ngOnInit() {
        this.hideIssueListForTeamTab();
    }

    hideIssueListForTeamTab() {
        if (location.pathname.indexOf('tab') >= 0)
            this.isIssueListVisible = false;
    }

    componentActivated(component: Component) {
    }

    ngAfterViewChecked(): void {
    }

    componentDeactived(component: Component) {
    }
}
