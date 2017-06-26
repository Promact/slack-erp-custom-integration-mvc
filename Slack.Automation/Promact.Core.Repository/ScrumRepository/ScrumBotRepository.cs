﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Promact.Core.Repository.OauthCallsRepository;
using Promact.Core.Repository.SlackChannelRepository;
using Promact.Core.Repository.SlackUserRepository;
using Promact.Erp.DomainModel.ApplicationClass;
using Promact.Erp.DomainModel.ApplicationClass.SlackRequestAndResponse;
using Promact.Erp.DomainModel.DataRepository;
using Promact.Erp.DomainModel.Models;
using Promact.Core.Repository.AttachmentRepository;
using Promact.Core.Repository.BotQuestionRepository;
using Promact.Core.Repository.BaseRepository;
using NLog;
using Promact.Core.Repository.ScrumSetUpRepository;
using Promact.Erp.Util.StringLiteral;
using Newtonsoft.Json;

namespace Promact.Core.Repository.ScrumRepository
{
    public class ScrumBotRepository : RepositoryBase, IScrumBotRepository
    {

        #region Private Variable 


        private readonly IRepository<TemporaryScrumDetails> _tempScrumDetailsDataRepository;
        private readonly IRepository<ScrumAnswer> _scrumAnswerDataRepository;
        private readonly IRepository<Scrum> _scrumDataRepository;
        private readonly IRepository<Question> _questionDataRepository;
        private readonly IRepository<SlackUserDetails> _slackUserDetailsDataRepository;
        private readonly IRepository<ApplicationUser> _applicationUser;
        private readonly ISlackChannelRepository _slackChannelRepository;
        private readonly IOauthCallsRepository _oauthCallsRepository;
        private readonly ISlackUserRepository _slackUserDetailRepository;
        private readonly AppStringLiteral _stringConstant;
        private readonly IBotQuestionRepository _botQuestionRepository;
        private readonly IScrumSetUpRepository _scrumSetUpRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        #endregion


        #region Constructor


        public ScrumBotRepository(IRepository<TemporaryScrumDetails> tempScrumDetailsDataRepository,
            IRepository<ScrumAnswer> scrumAnswerDataRepository,
            IRepository<Scrum> scrumDataRepository, IRepository<Question> questionDataRepository,
            IRepository<SlackUserDetails> slackUserDetailsDataRepository,
            ISlackChannelRepository slackChannelRepository, IOauthCallsRepository oauthCallsRepository,
            ISlackUserRepository slackUserDetailRepository, ISingletonStringLiteral stringConstant,
            IBotQuestionRepository botQuestionRepository, IMapper mapper, IScrumSetUpRepository scrumSetUpRepository,
            IRepository<ApplicationUser> applicationUser, IAttachmentRepository attachmentRepository)
            : base(applicationUser, attachmentRepository)
        {
            _tempScrumDetailsDataRepository = tempScrumDetailsDataRepository;
            _scrumAnswerDataRepository = scrumAnswerDataRepository;
            _logger = LogManager.GetLogger("ScrumBotModule");
            _scrumDataRepository = scrumDataRepository;
            _questionDataRepository = questionDataRepository;
            _slackUserDetailRepository = slackUserDetailRepository;
            _slackChannelRepository = slackChannelRepository;
            _oauthCallsRepository = oauthCallsRepository;
            _slackUserDetailsDataRepository = slackUserDetailsDataRepository;
            _stringConstant = stringConstant.StringConstant;
            _botQuestionRepository = botQuestionRepository;
            _applicationUser = applicationUser;
            _scrumSetUpRepository = scrumSetUpRepository;
            _mapper = mapper;
        }


        #endregion


        #region Public Method 


