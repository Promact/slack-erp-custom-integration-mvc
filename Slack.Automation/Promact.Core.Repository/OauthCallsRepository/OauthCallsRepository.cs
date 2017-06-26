﻿using Newtonsoft.Json;
using Promact.Erp.DomainModel.ApplicationClass;
using Promact.Erp.Util.HttpClient;
using Promact.Erp.Util.StringLiteral;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Promact.Core.Repository.OauthCallsRepository
{
    public class OauthCallsRepository : IOauthCallsRepository
    {

        #region Private Variables

        private readonly IHttpClientService _httpClientService;
        private readonly AppStringLiteral _stringConstant;
        #endregion


        #region Constructor

        public OauthCallsRepository(IHttpClientService httpClientService, ISingletonStringLiteral stringConstant)
        {
            _httpClientService = httpClientService;
            _stringConstant = stringConstant.StringConstant;
        }

        #endregion


        #region Public Methods


        /// <summary>
        /// Method to call an api from project oAuth server and get Employee detail by their slack userId. - SS
        /// </summary>
        /// <param name="slackUserId">userId of slack user</param>
        /// <param name="accessToken">user's access token from Promact OAuth Server</param>
        /// <returns>user Details.Object of User</returns>
        public async Task<User> GetUserByUserIdAsync(string userId, string accessToken)
        {
            User userDetails = new User();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.DetailsAndSlashForUrl, userId);
            var response = await _httpClientService.GetAsync(_stringConstant.UserUrl, requestUrl, accessToken, _stringConstant.Bearer);
            if (response != null)
            {
                userDetails = JsonConvert.DeserializeObject<User>(response);
            }
            return userDetails;
        }


        /// <summary>
        /// Method to call an api from project oAuth server and get List of TeamLeader's slack UserName from employee userName. - SS
        /// </summary>
        /// <param name="slackUserId">userId of slack user</param>
        /// <param name="accessToken">user's access token from Promact OAuth Server</param>
        /// <returns>teamLeader details.List of object of User</returns>
        public async Task<List<User>> GetTeamLeaderUserIdAsync(string userId, string accessToken)
        {
            List<User> teamLeader = new List<User>();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.TeamLeaderDetailsUrl, userId);
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUserUrl, requestUrl, accessToken, _stringConstant.Bearer);
            if (response != null)
            {
                teamLeader = JsonConvert.DeserializeObject<List<User>>(response);
            }
            return teamLeader;
        }


        /// <summary>
        /// Method to call an api from project oAuth server and get List of Management People's Slack UserName. - SS
        /// </summary>
        /// <param name="accessToken">user's access token from Promact OAuth Server</param>
        /// <returns>management details.List of object of User</returns>
        public async Task<List<User>> GetManagementUserNameAsync(string accessToken)
        {
            List<User> management = new List<User>();
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUserUrl, _stringConstant.ManagementDetailsUrl, accessToken, _stringConstant.Bearer);
            if (response != null)
            {
                management = JsonConvert.DeserializeObject<List<User>>(response);
            }
            return management;
        }


        /// <summary>
        /// Method to call an api from project oAuth server and get Project details of the given project id. - JJ 
        /// </summary>
        /// <param name="projectId">Id of OAuth Project</param>
        /// <returns>object of ProjectAc</returns>
        public async Task<ProjectAc> GetProjectDetailsAsync(int projectId, string accessToken)
        {
            string requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.ProjectDetailUrl, projectId.ToString());
            string response = await _httpClientService.GetAsync(_stringConstant.ProjectUrl, requestUrl, accessToken, _stringConstant.Bearer);
            ProjectAc project = new ProjectAc();
            if (!string.IsNullOrEmpty(response))
            {
                project = JsonConvert.DeserializeObject<ProjectAc>(response);
            }
            return project;
        }
                   

        /// <summary>
        /// Method to call an api of oAuth server and get Casual leave allowed to user by user slackName. - SS
        /// </summary>
        /// <param name="userId">userId of user</param>
        /// <param name="accessToken">user's access token from Promact OAuth Server</param>
        /// <returns>Number of casual leave allowed. Object of LeaveAllowed</returns>
        public async Task<LeaveAllowed> AllowedLeave(string userId, string accessToken)
        {
            LeaveAllowed allowedLeave = new LeaveAllowed();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.CasualLeaveUrl, userId);
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUserUrl, requestUrl, accessToken, _stringConstant.Bearer);
            if (response != null)
            {
                allowedLeave = JsonConvert.DeserializeObject<LeaveAllowed>(response);
            }
            return allowedLeave;
        }


        /// <summary>
        /// Method to call an api from oAuth server and get whether user is admin or not. - SS
        /// </summary>
        /// <param name="userId">userId of slack user</param>
        /// <param name="accessToken">user's access token from Promact OAuth Server</param>
        /// <returns>true if user has admin role else false</returns>
        public async Task<bool> UserIsAdminAsync(string userId, string accessToken)
        {
            bool result = false;
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.UserIsAdmin, userId);
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUserUrl, requestUrl, accessToken, _stringConstant.Bearer);
            if (response != null)
            {
                result = JsonConvert.DeserializeObject<bool>(response);
            }
            return result;
        }

        /// <summary>
        /// Method to get list of projects from oauth-server for an user
        /// </summary>
        /// <param name="userId">userId of user</param>
        /// <param name="accessToken">user's access token from Promact OAuth Server</param>
        /// <returns></returns>
        public async Task<List<ProjectAc>> GetListOfProjectsEnrollmentOfUserByUserIdAsync(string userId, string accessToken)
        {
            List<ProjectAc> projects = new List<ProjectAc>();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.DetailsAndSlashForUrl, userId);
            var response = await _httpClientService.GetAsync(_stringConstant.ProjectUrl, requestUrl, accessToken, _stringConstant.Bearer);
            if(response != null)
            {
                projects = JsonConvert.DeserializeObject<List<ProjectAc>>(response);
            }
            return projects;
        }

        /// <summary>
        /// Method to get list of team member by project Id
        /// </summary>
        /// <param name="projectId">project Id</param>
        /// <param name="accessToken">access token</param>
        /// <returns></returns>
        public async Task<List<User>> GetAllTeamMemberByProjectIdAsync(int projectId, string accessToken)
        {
            List<User> teamMembers = new List<User>();
            var requestUrl = string.Format(_stringConstant.FirstAndSecondIndexStringFormat, _stringConstant.DetailsAndSlashForUrl, projectId);
            var response = await _httpClientService.GetAsync(_stringConstant.UserUrl, requestUrl, accessToken, _stringConstant.Bearer);
            if(response != null)
            {
                teamMembers = JsonConvert.DeserializeObject<List<User>>(response);
            }
            return teamMembers;
        }
        

        #endregion
    }
}