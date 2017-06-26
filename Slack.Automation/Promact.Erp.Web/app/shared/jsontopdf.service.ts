﻿import { Injectable, OnInit } from '@angular/core';
import { StringConstant } from './stringConstant';
declare let jsPDF: any;

@Injectable()
export class JsonToPdfService {
    constructor(private stringConstant: StringConstant) { }
    jsPDF: any;
    exportJsonToPdf(columns: any, rows: any) {
        let doc = new jsPDF(this.stringConstant.portrait, this.stringConstant.unit, this.stringConstant.format);
        doc.autoTable(columns, rows, {
            styles: {
                theme: this.stringConstant.theme,
                overflow: this.stringConstant.overflow,
                pageBreak: this.stringConstant.pageBreak,
                tableWidth: this.stringConstant.tableWidth,
            },
        });
        doc.save(this.stringConstant.save);
    }
}