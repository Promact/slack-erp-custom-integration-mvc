﻿using Promact.Core.Repository.AttachmentRepository;
using Promact.Core.Repository.HttpClientRepository;
using Promact.Core.Repository.ProjectUserCall;
using Promact.Erp.DomainModel.ApplicationClass;
using Promact.Erp.DomainModel.DataRepository;
using Promact.Erp.DomainModel.Models;
using Promact.Erp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Promact.Core.Repository.ScrumRepository
{
    public class ScrumBotRepository : IScrumBotRepository
    {

        #region Private Variable

        private readonly IRepository<ScrumAnswer> _scrumAnswerRepository;
        private readonly IRepository<Scrum> _scrumRepository;
        private readonly IRepository<ApplicationUser> _applicationUser;
        private readonly IRepository<Question> _questionRepository;
        private readonly IProjectUserCallRepository _projectUser;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IHttpClientRepository _httpClientRepository;
        #endregion


        #region Constructor

        public ScrumBotRepository(IRepository<ScrumAnswer> scrumAnswerRepository, IProjectUserCallRepository projectUser,
            IRepository<Scrum> scrumRepository, IAttachmentRepository attachmentRepository, IRepository<Question> questionRepository,
            IHttpClientRepository httpClientRepository, IRepository<ApplicationUser> applicationUser)
        {
            _scrumAnswerRepository = scrumAnswerRepository;
            _scrumRepository = scrumRepository;
            _questionRepository = questionRepository;
            _projectUser = projectUser;
            _applicationUser = applicationUser;
            _attachmentRepository = attachmentRepository;
            _httpClientRepository = httpClientRepository;
        }

        #endregion


        #region Public Methods   


        /// <summary>
        /// This method is called whenever a message other than "scrumn time" or "leave username" is written in the group during scrum meeting. - JJ
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Message"></param>
        /// <param name="GroupName"></param>
        /// <returns>The next Question Statement</returns>
        public async Task<string> AddScrumAnswer(string UserName, string Message, string GroupName)
        {
            try
            {
                List<Scrum> scrum = _scrumRepository.Fetch(x => x.GroupName.Equals(GroupName) && x.ScrumDate.Date == DateTime.Now.Date).ToList();
                string message = "";

                if (scrum.Any())
                {
                    // getting user name from user's slack name
                    var applicationUser = _applicationUser.FirstOrDefault(x => x.SlackUserName == UserName);
                    // getting access token for that user
                    var accessToken = await _attachmentRepository.AccessToken(applicationUser.UserName);

                    List<Question> questions = _questionRepository.Fetch(x => x.Type == 1).ToList();
                    int questionCount = questions.Count();
                    //employees of the given group name fetched from the oauth server
                    List<User> employees = await _projectUser.GetUsersByGroupName(GroupName, accessToken);
                    //scrum answer of that day's scrum
                    List<ScrumAnswer> scrumAnswer = _scrumAnswerRepository.Fetch(x => x.ScrumId == scrum.FirstOrDefault().Id).ToList();
                    if ((employees.Count() * questionCount) > scrumAnswer.Count())
                    {
                        Question firstQuestion = questions.OrderBy(x => x.OrderNumber).FirstOrDefault();
                        if (scrumAnswer.Any())
                        {
                            ScrumAnswer lastScrumAnswer = scrumAnswer.OrderByDescending(x => x.Id).FirstOrDefault();
                            int answerListCount = scrumAnswer.FindAll(x => x.EmployeeId == lastScrumAnswer.EmployeeId).Count();
                            string questionStatement = "";
                            if (answerListCount < questionCount)
                            {
                                Question prevQuestion = _questionRepository.FirstOrDefault(x => x.Id == lastScrumAnswer.QuestionId);
                                Question question = _questionRepository.FirstOrDefault(x => x.Type == 1 && x.OrderNumber == prevQuestion.OrderNumber + 1);
                                AddAnswer(lastScrumAnswer.ScrumId, question.Id, lastScrumAnswer.EmployeeId, Message);

                                if (questionCount == answerListCount + 1)
                                {
                                    //if the employee who answered now has answered all the questions
                                    List<string> list = scrumAnswer.Select(x => x.EmployeeId).ToList();
                                    List<string> idList = employees.Where(x => !scrumAnswer.Select(y => y.EmployeeId).ToList().Contains(x.Id)).Select(x => x.Id).ToList();
                                    if (idList.FirstOrDefault() != null)
                                    {
                                        //first question to the next employee
                                        User user = employees.FirstOrDefault(x => x.Id == idList.FirstOrDefault());
                                        questionStatement = StringConstant.GoodDay + "<@" + user.SlackUserName + ">!\n" + FetchQuestion(null, true);
                                    }
                                    else
                                        //all employees have been asked questions
                                        questionStatement = StringConstant.ScrumComplete;
                                }
                                else
                                {
                                    //next question to the same employee
                                    User user = employees.FirstOrDefault(x => x.Id == lastScrumAnswer.EmployeeId);
                                    questionStatement = "<@" + user.SlackUserName + "> " + FetchQuestion(question.Id, false);
                                }
                                message = questionStatement;
                            }
                            else
                            {
                                //A particular employee's first answer
                                User user = employees.Where(x => x.SlackUserName == UserName).FirstOrDefault();
                                AddAnswer(lastScrumAnswer.ScrumId, firstQuestion.Id, user.Id, Message);
                                message = "<@" + user.SlackUserName + "> " + FetchQuestion(firstQuestion.Id, false);
                            }
                        }
                        else
                        {
                            //First Employee's first answer
                            User user = employees.FirstOrDefault(x => x.SlackUserName == UserName);
                            AddAnswer(scrum.FirstOrDefault().Id, firstQuestion.Id, user.Id, Message);
                            message = "<@" + user.SlackUserName + "> " + FetchQuestion(firstQuestion.Id, false);
                        }
                    }
                }
                return message;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }


        /// <summary>
        /// This method will be called when the keyword "scrum time" is encountered
        /// </summary>
        /// <param name="GroupName"></param>
        /// <param name="UserName"></param>
        /// <returns>The next question or the scrum complete message</returns>
        public async Task<string> StartScrum(string GroupName, string UserName)
        {
            try
            {
                var scrumList = _scrumRepository.Fetch(x => x.GroupName.Equals(GroupName) && x.ScrumDate.Date == DateTime.Now.Date).ToList();
                string message = "";
                var applicationUser = _applicationUser.FirstOrDefault(x => x.SlackUserName == UserName);
                var accessToken = await _attachmentRepository.AccessToken(applicationUser.UserName);

                if (!(scrumList.Any()))
                {
                    ProjectAc project;
                    try
                    {
                        project = await _projectUser.GetProjectDetails(GroupName, accessToken);
                    }
                    catch (Exception ex)
                    {
                        //  return StringConstant.ServerClosed;
                        throw ex;
                    }
                    if (project != null && project.Id > 0)
                    {
                        List<User> employees;
                        try
                        {
                            employees = await _projectUser.GetUsersByGroupName(GroupName, accessToken);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        if (employees.Count != 0)
                        {
                            Scrum scrum = new Scrum();
                            scrum.CreatedOn = DateTime.UtcNow;
                            scrum.GroupName = GroupName;
                            scrum.ScrumDate = DateTime.UtcNow.Date;
                            scrum.ProjectId = project.Id;
                            scrum.TeamLeaderId = project.TeamLeaderId;
                            _scrumRepository.Insert(scrum);
                            _scrumRepository.Save();

                            User firstEmployee = employees.FirstOrDefault();
                            Question question = _questionRepository.Fetch(x => x.Type == 1).OrderBy(x => x.OrderNumber).FirstOrDefault();
                            if (question != null)
                                message = StringConstant.GoodDay + "<@" + firstEmployee.SlackUserName + ">!\n" + question.QuestionStatement;
                            else
                                message = StringConstant.NoQuestion;
                        }
                        else
                            message = StringConstant.NoEmployeeFound;
                    }
                    else
                        message = StringConstant.NoProjectFound;
                }
                else
                    //if scrum meeting was interrupted. "scrum time" is written to resume scrum meeting. So next question is fetched.
                    message = await GetQuestion(scrumList.FirstOrDefault().Id, GroupName, accessToken, null, null);
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
                //return String.Empty;
            }
        }


        /// <summary>
        /// This method will be called when the keyword "scrum time" is encountered
        /// </summary>
        /// <param name="GroupName"></param>
        /// <param name="UserName"></param>
        /// <returns>The next question or the scrum complete message</returns>
        public async Task<string> StartScrumTest(string GroupName, string UserName)
        {
            try
            {
                var scrumList = _scrumRepository.Fetch(x => x.GroupName.Equals(GroupName) && x.ScrumDate.Date == DateTime.Now.Date).ToList();
                string message = "";
                var applicationUser = _applicationUser.FirstOrDefault(x => x.SlackUserName == UserName);
                var accessToken = await _attachmentRepository.AccessToken(applicationUser.UserName);

                if (!(scrumList.Any()))
                {
                    ProjectAc project;
                    try
                    {
                        project = await _projectUser.GetProjectDetails(GroupName, accessToken);
                    }
                    catch (Exception ex)
                    {
                        //  return StringConstant.ServerClosed;
                        throw ex;
                    }
                    return StringConstant.ScrumHelp;
                }
                else
                    //if scrum meeting was interrupted. "scrum time" is written to resume scrum meeting. So next question is fetched.
                    message = await GetQuestion(scrumList.FirstOrDefault().Id, GroupName, accessToken, null, null);
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
                //return String.Empty;
            }
        }



        /// <summary>
        /// This method will be called when the keyword "leave" is received as reply of a group member. - JJ
        /// </summary>
        /// <param name="GroupName"></param>
        /// <param name="LeaveApplicant"></param>
        /// <param name="UserName"></param>
        /// <returns>Question to the next person or other scrum status</returns>
        public async Task<string> Leave(string GroupName, string UserName, string LeaveApplicant)
        {
            try
            {
                var returnMsg = "";
                var scrum = _scrumRepository.Fetch(x => x.GroupName.Equals(GroupName) && x.ScrumDate.Date == DateTime.Now.Date).FirstOrDefault();
                if (scrum != null)
                {
                    // getting user name from user's slack name
                    var applicationUser = _applicationUser.FirstOrDefault(x => x.SlackUserName == UserName);
                    // getting access token for that user
                    var accessToken = await _attachmentRepository.AccessToken(applicationUser.UserName);

                    List<ScrumAnswer> scrumAnswer = _scrumAnswerRepository.Fetch(x => x.ScrumId == scrum.Id).ToList();
                    List<Question> questions = _questionRepository.Fetch(x => x.Type == 1).OrderBy(x => x.OrderNumber).ToList();
                    List<User> employees = await _projectUser.GetUsersByGroupName(GroupName, accessToken);
                    if ((questions.Count * employees.Count) == scrumAnswer.Count)
                        //if scrum of all the employees for that day is already recorded
                        returnMsg = StringConstant.ScrumAlreadyConducted;
                    else
                    {
                        var employee = employees.FirstOrDefault(x => x.SlackUserName == LeaveApplicant);
                        if (employee == null)
                            return "Sorry." + LeaveApplicant + " is not a member of this project.";
                        else
                        {
                            //all the scrum questions are answered as "leave"
                            foreach (var question in questions)
                            {
                                AddAnswer(scrum.Id, question.Id, employee.Id, StringConstant.Leave);
                            }
                        }
                        returnMsg = await GetQuestion(scrum.Id, GroupName, accessToken, questions, employees);
                    }
                }
                else
                    returnMsg = StringConstant.ScrumNotStarted;
                return returnMsg;
            }
            catch (Exception)
            {
                return StringConstant.ServerClosed;
            }
        }


        /// <summary>
        /// This method is used to add Scrum answer to the database
        /// </summary>
        /// <param name="ScrumID"></param>
        /// <param name="QuestionId"></param>
        /// <param name="EmployeeId"></param>
        /// <param name="Message"></param>
        /// <returns>true if scrum answer is added successfully</returns>
        public bool AddAnswer(int ScrumID, int QuestionId, string EmployeeId, string Message)
        {
            try
            {
                var answer = new ScrumAnswer();
                answer.Answer = Message;
                answer.AnswerDate = DateTime.UtcNow;
                answer.CreatedOn = DateTime.UtcNow;
                answer.EmployeeId = EmployeeId;
                answer.QuestionId = QuestionId;
                answer.ScrumId = ScrumID;
                _scrumAnswerRepository.Insert(answer);
                _scrumAnswerRepository.Save();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Used to fetch the next question based on the given parameters
        /// </summary>
        /// <param name="ScrumId"></param>
        /// <param name="GroupName"></param>
        /// <param name="accessToken"></param>
        /// <param name="employees"></param>
        /// <param name="questions"></param>
        /// <param name="scrumAnswer"></param>
        /// <returns>The next question or the scrum complete message</returns>
        private async Task<string> GetQuestion(int ScrumId, string GroupName, string accessToken, List<Question> questions, List<User> employees)
        {
            try
            {
                string returnMsg = StringConstant.NoEmployeeFound;
                List<ScrumAnswer> scrumAnswer = _scrumAnswerRepository.Fetch(x => x.ScrumId == ScrumId).ToList();
                if (questions == null || employees == null)
                {
                    questions = _questionRepository.Fetch(x => x.Type == 1).OrderBy(x => x.OrderNumber).ToList();
                    employees = await _projectUser.GetUsersByGroupName(GroupName, accessToken);
                }
                if (employees.Count > 0)
                {
                    if (questions.Count > 0)
                    {
                        if (scrumAnswer.Any())
                        {
                            int questionCount = questions.Count();
                            //last acrum answer of the given scrum id.
                            ScrumAnswer lastScrumAnswer = scrumAnswer.OrderByDescending(x => x.Id).FirstOrDefault();
                            //no. of answers given by the employee who gave the last scrum answer.
                            int answerListCount = scrumAnswer.FindAll(x => x.EmployeeId == lastScrumAnswer.EmployeeId).Count();

                            if (answerListCount == questionCount)
                            {
                                //all questions have been asked to the previous employee 
                                //now fetch the first question to the next employee
                                var list = scrumAnswer.Select(x => x.EmployeeId).ToList();
                                var idList = employees.Where(x => !scrumAnswer.Select(y => y.EmployeeId).ToList().Contains(x.Id)).Select(x => x.Id).ToList();
                                if (idList != null && idList.Count > 0)
                                {
                                    User user = employees.FirstOrDefault(x => x.Id == idList.FirstOrDefault());
                                    returnMsg = StringConstant.GoodDay + "<@" + user.SlackUserName + ">!\n" + FetchQuestion(null, true);
                                }
                                else
                                    //answers of all the employees has been recorded
                                    returnMsg = StringConstant.ScrumComplete;
                            }
                            else
                            {
                                //as not all questions have been answered by the last employee,the next question to that employee will be asked
                                User user = employees.FirstOrDefault(x => x.Id == lastScrumAnswer.EmployeeId);
                                returnMsg = "<@" + user.SlackUserName + "> " + FetchQuestion(lastScrumAnswer.QuestionId, false);
                            }
                        }
                        else
                            //no scrum answer has been recorded yet. So first question to the first employee
                            returnMsg = StringConstant.GoodDay + "<@" + employees.FirstOrDefault().SlackUserName + ">!\n" + questions.FirstOrDefault().QuestionStatement;
                    }
                    else
                        returnMsg = StringConstant.NoQuestion;
                }
                return returnMsg;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }


        /// <summary>
        /// This method fetches the Question statement of next question of the given questionId
        /// </summary>
        /// <param name="QuestionId"></param>
        /// <param name="isFirstQuestion"></param>
        /// <returns></returns>
        private string FetchQuestion(int? QuestionId, bool isFirstQuestion)
        {
            try
            {
                if (isFirstQuestion)
                {
                    var question = _questionRepository.Fetch(x => x.Type == 1).OrderBy(x => x.OrderNumber).FirstOrDefault();
                    return question.QuestionStatement;
                }
                else
                {
                    var orderNumber = _questionRepository.FirstOrDefault(x => x.Id == QuestionId).OrderNumber;
                    var question = _questionRepository.FirstOrDefault(x => x.OrderNumber == orderNumber + 1 && x.Type == 1);
                    if (question != null)
                        return question.QuestionStatement;
                    else
                        return String.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}
