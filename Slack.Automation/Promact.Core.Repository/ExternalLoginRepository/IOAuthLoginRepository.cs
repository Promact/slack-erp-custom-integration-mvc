﻿using Promact.Erp.DomainModel.ApplicationClass;
using Promact.Erp.DomainModel.Models;
using System.Threading.Tasks;

namespace Promact.Core.Repository.ExternalLoginRepository
{
    public interface IOAuthLoginRepository
    {
        /// <summary>
        /// Method to add a new user in Application user table and store user's external login information in UserLogin table
        /// </summary>
        /// <param name="email"></param>
        /// <param name="refreshToken"></param>
        /// <param name="userId"></param>
        /// <returns>user information</returns>
        Task<ApplicationUser> AddNewUserFromExternalLoginAsync(string email, string refreshToken, string userId);


        /// <summary>
        /// Method to get OAuth Server's app information
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns>Oauth</returns>
        OAuthApplication ExternalLoginInformation(string refreshToken);


        /// <summary>
        /// Method to add Slack Users,channels and groups information 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task AddSlackUserInformationAsync(string code);


        /// <summary>
        /// Method check user slackid is exists or ot 
        /// </summary>
        /// <param name="userId">login user id</param>
        /// <returns>empty string</returns>
        Task<string> CheckUserSlackInformation(string userId);
    }
}
