﻿using Promact.Erp.DomainModel.ApplicationClass;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Promact.Core.Repository.ProjectUserCall
{
    public interface IProjectUserCallRepository
    {
        /// <summary>
        /// Method to call an api of oAuth server and get Employee detail by their slack userName
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>user Details</returns>
        Task<User> GetUserByUsername(string userName, string accessToken);

        /// <summary>
        /// Method to call an api of oAuth server and get List of TeamLeader's slack UserName from employee userName
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>teamLeader details</returns>
        Task<List<User>> GetTeamLeaderUserName(string userName, string accessToken);

        /// <summary>
        /// Method to call an api of oAuth server and get List of Management People's Slack UserName
        /// </summary>
        /// <returns>management details</returns>
        Task<List<User>> GetManagementUserName(string accessToken);


        /// <summary>
        /// Method to call an api from project oAuth server and get Project details of the given group - JJ
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>object of ProjectAc</returns>
        Task<ProjectAc> GetProjectDetails(string groupName, string accessToken);

        /// <summary>
        /// This method is used to fetch list of users/employees of the given group name. - JJ
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="accessToken"></param>
        /// <returns>list of object of User</returns>
        Task<List<User>> GetUsersByGroupName(string groupName, string accessToken);

        

        /// <summary>
        /// Method to call an api of oAuth server and get Casual leave allowed to user by user slackName
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="accessToken"></param>
        /// <returns>Number of casual leave allowed</returns>
        Task<LeaveAllowed> CasualLeave(string slackUserName, string accessToken);
        //Task<List<User>> GetUsersByGroupName(string groupName);

        /// <summary>
        /// Method to call an api from project oAuth server and get Employee detail by their Id
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="accessToken"></param>
        /// <returns>User Details</returns>
        Task<User> GetUserByEmployeeId(string employeeId, string accessToken);

        /// <summary>
        /// Method to call an api from project oAuth server and get logged in user details by their username
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="accessToken"></param>
        /// <returns>User Details</returns>
        Task<User> GetUserByUserName(string userName, string accessToken);

        /// <summary>
        /// Method to call an api from oauth server and get all the users including in a project using teamleader id
        /// </summary>
        /// <param name="teamLeaderId"></param>
        /// <param name="accessToken"></param>
        /// <returns>list of users in a project</returns>
        Task<List<User>> GetProjectUsersByTeamLeaderId(string teamLeaderId, string accessToken);

        /// <summary>
        /// Method to call an api from oAuth server and get whether user is admin or not
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="accessToken"></param>
        /// <returns>true or false</returns>
        Task<bool> UserIsAdmin(string userName, string accessToken);
    }
}
