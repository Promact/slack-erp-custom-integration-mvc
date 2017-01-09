﻿using Newtonsoft.Json;
using Promact.Core.Repository.AttachmentRepository;
using Promact.Erp.DomainModel.ApplicationClass;
using Promact.Erp.Util.HttpClient;
using Promact.Erp.Util.StringConstants;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Promact.Core.Repository.OauthCallsRepository
{
    public class OauthCallsRepository : IOauthCallsRepository
    {
        #region Private Variables
        private readonly IHttpClientService _httpClientService;
        private readonly IStringConstantRepository _stringConstant;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly HttpContextBase _httpContextBase;
        #endregion

        #region Constructor
        public OauthCallsRepository(IHttpClientService httpClientService, IStringConstantRepository stringConstant, 
            IAttachmentRepository attachmentRepository, HttpContextBase httpContextBase)
        {
            _httpClientService = httpClientService;
            _stringConstant = stringConstant;
            _attachmentRepository = attachmentRepository;
            _httpContextBase = httpContextBase;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Method to call an api from project oAuth server and get Employee detail by their slack userId
        /// </summary>
        /// <param name="slackUserId"></param>
        /// <param name="accessToken"></param>
        /// <returns>user Details</returns>
        public async Task<User> GetUserByUserIdAsync(string slackUserId, string accessToken)
        {
            User userDetails = new User();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.UserDetailsUrl, slackUserId);
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUserUrl, requestUrl, accessToken);
            if (response != null)
            {
                userDetails = JsonConvert.DeserializeObject<User>(response);
            }
            return userDetails;
        }

        /// <summary>
        /// Method to call an api from project oAuth server and get List of TeamLeader's slack UserName from employee userName
        /// </summary>
        /// <param name="slackUserId"></param>
        /// <param name="accessToken"></param>
        /// <returns>teamLeader details</returns>
        public async Task<List<User>> GetTeamLeaderUserIdAsync(string slackUserId, string accessToken)
        {
            List<User> teamLeader = new List<User>();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.TeamLeaderDetailsUrl, slackUserId);
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUserUrl, requestUrl, accessToken);
            if (response != null)
            {
                teamLeader = JsonConvert.DeserializeObject<List<User>>(response);
            }
            return teamLeader;
        }

        /// <summary>
        /// Method to call an api from project oAuth server and get List of Management People's Slack UserName
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns>management details</returns>
        public async Task<List<User>> GetManagementUserNameAsync(string accessToken)
        {
            List<User> management = new List<User>();
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUserUrl, _stringConstant.ManagementDetailsUrl, accessToken);
            if (response != null)
            {
                management = JsonConvert.DeserializeObject<List<User>>(response);
            }
            return management;
        }


        /// <summary>
        /// Method to call an api from project oAuth server and get Project details of the given group 
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="accessToken"></param>
        /// <returns>object of ProjectAc</returns>
        public async Task<ProjectAc> GetProjectDetailsAsync(string groupName, string accessToken)
        {
            var requestUrl = groupName;
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUrl, requestUrl, accessToken);
            ProjectAc project = new ProjectAc();
            if (response != null)
            {
                project = JsonConvert.DeserializeObject<ProjectAc>(response);
            }
            return project;
        }


        /// <summary>
        /// This method is used to fetch list of users/employees of the given group name
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="accessToken"></param>
        /// <returns>list of object of User</returns>
        public async Task<List<User>> GetUsersByGroupNameAsync(string groupName, string accessToken)
        {
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.UsersDetailByGroupUrl, groupName);
            var response = await _httpClientService.GetAsync(_stringConstant.UserUrl, requestUrl, accessToken);
            List<User> users = new List<User>();
            if (response != null)
            {
                users = JsonConvert.DeserializeObject<List<User>>(response);
            }
            return users;
        }

        /// <summary>
        /// Method to call an api from project oAuth server and get Employee detail by their Id
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="accessToken"></param>
        /// <returns>User Details</returns>
        public async Task<User> GetUserByEmployeeIdAsync(string employeeId, string accessToken)
        {
            User userDetails = new User();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, employeeId, _stringConstant.UserDetailUrl);
            var response = await _httpClientService.GetAsync(_stringConstant.UserUrl, requestUrl, accessToken);
            if (response != null)
            {
                userDetails = JsonConvert.DeserializeObject<User>(response);
            }
            return userDetails;
        }

        /// <summary>
        /// Method to call an api of oAuth server and get Casual leave allowed to user by user slackName
        /// </summary>
        /// <param name="slackUserId"></param>
        /// <param name="accessToken"></param>
        /// <returns>Number of casual leave allowed</returns>
        public async Task<LeaveAllowed> CasualLeaveAsync(string slackUserId, string accessToken)
        {
            LeaveAllowed casualLeave = new LeaveAllowed();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.CasualLeaveUrl, slackUserId);
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUserUrl, requestUrl, accessToken);
            if (response != null)
            {
                casualLeave = JsonConvert.DeserializeObject<LeaveAllowed>(response);
            }
            return casualLeave;
        }

        /// <summary>
        /// Method to call an api from oauth server and get all the projects under a specific teamleader id along with users in it
        /// </summary>
        /// <param name="teamLeaderId"></param>
        /// <param name="accessToken"></param>
        /// <returns>list of users in a project</returns>
        public async Task<List<User>> GetProjectUsersByTeamLeaderIdAsync(string teamLeaderId, string accessToken)
        {
            List<User> projectUsers = new List<User>();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, teamLeaderId, _stringConstant.ProjectUsersByTeamLeaderId);
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUrl, requestUrl, accessToken);
            if (response != null)
            {
                projectUsers = JsonConvert.DeserializeObject<List<User>>(response);
            }
            return projectUsers;
        }

        /// <summary>
        /// Method to call an api from oAuth server and get whether user is admin or not
        /// </summary>
        /// <param name="slackUserId"></param>
        /// <param name="accessToken"></param>
        /// <returns>true or false</returns>
        public async Task<bool> UserIsAdminAsync(string slackUserId, string accessToken)
        {
            bool result = false;
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.UserIsAdmin, slackUserId);
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUserUrl, requestUrl, accessToken);
            if (response != null)
            {
                result = JsonConvert.DeserializeObject<bool>(response);
            }
            return result;
        }

        /// <summary>
        /// Used to get user role
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <returns>user details</returns>
        public async Task<List<UserRoleAc>> GetUserRoleAsync(string userId, string accessToken)
        {
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, userId, _stringConstant.UserRoleUrl);
            var response = await _httpClientService.GetAsync(_stringConstant.UserUrl, requestUrl, accessToken);
            var userRoleListAc = JsonConvert.DeserializeObject<List<UserRoleAc>>(response);
            return userRoleListAc;
        }

        /// <summary>
        /// List of employee under this employee
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <returns>List of user</returns>
        public async Task<List<UserRoleAc>> GetListOfEmployeeAsync(string userId, string accessToken)
        {
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, userId,_stringConstant.TeamMembersUrl);
            var response = await _httpClientService.GetAsync(_stringConstant.UserUrl, requestUrl, accessToken);
            var userRoleListAc = JsonConvert.DeserializeObject<List<UserRoleAc>>(response);
            return userRoleListAc;
        }



        /// <summary>
        /// Method is used to call an api from oauth server and return list of all the projects
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns>list of all the projects</returns>
        public async Task<List<ProjectAc>> GetAllProjectsAsync(string accessToken)
        {
            List<ProjectAc> projects = new List<ProjectAc>();
            var requestUrl = _stringConstant.AllProjectUrl;
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUrl, requestUrl, accessToken);
            if (response != null)
            {
                projects = JsonConvert.DeserializeObject<List<ProjectAc>>(response);
            }
            return projects;
        }

        /// <summary>
        /// Method to call an api from oauth server and get the details of a project using projecId
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="accessToken"></param>
        /// <returns>Details of a project</returns>
        public async Task<ProjectAc> GetProjectDetailsAsync(int projectId)
        {
            var accessToken = await AccessTokenOfRequestedUser();
            ProjectAc project = new ProjectAc();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, projectId,_stringConstant.GetProjectDetails);
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUrl, requestUrl, accessToken);
            if(response != null)
            {
                project = JsonConvert.DeserializeObject<ProjectAc>(response);
            }
            return project;
        }

        private async Task<string> AccessTokenOfRequestedUser()
        {
            var user = _httpContextBase.User.Identity.Name;
            return await _attachmentRepository.UserAccessTokenAsync(user);
        }
        #endregion
    }
}