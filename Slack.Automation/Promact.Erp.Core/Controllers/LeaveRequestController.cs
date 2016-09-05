﻿using Microsoft.AspNet.Identity.Owin;
using NLog;
using Promact.Core.Repository.AttachmentRepository;
using Promact.Core.Repository.Client;
using Promact.Core.Repository.DataRepository;
using Promact.Core.Repository.SlackRepository;
using Promact.Erp.DomainModel.ApplicationClass;
using Promact.Erp.DomainModel.ApplicationClass.SlackRequestAndResponse;
using Promact.Erp.DomainModel.Models;
using Promact.Erp.Util;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Promact.Erp.Core.Controllers
{
    public class LeaveRequestController : WebApiBaseController
    {
        private readonly ISlackRepository _slackRepository;
        private readonly IClient _client;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IRepository<ApplicationUser> _userManager;
        private readonly ILogger _logger;

        public LeaveRequestController(ISlackRepository slackRepository, IClient client, IAttachmentRepository attachmentRepository, IRepository<ApplicationUser> userManager, ILogger logger)
        {
            _slackRepository = slackRepository;
            _client = client;
            _attachmentRepository = attachmentRepository;
            _userManager = userManager;
            _logger = logger;
        }
        /// <summary>
        /// Slack Call for Slash Command
        /// </summary>
        /// <param name="blog"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("leaves/slackcall")]
        public async Task<IHttpActionResult> SlackRequest()
        {
            var request = HttpContext.Current.Request.Form;
            var leave = _attachmentRepository.SlashCommandTransfrom(request);
            try
            {
                leave.Text.ToLower();
                var slackText = _attachmentRepository.SlackText(leave.Text);
                var action = (SlackAction)Enum.Parse(typeof(SlackAction), slackText[0]);
                var user = _userManager.FirstOrDefault(x => x.SlackUserName == leave.Username);
                var accessToken = await _attachmentRepository.AccessToken(user.UserName);
                switch (action)
                {
                    case SlackAction.apply:
                        {
                            var leaveRequest = await _slackRepository.LeaveApply(slackText, leave, accessToken);
                            if (leaveRequest.Id != 0)
                            {
                                await _client.SendMessageWithAttachmentIncomingWebhook(leave, leaveRequest, accessToken);
                            }
                        }
                        break;
                    case SlackAction.list:
                        await _slackRepository.SlackLeaveList(slackText, leave, accessToken);
                        break;
                    case SlackAction.cancel:
                        await _slackRepository.SlackLeaveCancel(slackText, leave, accessToken);
                        break;
                    case SlackAction.status:
                        await _slackRepository.SlackLeaveStatus(slackText, leave, accessToken);
                        break;
                    case SlackAction.balance:
                        await _slackRepository.SlackLeaveBalance(leave,accessToken);
                        break;
                    default:
                        _slackRepository.SlackLeaveHelp(leave);
                        break;
                }
                return Ok();
            }
            // If throws any type of error it will give same message in slack by response_url
            catch (Exception ex)
            {
                var replyText = string.Format("{0}{1}{2}{1}{3}", StringConstant.LeaveBalanceErrorMessage, Environment.NewLine, StringConstant.OrElseString, StringConstant.SlackErrorMessage);
                _client.SendMessage(leave, replyText);
                _logger.Error(ex, StringConstant.LoggerErrorMessageLeaveRequestControllerSlackRequest);
                return BadRequest(ex.ToString());
            }
        }

        /// <summary>
        /// Method to update the leave details. Response will be from slack interactive message button
        /// </summary>
        /// <param name="leaveResponse"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("leaves/slackbuttoncall")]
        public IHttpActionResult SlackButtonRequest()
        {
            try
            {
            SlashChatUpdateResponse leaveResponse = new SlashChatUpdateResponse();
            _slackRepository.UpdateLeave(leaveResponse);
            return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, StringConstant.LoggerErrorMessageLeaveRequestControllerSlackButtonRequest);
                throw;
            }
        }
    }
}
