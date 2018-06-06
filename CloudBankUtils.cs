using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CloudBankTester
{
    

    class CloudBankUtils : ICloudBankUtils
    {
        //Fields
        private BankKeys keys;
        private string rawStackForDeposit;
        private string rawStackFromWithdrawal;
        private string rawReceipt;
        private HttpClient cli;
        private string receiptNumber;
        private int totalCoinsWithdrawn;
        public int onesInBank { get; private set; }
        public int fivesInBank { get; private set; }
        public int twentyFivesInBank { get; private set; }
        public int hundresInBank { get; private set; }
        public int twohundredfiftiesInBank { get; private set; }


        //Constructor

        public CloudBankUtils( BankKeys startKeys ) {
            keys = startKeys;
            cli = new HttpClient();
            totalCoinsWithdrawn = 0;
            onesInBank = 0;
            fivesInBank = 0;
            twentyFivesInBank = 0;
            hundresInBank = 0;
            twohundredfiftiesInBank = 0;
        }//end constructor


        //Methods
        public async Task showCoins()
        {
            Console.Out.WriteLine("https://" + keys.publickey + "/show_coins?k=" + keys.privatekey);
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("pk", keys.privatekey) });
            string json = "error";
            try
            {
                var showCoins = await cli.PostAsync("https://" + keys.publickey + "/show_coins.aspx", formContent);
                json = await showCoins.Content.ReadAsStringAsync();
                var bankTotals = JsonConvert.DeserializeObject<BankTotal>(json);
                if (bankTotals.status == "coins_shown")
                {
                    onesInBank = bankTotals.ones;
                    fivesInBank = bankTotals.fives;
                    twentyFivesInBank = bankTotals.twentyfives;
                    hundresInBank = bankTotals.hundreds;
                    twohundredfiftiesInBank = bankTotals.twohundredfifties;
                }
                else
                {
                    Console.Out.WriteLine(bankTotals.status);
                    var failResponse = JsonConvert.DeserializeObject<FailResponse>(json);
                    Console.Out.WriteLine(failResponse.message);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("Check your connection, or your public key");
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


        public void loadStackFromFile(string filepath)
        {
            rawStackForDeposit = File.ReadAllText(filepath);
        }

        public async Task sendStackToCloudBank( string toPublicURL)
        {
            string CloudBankFeedback = "";
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("stack", rawStackForDeposit) });
            Console.Out.WriteLine("CloudBank request: " + toPublicURL + "/deposit_one_stack");
            Console.Out.WriteLine("Stack File: " + rawStackForDeposit);
            try
            {
                var result_stack = await cli.PostAsync("https://"+toPublicURL + "/deposit_one_stack.aspx", formContent);
                CloudBankFeedback = await result_stack.Content.ReadAsStringAsync();
                var cbf = JsonConvert.DeserializeObject<DepositResponse>(CloudBankFeedback);
                Console.Out.WriteLine(cbf.message);
                receiptNumber = cbf.receipt;
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("Check your connection, or your public key");
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




        public async Task getReceipt(string toPublicURL)
        {
            Console.Out.WriteLine("Geting Receipt: " + "https://" + toPublicURL + "/" + keys.privatekey + "/Receipts/" + receiptNumber + ".json");
            try
            {
                var result_receipt = await cli.GetAsync("https://" + toPublicURL + "/" + keys.privatekey + "/Receipts/" + receiptNumber + ".json");
                rawReceipt = await result_receipt.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("Check your connection, your public key, or you may not have made a Deposit yet.");
                return;
            }
            
        }//End get Receipt

        public async Task getStackFromCloudBank( int amountToWithdraw)
        {
            totalCoinsWithdrawn = amountToWithdraw;
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string,string>("amount",amountToWithdraw.ToString()),
                new KeyValuePair<string, string>("pk", keys.privatekey)});
            try
            {
                var result_stack = await cli.PostAsync("https://" + keys.publickey + "/withdraw_account.aspx", formContent);
                rawStackFromWithdrawal = await result_stack.Content.ReadAsStringAsync();
                var failResponse = JsonConvert.DeserializeObject<FailResponse>(rawStackFromWithdrawal);
                Console.Out.WriteLine(failResponse.status);
                Console.Out.WriteLine(failResponse.message);
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("Check your connection, or your public key");
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

        public async Task getReceiptFromCloudBank(string toPublicURL)
        {
            var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string,string>("rn",receiptNumber),
                new KeyValuePair<string, string>("pk", keys.privatekey)});
            try
            {
                var result_receipt = await cli.PostAsync("https://" + keys.publickey + "/get_receipt.aspx", formContent);
                string rawReceipt = await result_receipt.Content.ReadAsStringAsync();
                var deserialReceipt = JsonConvert.DeserializeObject<Receipt>(rawReceipt);
                for (int i = 0; i < deserialReceipt.rd.Length; i++)
                    if (deserialReceipt.rd[i].status == "authentic")
                        totalCoinsWithdrawn += getDenomination(deserialReceipt.rd[i].sn);
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("Check your connection, or your public key");
                return;
            }
            catch(JsonReaderException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(rawReceipt);
                return;
            }
            catch(JsonSerializationException ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(rawReceipt);
                return;
            }


            try
            {
                var formContent2 = new FormUrlEncodedContent(new[] { new KeyValuePair<string,string>("amount",totalCoinsWithdrawn.ToString()),
                new KeyValuePair<string, string>("pk", keys.privatekey)});
                var result_stack = await cli.PostAsync("https://" + keys.publickey + "/withdraw_account.aspx", formContent2);
                rawStackFromWithdrawal = await result_stack.Content.ReadAsStringAsync();
                var failResponse = JsonConvert.DeserializeObject<FailResponse>(rawStackFromWithdrawal);
                Console.Out.WriteLine(failResponse.status);
                Console.Out.WriteLine(failResponse.message);
            }
            catch (JsonReaderException ex)
            {
                Console.Out.WriteLine(rawStackFromWithdrawal);
            }
            catch (JsonSerializationException ex)
            {
                Console.Out.WriteLine(rawStackFromWithdrawal);
            }
            catch (HttpRequestException ex)
            {
                Console.Out.WriteLine("Exception: " + ex.Message);
                Console.Out.WriteLine("Check your connection, or your public key");
            }

        }

        public string interpretReceipt()
        {
            string interpretation = "";
            try
            {
                //tell the client how many coins were uploaded how many counterfeit, etc.
                var deserialReceipt = JsonConvert.DeserializeObject<Receipt>(rawReceipt);
                int totalNotes = deserialReceipt.total_authentic + deserialReceipt.total_fracked;
                int totalCoins = 0;
                for (int i = 0; i < deserialReceipt.rd.Length; i++)
                    if (deserialReceipt.rd[i].status == "authentic")
                        totalCoins += getDenomination(deserialReceipt.rd[i].sn);
                interpretation = "receipt number: " + deserialReceipt.receipt_id + " total authentic notes: " + totalNotes + " total authentic coins: " + totalCoins;

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
           
            return interpretation;
        }

        public void saveStackToFile(string path)
        {
            File.WriteAllText(path + getStackName(), rawStackFromWithdrawal);
            //WriteFile(path + stackName, rawStackFromWithdrawal);
        }

        public string getStackName()
        {
            return totalCoinsWithdrawn + ".CloudCoin." + receiptNumber + ".stack";
        }

        public async Task transferCloudCoins( string toPublicKey, int coinsToSend) {
            //Download amount
            await getStackFromCloudBank(coinsToSend);
            rawStackForDeposit = rawStackFromWithdrawal;//Make it so it will send the stack it recieved
            await sendStackToCloudBank( toPublicKey);
            //Upload amount
        }//end transfer


    }//end class
}//end namespace
