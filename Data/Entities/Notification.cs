using System;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
//using MongoDB.Bson.Serialization.Attributes;

namespace Data.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public bool Seen { get; set; } = false;
        public string Action { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public object Data { get; set; }
        public string TypeModel { get; set; }
    }
}