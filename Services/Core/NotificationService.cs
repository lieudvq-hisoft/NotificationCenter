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
        Task CreateReceipt(KafkaModel model);
        Task InventoryThresholdWarning(KafkaModel model);
        Task<ResultModel> Get(Guid userId);
        Task<ResultModel> GetById(Guid Id);
        Task<ResultModel> SeenNotify(Guid id, Guid userId);
        Task<ResultModel> DeleteNotify(Guid id, Guid userId);
    }
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly INotificationHub _notificationHub;
        private readonly IFireBaseNotificationService _fireBaseNotificationService;
        public NotificationService(AppDbContext dbContext, IMapper mapper, INotificationHub notificationHub, IFireBaseNotificationService fireBaseNotificationService)
		{
            _dbContext = dbContext;
            _mapper = mapper;
            _notificationHub = notificationHub;
            _fireBaseNotificationService = fireBaseNotificationService;
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

        public async Task CreateReceipt(KafkaModel kafkaModel)
        {
            try
            {
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                   var notification = new Notification
                    {
                       Title = "New Receipt",
                       Body = "There's a new receipt just created",
                       Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                       UserId = item,
                       TypeModel = "Receipt"
                   };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
        }

        public async Task InventoryThresholdWarning(KafkaModel kafkaModel)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload);
                var product = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductModel>(json);
                foreach (var item in kafkaModel.UserReceiveNotice)
                {
                    var notification = new Notification
                    {
                        Title = "Low Inventory Threshold Warning",
                        Body = product!.SerialNumber + ": " + product!.Name,
                        Data = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel.Payload),
                        UserId = item,
                        TypeModel = "Product"
                    };
                    SendNotifyFcm(item, notification, notification.Title, notification.Body);
                    await _dbContext.Notifications.InsertOneAsync(notification);
                    await _notificationHub.NewNotification(_mapper.Map<Notification, NotificationModel>(notification), notification.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
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

        public async Task<ResultModel> SeenNotify(Guid id, Guid userId)
        {
            var result = new ResultModel();
            try
            {
                var notification = _dbContext.Notifications.Find(_ => _.Id == id && _.UserId == userId && !_.IsDeleted).FirstOrDefault();
                if (notification == null)
                {
                    result.Succeed = false;
                    result.ErrorMessage = "Notification not found";
                    return result;
                }
                notification.Seen = true;
                _dbContext.Notifications.ReplaceOne(_ => _.Id == notification.Id, notification);
                result.Data = notification.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> DeleteNotify(Guid id, Guid userId)
        {
            var result = new ResultModel();
            try
            {
                var notification = _dbContext.Notifications.Find(_ => _.Id == id && _.UserId == userId && !_.IsDeleted).FirstOrDefault();
                if (notification == null)
                {
                    result.Succeed = false;
                    result.ErrorMessage = "Notification not found";
                    return result;
                }
                _dbContext.Notifications.DeleteOne(_ => _.Id == notification.Id);
                result.Data = notification.Id;
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
                    await _notificationHub.NewNotificationCount(userReceive.CurrenNoticeCount, userReceive.Id.ToString());
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

