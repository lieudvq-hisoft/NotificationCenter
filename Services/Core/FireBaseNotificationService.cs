using Microsoft.Extensions.Options;
using CorePush.Google;
using System.Text;
using Data.DataAccess;
using MongoDB.Driver;
using Data.Models;
using static Data.Models.GoogleNotification;
using FcmResponse = Data.Models.FcmResponse;

namespace Services.Core
{
    public interface IFireBaseNotificationService
    {
        Task<ResponseFcmModel> SendNotification(NotificationFcmModel notificationModel);
    }
    public class FireBaseNotificationService : IFireBaseNotificationService
    {
        private readonly AppDbContext _dbContext;
        private readonly FcmNotificationSetting _fcmNotificationSetting;
        private readonly string fcmServerKey;
        private readonly HttpClient httpClient;
        public FireBaseNotificationService(IOptions<FcmNotificationSetting> settings, AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _fcmNotificationSetting = settings.Value;
            fcmServerKey = fcmServerKey = _fcmNotificationSetting.ServerKey;
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://fcm.googleapis.com/fcm/");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={fcmServerKey}");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        }

        public async Task<ResponseFcmModel> SendNotification(NotificationFcmModel notificationModel)
        {
            {
                ResponseFcmModel result = new ResponseFcmModel();
                try
                {
                    FcmSettings settings = new FcmSettings()
                    {
                        SenderId = _fcmNotificationSetting.SenderId,
                        ServerKey = _fcmNotificationSetting.ServerKey
                    };
                    GoogleNotification notification = new GoogleNotification();
                    DataPayload dataPayload = new DataPayload();
                    dataPayload.title = notificationModel.Title;
                    dataPayload.body = notificationModel.Body;
                    dataPayload.sound = "default";

                    notification.registration_ids = notificationModel.RegistrationIds;
                    notification.data = notificationModel.Data;
                    notification.notification = dataPayload;

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(notification);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("send", content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        var fcmResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<FcmResponse>(responseContent);
                        if (fcmResponse.Failure != 0)
                        {
                            var user = _dbContext.Users.Find(_ => _.Id == notificationModel.UserId && !_.IsDeleted).FirstOrDefault();
                            for (int i = 0; i < fcmResponse.Results.Count; i++)
                            {
                                if (fcmResponse.Results[i].Error != null)
                                {
                                    if (user != null && user.FcmTokens.Contains(notificationModel.RegistrationIds[i]))
                                    {
                                        user.FcmTokens.Remove(notificationModel.RegistrationIds[i]);
                                        _dbContext.Users.ReplaceOne(_ => _.Id == user.Id, user);
                                    }
                                }
                            }

                        }
                        result.IsSuccess = true;
                        result.Message = responseContent;
                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = responseContent;
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.Message = "Something went wrong";
                    return result;
                }
            }
        }
    }
}

