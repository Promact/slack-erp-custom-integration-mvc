﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Promact.Erp.DomainModel.ApplicationClass;

namespace Promact.Core.Repository.LeaveReportRepository
{
    public interface ILeaveReportRepository
    {
        /// <summary>
        /// Method that returns the list of employees with their leave status based on their roles
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="userName"></param>
        /// <returns>List of employees with leave status based on roles</returns> 
        Task<IEnumerable<LeaveReport>> LeaveReport(string accessToken,string userName);

        /// <summary>
        /// Method that returns the details of leave for an employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="accessToken"></param>
        /// <returns>Details of leave for an employee</returns>
        Task<IEnumerable<LeaveReportDetails>> LeaveReportDetails(string employeeId, string accessToken);
        
    }
}
