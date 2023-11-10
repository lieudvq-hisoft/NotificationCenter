using System;
namespace Data.Models
{
    public class UserFromKafka
    {
        public Guid Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string? Address { get; set; }
    }

    public class UserModel : UserFromKafka
    {
        public int CurrenNoticeCount { get; set; }
        public List<string> FcmTokens { get; set; }
    }

    public class BindFcmtokenModel
    {
        public string Fcmtoken { get; set; }
    }

    public class DeleteFcmtokenModel
    {
        public string Fcmtoken { get; set; }
        public string UserId { get; set; }
    }
}

