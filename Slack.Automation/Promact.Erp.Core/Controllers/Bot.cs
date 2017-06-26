﻿using Autofac;
using NLog;
using Promact.Core.Repository.LeaveManagementBotRepository;
using Promact.Core.Repository.ScrumRepository;
using Promact.Core.Repository.SlackUserRepository;
using Promact.Core.Repository.TaskMailRepository;
using Promact.Erp.Util.EnvironmentVariableRepository;
using Promact.Erp.Util.ExceptionHandler;
using Promact.Erp.Util.StringLiteral;
using SlackAPI;
using SlackAPI.WebSocketMessages;
using System;
using System.Net.Http;
using System.Threading.Tasks;


namespace Promact.Erp.Core.Controllers
{
    public class Bot
    {
        #region Private Variables
        private readonly ITaskMailRepository _taskMailRepository;
        private readonly ISlackUserRepository _slackUserDetailsRepository;
        private readonly ILogger _scrumlogger;
        private readonly ILogger _logger;
        private readonly AppStringLiteral _stringConstant;
        private readonly IEnvironmentVariableRepository _environmentVariableRepository;
        private static string _scrumBotId;
        private readonly IComponentContext _component;
        private ILeaveManagementBotRepository _leaveManagementBotRepository;
        #endregion

        #region Constructor
        public Bot(ITaskMailRepository taskMailRepository, ISlackUserRepository slackUserDetailsRepository,
            ISingletonStringLiteral stringConstant, IEnvironmentVariableRepository environmentVariableRepository,
            IComponentContext component)
        {
            _taskMailRepository = taskMailRepository;
            _slackUserDetailsRepository = slackUserDetailsRepository;
            _logger = LogManager.GetLogger("TaskBotModule");
            _scrumlogger = LogManager.GetLogger("ScrumBotModule");
            _stringConstant = stringConstant.StringConstant;
            _environmentVariableRepository = environmentVariableRepository;
            _component = component;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Used to connect task mail bot and to capture task mail
        /// </summary>
        public void TaskMailBot()
        {
            _logger.Info("TaskMailAccessToken : " + _environmentVariableRepository.TaskmailAccessToken);
            SlackSocketClient client = new SlackSocketClient(_environmentVariableRepository.TaskmailAccessToken);
            // assigning bot token on Slack Socket Client
            // Creating a Action<MessageReceived> for Slack Socket Client to get connect. No use in task mail bot
            MessageReceived messageReceive = new MessageReceived();
            messageReceive.ok = true;
            Action<MessageReceived> showMethod = (MessageReceived messageReceived) => new MessageReceived();
            // Telling Slack Socket Client to the bot whose access token was given early
            client.Connect((connected) => { });

            _logger.Info("Task mail bot connected");
            // Method will hit when someone send some text in task mail bot
            client.OnMessageReceived += async (message) =>
            {
                string replyText = string.Empty;
                var user = await _slackUserDetailsRepository.GetByIdAsync(message.user);
                try
                {
                    _logger.Info("Task mail bot receive message : " + message.text);
                    if (user != null)
                    {
                        _logger.Info("User : " + user.Name);
                        if (message.text.ToLower() == _stringConstant.TaskMailSubject.ToLower())
                        {
                            _logger.Info("Task Mail process start - StartTaskMailAsync");
                            replyText = await _taskMailRepository.StartTaskMailAsync(user.UserId);
                            _logger.Info("Task Mail process done : " + replyText);
                        }
                        else
                        {
                            _logger.Info("Task Mail process start - QuestionAndAnswerAsync");
                            replyText = await _taskMailRepository.QuestionAndAnswerAsync(message.text, user.UserId);
                            _logger.Info("Task Mail process done : " + replyText);
                        }
                    }
                    else
                    {
                        replyText = _stringConstant.NoSlackDetails;
                        _logger.Info("User is null : " + replyText);
                    }
                }
                catch (SessionExpiredException)
                {
                    _logger.Info(user.Name + "- session expired.");
                    replyText = _stringConstant.SessionExpiredMessage;
                }
                catch (Exception ex)
                {
                    _logger.Error(_stringConstant.LoggerErrorMessageTaskMailBot + _stringConstant.Space + ex.Message +
                        Environment.NewLine + ex.StackTrace);
                    replyText = _stringConstant.ExceptionMessageBugCreate;
                }
                // Method to send back response to task mail bot
                client.SendMessage(showMethod, message.channel, replyText);
                _logger.Info("Reply message sended");
            };
        }

        /// <summary>
        /// Used for Scrum meeting bot connection and to conduct scrum meeting. - JJ 
        /// </summary>
        public void Scrum()
        {
            SlackSocketClient client = new SlackSocketClient(_environmentVariableRepository.ScrumBotToken);//scrumBot

            // Creating a Action<MessageReceived> for Slack Socket Client to get connected.
            MessageReceived messageReceive = new MessageReceived();
            messageReceive.ok = true;
            Action<MessageReceived> showMethod = (MessageReceived messageReceived) => new MessageReceived();
            //Connecting the bot of the given token 
            client.Connect((connected) =>
            {
                _scrumBotId = connected.self.id;
            });

            // Method will be called when someone sends message
            client.OnMessageReceived += (message) =>
            {
                _scrumlogger.Debug("Scrum bot got message :" + message);
                string replyText = string.Empty;
                try
                {
                    IScrumBotRepository scrumBotRepository = _component.Resolve<IScrumBotRepository>();
                    _scrumlogger.Debug("Scrum bot got message : " + message.text + " From user : " + message.user + " Of channel : " + message.channel);
                    replyText = scrumBotRepository.ProcessMessagesAsync(message.user, message.channel, message.text, _scrumBotId).Result;
                    _scrumlogger.Debug("Scrum bot got reply : " + replyText + " To user : " + message.user + " Of channel : " + message.channel);
                }
                catch (TaskCanceledException ex)
                {
                    replyText = _stringConstant.ExceptionMessageBugCreate;
                    _scrumlogger.Trace(ex.StackTrace);
                    _scrumlogger.Error("\n" + _stringConstant.LoggerScrumBot + " OAuth Server Not Responding " + ex.InnerException);
                    replyText = _stringConstant.ExceptionMessageBugCreate;
                }
                catch (HttpRequestException ex)
                {
                    replyText = _stringConstant.ExceptionMessageBugCreate;
                    _scrumlogger.Trace(ex.StackTrace);
                    _scrumlogger.Error("\n" + _stringConstant.LoggerScrumBot + " OAuth Server Closed \nInner exception :\n" + ex.InnerException);
                    replyText = _stringConstant.ExceptionMessageBugCreate;
                }
                catch (SessionExpiredException)
                {
                    replyText = _stringConstant.SessionExpiredMessage;
                }
                catch (Exception ex)
                {
                    replyText = _stringConstant.ExceptionMessageBugCreate;
                    _scrumlogger.Trace(ex.StackTrace);
                    _scrumlogger.Error("\n" + _stringConstant.LoggerScrumBot + " Generic exception \nMessage : \n" + ex.Message + "\nInner exception :\n" + ex.InnerException);
                }
                if (!string.IsNullOrEmpty(replyText))
                {
                    _scrumlogger.Debug("Scrum bot sending reply");
                    client.SendMessage(showMethod, message.channel, replyText);
                    _scrumlogger.Debug("Scrum bot sent reply");
                }
            };
        }

        /// <summary>
        /// Used to connect leave management bot & send and recieve - SS
        /// </summary>
        public void LeaveManagement()
        {
            SlackSocketClient client = new SlackSocketClient(_environmentVariableRepository.LeaveManagementBotAccessToken);
            // assigning bot token on Slack Socket Client
            // Creating a Action<MessageReceived> for Slack Socket Client to get connect. No use in task mail bot
            MessageReceived messageReceive = new MessageReceived();
            messageReceive.ok = true;
            Action<MessageReceived> showMethod = (MessageReceived messageReceived) => new MessageReceived();
            // Telling Slack Socket Client to the bot whose access token was given early
            client.Connect((connected) => { });
            // Method will hit when someone send some text in task mail bot
            client.OnMessageReceived += async (message) =>
            {
                string replyText = string.Empty;
                try
                {
                    _leaveManagementBotRepository = _component.Resolve<ILeaveManagementBotRepository>();
                    bool errorInUserConversion;
                    message.text = _leaveManagementBotRepository.ProcessToConvertSlackUserRegexIdToSlackId(message.text, out errorInUserConversion);
                    if (!errorInUserConversion)
                    {
                        var user = _slackUserDetailsRepository.GetByIdAsync(message.user).Result;
                        var text = message.text.ToLower();
                        if (user != null)
                            replyText = await _leaveManagementBotRepository.ProcessLeaveAsync(user.UserId, text);
                        else
                            replyText = _stringConstant.NoSlackDetails;
                    }
                    else
                        replyText = message.text;
                }
                catch (SessionExpiredException)
                {
                    _logger.Info("session expired.");
                    replyText = _stringConstant.SessionExpiredMessage;
                }
                catch (Exception ex)
                {
                    _logger.Error(_stringConstant.LoggerErrorMessageTaskMailBot + _stringConstant.Space + ex.Message +
                        Environment.NewLine + ex.StackTrace);
                    replyText = _stringConstant.ExceptionMessageBugCreate;
                }
                // Method to send back response to task mail bot
                client.SendMessage(showMethod, message.channel, replyText);
            };
        }
        #endregion
    }
}
