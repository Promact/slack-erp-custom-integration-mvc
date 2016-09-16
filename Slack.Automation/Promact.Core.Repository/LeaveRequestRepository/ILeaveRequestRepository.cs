﻿using Promact.Erp.DomainModel.ApplicationClass;
using Promact.Erp.DomainModel.Models;
using System.Collections.Generic;

namespace Promact.Core.Repository.LeaveRequestRepository
{
    public interface ILeaveRequestRepository
    {
        /// <summary>
        /// Method to apply Leave
        /// </summary>
        /// <param name="leave"></param>
        void ApplyLeave(LeaveRequest leave);

        /// <summary>
        /// Method to get All List of leave
        /// </summary>
        /// <returns>List of leave</returns>
        IEnumerable<LeaveRequest> LeaveList();

        /// <summary>
        /// Method used to cancel the leave request using its integer leaveId
        /// </summary>
        /// <param name="leaveId"></param>
        /// <returns>leave which has been cancelled</returns>
        LeaveRequest CancelLeave(int leaveId);

        /// <summary>
        /// Method to get leave list corresponding each user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>List of leave of a particular user</returns>
        IEnumerable<LeaveRequest> LeaveListByUserId(string userId);

        /// <summary>
        /// Method to get the last leave request status corresponding to each user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>latest leave details of a particular user</returns>
        LeaveRequest LeaveListStatusByUserId(string userId);

        /// <summary>
        /// Get a particular leave detail using leaveId
        /// </summary>
        /// <param name="leaveId"></param>
        /// <returns>leave</returns>
        LeaveRequest LeaveById(int leaveId);

        /// <summary>
        /// Method to update leave request
        /// </summary>
        /// <param name="leave"></param>
        void UpdateLeave(LeaveRequest leave);

        /// <summary>
        /// Method to get number of leave taken by a user
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns>number of leave taken</returns>
        LeaveAllowed NumberOfLeaveTaken(string employeeId);
    }
}
