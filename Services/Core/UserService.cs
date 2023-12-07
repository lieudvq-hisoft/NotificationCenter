using System;
using AutoMapper;
using Data.DataAccess;
using Data.Entities;
using Data.Models;
using MongoDB.Driver;

namespace Services.Core
{
    public interface IUserService
    {
        Task AddFromKafka(UserFromKafka model);
        Task UpdateFromKafka(UserFromKafka model);
        Task<ResultModel> BindFcmtoken(BindFcmtokenModel model, Guid userId);
        Task<ResultModel> DeleteFcmToken(string fcmToken, Guid userId);
        Task<ResultModel> Profile(Guid userId);
        Task<ResultModel> Get();
        Task<ResultModel> SeenCurrenNoticeCount(Guid userId);
    }
    public class UserService : IUserService
	{
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IFireBaseNotificationService _fireBaseNotificationService;
        public UserService(AppDbContext dbContext, IMapper mapper, INotificationService notificationService, IFireBaseNotificationService fireBaseNotificationService)
		{
            _dbContext = dbContext;
            _mapper = mapper;
            _notificationService = notificationService;
            _fireBaseNotificationService = fireBaseNotificationService;
        }

        public async Task AddFromKafka(UserFromKafka model)
        {
            try
            {
                var user = _mapper.Map<UserFromKafka, User>(model);
                await _dbContext.Users.InsertOneAsync(user);
            }
            catch (Exception e)
            {
            }
        }

        public async Task UpdateFromKafka(UserFromKafka model)
        {
            try
            {
                var user = _dbContext.Users.Find(_ => _.Id == model.Id).FirstOrDefault();
                if (user == null)
                {
                    await _dbContext.Users.InsertOneAsync(_mapper.Map<UserFromKafka, User>(model));
                }else
                {
                    var updateDefs = new UpdateDefinition<User>[]
                        {

                        };

                    var update = Builders<User>.Update.Combine(
                            Builders<User>.Update.Set(_ => _.Address, model.Address),
                            Builders<User>.Update.Set(_ => _.FirstName, model.FirstName),
                            Builders<User>.Update.Set(_ => _.LastName, model.LastName),
                            Builders<User>.Update.Set(_ => _.PhoneNumber, model.PhoneNumber)
                            );
                    //var update = Builders<User>.Update.Set(_ => _.Address, model.Address);

                    _dbContext.Users.FindOneAndUpdate(_ => _.Id == user.Id, update);
                }
                var userResult = _dbContext.Users.Find(_ => _.Id == model.Id).FirstOrDefault();
                var notification = new Notification
                {
                    Title = "Update Profile",
                    Body = "Your personal information has been updated",
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(userResult),
                    UserId = userResult.Id
                };
                await _notificationService.Add(notification);
                SendNotifyFcm(userResult.Id, notification, notification.Title, notification.Body);
            }
            catch (Exception e)
            {
            }
        }

        public async Task<ResultModel> Get()
        {
            var result = new ResultModel();
            result.Succeed = false;
            try
            {
                var users = _dbContext.Users.Find(_ => !_.IsDeleted)
                     .ToList()
                     .OrderByDescending(_n => _n.DateCreated).ToList();
                result.Succeed = true;
                result.Data = _mapper.Map<List<User>, List<UserModel>>(users);
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> BindFcmtoken(BindFcmtokenModel model, Guid userId)
        {
            var result = new ResultModel();
            try
            {
                var user = _dbContext.Users.Find(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
                if(user == null)
                {
                    result.Succeed = false;
                    result.ErrorMessage = "User not found";
                    return result;
                }
                if (!user.FcmTokens.Contains(model.FcmToken))
                {
                    user.FcmTokens.Add(model.FcmToken);
                    user.DateUpdated = DateTime.Now;
                    _dbContext.Users.ReplaceOne(_ => _.Id == user.Id, user);
                }
                result.Data = model.FcmToken;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> Profile(Guid userId)
        {
            var result = new ResultModel();
            try
            {
                var user = _dbContext.Users.Find(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
                if (user == null)
                {
                    result.Succeed = false;
                    result.ErrorMessage = "User not found";
                    return result;
                }
                result.Data = _mapper.Map<User, UserModel>(user);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> DeleteFcmToken(string fcmToken, Guid userId)
        {
            var result = new ResultModel();
            try
            {
                var user = _dbContext.Users.Find(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
                if (user != null && user.FcmTokens.Contains(fcmToken))
                {
                    user.FcmTokens.Remove(fcmToken);
                    _dbContext.Users.ReplaceOne(_ => _.Id == user.Id, user);
                    result.Data = "Delete successful!";
                }
                if (result.Data == null)
                {
                    result.Data = "Delete failed!";
                }
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> SeenCurrenNoticeCount(Guid userId)
        {
            var result = new ResultModel();
            try
            {
                var user = _dbContext.Users.Find(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
                if (user == null)
                {
                    result.Succeed = false;
                    result.ErrorMessage = "User not found";
                    return result;
                }
                user.CurrenNoticeCount = 0;
                _dbContext.Users.ReplaceOne(_ => _.Id == user.Id, user);
                result.Data = user.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        private async void SendNotifyFcm(Guid userReceiveId, Notification data, string title, string body)
        {
            try
            {
                var userReceive = _dbContext.Users.Find(_ => _.Id == userReceiveId && !_.IsDeleted).FirstOrDefault();
                if (userReceive != null)
                {
                    userReceive.CurrenNoticeCount++;
                    _dbContext.Users.ReplaceOne(_ => _.Id == userReceiveId, userReceive);
                    if (userReceive != null && userReceive.FcmTokens.Count > 0)
                    {
                        var result = new NotificationFcmModel
                        {
                            Title = title,
                            Body = body,
                            Data = data,
                            RegistrationIds = userReceive.FcmTokens,
                            UserId = userReceiveId,
                        };
                        await _fireBaseNotificationService.SendNotification(result);
                    }
                }

            }
            catch (Exception ex)
            {

            }

        }


    }

}

