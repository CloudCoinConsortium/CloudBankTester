using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CloudBankTester
{


    class EchoResponse : BaseResponse
    {

        [JsonProperty("readyCount")]
        public string readyCount { get; set; }

        [JsonProperty("notReadyCount")]
        public string notReadyCount { get; set; }

        public EchoResponse(){}
    }
}
