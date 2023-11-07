using System;
using AutoMapper;
using Data.DataAccess;
using Data.Entities;
using Data.Models;
using MongoDB.Driver;

namespace Services.Core
{
    public interface INotificationService
    {
        Task<ResultModel> Add(NotificationCreateModel model);
    }
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        public NotificationService(AppDbContext dbContext, IMapper mapper)
		{
            _dbContext = dbContext;
            _mapper = mapper;
		}

        public async Task<ResultModel> Add(NotificationCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var notify = _mapper.Map<NotificationCreateModel, Notification>(model);
                await _dbContext.Notifications.InsertOneAsync(notify);
                var list = _dbContext.Notifications.Find(_ => !_.IsDeleted).ToList();
                result.Data = list;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }
    }
}

