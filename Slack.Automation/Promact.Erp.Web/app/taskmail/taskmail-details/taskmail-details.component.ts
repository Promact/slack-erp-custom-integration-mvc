﻿import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute } from '@angular/router';
import { TaskService } from '../taskmail.service';
import { TaskMailDetailsModel } from '../taskmaildetails.model';
import { TaskMailModel } from '../taskmail.model';
import { TaskMailStatus } from '../../enums/TaskMailStatus';
import { DatePipe } from '@angular/common';
import { StringConstant } from '../../shared/stringConstant';
import { LoaderService } from '../../shared/loader.service';
 
@Component({
    moduleId: module.id,
    selector: 'date-pipe',
    templateUrl: "taskmail-details.html",
    providers: [StringConstant]
})
export class TaskMailDetailsComponent implements OnInit {
    taskMail: Array<TaskMailModel>;
    public isMaxDate: boolean;
    public isMinDate: boolean;
    public selectedDate: string;
    public maxDate: Date; // For Date Picker
    public minDate: Date; // For Date Picker
    public isHide: boolean;
    constructor(private route: ActivatedRoute, private router: Router, private taskService: TaskService, private stringConstant: StringConstant, private loader: LoaderService) {
    }
    ngOnInit() {
        this.getTaskMailDetails();
    }
    getTaskMailDetails() {
        this.loader.loader = true;
        this.route.params.subscribe(params => {
            if (params[this.stringConstant.userRole] === this.stringConstant.RoleAdmin) {
                this.isHide = false;
            }
            else {
                this.isHide = true;
            }
            this.isMaxDate = true;
            this.taskService.getTaskMailDetailsReport(params[this.stringConstant.paramsUserId], params[this.stringConstant.userRole], params[this.stringConstant.paramsUserName]).subscribe(taskMails => {
                for (let index = 0; index < taskMails.length; index++) {
                    let task = taskMails[index];
                    let taskDetails = task.TaskMails;
                    if (taskDetails !== null && taskDetails !== undefined) {
                        for (let taskIndex = 0; taskIndex < taskDetails.length; taskIndex++) {
                            let taskDetail = taskDetails[taskIndex];
                            taskDetail.Comment = this.replaceSpecialCharacter(taskDetail.Comment);
                            taskDetail.Description = this.replaceSpecialCharacter(taskDetail.Description);
                        }
                    }
                }
                this.taskMail = taskMails;
                let datePipeMinDate = new DatePipe(this.stringConstant.medium);
                this.minDate = new Date(this.taskMail[0].MinDate);
                if (datePipeMinDate.transform(this.taskMail[0].MinDate, this.stringConstant.dateDefaultFormat) === datePipeMinDate.transform(this.taskMail[0].CreatedOn, this.stringConstant.dateDefaultFormat)) {
                    this.isMinDate = true;
                }
                let datePipeMaxDate = new DatePipe(this.stringConstant.medium);
                this.maxDate = new Date(this.taskMail[0].MaxDate);
                this.taskMail.forEach(taskmails => {
                    let datePipe = new DatePipe(this.stringConstant.medium);
                    taskmails.CreatedOns = datePipe.transform(taskmails.CreatedOn, this.stringConstant.dateFormat);
                    taskmails.TaskMails.forEach(taskMail => {
                        taskMail.StatusName = TaskMailStatus[taskMail.Status];
                    });
                });
                this.loader.loader = false;
            });
        });

    }
    getTaskMailList() {
        this.router.navigate([this.stringConstant.taskList]);
    }
    getTaskMailPrevious(UserName: any, UserId: any, UserRole: any, CreatedOn: any) {
        this.loader.loader = true;
        this.selectedDate = this.stringConstant.empty;
        this.taskService.getTaskMailDetailsReportPreviousDate(UserName, UserId, UserRole, CreatedOn).subscribe(taskMails => {
            for (let index = 0; index < taskMails.length; index++) {
                let task = taskMails[index];
                let taskDetails = task.TaskMails;
                if (taskDetails !== null && taskDetails !== undefined) {
                    for (let taskIndex = 0; taskIndex < taskDetails.length; taskIndex++) {
                        let taskDetail = taskDetails[taskIndex];
                        taskDetail.Comment = this.replaceSpecialCharacter(taskDetail.Comment);
                        taskDetail.Description = this.replaceSpecialCharacter(taskDetail.Description);
                    }
                }
            }
            this.taskMail = taskMails;
            let datePipeMinDate = new DatePipe(this.stringConstant.medium);
            if (datePipeMinDate.transform(this.taskMail[0].MinDate, this.stringConstant.dateDefaultFormat) === datePipeMinDate.transform(this.taskMail[0].CreatedOn, this.stringConstant.dateDefaultFormat)) {
                this.isMinDate = true;
            }
            this.taskMail.forEach(taskmails => {
                let datePipe = new DatePipe(this.stringConstant.medium);
                taskmails.CreatedOns = datePipe.transform(taskmails.CreatedOn, this.stringConstant.dateFormat);
                taskmails.TaskMails.forEach(taskMail => {
                    taskMail.StatusName = TaskMailStatus[taskMail.Status];
                });

            });
            this.loader.loader = false;
        });
        this.isMaxDate = false;
    }
    getTaskMailNext(UserName: any, UserId: any, UserRole: any, CreatedOn: any) {
        this.loader.loader = true;
        this.selectedDate = this.stringConstant.empty;
        this.taskService.getTaskMailDetailsReportNextDate(UserName, UserId, UserRole, CreatedOn).subscribe(taskMails => {
            for (let index = 0; index < taskMails.length; index++) {
                let task = taskMails[index];
                let taskDetails = task.TaskMails;
                if (taskDetails !== null && taskDetails !== undefined) {
                    for (let taskIndex = 0; taskIndex < taskDetails.length; taskIndex++) {
                        let taskDetail = taskDetails[taskIndex];
                        taskDetail.Comment = this.replaceSpecialCharacter(taskDetail.Comment);
                        taskDetail.Description = this.replaceSpecialCharacter(taskDetail.Description);
                    }
                }
            }
            this.taskMail = taskMails;
            let datePipeMaxDate = new DatePipe(this.stringConstant.medium);
            if (datePipeMaxDate.transform(this.taskMail[0].MaxDate, this.stringConstant.dateDefaultFormat) === datePipeMaxDate.transform(this.taskMail[0].CreatedOn, this.stringConstant.dateDefaultFormat)) {
                this.isMaxDate = true;
            }
            this.isMinDate = false;
            this.taskMail.forEach(taskmails => {
                let datePipe = new DatePipe(this.stringConstant.medium);
                taskmails.CreatedOns = datePipe.transform(taskmails.CreatedOn, this.stringConstant.dateFormat);
                taskmails.TaskMails.forEach(taskMail => {
                    taskMail.StatusName = TaskMailStatus[taskMail.Status];

                });
            });
            this.loader.loader = false;
        });
    }
    getTaskMailForSelectedDate(UserName: any, UserId: any, UserRole: any, CreatedOn: any, SelectedDate: any) {
        this.loader.loader = true;
        let datePipeSelectedDate = new DatePipe(this.stringConstant.medium);
        let selectedDate = datePipeSelectedDate.transform(SelectedDate, this.stringConstant.dateDefaultFormat);
        this.taskService.getTaskMailDetailsReportSelectedDate(UserName, UserId, UserRole, CreatedOn, selectedDate).subscribe(taskMails => {
            for (let index = 0; index < taskMails.length; index++) {
                let task = taskMails[index];
                let taskDetails = task.TaskMails;
                if (taskDetails !== null && taskDetails !== undefined) {
                    for (let taskIndex = 0; taskIndex < taskDetails.length; taskIndex++) {
                        let taskDetail = taskDetails[taskIndex];
                        taskDetail.Comment = this.replaceSpecialCharacter(taskDetail.Comment);
                        taskDetail.Description = this.replaceSpecialCharacter(taskDetail.Description);
                    }
                }
            }
            this.taskMail = taskMails;
            this.isMaxDate = false;
            this.isMinDate = false;
            let datePipe = new DatePipe(this.stringConstant.medium);
            if (datePipe.transform(this.taskMail[0].MaxDate, this.stringConstant.dateFormat) === datePipe.transform(this.taskMail[0].CreatedOn, this.stringConstant.dateFormat)) {
                this.isMaxDate = true;
            }
            if (datePipe.transform(this.taskMail[0].MinDate, this.stringConstant.dateFormat) === datePipe.transform(this.taskMail[0].CreatedOn, this.stringConstant.dateFormat)) {
                this.isMinDate = true;
            }
            this.taskMail.forEach(taskmails => {
                taskmails.CreatedOns = datePipe.transform(taskmails.CreatedOn, this.stringConstant.dateFormat);
                taskmails.TaskMails.forEach(taskMail => {
                    taskMail.StatusName = TaskMailStatus[taskMail.Status];
                });
            });
            this.loader.loader = false;
        });
    }

    replaceSpecialCharacter(text: string) {
        return text.replace('&amp;', '&').replace('&lt;', '<').replace('&gt;', '>');
    }
}