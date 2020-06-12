using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CloudBankTester
{
    

    public class CloudBankUtils 
    {
        //Fields
        private BankKeys keys;
        private string rawStackForDeposit;
        private string rawStackFromWithdrawal;
        private string rawReceipt;
        private HttpClient cli;
        private string receiptNumber;
        private int totalCoinsWithdrawn;

        public bool haveKeys { get; private set; }
        public bool haveStackFromWithdrawal { get; private set; }
        public bool haveReceipt { get; private set; }
        public int onesInBank { get; private set; }
        public int fivesInBank { get; private set; }
        public int twentyFivesInBank { get; private set; }
        public int hundredsInBank { get; private set; }
        public int twohundredfiftiesInBank { get; private set; }


        //Constructor

        public CloudBankUtils( BankKeys startKeys ) {
            keys = startKeys;
            haveKeys = false;
            if (startKeys != null)
                haveKeys = true;
            cli = new HttpClient();
            totalCoinsWithdrawn = 0;
            onesInBank = 0;
            fivesInBank = 0;
            twentyFivesInBank = 0;
            hundredsInBank = 0;
            twohundredfiftiesInBank = 0;
        }//end constructor

        public CloudBankUtils(BankKeys startKeys, HttpClient client)
        {
            keys = startKeys;
            haveKeys = false;
            if (startKeys != null)
                haveKeys = true;
            cli = client;
            totalCoinsWithdrawn = 0;
            onesInBank = 0;
            fivesInBank = 0;
            twentyFivesInBank = 0;
            hundredsInBank = 0;
            twohundredfiftiesInBank = 0;
            haveReceipt = false;
        }//end constructor


        //Methods

        ///<summary>Calls the CloudService's show_coins service for the server that this object holds the keys for.</summary>
        //The results are saved in this class's public properties if successful.
        public async Task showCoins()
        {
            if(!haveKeys)
            {
                Console.Out.WriteLine("Keys not found");
                return;
            }
            //the private key is sent as form url encoded content
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("pk", keys.privatekey), new KeyValuePair<string, string>("account", keys.account) });
            string json = "error";
            try
            {
                var showCoins = await cli.PostAsync("https://"+keys.url+"/service/show_coins", formContent);
                json = await showCoins.Content.ReadAsStringAsync();
                var bankTotals = JsonConvert.DeserializeObject<BankTotal>(json);
                if (bankTotals.status == "coins_shown")
                {
                    onesInBank = bankTotals.ones;
                    fivesInBank = bankTotals.fives;
                    twentyFivesInBank = bankTotals.twentyfives;
                    hundredsInBank = bankTotals.hundreds;
                    twohundredfiftiesInBank = bankTotals.twohundredfifties;
                }
                else
                {
                    Console.Out.WriteLine(bankTotals.status);
                    var failResponse = JsonConvert.DeserializeObject<BaseResponse>(json);
                    Console.Out.WriteLine(failResponse.message);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("InnerException: " + ex.InnerException.Message);
                Console.Out.WriteLine("Check your connection, or your public key, or if the url you are trying to connect to is secure.");
                return;
            }//end try catch
            catch (JsonSerializationException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(json);
            }
            catch (JsonReaderException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(json);
            }
        }//end show coins

        public async Task echoFromBank()
        {
            if (!haveKeys)
            {
                Console.Out.WriteLine("Keys not found");
                return;
            }
            //the private key is sent as form url encoded content
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("pk", keys.privatekey), new KeyValuePair<string, string>("account", keys.account) });
            string json = "error";
            try
            {
                var echo = await cli.GetAsync("https://" + keys.url + "/service/echo");
                json = await echo.Content.ReadAsStringAsync();
                var r = JsonConvert.DeserializeObject<EchoResponse>(json);
                if (r.status == "ready")
                {
                    Console.Out.WriteLine(r.message);
                }
                else
                {
                    Console.Out.WriteLine(r.status);
                    var failResponse = JsonConvert.DeserializeObject<BaseResponse>(json);
                    Console.Out.WriteLine(failResponse.message);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("InnerException: " + ex.InnerException.Message);
                Console.Out.WriteLine("Check your connection, or your public key, or if the url you are trying to connect to is secure.");
                return;
            }//end try catch
            catch (JsonSerializationException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(json);
            }
            catch (JsonReaderException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(json);
            }
        }//end echo 

        public async Task printWelcomeFromBank()
        {
            if (!haveKeys)
            {
                Console.Out.WriteLine("Keys not found");
                return;
            }
            //the private key is sent as form url encoded content
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("pk", keys.privatekey), new KeyValuePair<string, string>("account", keys.account) });
            string json = "error";
            try
            {
                var welcome = await cli.GetAsync("https://"+keys.url+"/service/print_welcome");
                json = await welcome.Content.ReadAsStringAsync();
                var r = JsonConvert.DeserializeObject<BaseResponse>(json);
                if (r.status == "ready")
                {
                    Console.Out.WriteLine(r.message);
                }
                else
                {
                    Console.Out.WriteLine(r.status);
                    var failResponse = JsonConvert.DeserializeObject<BaseResponse>(json);
                    Console.Out.WriteLine(failResponse.message);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("InnerException: " + ex.InnerException.Message);
                Console.Out.WriteLine("Check your connection, or your public key, or if the url you are trying to connect to is secure.");
                //throw ex;
                return;
            }//end try catch
            catch (JsonSerializationException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(json);
            }
            catch (JsonReaderException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(json);
            }
        }//end echo 

        ///<summary>Sets rawStackForDeposit to a CloudCoin stack read from a file</summary>
        ///<param name="filepath">The full filepath and filename of the CloudCoin stack that is being loaded</param> 
        public void loadStackFromFile(string filepath)
        {
            rawStackForDeposit = File.ReadAllText(filepath);
        }

        ///<summary>Sends the CloudCoin in rawStackForDeposit to the CloudService server that this object holds the keys for</summary>
        //loadStackFromFile needs to be called first
        public async Task sendStackToCloudBank()
        {
            if (!haveKeys)
            {
                Console.Out.WriteLine("Keys not found");
                return;
            }
            string CloudBankFeedback = "";
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("stack", rawStackForDeposit) , new KeyValuePair<string, string>("account", keys.account),
                new KeyValuePair<string, string>("memo", "SentFromTester") });
            try
            {
                var result_stack = await cli.PostAsync("https://" + keys.url + "/service/deposit_one_stack", formContent);
                CloudBankFeedback = await result_stack.Content.ReadAsStringAsync();
                var cbf = JsonConvert.DeserializeObject<BaseResponse>(CloudBankFeedback);
                Console.Out.WriteLine(cbf.message);
                receiptNumber = cbf.receipt;
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("InnerException: " + ex.InnerException.Message);
                Console.Out.WriteLine("Check your connection, or your public key, or if the url you are trying to connect to is secure.");
                return;
            }
            catch (JsonSerializationException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(CloudBankFeedback);
            }
            catch (JsonReaderException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(CloudBankFeedback);
            }

        }//End send stack

        ///<summary>Sends the CloudCoin in rawStackForDeposit to a CloudService server specified by parameter <paramref name="toPublicURL"/></summary>
        ///<param name="toPublicURL">The url of the CloudService server the CloudCoins are being sent to. Do not include "https://"</param>
        //loadStackFromFile needs to be called first
        public async Task sendStackToCloudBank(string toPublicURL)
        {
            if (!haveKeys)
            {
                Console.Out.WriteLine("Keys not found");
                return;
            }
            string CloudBankFeedback = "";
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("stack", rawStackForDeposit), new KeyValuePair<string, string>("account", keys.account),
                new KeyValuePair<string, string>("memo", "SentFromTester")});
            try
            {
                var result_stack = await cli.PostAsync("https://"+toPublicURL+"/service/deposit_one_stack", formContent);
                CloudBankFeedback = await result_stack.Content.ReadAsStringAsync();
                var cbf = JsonConvert.DeserializeObject<BaseResponse>(CloudBankFeedback);
                Console.Out.WriteLine(cbf.message);
                receiptNumber = cbf.receipt;
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("InnerException: " + ex.InnerException.Message);
                Console.Out.WriteLine("Check your connection, or your public key, or if the url you are trying to connect to is secure.");
                return;
            }
            catch (JsonSerializationException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(CloudBankFeedback);
            }
            catch (JsonReaderException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(CloudBankFeedback);
            }

        }//End send stack

        ///<summary>Retrieve the receipt generated by the CloudService for the last sendStackToCloudBank call</summary>
        //Requires sendStackToCloudBank to have been previously called
        //The retrieved receipt will be saved in rawReceipt
        public async Task getReceipt()
        {
            if (!haveKeys)
            {
                Console.Out.WriteLine("Keys not found");
                return;
            }
            try
            {
                var result_receipt = await cli.GetAsync("https://" + keys.url + "/service/get_receipt?rn="+ receiptNumber +"&account="+keys.account
                    +"&pk="+keys.privatekey);
                rawReceipt = await result_receipt.Content.ReadAsStringAsync();
                Receipt desReceipt = JsonConvert.DeserializeObject<Receipt>(rawReceipt);
                if (desReceipt.status == "fail" || desReceipt.status == "error")
                {
                    Console.Out.WriteLine(rawReceipt);
                }
                else
                {
                    Console.Out.WriteLine("Received Receipt");
                    haveReceipt = true;
                }
                

            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("InnerException: " + ex.InnerException.Message);
                Console.Out.WriteLine("Check your connection, your public key, or you may not have made a Deposit yet.");
                return;
            }
            
        }//End get Receipt


        ///<summary>Retrieves CloudCoins from CloudService server that this object holds the keys for.</summary>
        //The resulting stack that is retrieved is saved in rawStackFromWithdrawal
        ///<param name="amountToWithdraw">The amount of CloudCoins to withdraw</param>
        public async Task getStackFromCloudBank( int amountToWithdraw)
        {
            if (!haveKeys)
            {
                Console.Out.WriteLine("Keys not found");
                return;
            }
            totalCoinsWithdrawn = amountToWithdraw;
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string,string>("amount",amountToWithdraw.ToString()),
                new KeyValuePair<string, string>("pk", keys.privatekey), new KeyValuePair<string, string>("account", keys.account) ,new KeyValuePair<string, string>("memo", "Receive for tester")});
            try
            {
                var result_stack = await cli.PostAsync("https://"+keys.url+"/service/withdraw_one_stack", formContent);
                rawStackFromWithdrawal = await result_stack.Content.ReadAsStringAsync();
                var failResponse = JsonConvert.DeserializeObject<BaseResponse>(rawStackFromWithdrawal);
                if (failResponse.status != null)
                {
                    haveStackFromWithdrawal = false;
                    Console.Out.WriteLine(failResponse.status);
                    Console.Out.WriteLine(failResponse.message);
                }else
                {
                    haveStackFromWithdrawal = true;
                    Console.Out.WriteLine("Collected Stack from CloudBank");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("InnerException: " + ex.InnerException.Message);
                Console.Out.WriteLine("Check your connection, or your public key, or if the url you are trying to connect to is secure.");
                return;
            }
            catch (JsonReaderException ex)
            {
                Console.Out.WriteLine(rawStackFromWithdrawal);
            }
            catch (JsonSerializationException ex)
            {
                Console.Out.WriteLine(rawStackFromWithdrawal);
            }
        }//End get stack from cloudbank

        ///<summary>Calculate a CloudCoin note's denomination using it serial number(sn)</summary>
        private int getDenomination(int sn)
        {
            int nom = 0;
            if ((sn < 1))
            {
                nom = 0;
            }
            else if ((sn < 2097153))
            {
                nom = 1;
            }
            else if ((sn < 4194305))
            {
                nom = 5;
            }
            else if ((sn < 6291457))
            {
                nom = 25;
            }
            else if ((sn < 14680065))
            {
                nom = 100;
            }
            else if ((sn < 16777217))
            {
                nom = 250;
            }
            else
            {
                nom = '0';
            }

            return nom;
        }//end get denomination


        /// <summary>
        /// This allows the caller to send CloudCoins from the CloudBank to a Skywallet Account.The caller will specify the amount to be sent. The CloudBank will make change if necessary.
        /// </summary>
        public async Task SendToSkywallet(int amountToSend, string skywalletDestination)/////TODO: add the optional parameters
        {
            if (!haveKeys)
            {
                Console.Out.WriteLine("Keys not found");
                return;
            }
            string CloudBankFeedback = "";
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("pk", keys.privatekey), new KeyValuePair<string, string>("account", keys.account),
            new KeyValuePair<string,string>("amount", amountToSend.ToString()), new KeyValuePair<string,string>("to", skywalletDestination)});

            try
            {
                var result_stack = await cli.PostAsync("https://"+keys.url+"/service/send_to_skywallet", formContent);
                CloudBankFeedback = await result_stack.Content.ReadAsStringAsync();
                var cbf = JsonConvert.DeserializeObject<BaseResponse>(CloudBankFeedback);
                Console.Out.WriteLine(cbf.message);
                
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("InnerException: " + ex.InnerException.Message);
                Console.Out.WriteLine("Check your connection, or your public key, or if the url you are trying to connect to is secure.");
                return;
            }
            catch (JsonSerializationException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(CloudBankFeedback);
            }
            catch (JsonReaderException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(CloudBankFeedback);
            }
        }//end send skywallet

        /// <summary>
        /// Downloads all the coin in a Skywallet to a local wallet. This is will return that the process has started but not the results.
        /// </summary>
        public async Task RecieveFromSkywallet()/////TODO: add the optional parameters
        {
            if (!haveKeys)
            {
                Console.Out.WriteLine("Keys not found");
                return;
            }
            string CloudBankFeedback = "";
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("pk", keys.privatekey), new KeyValuePair<string, string>("account", keys.account)});

            try
            {
                var result_stack = await cli.PostAsync("https://" + keys.url + "/service/receive_from_skywallet", formContent);
                CloudBankFeedback = await result_stack.Content.ReadAsStringAsync();
                var cbf = JsonConvert.DeserializeObject<BaseResponse>(CloudBankFeedback);
                Console.Out.WriteLine(cbf.message);

            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("InnerException: " + ex.InnerException.Message);
                Console.Out.WriteLine("Check your connection, or your public key, or if the url you are trying to connect to is secure.");
                return;
            }
            catch (JsonSerializationException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(CloudBankFeedback);
            }
            catch (JsonReaderException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(CloudBankFeedback);
            }
        }//end receive from skywallet

        /// <summary>
        /// 
        /// </summary>
        public async Task TransferBetweenSkywallets(int amountToSend, string skywalletDestination)///////////////////////////
        {
            if (!haveKeys)
            {
                Console.Out.WriteLine("Keys not found");
                return;
            }
            string CloudBankFeedback = "";
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("pk", keys.privatekey), new KeyValuePair<string, string>("account", keys.account),
            new KeyValuePair<string,string>("amount", amountToSend.ToString()), new KeyValuePair<string,string>("to", skywalletDestination)});

            try
            {
                var result_stack = await cli.PostAsync("https://"+keys.url+"/service/transfer_between_skywallets", formContent);
                CloudBankFeedback = await result_stack.Content.ReadAsStringAsync();
                var cbf = JsonConvert.DeserializeObject<BaseResponse>(CloudBankFeedback);
                Console.Out.WriteLine(cbf.message);

            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("InnerException: " + ex.InnerException.Message);
                Console.Out.WriteLine("Check your connection, or your public key, or if the url you are trying to connect to is secure.");
                return;
            }
            catch (JsonSerializationException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(CloudBankFeedback);
            }
            catch (JsonReaderException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(CloudBankFeedback);
            }
        }//end send skywallet

        ///<summary>Parses pertinent information from the receipt last gathered by getReceipt and returns it in the form of an Interpretation object</summary>
        public Interpretation interpretReceipt()
        {
            Interpretation inter = new Interpretation();
            string interpretation = "";
            if (!haveReceipt)
            {
                interpretation = "Receipt not found. Have you run GetReceipt()?";
            }

            try
            {
                //tell the client how many coins were uploaded how many counterfeit, etc.
                var deserialReceipt = JsonConvert.DeserializeObject<Receipt>(rawReceipt);
                int totalNotes = deserialReceipt.total_authentic + deserialReceipt.total_fracked;
                int totalCoins = 0;
                for (int i = 0; i < deserialReceipt.receipt_detail.Length; i++)
                    if (deserialReceipt.receipt_detail[i].status == "authentic")
                        totalCoins += getDenomination(Int32.Parse(deserialReceipt.receipt_detail[i].nnsn.Split('.')[1]));
                interpretation = "receipt number: " + deserialReceipt.receipt_id + " total authentic notes: " + totalNotes + " total authentic coins: " + totalCoins;
                
                inter.receipt = deserialReceipt;
                inter.totalAuthenticCoins = totalCoins;
                inter.totalAuthenticNotes = totalNotes;

            }catch(JsonSerializationException ex)
            {
                Console.Out.WriteLine(ex.Message);
                interpretation = rawReceipt;
            }
            catch(JsonReaderException ex)
            {
                Console.Out.WriteLine(ex.Message);
                interpretation = rawReceipt;
            }
            catch(NullReferenceException ex)
            {
                Console.Out.WriteLine(ex.Message);
                interpretation = rawReceipt;
            }
            inter.interpretation = interpretation;
            return inter;
        }

        ///<summary>Writes a CloudCoin stack file for the CloudCoin retrieved the last call of either getStackFromCloudBank or getReceiptFromCloudBank</summary>
        ///<param name="path">The full file path where the new file will be written</param> 
        public void saveStackToFile(string path)
        {
            File.WriteAllText(path + getStackName(), rawStackFromWithdrawal);
            //WriteFile(path + stackName, rawStackFromWithdrawal);
        }

        ///<summary>Generates a filename for the CloudCoin stack file to be written by saveStackToFile</summary>
        public string getStackName()
        {
            if (receiptNumber == null)
            {
                DateTime date = DateTime.Now;
                string tag = "Withdrawal" + date.ToString("MMddyyyyhhmmsff");
                return totalCoinsWithdrawn + ".CloudCoin." + tag + ".stack";
            }
            return totalCoinsWithdrawn + ".CloudCoin." + receiptNumber + ".stack";
        }

        ///<summary>Calls getStackFromCloudBank and sendStackToCloudBank in order to transfer CloudCoins from one CloudService to another</summary>
        ///<param name="coinsToSend">The amount of CloudCoins to be transfered</param>
        ///<param name="toPublicKey"> The public url of the CloudService that is receiving the CloudCoins</param>
        public async Task transferCloudCoins( string toPublicKey, int coinsToSend) {
            //Download amount
            await getStackFromCloudBank(coinsToSend);
            rawStackForDeposit = rawStackFromWithdrawal;//Make it so it will send the stack it recieved
            await sendStackToCloudBank( toPublicKey);
            //Upload amount
        }//end transfer


    }//end class
}//end namespace
