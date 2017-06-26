﻿using Microsoft.AspNet.Identity;
using Promact.Erp.DomainModel.ApplicationClass.SlackRequestAndResponse;
using Promact.Erp.DomainModel.Models;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Promact.Core.Repository.AttachmentRepository
{
    public interface IAttachmentRepository
    {
        /// <summary>
        /// Method to create attchment of slack with a text as reply, can be used generically
        /// </summary>
        /// <param name="leaveRequestId">leave request Id</param>
        /// <param name="replyText">reply text to be send</param>
        /// <returns>attachment to be send on slack</returns>
        List<SlashAttachment> SlackResponseAttachment(string leaveRequestId, string replyText);

        /// <summary>
        /// Method will create text corresponding to leave details and user, which will to be send on slack as reply
        /// </summary>
        /// <param name="username">User's slack name</param>
        /// <param name="leave">leave details of user</param>
        /// <returns>string replyText</returns>
        string ReplyText(string username, LeaveRequest leave);

        /// <summary>
        /// Way to break string by spaces only if spaces are not between quotes
        /// </summary>
        /// <param name="text">slash command text</param>
        /// <returns>List of string slackText</returns>
        List<string> SlackText(string text);

        /// <summary>
        /// Method to transform NameValueCollection to SlashCommand class
        /// </summary>
        /// <param name="value">current context value</param>
        /// <returns>SlashCommand object with value</returns>
        SlashCommand SlashCommandTransfrom(NameValueCollection value);

        /// <summary>
        /// Method to get accessToken for Promact OAuth corresponding to username
        /// </summary>
        /// <param name="username">User's email or username</param>
        /// <returns>access token from AspNetUserLogin table</returns>
        Task<string> UserAccessTokenAsync(string username);

        /// <summary>
        /// Method will create text corresponding to sick leave details and user, which will to be send on slack as reply
        /// </summary>
        /// <param name="username">User's slack username</param>
        /// <param name="leave">leave details of user</param>
        /// <returns>string replyText</returns>
        string ReplyTextSick(string username, LeaveRequest leave);

        /// <summary>
        /// Attachment created to be send in slack without any interactive button
        /// </summary>
        /// <param name="leaveRequestId">leave request Id</param>
        /// <param name="replyText">reply text of leave to be send on slack</param>
        /// <returns>attachment to be send on slack</returns>
        List<SlashAttachment> SlackResponseAttachmentWithoutButton(string leaveRequestId, string replyText);

        /// <summary>
        /// Method to convert slash response to SlashChatUpdateResponse
        /// </summary>
        /// <param name="value">current context value</param>
        /// <returns>SlashChatUpdateResponse</returns>
        SlashChatUpdateResponse SlashChatUpdateResponseTransfrom(NameValueCollection value);

        /// <summary>
        /// Method to get task mail in slack message format in string
        /// </summary>
        /// <param name="taskMailDetails">list of task mail details</param>
        /// <returns>task mail in string</returns>
        string GetTaskMailInStringFormat(IEnumerable<TaskMailDetails> taskMailDetails);
    }
}
