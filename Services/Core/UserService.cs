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
        Task<ResultModel> AddFromKafka(UserFromKafka model);
        Task<ResultModel> UpdateFromKafka(UserFromKafka model);
        Task<ResultModel> Get();
    }
    public class UserService : IUserService
	{
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        public UserService(AppDbContext dbContext, IMapper mapper)
		{
            _dbContext = dbContext;
            _mapper = mapper;
		}

        public async Task<ResultModel> AddFromKafka(UserFromKafka model)
        {
            var result = new ResultModel();
            result.Succeed = false;
            try
            {
                var user = _mapper.Map<UserFromKafka, User>(model);
                await _dbContext.Users.InsertOneAsync(user);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> UpdateFromKafka(UserFromKafka model)
        {
            var result = new ResultModel();
            result.Succeed = false;
            try
            {
                var user = _dbContext.Users.Find(_ => _.Id == model.Id).FirstOrDefault();
                if (user == null)
                {
                    user = _mapper.Map<UserFromKafka, User>(model);
                    await _dbContext.Users.InsertOneAsync(user);
                }else
                {
                    _dbContext.Users.FindOneAndReplace(_ => _.Id == user.Id, user);
                }
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
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
    }
}

