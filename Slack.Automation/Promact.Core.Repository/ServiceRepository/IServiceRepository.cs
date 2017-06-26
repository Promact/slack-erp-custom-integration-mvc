﻿using System.Threading.Tasks;

namespace Promact.Core.Repository.ServiceRepository
{
    public interface IServiceRepository 
    {
        /// <summary>
        /// This method used for get access token by refresh token.
        /// </summary>
        /// <param name="refreshToken">passed refresh token</param>
        /// <param name="userId">userId of user</param>
        /// <returns>access token</returns>
        Task<string> GerAccessTokenByRefreshToken(string refreshToken, string userId);
    }
}
