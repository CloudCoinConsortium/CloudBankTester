using Newtonsoft.Json;

namespace CloudBankTester
{
   

    public class BankKeys 
    {
        //Fields
        [JsonProperty("url")]
        public string url { get; set; }

        [JsonProperty("privatekey")]
        public string privatekey { get; set; }

        [JsonProperty("account")]
        public string account { get; set; }


        //Constructors
        public BankKeys()
        {

        }//end of constructor

        public BankKeys(string url, string privatekey, string account)
        {
            this.url = url;
            this.privatekey = privatekey;
            this.account = account;
        }//end of constructor


    }
}