        /// <summary>
        /// This will process the messages from slack and use appropriate methods to give a suitable response through Bot - JJ
        /// </summary>
        /// <param name="slackUserId">UserId of slack user</param>
        /// <param name="slackChannelId">slack channel id from which message is send</param>
        /// <param name="message">message from slack</param>
        /// <param name="scrumBotId">Id of the bot connected for conducting scrum</param>
        /// <returns>reply message</returns>      
        public async Task<string> ProcessMessagesAsync(string slackUserId, string slackChannelId, string message, string scrumBotId)
        {
            _logger.Info(DateTime.UtcNow.Date);
            string replyText = string.Empty;
            SlackUserDetailAc slackUserDetail = await _slackUserDetailRepository.GetByIdAsync(slackUserId);
            _logger.Info("\nSlack User Detail\n " + JsonConvert.SerializeObject(slackUserDetail));
            SlackChannelDetails slackChannelDetail = await _slackChannelRepository.GetByIdAsync(slackChannelId);
            _logger.Info("\nSlack Channel Detail\n " + JsonConvert.SerializeObject(slackChannelDetail));
            //the command is split to individual words
            //commnads ex: "scrum time", "leave @userId"
            string[] messageArray = message.Split(null);

            #region Added temporarily for testing purpose

            if (messageArray[0] == "delete")
            {
                var date = DateTime.UtcNow.Date;
                // get access token of user for promact oauth server
                var accessToken = await GetAccessToken(slackUserId);

                if (accessToken != null)
                {
                    if (slackChannelDetail != null && slackChannelDetail.ProjectId != null)
                    {
                        ProjectAc project = await GetOAuthProjectAsync((int)slackChannelDetail.ProjectId, accessToken);
                        if (project?.Id > 0)
                        {
                            Scrum scrum = _scrumDataRepository.FirstOrDefault(x => x.ProjectId == project.Id && x.ScrumDate == date);
                            if (scrum != null)
                            {
                                _scrumDataRepository.Delete(scrum.Id);
                                int scrumDelete = await _scrumDataRepository.SaveChangesAsync();
                                if (scrumDelete == 1)
                                    replyText += "scrum has been deleted\n";
                                else
                                    replyText += "scrum has not been deleted\n";
                                TemporaryScrumDetails temp = _tempScrumDetailsDataRepository.FirstOrDefault(x => x.ScrumId == scrum.Id);
                                if (temp != null)
                                {
                                    _tempScrumDetailsDataRepository.Delete(temp.Id);
                                    int deleteTemp = await _tempScrumDetailsDataRepository.SaveChangesAsync();
                                    if (deleteTemp == 1)
                                        replyText += "temp data has been deleted\n";
                                    else
                                        replyText += "temp data has not been deleted\n";
                                }
                            }
                            else
                                replyText += "no scrum cud be deleted\n";
                        }
                        else
                            replyText = "Project not found in OAuth\n";
                    }
                    else
                        replyText = "Slack channel not linked to any Project in OAuth\n";
                }
                else
                    replyText = "Please login with OAuth\n";
                return replyText;
            }

            #endregion

            if (slackUserDetail != null)
            {
                if (String.Compare(message, _stringConstant.ScrumHelp, StringComparison.OrdinalIgnoreCase) == 0) //when the message obtained is "scrum help"
                {
                    _logger.Debug("Scrum help message");
                    replyText = string.Format(_stringConstant.ScrumHelpMessage, scrumBotId);
                }
                else if (slackChannelDetail != null)
                {
                    if (String.Compare(messageArray[0], _stringConstant.Link, StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(messageArray[0], _stringConstant.Unlink, StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(message, _stringConstant.ListLinks, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        replyText = await _scrumSetUpRepository.ProcessSetUpMessagesAsync(slackUserId, slackChannelDetail, message);
                        if (string.IsNullOrEmpty(replyText))
                            replyText = await AddScrumAnswerAsync(message, slackChannelDetail.ProjectId, slackUserId, true);
                    }
                    else
                    {
                        #region code specific to scrum

                        if (slackChannelDetail.ProjectId != null)
                        {
                            //commands could be "scrum halt" or "scrum resume"
                            if (String.Compare(message, _stringConstant.ScrumHalt, StringComparison.OrdinalIgnoreCase) == 0 ||
                                String.Compare(message, _stringConstant.ScrumResume, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                _logger.Debug("Scrum command is :" + message);
                                replyText = await ScrumAsync((int)slackChannelDetail.ProjectId, messageArray[1].ToLower(), slackUserId);
                            }
                            //a particular user is on leave. command would be like "leave <@userId>"
                            else if (((String.Compare(messageArray[0], _stringConstant.Leave, StringComparison.OrdinalIgnoreCase) == 0) || (String.Compare(messageArray[0], _stringConstant.Start, StringComparison.OrdinalIgnoreCase) == 0)) && messageArray.Length == 2)
                            {
                                _logger.Debug("Scrum command is leave or start");
                                //"<@".Length is 2
                                int fromIndex = message.IndexOf("<@", StringComparison.Ordinal) + 2;
                                int toIndex = message.LastIndexOf(">", StringComparison.Ordinal);
                                if (toIndex > 0 && fromIndex > 1)
                                {
                                    //the slack userId is fetched
                                    string applicantId = message.Substring(fromIndex, toIndex - fromIndex);
                                    _logger.Debug("Scrum command is leave or start. User mentioned is :" + applicantId);
                                    if (String.Compare(messageArray[0], _stringConstant.Leave, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        _logger.Debug("Scrum command is leave");
                                        //fetch the user of the given userId
                                        SlackUserDetailAc applicantDetailsAc = await _slackUserDetailRepository.GetByIdAsync(applicantId);
                                        replyText = applicantDetailsAc != null ? await LeaveAsync((int)slackChannelDetail.ProjectId, slackUserId, applicantId) : _stringConstant.NotAUser;
                                    }
                                    else
                                    {
                                        if (String.Compare(applicantId, scrumBotId, StringComparison.Ordinal) == 0)
                                        {
                                            _logger.Debug("Scrum command is start");
                                            replyText = await ScrumAsync((int)slackChannelDetail.ProjectId, messageArray[0].ToLower(), slackUserId);
                                        }
                                        else
                                        {
                                            _logger.Debug("Invalid start command");
                                            replyText = string.Format(_stringConstant.InValidStartCommand, scrumBotId);
                                        }
                                    }
                                }
                                else //when command would be like "leave <@>"
                                {
                                    _logger.Debug("Invalid leave command. So call AddScrumAnswerAsync method");
                                    replyText = await AddScrumAnswerAsync(message, (int)slackChannelDetail.ProjectId, slackUserId, false);
                                }
                            }
                            else  //all other texts
                            {
                                _logger.Debug("AddScrumAnswerAsync method");
                                replyText = await AddScrumAnswerAsync(message, (int)slackChannelDetail.ProjectId, slackUserId, false);
                            }
                        }
                        else
                        {
                            if (await IsScrumStartLeaveLinkCommandAsync(scrumBotId, message, messageArray)
                              || String.Compare(message, _stringConstant.ScrumHalt, StringComparison.OrdinalIgnoreCase) == 0
                              || String.Compare(message, _stringConstant.ScrumResume, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                replyText = _stringConstant.ProjectChannelNotLinked;
                            }
                        }

                        #endregion
                    }
                }
                else   //channel is not registered in the database
                {
                    //If channel is not registered in the database and the command encountered is "add channel channelname"
                    if (String.Compare(messageArray[0], _stringConstant.Add, StringComparison.OrdinalIgnoreCase) == 0 &&
                        String.Compare(messageArray[1], _stringConstant.Channel, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        _logger.Debug("AddChannelManuallyAsync method");
                        replyText = await _scrumSetUpRepository.AddChannelManuallyAsync(messageArray[2], slackChannelId, slackUserId);
                    }
                    //If any of the commands which scrum bot recognizes is encountered                  
                    else if (await IsScrumStartLeaveLinkCommandAsync(scrumBotId, message, messageArray)
                       || String.Compare(message, _stringConstant.ScrumHalt, StringComparison.OrdinalIgnoreCase) == 0
                       || String.Compare(message, _stringConstant.ScrumResume, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        _logger.Debug("Channel is not in our db so give instruction to add channel");
                        replyText = _stringConstant.ChannelAddInstruction;
                    }
                }
            }
            else
            {
                Scrum scrum;
                if (slackChannelDetail?.ProjectId != null)
                {
                    DateTime today = DateTime.UtcNow.Date;
                    scrum = await _scrumDataRepository.FirstOrDefaultAsync(x => x.ProjectId == slackChannelDetail.ProjectId &&
                            DbFunctions.TruncateTime(x.ScrumDate) == today);
                    _logger.Info(scrum?.ScrumDate);
                }
                else
                    scrum = null;
                if (await IsScrumStartLeaveLinkCommandAsync(scrumBotId, message, messageArray)
                   || String.Compare(message, _stringConstant.ScrumHalt, StringComparison.OrdinalIgnoreCase) == 0
                   || String.Compare(message, _stringConstant.ScrumResume, StringComparison.OrdinalIgnoreCase) == 0
                   || (scrum != null && scrum.IsOngoing && !scrum.IsHalted))
                {
                    _logger.Debug("Slack user is not in our db.");
                    replyText = _stringConstant.SlackUserNotFound;
                }
            }
            return replyText;
        }


        #region Temporary Scrum Details


        /// <summary>
        /// Store the scrum details temporarily in a database - JJ
        /// </summary>
        /// <param name="scrumId">Id of scrum of the channel for the day</param>
        /// <param name="slackUserId">UserId of slack user</param>
        /// <param name="answerCount">Number of answers of the user</param>
        /// <param name="questionId">The Id of the last question asked to the user</param>
        /// <returns></returns>
        public async Task AddTemporaryScrumDetailsAsync(int scrumId, string slackUserId, int answerCount, int questionId)
        {
            TemporaryScrumDetails tempScrumDetails = await _tempScrumDetailsDataRepository.FirstOrDefaultAsync(x => x.ScrumId == scrumId
            && DbFunctions.TruncateTime(x.CreatedOn) == DateTime.UtcNow.Date);
            if (tempScrumDetails == null)
            {
                TemporaryScrumDetails temporaryScrumDetails = new TemporaryScrumDetails();
                temporaryScrumDetails.ScrumId = scrumId;
                temporaryScrumDetails.SlackUserId = slackUserId;
                temporaryScrumDetails.AnswerCount = answerCount;
                temporaryScrumDetails.QuestionId = questionId;
                temporaryScrumDetails.CreatedOn = DateTime.UtcNow.Date;
                _tempScrumDetailsDataRepository.Insert(temporaryScrumDetails);
                await _tempScrumDetailsDataRepository.SaveChangesAsync();
            }
        }


        #endregion


        #endregion


        #region Private Methods


        #region Temporary Scrum Details


        /// <summary>
        /// Fetch the temporary scrum details of the given projectId for today. - JJ
        /// </summary>
        /// <param name="scrumId">Id of scrum of the channel for the day</param>
        /// <returns>object of TemporaryScrumDetails</returns>
        private async Task<TemporaryScrumDetails> FetchTemporaryScrumDetailsAsync(int scrumId)
        {
            DateTime date = DateTime.UtcNow.Date;
            TemporaryScrumDetails temporaryScrumDetails = await _tempScrumDetailsDataRepository.FirstOrDefaultAsync(x => DbFunctions.TruncateTime(x.CreatedOn) == date
            && x.ScrumId == scrumId);
            return temporaryScrumDetails;
        }


        /// <summary>
        /// Remove all the temporary data of the scrum of the given scrumId from the list of the given day. - JJ
        /// </summary>
        /// <param name="scrumId">Id of scrum of the channel for the day</param>
        /// <returns></returns>
        private async Task RemoveTemporaryScrumDetailsAsync(int scrumId)
        {
            DateTime date = DateTime.UtcNow.Date;
            TemporaryScrumDetails temporaryScrumDetails = await _tempScrumDetailsDataRepository.FirstOrDefaultAsync(x => x.ScrumId == scrumId
            && DbFunctions.TruncateTime(x.CreatedOn) == date);
            if (temporaryScrumDetails != null)
            {
                _tempScrumDetailsDataRepository.Delete(temporaryScrumDetails.Id);
                await _tempScrumDetailsDataRepository.SaveChangesAsync();
            }
        }


        /// <summary>
        /// Get the slack user who was last asked question to. - JJ
        /// </summary>
        /// <param name="scrumId">id of scrum of the channel for that day</param>
        /// <param name="users">List of users of the project(in OAuth) corresponding to slack channel</param>
        /// <returns>object of SlackUserDetails</returns>
        private async Task<SlackUserDetailAc> GetSlackUserAsync(int scrumId, List<User> users)
        {
            TemporaryScrumDetails temporaryScrumDetails = await FetchTemporaryScrumDetailsAsync(scrumId);
            SlackUserDetailAc slackUserDetailsAc = await _slackUserDetailRepository.GetByIdAsync(temporaryScrumDetails.SlackUserId);
            if (slackUserDetailsAc != null)
            {
                User user = users.FirstOrDefault(x => x.SlackUserId == temporaryScrumDetails.SlackUserId);
                if (user != null)
                {
                    slackUserDetailsAc.IsActive = user.IsActive;
                    slackUserDetailsAc.Deleted = false;
                }
                else
                    slackUserDetailsAc.Deleted = true;
            }
            return slackUserDetailsAc;
        }


        /// <summary>
        /// Update the scrum details temporarily stored in the database. - JJ
        /// </summary>
        /// <param name="slackUserId">Slack user's Id</param>
        /// <param name="scrumId">scrum id of the project for the day</param>
        /// <param name="users">List of users of the project</param>
        /// <param name="questionId">Id of last question asked to the user</param>
        private async Task UpdateTemporaryScrumDetailsAsync(string slackUserId, int scrumId, List<User> users, int? questionId)
        {
            DateTime date = DateTime.UtcNow.Date;
            TemporaryScrumDetails temporaryScrumDetails = await _tempScrumDetailsDataRepository.FirstOrDefaultAsync(x => x.ScrumId == scrumId
            && DbFunctions.TruncateTime(x.CreatedOn) == date);
            if (temporaryScrumDetails != null)
            {
                User user = users.FirstOrDefault(x => x.SlackUserId == slackUserId);
                int answerCount = _scrumAnswerDataRepository.FetchAsync(x => x.ScrumId == scrumId && x.EmployeeId == user.Id).Result.Count();
                temporaryScrumDetails.SlackUserId = slackUserId;
                temporaryScrumDetails.AnswerCount = answerCount;
                temporaryScrumDetails.QuestionId = questionId;
                _tempScrumDetailsDataRepository.Update(temporaryScrumDetails);
                await _tempScrumDetailsDataRepository.SaveChangesAsync();
            }
        }


        #endregion


        /// <summary>
        /// Check whether the user with the given slack id is active or not - JJ
        /// </summary>
        /// <param name="slackUserId">slack user Id of the interacting user</param>
        /// <param name="users">List of users of the project</param>
        /// <param name="teamLeaderId">Id of the team leader of the project</param>
        /// <returns>true if active else false</returns>
        private async Task<bool> CheckUserAsync(string slackUserId, List<User> users, string teamLeaderId)
        {
            User user = users.FirstOrDefault(x => x.SlackUserId == slackUserId);
            if (user == null)
            {
                return (await _applicationUser.FirstOrDefaultAsync(x => x.Id == teamLeaderId && x.SlackUserId == slackUserId) != null);
            }
            if (user != null && user.IsActive)
                return true;

            return false;
        }


        /// <summary>
        /// Checks whether the command is a valid scrum start,leave or link command - JJ
        /// </summary>
        /// <param name="scrumBotId">Slack Id of the scrum bot</param>
        /// <param name="message">the actual message</param>
        /// <param name="messageArray">message divided by space</param>
        /// <returns>true if it is a valid start or leave command else false</returns>
        private async Task<bool> IsScrumStartLeaveLinkCommandAsync(string scrumBotId, string message, string[] messageArray)
        {
            if (((String.Compare(messageArray[0], _stringConstant.Leave, StringComparison.OrdinalIgnoreCase) == 0) || (String.Compare(messageArray[0], _stringConstant.Start, StringComparison.OrdinalIgnoreCase) == 0)) && messageArray.Length == 2)
            {
                //"<@".Length is 2
                int fromIndex = message.IndexOf("<@", StringComparison.Ordinal) + 2;
                int toIndex = message.LastIndexOf(">", StringComparison.Ordinal);
                if (toIndex > 0 && fromIndex > 1)
                {
                    //the slack userId is fetched
                    string applicantId = message.Substring(fromIndex, toIndex - fromIndex);
                    if (String.Compare(messageArray[0], _stringConstant.Leave, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        //fetch the user of the given userId
                        SlackUserDetailAc applicantDetailsAc = await _slackUserDetailRepository.GetByIdAsync(applicantId);
                        return applicantDetailsAc != null ? true : false;
                    }
                    else
                    {
                        if (String.Compare(applicantId, scrumBotId, StringComparison.Ordinal) == 0)
                            return true;
                    }
                }
            }
            else if (String.Compare(messageArray[0], _stringConstant.Link, StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(messageArray[0], _stringConstant.Unlink, StringComparison.OrdinalIgnoreCase) == 0)
            {
                _logger.Debug("Link message in Scrum Repo before replacing " + message);
                message = message.Replace("“", "\"").Replace("”", "\"");
                _logger.Debug("Link message in Scrum Repo after replacing " + message);
                string[] msgArray = message.Split(null);
                int messageLength = message.Length - 1;
                int first = message.IndexOf('"') + 1; //first index of ".
                int last = message.IndexOf('"', message.IndexOf('"') + 1);//last index of "
                int projectNameStartIndex = msgArray[0].Length + 2;// index from where the name of the project starts

                if (messageLength == last && first == projectNameStartIndex)
                {
                    //fetch the project name from the message string
                    string name = message.Substring(first, last - first);
                    if (string.IsNullOrEmpty(name))// ex. link ""
                        return false;// it will be considered as a normal message
                    return true;
                }
                return false;
            }
            else if (String.Compare(message, _stringConstant.ListLinks, StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            return false;
        }


        /// <summary>
        /// Fetch today's scrum - JJ
        /// </summary>
        /// <param name="projectId">slack channel id from which message is send</param>
        /// <returns>Object of Scrum</returns>
        private async Task<Scrum> GetScrumAsync(int projectId)
        {
            DateTime today = DateTime.UtcNow.Date;
            var scrum = await _scrumDataRepository.FirstOrDefaultAsync(x => x.ProjectId == projectId &&
                        DbFunctions.TruncateTime(x.ScrumDate) == today);
            _logger.Info(scrum?.ScrumDate);
            return scrum;
        }


        /// <summary>
        ///  This method is called whenever a message other than the default keywords is written in the channel. - JJ
        /// </summary>
        /// <param name="message">the message that interacting user sends</param>
        /// <param name="projectId">id of the project of the slack channel from which the message has been send</param>
        /// <param name="isLinkCommand">it indicates whether the message was already processed as a link command</param>
        /// <param name="slackUserId">slack user Id of the interacting user</param>
        /// <returns>the next question statement</returns>
        private async Task<string> AddScrumAnswerAsync(string message, int? projectId, string slackUserId, bool isLinkCommand)
        {
            if (projectId != null)
            {
                //today's scrum of the channel 
                Scrum scrum = await GetScrumAsync((int)projectId);
                if (scrum != null && scrum.IsOngoing && !scrum.IsHalted)
                {
                    // get access token of user for promact oauth server
                    var accessToken = await GetAccessToken(slackUserId);
                    if (accessToken != null)
                    {
                        //list of scrum questions. Type = BotQuestionType.Scrum
                        List<Question> questions = await _botQuestionRepository.GetQuestionsByTypeAsync(BotQuestionType.Scrum);
                        ProjectAc project = await GetOAuthProjectAsync((int)projectId, accessToken);
                        //users of the given channel name fetched from the oauth server
                        List<User> users = project?.Users;
                        ScrumStatus scrumStatus = await FetchScrumStatusAsync(project, users, questions);
                        //scrumStatus could be anything like the project is in-active
                        if (scrumStatus == ScrumStatus.OnGoing)
                        {
                            //status would be empty if the interacting user is same as the expected user.
                            string status = await ExpectedUserAsync(scrum.Id, questions, users, slackUserId, scrum.ProjectId);
                            if (string.IsNullOrEmpty(status))
                            {
                                TemporaryScrumDetails temporaryScrumDetails = await FetchTemporaryScrumDetailsAsync(scrum.Id);
                                User user = users.First(x => x.SlackUserId == temporaryScrumDetails.SlackUserId);
                                if (temporaryScrumDetails.QuestionId != null)
                                {
                                    AddAnswer(scrum.Id, (int)temporaryScrumDetails.QuestionId, user.Id, message, ScrumAnswerStatus.Answered);
                                    await _scrumAnswerDataRepository.SaveChangesAsync();
                                }
                                else
                                    return _stringConstant.AnswerNotRecorded;

                                //update the details in temporary table 
                                await UpdateTemporaryScrumDetailsAsync(slackUserId, scrum.Id, users, null);
                                //get the next question
                                return await GetQuestionAsync(scrum.Id, questions, users, scrum.ProjectId);
                            }
                            //the user interacting is not the expected user
                            else
                                return status;
                        }
                        else
                            return ReplyStatusofScrumToClient(scrumStatus);
                    }
                    else
                        // if user doesn't exist then this message will be shown to user
                        return _stringConstant.YouAreNotInExistInOAuthServer;
                }
            }
            else if (isLinkCommand && projectId == null)
                return _stringConstant.ProjectChannelNotLinked;
            return string.Empty;
        }


        /// <summary>
        /// This method will be called when the keyword "scrum time" or "scrum halt" or "scrum resume" is encountered. - JJ
        /// </summary>
        /// <param name="projectId">id of the project of the slack channel from which the message has been send</param>
        /// <param name="parameter">the keyword(second word) send by the user</param>      
        /// <param name="slackUserId">slack userId of the interacting user</param>
        /// <returns>The question or the status of the scrum</returns>
        private async Task<string> ScrumAsync(int projectId, string parameter, string slackUserId)
        {
            //because any command outside the scrum time must not be entertained except with the replies like "scrum is concluded","scrum has not started" or "scrum has not started".
            Scrum scrum = await GetScrumAsync(projectId);
            ScrumActions scrumCommand = (ScrumActions)Enum.Parse(typeof(ScrumActions), parameter);
            if (scrumCommand == ScrumActions.start || scrum != null)
            {
                if (scrumCommand == ScrumActions.start || scrum.IsOngoing)
                {
                    // get access token of user for promact oauth server
                    var accessToken = await GetAccessToken(slackUserId);
                    if (accessToken != null)
                    {
                        ProjectAc project = await GetOAuthProjectAsync(projectId, accessToken);
                        List<User> users = project?.Users;
                        ScrumStatus scrumStatus = await FetchScrumStatusAsync(project, users, null);
                        if (users?.Count > 0)
                        {
                            if (await CheckUserAsync(slackUserId, users, project.TeamLeaderId))
                            {
                                switch (scrumCommand)
                                {
                                    case ScrumActions.halt:
                                        //keyword encountered is "scrum halt"
                                        return await ScrumHaltAsync(scrum, scrumStatus);
                                    case ScrumActions.resume:
                                        //keyword encountered is "scrum resume"
                                        return await ScrumResumeAsync(scrum, users, scrumStatus);
                                    case ScrumActions.start:
                                        //keyword encountered is "start <@botId>"
                                        return await StartScrumAsync(projectId, users, project, scrumStatus);
                                    default:
                                        return string.Empty;
                                }
                            }
                            //if user is in-active
                            string returnMessage;
                            switch (scrumStatus)
                            {
                                case ScrumStatus.Halted:
                                    returnMessage = (scrumCommand == ScrumActions.resume ? _stringConstant.ScrumCannotBeResumed : string.Empty) + string.Format(_stringConstant.InActiveInOAuth, slackUserId);
                                    break;
                                //scrum is in progress
                                case ScrumStatus.OnGoing:
                                    List<Question> questions = await _botQuestionRepository.GetQuestionsByTypeAsync(BotQuestionType.Scrum);
                                    if (scrum != null)
                                        returnMessage = await GetReplyToUserAsync(users, project.Id, scrum.Id, slackUserId, questions);
                                    else
                                        return _stringConstant.ErrorMsgNewPrivateChannel;
                                    if (scrumCommand == ScrumActions.resume)
                                        returnMessage = _stringConstant.ScrumNotHalted + Environment.NewLine + returnMessage;
                                    else if (scrumCommand == ScrumActions.halt)
                                        returnMessage = _stringConstant.ScrumCannotBeHalted + Environment.NewLine + returnMessage;
                                    break;

                                //for all other status of the scrum
                                default:
                                    returnMessage = ReplyStatusofScrumToClient(scrumStatus);
                                    break;
                            }
                            return returnMessage;
                        }
                        else
                            return _stringConstant.NoEmployeeFound;
                    }
                    return _stringConstant.YouAreNotInExistInOAuthServer;
                }
                return _stringConstant.ScrumAlreadyConducted;
            }
            return _stringConstant.ScrumNotStarted;
        }


        /// <summary>
        /// This method will be called when the keyword "leave @username" is received as reply from a channel member. - JJ
        /// </summary>
        /// <param name="projectId">id of the project of the slack channel from which the message has been send</param>
        /// <param name="slackUserId">slack user Id of the interacting user</param>
        /// <param name="applicantId">slack user id of the user who is being marked on leave</param>
        /// <returns>Question to the next person or other scrum status</returns>
        private async Task<string> LeaveAsync(int projectId, string slackUserId, string applicantId)
        {
            string returnMsg;
            //we will have to check whether the scrum is on going or not before calling FetchScrumStatus()
            //because any command outside the scrum time must not be entertained except with the replies like "scrum is concluded","scrum has not started" or "scrum has not started".
            Scrum scrum = await GetScrumAsync(projectId);
            if (scrum != null)
            {
                if (scrum.IsOngoing)
                {
                    if (!scrum.IsHalted)
                    {
                        // get access token of user for promact oauth server
                        var accessToken = await GetAccessToken(slackUserId);
                        if (accessToken != null)
                        {
                            List<Question> questions = await _botQuestionRepository.GetQuestionsByTypeAsync(BotQuestionType.Scrum);
                            ProjectAc project = await GetOAuthProjectAsync(projectId, accessToken);
                            List<User> users = project?.Users;
                            ScrumStatus scrumStatus = await FetchScrumStatusAsync(project, users, questions);
                            if (scrumStatus == ScrumStatus.OnGoing)
                            {
                                if (await CheckUserAsync(slackUserId, users, project.TeamLeaderId))
                                    returnMsg = await MarkLeaveAsync(users, scrum.Id, questions, scrum.ProjectId, slackUserId, applicantId);
                                else
                                    //when the applicant is not in OAuth or not user of the project or is in-active inOAuth
                                    returnMsg = await GetReplyToUserAsync(users, project.Id, scrum.Id, slackUserId, questions);
                            }
                            else
                                returnMsg = ReplyStatusofScrumToClient(scrumStatus);
                        }
                        else
                            // if user doesn't exist in OAuth or hasn't logged in with Promact OAuth then this message will be shown to user
                            returnMsg = _stringConstant.YouAreNotInExistInOAuthServer;
                    }
                    else
                        returnMsg = _stringConstant.ScrumIsHalted;
                }
                else
                    returnMsg = _stringConstant.ScrumAlreadyConducted;
            }
            else
                returnMsg = _stringConstant.ScrumNotStarted;
            return returnMsg;
        }


        /// <summary>
        /// This method is used to add/update Scrum answer to/in the database. - JJ
        /// </summary>
        /// <param name="scrumId">Id of scrum of the channel for that day</param>
        /// <param name="questionId">Id of the question which is answered</param>
        /// <param name="userId">Id of the user who has answered</param>
        /// <param name="message">answer</param>
        /// <param name="scrumAnswerStatus">the status of the answer like. Answered,Leave,etc</param>
        private void AddAnswer(int scrumId, int questionId, string userId, string message, ScrumAnswerStatus scrumAnswerStatus)
        {
            ScrumAnswer answer = new ScrumAnswer();
            answer.Answer = message;
            answer.AnswerDate = DateTime.UtcNow;
            answer.CreatedOn = DateTime.UtcNow;
            answer.EmployeeId = userId;
            answer.QuestionId = questionId;
            answer.ScrumId = scrumId;
            answer.ScrumAnswerStatus = scrumAnswerStatus;
            _scrumAnswerDataRepository.Insert(answer);
        }


        /// <summary>
        /// This method will be called when the keyword "scrum time" is encountered. - JJ
        /// </summary>
        /// <param name="projectId">slack channel id from which message is send</param>
        /// <param name="users">List of users of the project(in OAuth) corresponding to slack channel</param>
        /// <param name="project">project(in OAuth) corresponding to slack channel</param>
        ///<param name="scrumStatus">status of scrum</param>
        /// <returns>The next question or the scrum complete message</returns>
        private async Task<string> StartScrumAsync(int projectId, List<User> users, ProjectAc project, ScrumStatus scrumStatus)
        {
            string replyMessage = string.Empty;
            List<Question> questionList = await _botQuestionRepository.GetQuestionsByTypeAsync(BotQuestionType.Scrum);
            //only if scrum has not been conducted in the day can scrum start.
            if (scrumStatus == ScrumStatus.NotStarted)
            {
                Question question = questionList.First();
                User firstUser = users.FirstOrDefault(x => x.IsActive);
                if (firstUser != null)
                {
                    SlackUserDetailAc slackUserDetailAc = await _slackUserDetailRepository.GetByIdAsync(firstUser.SlackUserId);
                    if (slackUserDetailAc == null)
                    {
                        List<string> idList = users.Where(y => y.IsActive).Select(y => y.SlackUserId).ToList();
                        //fetch the next slack user who is an active user of the project.
                        SlackUserDetails slackUserDetail = _slackUserDetailsDataRepository.FirstOrDefault(x => idList.Contains(x.UserId));
                        if (slackUserDetail != null)
                        {
                            firstUser = users.First(x => x.SlackUserId == slackUserDetail.UserId);
                            slackUserDetailAc = _mapper.Map<SlackUserDetailAc>(slackUserDetail);
                        }
                        else
                            return _stringConstant.NoEmployeeFound;
                    }
                    Scrum scrum = new Scrum();
                    scrum.CreatedOn = DateTime.UtcNow.Date;
                    scrum.ScrumDate = DateTime.UtcNow.Date;
                    scrum.ProjectId = project.Id;
                    scrum.TeamLeaderId = project.TeamLeaderId;
                    scrum.IsHalted = false;
                    scrum.IsOngoing = true;
                    _scrumDataRepository.Insert(scrum);
                    await _scrumDataRepository.SaveChangesAsync();

                    //add the scrum details to the temporary table
                    await AddTemporaryScrumDetailsAsync(scrum.Id, firstUser.SlackUserId, 0, question.Id);
                    //first user is asked questions along with the previous day status (if any)
                    replyMessage = string.Format(_stringConstant.GoodDay, slackUserDetailAc.UserId) + FetchPreviousDayStatus(firstUser.Id, project.Id, questionList) + question.QuestionStatement + Environment.NewLine;
                }
                else
                    //no active users are found
                    replyMessage = _stringConstant.NoEmployeeFound;
            }
            else if (scrumStatus == ScrumStatus.OnGoing)
            {
                Scrum scrum = await GetScrumAsync(projectId);
                SlackUserDetailAc prevUserAc = new SlackUserDetailAc();
                if (scrum != null)
                    //user to whom the last question was asked
                    prevUserAc = await GetSlackUserAsync(scrum.Id, users);
                else
                    return _stringConstant.ErrorMsgNewPrivateChannel;

                if (!string.IsNullOrEmpty(prevUserAc?.Name))
                {
                    if (prevUserAc.Deleted)
                        //user is not part of the project in OAuth
                        replyMessage = string.Format(_stringConstant.UserNotInProject, prevUserAc.UserId);

                    else if (!prevUserAc.IsActive)
                        //reply to the user to whom the last question was asked. but this user is in active now
                        replyMessage = string.Format(_stringConstant.InActiveInOAuth, prevUserAc.UserId);

                }
                //if scrum meeting was interrupted. "scrum time" is written to resume scrum meeting. So next question is fetched.
                replyMessage += await GetQuestionAsync(scrum.Id, questionList, users, project.Id);
            }
            else
                //for all other statuses.
                return ReplyStatusofScrumToClient(scrumStatus);
            return replyMessage;
        }


        /// <summary>
        /// This method is used to mark a user's answer on leave. - JJ
        /// </summary>
        /// <param name="users">List of users of the project(in OAuth) corresponding to slack channel</param>
        /// <param name="scrumId">id of scrum of the channel for that day</param>
        /// <param name="questions">List of questions to be asked in scrum</param>
        /// <param name="projectId">Id of project(in OAuth) corresponding to slack channel</param>
        /// <param name="slackUserId">slack userId of the interacting user</param>
        /// <param name="applicantId">slack user id of the user who is being marked on leave</param>
        /// <returns>Question to the next user or status of the request</returns>
        private async Task<string> MarkLeaveAsync(List<User> users, int scrumId, List<Question> questions, int projectId, string slackUserId, string applicantId)
        {
            string returnMsg = string.Empty;
            User user = users.FirstOrDefault(x => x.SlackUserId == applicantId);
            if (user != null)
            {
                if (user.IsActive)
                {
                    //checks whether the applicant is the expected user
                    string status = await ExpectedUserAsync(scrumId, questions, users, applicantId, projectId);
                    //if the interacting user is the expected user
                    if (string.IsNullOrEmpty(status))
                    {
                        //if applying user tries to mark himself/herself as on leave
                        if (String.Compare(slackUserId, applicantId, StringComparison.OrdinalIgnoreCase) == 0)
                            return _stringConstant.LeaveError;

                        string expectedUserId = user.Id;
                        //fetch the scrum answer of the user given on that day
                        IEnumerable<ScrumAnswer> scrumAnswer = await _scrumAnswerDataRepository.FetchAsync(x => x.ScrumId == scrumId && x.EmployeeId == expectedUserId && x.ScrumAnswerStatus == ScrumAnswerStatus.Answered);
                        //If no answer from the user has been obtained yet.
                        if (!scrumAnswer.Any())
                        {
                            //all the scrum questions are answered as "leave"
                            foreach (Question question in questions)
                            {
                                AddAnswer(scrumId, question.Id, expectedUserId, _stringConstant.Leave, ScrumAnswerStatus.Leave);
                            }
                            await _scrumAnswerDataRepository.SaveChangesAsync();
                            await UpdateTemporaryScrumDetailsAsync(applicantId, scrumId, users, null);
                        }
                        else
                            //If the applicant has already answered questions
                            returnMsg = string.Format(_stringConstant.AlreadyAnswered, applicantId);
                    }
                    else
                        return status;
                }
                else
                    return await GetReplyToUserAsync(users, projectId, scrumId, applicantId, questions);
            }
            else
                returnMsg = string.Format(_stringConstant.UserNotInProject, applicantId);
            //fetches the next question or status and returns
            return returnMsg + await GetQuestionAsync(scrumId, questions, users, projectId);
        }


        /// <summary>
        /// Used to fetch the next question based on the given parameters. JJ
        /// </summary>
        /// <param name="scrumId">id of scrum of the channel for that day</param>
        /// <param name="questions">List of questions to be asked in scrum</param>
        /// <param name="users">List of users of the project(in OAuth) corresponding to slack channel</param>
        /// <param name="projectId">Id of project(in OAuth) corresponding to slack channel</param>
        /// <returns>The next question or the scrum complete message</returns>
        private async Task<string> GetQuestionAsync(int scrumId, List<Question> questions, List<User> users, int projectId)
        {
            List<ScrumAnswer> scrumAnswers = _scrumAnswerDataRepository.FetchAsync(x => x.ScrumId == scrumId).Result.ToList();
            User user = new User();
            TemporaryScrumDetails temporaryScrumDetails = await FetchTemporaryScrumDetailsAsync(scrumId);
            if (temporaryScrumDetails != null)
            {
                //user to whom the last question was asked
                User prevUser = users.FirstOrDefault(x => x.SlackUserId == temporaryScrumDetails.SlackUserId && x.IsActive);
                //list of active users who have not answered yet  
                List<string> slackUserIdList = await _slackUserDetailsDataRepository.GetAll().Select(x => x.UserId).ToListAsync();
                List<User> activeUnAnsweredUserList = users.Where(x => x.IsActive && slackUserIdList.Contains(x.SlackUserId) && !scrumAnswers.Select(y => y.EmployeeId).ToList().Contains(x.Id)).ToList();
                _logger.Debug("Unanswered user list" + JsonConvert.SerializeObject(activeUnAnsweredUserList));
                if (scrumAnswers.Any())
                {
                    int questionCount = questions.Count();
                    //all questions have been asked to the previous user      
                    if (temporaryScrumDetails.AnswerCount == 0 || temporaryScrumDetails.AnswerCount == questionCount)
                    {
                        user = activeUnAnsweredUserList.FirstOrDefault();
                        if (prevUser != null)
                            user = activeUnAnsweredUserList.FirstOrDefault(x => x.SlackUserId == prevUser.SlackUserId);
                        //temporaryScrumDetails.AnswerCount == questionCount - because if the previous user has answered all
                        //his questions then next user must be asked question
                        if (temporaryScrumDetails.AnswerCount == questionCount || user == null)
                            user = activeUnAnsweredUserList.FirstOrDefault();
                    }
                    else
                    {
                        //as not all questions have been answered by the last user,the next question to that user will be asked
                        if (prevUser != null)
                        {
                            SlackUserDetailAc slackUserDetailAc = await _slackUserDetailRepository.GetByIdAsync(prevUser.SlackUserId);
                            if (slackUserDetailAc != null)
                            {
                                //last scrum answer of the given scrum id.
                                ScrumAnswer lastScrumAnswer = scrumAnswers.OrderByDescending(x => x.Id).First(x => x.EmployeeId == prevUser.Id);
                                Question question = await FetchQuestionAsync(lastScrumAnswer.QuestionId);
                                if (question != null)
                                {
                                    await UpdateTemporaryScrumDetailsAsync(prevUser.SlackUserId, scrumId, users, question.Id);
                                    return string.Format(_stringConstant.NameFormat, slackUserDetailAc.UserId) + question.QuestionStatement + Environment.NewLine;
                                }
                                return _stringConstant.NoQuestion;
                            }
                        }
                        user = activeUnAnsweredUserList.FirstOrDefault();
                    }
                }
                else
                    user = prevUser ?? activeUnAnsweredUserList.FirstOrDefault();  //preveUser == null, if a user was asked a question before but at present is not active
            }
            if (!string.IsNullOrEmpty(user?.SlackUserId))
            {
                SlackUserDetailAc slackUserAc = await _slackUserDetailRepository.GetByIdAsync(user.SlackUserId);
                Question firstQuestion = questions.First();
                //update the temporary scrum details with the next id of the question to be asked
                await UpdateTemporaryScrumDetailsAsync(user.SlackUserId, scrumId, users, firstQuestion.Id);
                //as it is the first question to the user also fetch the previous day scrum status.
                return string.Format(_stringConstant.GoodDay, slackUserAc.UserId) + FetchPreviousDayStatus(user.Id, projectId, questions) + firstQuestion.QuestionStatement + Environment.NewLine;
            }
            return await MarkScrumCompleteAsync(scrumId, users, questions.Count());
        }


        /// <summary>
        /// Used to check and mark scrum as completed. - JJ
        /// </summary>
        /// <param name="scrumId">id of scrum of the channel for that day</param>
        /// <param name="users">list of users of the project(in OAuth) corresponding to slack channel</param>
        /// <param name="questionCount">number of questions asked during scrum to a user</param>
        /// <returns>reply to user</returns>
        /// <remarks>If scrum is completed then message saying that the scrum is complete 
        /// or if any active emplpoyee is pending to answer then that question</remarks>
        private async Task<string> MarkScrumCompleteAsync(int scrumId, List<User> users, int questionCount)
        {
            //list of scrum answers of the given scrumId            
            List<ScrumAnswer> scrumAnswers = _scrumAnswerDataRepository.Fetch(x => x.ScrumId == scrumId).OrderBy(x => x.Id).ToList();
            User user = new User();
            Question question = new Question();
            SlackUserDetailAc slackUserDetailAc = new SlackUserDetailAc();
            string nextQuestion = string.Empty;

            var scrumAnswersInComplete = scrumAnswers.GroupBy(m => m.EmployeeId)
                .Select(g => new
                {
                    AnswerCount = g.Count(),
                    g.First().EmployeeId,
                    Answers = g
                }).ToList();

            if (scrumAnswersInComplete.Any(x => x.AnswerCount < questionCount))
            {
                var userIdObjects = scrumAnswersInComplete.FindAll(x => x.AnswerCount < questionCount);
                foreach (var userIdObject in userIdObjects)
                {
                    user = users.FirstOrDefault(x => x.Id == userIdObject.EmployeeId && x.IsActive);
                    //check whether those who didn't answer now are active or not
                    if (!string.IsNullOrEmpty(user?.Id))
                    {
                        slackUserDetailAc = await _slackUserDetailRepository.GetByIdAsync(user.SlackUserId);
                        if (slackUserDetailAc != null)
                        {
                            question = await FetchQuestionAsync(userIdObject.Answers.OrderByDescending(x => x.Id).First().QuestionId);
                            nextQuestion = question != null ? question.QuestionStatement : string.Empty;
                            break;
                        }
                    }
                }
            }
            //if the nextQuestion is fetched then it means that there are questions to be asked to user
            if (!string.IsNullOrEmpty(nextQuestion))
            {
                await UpdateTemporaryScrumDetailsAsync(user.SlackUserId, scrumId, users, question.Id);
                return string.Format(_stringConstant.MarkedInActive, slackUserDetailAc.UserId) + nextQuestion;
            }
            //if no questions are pending then scrum is marked to be complete
            if (await UpdateScrumAsync(scrumId, false, false) == 1)
                //answers of all the users has been recorded            
                return _stringConstant.ScrumComplete;
            return _stringConstant.ErrorMsg;
        }


        /// <summary>
        /// Update scrum status to not in progress scrum. JJ
        /// </summary>
        /// <param name="scrumId">id of scrum of the channel for that day</param>
        /// <param name="isHalted">bit indicating whether scrum is halted</param>
        /// <param name="isOngoing">bit indicating whether scrum is in progress</param>
        /// <returns>1 if successfully updated</returns>
        private async Task<int> UpdateScrumAsync(int scrumId, bool isOngoing, bool isHalted)
        {
            if (!isOngoing)
                await RemoveTemporaryScrumDetailsAsync(scrumId);
            Scrum scrum = await _scrumDataRepository.FirstAsync(x => x.Id == scrumId);
            _logger.Info(scrum?.ScrumDate);
            scrum.IsOngoing = isOngoing;
            scrum.IsHalted = isHalted;
            _scrumDataRepository.Update(scrum);
            return await _scrumDataRepository.SaveChangesAsync();
        }


        /// <summary>
        /// Used to check whether the applicant is the expected user. - JJ
        /// </summary>
        /// <param name="scrumId">id of scrum of the channel for that day</param>
        /// <param name="questions">List of questions to be asked in scrum</param>
        /// <param name="users">List of users of the project(in OAuth) corresponding to slack channel</param>
        /// <param name="applicantId">slack user id</param>
        /// <param name="projectId">Id of project(in OAuth) corresponding to slack channel</param>
        /// <returns>empty string if the expected user is same as the applicant</returns>
        private async Task<string> ExpectedUserAsync(int scrumId, List<Question> questions, List<User> users, string applicantId, int projectId)
        {
            //List of scrum answer of the given scrumId.
            List<ScrumAnswer> scrumAnswer = _scrumAnswerDataRepository.FetchAsync(x => x.ScrumId == scrumId).Result.ToList();
            User user;
            TemporaryScrumDetails temporaryScrumDetails = await FetchTemporaryScrumDetailsAsync(scrumId);
            //list of user ids who have not answer yet and are still active                     

            User prevUser = users.FirstOrDefault(x => x.SlackUserId == temporaryScrumDetails.SlackUserId);
            if (prevUser == null || !prevUser.IsActive)//the previous user is either in-active or not a member of the project in OAuth

            {// the next user is chosen from the list of users who have not answer yet and are still active                 
                // could be null too    
                _logger.Debug("ExpectedUserAsync, list of users" + JsonConvert.SerializeObject(users));
                user = users.FirstOrDefault(x => x.IsActive && !scrumAnswer.Select(y => y.EmployeeId).ToList().Contains(x.Id));
            }
            else
                user = prevUser;
            if (user != null)
                _logger.Debug("ExpectedUserAsync, user found" + JsonConvert.SerializeObject(user));
            return await ProcessExpectedUserResultAsync(user, applicantId, users, projectId, scrumId, questions);
        }


        /// <summary>
        /// Gets the appropraite reply to the next user. JJ
        /// </summary>
        /// <param name="users">List of users of the project(in OAuth) corresponding to slack channel</param>
        /// <param name="projectId">Id of project(in OAuth) corresponding to slack channel</param>
        /// <param name="scrumId">id of scrum of the channel for that day</param>
        /// <param name="applicantId">slack user id</param>
        /// <param name="questions">List of questions to be asked in scrum</param>
        /// <returns>reply to the question, next question or the scrum status</returns>
        private async Task<string> GetReplyToUserAsync(List<User> users, int projectId, int scrumId, string applicantId, List<Question> questions)
        {
            bool fetchQuestion = false;
            User unexpectedUser = users.FirstOrDefault(x => x.SlackUserId == applicantId);
            //the user to whom the last question was asked. This user must be called before GetQuestionAsync() is called because if scrum is complete then temporary data is deleted and this user cannot be fetched.
            SlackUserDetailAc prevUser = await GetSlackUserAsync(scrumId, users);
            string reply = await GetQuestionAsync(scrumId, questions, users, projectId);
            if (unexpectedUser != null && !unexpectedUser.IsActive)
            {
                fetchQuestion = true;
                reply = string.Format(_stringConstant.InActiveInOAuth, applicantId) + reply;
            }
            bool isPreviousUserNull = string.IsNullOrEmpty(prevUser?.Name);
            //if unexpectedUser is null it means that the user is not a member of the project in OAuth
            //in that case even the user who user who was asked the last question to(i.e prevUser) is same as this user, it is alright
            if (!isPreviousUserNull && (prevUser.UserId != applicantId || unexpectedUser == null))
            {
                //the user who user who was asked the last question to(i.e prevUser) is not a member of the project in OAuth
                if (prevUser.Deleted)
                {
                    fetchQuestion = true;
                    reply = string.Format(_stringConstant.UserNotInProject, prevUser.UserId) + reply;
                }
                else if (!prevUser.IsActive)
                {
                    fetchQuestion = true;
                    reply = string.Format(_stringConstant.InActiveInOAuth, prevUser.UserId) + reply;
                }
            }

            //issue is : scrum starts and first question is asked to first user. Remove first and second users from project.Let second user write scrum halt. Third user is asked question. Second user writes scrum halt again.
            //when the unexpectedUser user is null(user is not a member of project or not in OAuth) and previous user is not the interacting user right now
            //or when both unexpectedUser user and previous users are null.
            if ((unexpectedUser == null && !isPreviousUserNull && prevUser.UserId != applicantId) || (unexpectedUser == null && isPreviousUserNull))
                return string.Format(_stringConstant.UserNotInProject, applicantId) + reply;

            if (fetchQuestion)
                return reply;
            return string.Empty;
        }


        /// <summary>
        /// Check whether the given user can answer now. - JJ
        /// </summary>
        /// <param name="user">User who is expected to interact</param>
        /// <param name="applicantId">slack user id</param>
        /// <param name="users">List of users of the project(in OAuth) corresponding to slack channel</param>
        /// <param name="projectId">Id of project(in OAuth) corresponding to slack channel</param>
        /// <param name="scrumId">id of scrum of the channel for that day</param>
        /// <param name="questions">List of questions to be asked in scrum</param>
        /// <returns>status</returns>
        private async Task<string> ProcessExpectedUserResultAsync(User user, string applicantId, List<User> users, int projectId, int scrumId, List<Question> questions)
        {
            //the expected user and the interacting user are same and is active
            if (user?.SlackUserId == applicantId)
            {
                TemporaryScrumDetails tempScrumDetails = await FetchTemporaryScrumDetailsAsync(scrumId);
                //the expected interacting user is not the user to whom the last question was asked
                if (tempScrumDetails.SlackUserId != applicantId)
                {
                    //last question was asked to this user.
                    SlackUserDetailAc tempSlackUser = await _slackUserDetailRepository.GetByIdAsync(tempScrumDetails.SlackUserId);
                    if (tempSlackUser != null)
                    {
                        User userDetail = users.FirstOrDefault(x => x.SlackUserId == tempScrumDetails.SlackUserId);

                        if (userDetail == null)
                            // User is either not a member of the project or not in OAuth
                            return string.Format(_stringConstant.UserNotInProject, tempSlackUser.UserId) + await GetQuestionAsync(scrumId, questions, users, projectId);
                        if (!userDetail.IsActive)
                            return string.Format(_stringConstant.InActiveInOAuth, tempSlackUser.UserId) + await GetQuestionAsync(scrumId, questions, users, projectId);
                    }
                    else
                        return _stringConstant.UserNotInSlack + await GetQuestionAsync(scrumId, questions, users, projectId);
                }
                return string.Empty;
            }

            string reply = await GetReplyToUserAsync(users, projectId, scrumId, applicantId, questions);
            if (user != null)
            {
                SlackUserDetailAc expectedSlackUserAc = await _slackUserDetailRepository.GetByIdAsync(user.SlackUserId);
                if (expectedSlackUserAc != null)
                {
                    if (!user.IsActive)
                        //the expected user is marked as in-active in OAuth. So mark the answers as in active and fetch question to the next user
                        return string.Format(_stringConstant.InActiveInOAuth, expectedSlackUserAc.UserId) + await GetQuestionAsync(scrumId, questions, users, projectId);
                    //expected user is active
                    reply += string.Format(_stringConstant.PleaseAnswer, expectedSlackUserAc.UserId);
                }
                else
                    return _stringConstant.UserNotInSlack + await GetQuestionAsync(scrumId, questions, users, projectId);
            }
            else
            {
                if (string.IsNullOrEmpty(reply))//when scrum concludes
                    reply = await GetQuestionAsync(scrumId, questions, users, projectId);
            }
            return reply;
        }


        /// <summary>
        /// This method fetches the Question of next order of the given questionId - JJ
        /// </summary>
        /// <param name="questionId">Id of question to be fetched</param>
        /// <returns>object of Question</returns>
        private async Task<Question> FetchQuestionAsync(int questionId)
        {
            Question question = await _questionDataRepository.FirstOrDefaultAsync(x => x.Id == questionId);
            if (question != null)
            {
                //order number of the given question 
                int orderNumber = (int)question.OrderNumber;
                //question with the next order
                question = await _questionDataRepository.FirstOrDefaultAsync(x => x.OrderNumber == (QuestionOrder)(orderNumber + 1) && x.Type == BotQuestionType.Scrum);
            }
            return question;
        }


        /// <summary>
        /// Fetches the previous day's questions and answers of the user of the given id for the given project - JJ
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <param name="projectId">Id of project(in OAuth) corresponding to slack channel</param>
        /// <param name="questions">List of questions of scrum </param>
        /// <returns>previous day status</returns>
        private string FetchPreviousDayStatus(string userId, int projectId, List<Question> questions)
        {
            string previousDayStatus = string.Empty;
            DateTime date = DateTime.UtcNow.Date;
            //previous scrums' Ids of this channel(project)
            List<int> scrumIdList = _scrumDataRepository.FetchAsync(x => x.ProjectId == projectId
            && DbFunctions.TruncateTime(x.ScrumDate) < date).Result.Select(x => x.Id).ToList();
            //answers in which user was not on leave.
            List<ScrumAnswer> scrumAnswers = _scrumAnswerDataRepository.FetchAsync(x => scrumIdList.Contains(x.ScrumId) && x.EmployeeId == userId && x.ScrumAnswerStatus == ScrumAnswerStatus.Answered).Result.OrderByDescending(x => x.AnswerDate).ToList();
            if (scrumAnswers.Any() && questions.Any())
            {
                DateTime scrumDate = new DateTime();
                foreach (Question question in questions)
                {
                    //Question and the corresponding answer appended
                    ScrumAnswer scrumAnswer = scrumAnswers.FirstOrDefault(x => x.QuestionId == question.Id);
                    if (scrumAnswer != null)
                    {
                        if (string.IsNullOrEmpty(previousDayStatus))
                        {
                            scrumDate = scrumAnswer.AnswerDate;
                            previousDayStatus = Environment.NewLine + string.Format(_stringConstant.PreviousDayStatus, scrumAnswer.AnswerDate.ToShortDateString()) + Environment.NewLine;
                        }
                        if (scrumDate.Date == scrumAnswer.AnswerDate.Date)
                            previousDayStatus += string.Format(_stringConstant.PreviousDayScrumAnswer, question.QuestionStatement, scrumAnswer.Answer);
                    }
                }
            }
            if (!string.IsNullOrEmpty(previousDayStatus))
                return previousDayStatus + Environment.NewLine + _stringConstant.AnswerToday + Environment.NewLine + Environment.NewLine;

            return previousDayStatus;
        }


        /// <summary>
        /// Fetch the status of the scrum - JJ
        /// </summary>
        /// <param name="project">project(in OAuth) corresponding to slack channel</param>
        /// <param name="users">List of users of the project(in OAuth) corresponding to slack channel</param>
        /// <param name="questions">List of questions to be asked in scrum</param>
        /// <returns>object of ScrumStatus</returns>
        private async Task<ScrumStatus> FetchScrumStatusAsync(ProjectAc project, List<User> users, List<Question> questions)
        {
            if (project?.Id > 0)
            {
                if (project.IsActive)
                {
                    if (users != null && users.Any())
                    {
                        if (questions == null || !questions.Any())
                            questions = await _botQuestionRepository.GetQuestionsByTypeAsync(BotQuestionType.Scrum);
                        if (questions.Any())
                        {
                            DateTime today = DateTime.UtcNow.Date;
                            Scrum scrum = await _scrumDataRepository.FirstOrDefaultAsync(x => x.ProjectId == project.Id
                            && DbFunctions.TruncateTime(x.ScrumDate) == today);
                            if (scrum != null)
                            {
                                _logger.Info(scrum?.ScrumDate);
                                if (!scrum.IsHalted)
                                    return scrum.IsOngoing ? ScrumStatus.OnGoing : ScrumStatus.Completed;
                                //scrum is halted                              
                                return ScrumStatus.Halted;
                            }
                            // scrum not started
                            return ScrumStatus.NotStarted;
                        }
                        // no questions found 
                        return ScrumStatus.NoQuestion;
                    }
                    // no employees found in the project
                    return ScrumStatus.NoEmployee;
                }
                //  project is not active
                return ScrumStatus.InActiveProject;
            }
            //  no OAuth project found for this slack channel
            return ScrumStatus.NoProject;
        }


        /// <summary>
        /// Halt the scrum meeting - JJ
        /// </summary>
        /// <param name="scrum">scrum of the channel for that day</param>
        /// <param name="status">status of scrum</param>
        /// <returns>scrum halted message</returns>
        private async Task<string> ScrumHaltAsync(Scrum scrum, ScrumStatus status)
        {
            //keyword encountered is "scrum halt"
            if (status == (ScrumStatus.OnGoing))
            {
                //scrum halted
                await UpdateScrumAsync(scrum.Id, true, true);
                return _stringConstant.ScrumHalted;
            }
            if (status == (ScrumStatus.Halted))
                return _stringConstant.ScrumAlreadyHalted;
            return ReplyStatusofScrumToClient(status) + _stringConstant.ScrumCannotBeHalted;
        }


        /// <summary>
        /// Resume the scrum meeting - JJ
        /// </summary>
        /// <param name="scrum">scrum of the channel for that day</param>
        /// <param name="users">List of users of the project(in OAuth) corresponding to slack channel</param>
        /// <param name="status">status of scrum</param>
        /// <returns>scrum resume message along with the next question</returns>
        private async Task<string> ScrumResumeAsync(Scrum scrum, List<User> users, ScrumStatus status)
        {
            List<Question> questionList = await _botQuestionRepository.GetQuestionsByTypeAsync(BotQuestionType.Scrum);
            //keyword encountered is "scrum resume"      
            if (status == (ScrumStatus.Halted) || status == (ScrumStatus.OnGoing))
            {
                string returnMsg;
                if (status == (ScrumStatus.Halted))
                {
                    //scrum resumed
                    await UpdateScrumAsync(scrum.Id, true, false);
                    returnMsg = _stringConstant.ScrumResumed;
                }
                else
                    returnMsg = _stringConstant.ScrumNotHalted;

                //user to whom the last question was asked
                SlackUserDetailAc prevUser = await GetSlackUserAsync(scrum.Id, users);
                if (!string.IsNullOrEmpty(prevUser?.Name))
                {
                    if (prevUser.Deleted)//the previous user is not part of the project in OAuth
                        returnMsg += string.Format(_stringConstant.UserNotInProject, prevUser.UserId);

                    else if (!prevUser.IsActive)
                        returnMsg += string.Format(_stringConstant.InActiveInOAuth, prevUser.UserId);
                }
                //next question is fetched
                returnMsg += await GetQuestionAsync(scrum.Id, questionList, users, scrum.ProjectId);
                return returnMsg;
            }
            return ReplyStatusofScrumToClient(status) + _stringConstant.ScrumCannotBeResumed;
        }


        /// <summary>
        /// Select the appropriate reply to the client - JJ
        /// </summary>
        /// <param name="scrumStatus">Status of the scrum</param>
        /// <returns>appropriate message indicating the status of scrum</returns>
        private string ReplyStatusofScrumToClient(ScrumStatus scrumStatus)
        {
            string returnMessage;
            switch (scrumStatus)
            {
                case ScrumStatus.Completed:
                    returnMessage = _stringConstant.ScrumAlreadyConducted;
                    break;
                case ScrumStatus.Halted:
                    returnMessage = _stringConstant.ScrumIsHalted;
                    break;
                case ScrumStatus.NoEmployee:
                    returnMessage = _stringConstant.NoEmployeeFound;
                    break;
                case ScrumStatus.NoProject:
                    returnMessage = _stringConstant.NoProjectFound;
                    break;
                case ScrumStatus.InActiveProject:
                    returnMessage = _stringConstant.ProjectInActive;
                    break;
                case ScrumStatus.NoQuestion:
                    returnMessage = _stringConstant.NoQuestion;
                    break;
                case ScrumStatus.NotStarted:
                    returnMessage = _stringConstant.ScrumNotStarted;
                    break;
                case ScrumStatus.OnGoing:
                    returnMessage = _stringConstant.ScrumInProgress;
                    break;
                default: return null;
            }
            return returnMessage;
        }


        /// <summary>
        /// Get users of the OAuth project corresponding to the given slackChannelName - JJ
        /// </summary>
        /// <param name="projectId">slack channel name from which the message has been send</param>
        /// <param name="accessToken">Access token of the interacting user</param>
        /// <returns>list of object of User</returns>
        private async Task<ProjectAc> GetOAuthProjectAsync(int projectId, string accessToken)
        {
            ProjectAc project = await _oauthCallsRepository.GetProjectDetailsAsync(projectId, accessToken);
            //Users of the OAuth project corresponding to the given slackChannelName
            List<User> users = project?.Users;
            if (users?.Count > 0)
            {
                var ids = users.Select(a => a.Id);
                //Application Users in Erp who are members of the OAuth project corresponding to the given slackChannelName 
                var appUsers = _applicationUser.FetchAsync(x => ids.Contains(x.Id)).Result
                    .Select(y => new { Id = y.Id, SlackUserId = y.SlackUserId }).ToList();
                //assign SlackUserId 
                users.ForEach(x =>
                {
                    x.SlackUserId = appUsers.FirstOrDefault(y => y.Id == x.Id)?.SlackUserId;
                });
                project.Users = users.Where(x => !string.IsNullOrEmpty(x.SlackUserId)).ToList();
            }
            return project;
        }


        #endregion    
    }
}