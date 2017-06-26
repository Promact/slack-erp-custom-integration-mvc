﻿
import { Component, OnInit } from '@angular/core';
import { LoaderService } from './shared/loader.service';
import { AppComponentService } from './appcomponent.service';
import { EmailHashCode } from './shared/emailHashCode';

@Component({
    moduleId: module.id,
    selector: 'my-app',
    templateUrl: 'index.html'

})

export class AppComponent implements OnInit {
    userIsAdmin: boolean;
    hashCode: string;
    username: string;
    constructor(public loader: LoaderService, private httpService: AppComponentService, private emailHashCode: EmailHashCode) {
        this.userIsAdmin = false;
    }

    ngOnInit() {
        this.loader.loader = true;
        this.hashCode = this.emailHashCode.hashCode;
        this.httpService.getUserIsAdminOrNot().subscribe((result) => {
            this.userIsAdmin = result.IsAdmin;
            this.username = result.FirstName;
            this.loader.loader = false;
        });
    }
}