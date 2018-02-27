/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, AfterViewChecked, ViewChild, Output, EventEmitter } from '@angular/core';
import { FormControl, FormGroup, Validators, NgForm } from '@angular/forms';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { Router } from '@angular/router';
import { Issue } from '../shared/models/issue';
import { User } from '../shared/models/user';
import { IssueState } from '../shared/models/issueState';
import { Category } from '../shared/models/category';
import { Reason } from '../shared/models/reason';
import { AllowIssueClick } from '../shared/models/allowIssueClick';
import { IssueService } from '../services/issue.service';
import { ReasonService } from '../services/reason.service';
import { Constants } from '../shared/constants';
import { CommonUtil } from '../utils/commonUtil';
import { IssueListComponent } from '../issueList/issueList.component';

declare var fabric: any;
declare var jQuery: any;

@Component({
    templateUrl: './addIssue.component.html',
    selector: 'add-issue',
    styleUrls: ['./addIssue.component.css', '../shared/shared.css']
})

export class AddIssueComponent implements OnInit, AfterViewChecked {
    @Output() afterAddedIssue: EventEmitter<Issue> = new EventEmitter<Issue>();
    @Output() onClosed: EventEmitter<Issue> = new EventEmitter<Issue>();

    @ViewChild('issueForm') private issueForm: NgForm;

    selectedCategory = '';
    selectedUser = '';
    categories = new Array<Category>();
    toAddIssue = new Issue();
    users = new Array<User>();
    isSaving = false;
    teamId = '1';
    isShown = false;

    constructor(private issueService: IssueService, private reasonService: ReasonService, private router: Router) {
    }

    ngOnInit(): void {
    }

    open(): void {
        this.initTeamContext();
        jQuery("div.add-issue-dialog").find("li.ms-Dropdown-item").removeClass('is-selected');
        jQuery("div.add-issue-dialog").find('span.ms-Dropdown-title').html('');

        this.toAddIssue = new Issue();
        this.initCategories();
        this.initUsers();
        this.isShown = true;
    }

    close(): void {
        this.isShown = false;
        this.isSaving = false;
        this.onClosed.emit(this.toAddIssue);
    }

    initUsers() {
        this.issueService.getUsers(this.teamId)
            .subscribe(resp => {
                this.users = resp;
                if (this.users.length > 0) {
                    this.selectedUser = this.users[0].mail;
                }
                this.toAddIssue.owner = this.selectedUser;
            });
    }

    initCategories() {
        this.issueService.getCategories()
            .subscribe(resp => {
                this.categories = resp;
                if (this.categories.length > 0) {
                    this.selectedCategory = this.categories[0].name;
                }
                this.toAddIssue.category = this.getCategoryByName(this.selectedCategory);
            });
    }

    initTeamContext() {
        this.teamId = CommonUtil.getTeamId();
    }

    selectCategory(categoryName) {
        this.selectedCategory = categoryName;
    }

    selectUser(user) {
        this.selectedUser = user;
        this.toAddIssue.owner = this.selectedUser;
    }

    getCategoryByName(categoryName: string) {
        let filterResult = this.categories.filter(category => category.name == categoryName);
        if (filterResult.length > 0)
            return filterResult[0];
        return new Category(-1, '');
    }

    isSaveDisabled(): boolean {
        return !this.toAddIssue.name || !this.toAddIssue.metric || this.isSaving;
    }

    saveIssue(): void {
        this.isSaving = true;
        this.toAddIssue.category = this.getCategoryByName(this.selectedCategory);
        this.issueService.addIssue(this.toAddIssue, this.teamId)
            .subscribe(result => {
                if (result) {
                    this.toAddIssue = result;
                    this.close();
                }
            });
    }

    ngAfterViewChecked() {
    }
}
