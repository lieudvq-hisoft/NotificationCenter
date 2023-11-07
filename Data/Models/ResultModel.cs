using System;
namespace Data.Models
{
    public class ResultModel
    {
        public string ErrorMessage { get; set; }
        public object Data { get; set; }
        public bool Succeed { get; set; }
        public int Code { get; set; }
    }
}

