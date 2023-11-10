using AutoMapper;
using Data.DataAccess;
using Data.Entities;
using Data.Models;
using MongoDB.Driver;
using Services.SignalR;

namespace Services.Core
{
    public interface INotificationService
    {
        Task Add(Notification model);
        Task<ResultModel> Get(Guid userId);
        Task<ResultModel> GetById(Guid Id);
    }
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly INotificationHub _notificationHub;
        public NotificationService(AppDbContext dbContext, IMapper mapper, INotificationHub notificationHub)
		{
            _dbContext = dbContext;
            _mapper = mapper;
            _notificationHub = notificationHub;
		}

        public async Task Add(Notification notification)
        {
            try
            {
                await _dbContext.Notifications.InsertOneAsync(notification);
                await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());

            }
            catch (Exception e)
            {
                var r = e;
            }
        }

        public async Task<ResultModel> Get(Guid userId)
        {
            var result = new ResultModel();
            result.Succeed = false;
            try
            {
                var notifications = _dbContext.Notifications.Find(_ => _.UserId == userId &&  !_.IsDeleted)
                     .ToList()
                     .OrderByDescending(_n => _n.DateCreated).ToList();
                result.Succeed = true;
                result.Data = _mapper.Map<List<Notification>, List<NotificationModel>>(notifications);
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> GetById(Guid id)
        {
            var result = new ResultModel();
            result.Succeed = false;
            try
            {
                var notification = _dbContext.Notifications.Find(_ => _.Id == id && !_.IsDeleted).FirstOrDefault();
                var viewData = _mapper.Map<NotificationModel>(notification);
                //var dataDeserial = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(viewData.Data!.ToString()!);
                //viewData.Data = dataDeserial;
                result.Succeed = true;
                result.Data = viewData;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

    }
}

