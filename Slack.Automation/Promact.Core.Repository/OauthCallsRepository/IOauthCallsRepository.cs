﻿using Promact.Erp.DomainModel.ApplicationClass;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Promact.Core.Repository.OauthCallsRepository
{
    public interface IOauthCallsRepository
    {
        /// <summary>
        /// Method to call an api of oAuth server and get Employee detail by their slack userId
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="accessToken"></param>
        /// <returns>user Details</returns>
        Task<User> GetUserByUserIdAsync(string userName, string accessToken);

        /// <summary>
        /// Method to call an api of oAuth server and get List of TeamLeader's slack UserName from employee userName
        /// </summary>
        /// <param name="slackUserId"></param>
        /// <param name="accessToken"></param>
        /// <returns>teamLeader details</returns>
        Task<List<User>> GetTeamLeaderUserIdAsync(string slackUserId, string accessToken);

        /// <summary>
        /// Method to call an api of oAuth server and get List of Management People's Slack UserName
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns>management details</returns>
        Task<List<User>> GetManagementUserNameAsync(string accessToken);

        /// <summary>
        /// Used to get user role
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <returns>user details</returns>
        Task<List<UserRoleAc>> GetUserRoleAsync(string userId, string accessToken);

        /// <summary>
        /// Method to call an api from project oAuth server and get Project details of the given group 
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="accessToken"></param>
        /// <returns>object of ProjectAc</returns>
        Task<ProjectAc> GetProjectDetailsAsync(string groupName, string accessToken);

        /// <summary>
        /// List of employee under this employee
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <returns>List of user</returns>
        Task<List<UserRoleAc>> GetListOfEmployeeAsync(string userId, string accessToken);

        /// <summary>
        /// This method is used to fetch list of users/employees of the given group name
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="accessToken"></param>
        /// <returns>list of object of User</returns>
        Task<List<User>> GetUsersByGroupNameAsync(string groupName, string accessToken);


        /// <summary>
        /// Method to call an api of oAuth server and get Casual leave allowed to user by user slackName
        /// </summary>
        /// <param name="slackUserId"></param>
        /// <param name="accessToken"></param>
        /// <returns>Number of casual leave allowed</returns>
        Task<LeaveAllowed> CasualLeaveAsync(string slackUserId, string accessToken);

        /// <summary>
        /// Method to call an api from project oAuth server and get employee detail by their Id
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="accessToken"></param>
        /// <returns>User Details</returns>
        Task<User> GetUserByEmployeeIdAsync(string employeeId, string accessToken);

        /// <summary>
        /// Method to call an api from oauth server and get all the projects under a specific teamleader id along with users in it
        /// </summary>
        /// <param name="teamLeaderId"></param>
        /// <returns>list of users in a project</returns>
        Task<List<User>> GetProjectUsersByTeamLeaderIdAsync(string teamLeaderId);

        /// <summary>
        /// Method to call an api from oAuth server and get whether user is admin or not
        /// </summary>
        /// <param name="slackUserId"></param>
        /// <param name="accessToken"></param>
        /// <returns>true or false</returns>
        Task<bool> UserIsAdminAsync(string slackUserId, string accessToken);

        /// <summary>
        /// Method to call an api from oauth server and get the list of all the projects
        /// </summary>
        /// <returns>list of all the projects</returns>
        Task<List<ProjectAc>> GetAllProjectsAsync();

        /// <summary>
        /// Method to call an api from oauth server and get the details of a project using projecId
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>Details of a project</returns>
        Task<ProjectAc> GetProjectDetailsAsync(int projectId);
    }
}