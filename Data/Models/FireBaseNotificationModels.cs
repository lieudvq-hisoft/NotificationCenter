using System;
using Data.Entities;
using Newtonsoft.Json;

namespace Data.Models
{
    public class NotificationFcmModel
    {
        public Guid UserId { get; set; }
        public string DeviceId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Notification Data { get; set; }
        public List<string> RegistrationIds { get; set; }
    }

    public class ResponseFcmModel
    {
        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class FcmNotificationSetting
    {
        public string SenderId { get; set; }
        public string ServerKey { get; set; }
    }

    public class GoogleNotification
    {
        public class DataPayload
        {
            public string title { get; set; }
            public string body { get; set; }
            public string sound { get; set; }
            public string click_action { get; set; } = "FLUTTER_NOTIFICATION_CLICK";
        }
        public List<string> registration_ids { get; set; }
        public string priority { get; set; } = "high";
        public object data { get; set; }
        public DataPayload notification { get; set; }
        public string click_action { get; set; } = "FLUTTER_NOTIFICATION_CLICK";
    }

    public class FcmResponse
    {
        public long MulticastId { get; set; }
        public int Success { get; set; }
        public int Failure { get; set; }
        public int CanonicalIds { get; set; }
        public List<FcmResult> Results { get; set; }
    }

    public class FcmResult
    {
        public string MessageId { get; set; }
        public string RegistrationId { get; set; }
        public string Error { get; set; }
    }
}

