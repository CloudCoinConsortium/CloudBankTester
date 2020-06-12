using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudBankTester
{
    public class BaseResponse : IBankResponse
    {
        [JsonProperty("bank_server")]
        public string bank_server { get; set; }

        [JsonProperty("server")]
        private string server { set { bank_server = value; } }

        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("version")]
        public string version { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }

        [JsonProperty("time")]
        public string time { get; set; }

        [JsonProperty("account")]
        public string account { get; set; }

        [JsonProperty("receipt")]
        public string receipt { get; set; }

        [JsonProperty("reciept")]
        private string reciept { set { receipt = value; } }


        public BaseResponse(){}
    }
}
