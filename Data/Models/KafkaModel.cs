using System;
namespace Data.Models
{
    public class KafkaModel
    {
        public List<Guid> UserReceiveNotice { get; set; }
        public object Payload { get; set; }
    }
}

