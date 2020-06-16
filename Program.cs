using System;
using banktesterforms;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace CloudBankTester
{
    class Program
    {

        /* INSTANCE VARIABLES */
        public static KeyboardReader reader = new KeyboardReader();
        //  public static String rootFolder = System.getProperty("user.dir") + File.separator +"bank" + File.separator ;
        public static String rootFolder = AppDomain.CurrentDomain.BaseDirectory;
        public static String prompt = "Start Mode> ";
        public static String[] commandsAvailable = new String[] { "Load Bank Keys", "Show Coins",
            "Deposite Coin", "Withdraw Coins","Check Receipt", "Write Check","Get Check",
            "Echo", "Get Print Welcome from Bank", "Send to a Skywallet from Bank", "Receive from Skywallet to Bank", "Transfer to another Skywallet", "quit" };
   //public static int timeout = 10000; // Milliseconds to wait until the request is ended. 
       // public static FileUtils fileUtils = new FileUtils(rootFolder, bank);
        public static Random myRandom = new Random();
        public static string url = "";
        public static string privateKey = "";
        public static string account = "";
        public static string sign = "Sean Worthington";
        public static BankKeys myKeys;
        private static CloudBankUtils receiptHolder;
        private static HttpClientHandler han;
        private static HttpClient cli;
        




        /* MAIN METHOD */
        public static void Main(String[] args)
        {
            han = new HttpClientHandler();
            han.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };

            cli = new HttpClient(han);

            printWelcome();
            run().Wait(); // Makes all commands available and loops
            Console.Out.WriteLine("Thank you for using CloudBank Tester. Goodbye.");
        } // End main

        /* STATIC METHODS */
        public static async Task run()
        {
            bool restart = false;
            while (!restart)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine("");
                //  Console.Out.WriteLine("========================================");
                Console.Out.WriteLine("");
                Console.Out.WriteLine("Commands Available:");
                Console.ForegroundColor = ConsoleColor.White;
                int commandCounter = 1;
                foreach (String command in commandsAvailable)
                {
                    Console.Out.WriteLine(commandCounter + (". " + command));
                    commandCounter++;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.Write(prompt);
                Console.ForegroundColor = ConsoleColor.White;
                int commandRecieved = reader.readInt(1,13);


                switch (commandRecieved)
                {
                    case 1:
                        loadKeys();
                        break;
                    case 2:
                        await showCoins();
                        break;
                    case 3:
                        receiptHolder = await depositAsync();
                        break;
                    case 4:
                        await withdraw();
                        break;
                    case 5:
                        await receipt();
                        break;
                    case 6:
                        await writeCheck();
                        break;
                    case 7:
                        await GetCheck();
                        break;
                    case 8:
                        await Echo();
                        break;
                    case 9:
                        await bankPrintWelcome();
                        break;
                    case 10:
                        receiptHolder = await sendtoSkywallet();
                        break;
                    case 11:
                        receiptHolder = await recieveFromSkywallet();
                        break;
                    case 12:
                        receiptHolder = await transferBetweenSkywallets();
                        break;
                    case 13:
                        Console.Out.WriteLine("Goodbye!");
                        Environment.Exit(0);
                        break;
                    default:
                        Console.Out.WriteLine("Command failed. Try again.");
                        break;
                }// end switch
            }// end while
        }// end run method

        

        private static async Task bankPrintWelcome()
        {
            CloudBankUtils cbu = new CloudBankUtils(myKeys, cli);
            await cbu.printWelcomeFromBank();
        }

        private static async Task Echo()
        {
            CloudBankUtils cbu = new CloudBankUtils(myKeys, cli);
            await cbu.echoFromBank();
        }

        public static void printWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Out.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.Out.WriteLine("║                   CloudBank Tester v.11.19.17                    ║");
            Console.Out.WriteLine("║          Used to Authenticate Test CloudBank                     ║");
            Console.Out.WriteLine("║      This Software is provided as is with all faults, defects    ║");
            Console.Out.WriteLine("║          and errors, and without warranty of any kind.           ║");
            Console.Out.WriteLine("║                Free from the CloudCoin Consortium.               ║");
            Console.Out.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.ForegroundColor = ConsoleColor.White;
        } // End print welcome



        static void loadKeys()
        {
            //insert own keys
            url = "";
            privateKey = "";
            account = "";
            Console.Out.WriteLine("Public key is " + url );
            Console.Out.WriteLine("Private key is " + privateKey);
            Console.Out.WriteLine("account is " + account);
            myKeys = new BankKeys(url, privateKey, account);
        }

        /* Show coins will populate the CloudBankUtils with the totals of each denominations
         These totals are public properties that can be accessed */
        static async Task showCoins()
        {
            CloudBankUtils cbu = new CloudBankUtils(myKeys, cli);
            await cbu.showCoins();
            Console.Out.WriteLine("Ones in our bank:" + cbu.onesInBank  );
            Console.Out.WriteLine("Five in our bank:" + cbu.fivesInBank);
            Console.Out.WriteLine("Twenty Fives in our bank:" + cbu.twentyFivesInBank);
            Console.Out.WriteLine("Hundreds in our bank:" + cbu.hundredsInBank );
            Console.Out.WriteLine("Two Hundred Fifties in our bank:" + cbu.twohundredfiftiesInBank );
        }//end show coins


        /* Deposit allow you to send a stack file to the CloudBank */
        static async Task<CloudBankUtils> depositAsync()
        {
            CloudBankUtils sender = new CloudBankUtils( myKeys, cli);
            Console.Out.WriteLine("What is the path to your stack file?");
            //string path = reader.readString();
            string path = AppDomain.CurrentDomain.BaseDirectory ;
            path += reader.readString();
            Console.Out.WriteLine("Loading " + path);
            sender.loadStackFromFile(path);
            await sender.sendStackToCloudBank();
            return sender;
        }//end deposit

        static async Task<CloudBankUtils> sendtoSkywallet()
        {
            CloudBankUtils sender = new CloudBankUtils(myKeys, cli);
            Console.Out.WriteLine("Which skywallet are you sending to?");
            string sw = reader.readString();
            Console.Out.WriteLine("How Much?");
            int amount = reader.readInt();
            await sender.SendToSkywallet(amount, sw);
            return sender;
        }//end deposit

        private static async Task<CloudBankUtils> transferBetweenSkywallets()
        {
            CloudBankUtils sender = new CloudBankUtils(myKeys, cli);
            Console.Out.WriteLine("Which skywallet are you sending to?");
            string sw = reader.readString();
            Console.Out.WriteLine("How Much?");
            int amount = reader.readInt();
            await sender.TransferBetweenSkywallets(amount, sw);
            return sender;
        }

        private static async Task<CloudBankUtils> recieveFromSkywallet()
        {
            CloudBankUtils sender = new CloudBankUtils(myKeys, cli);
            await sender.RecieveFromSkywallet();
            return sender;
        }


        static async Task withdraw()
        {
            CloudBankUtils receiver;
            if (receiptHolder == null)
                receiver = new CloudBankUtils(myKeys, cli);
            else
                receiver = receiptHolder;

            Console.Out.WriteLine("How many CloudCoins are you withdrawing?");
            int amount = reader.readInt();
            await receiver.getStackFromCloudBank(amount);
            if (receiver.haveStackFromWithdrawal)
                receiver.saveStackToFile(AppDomain.CurrentDomain.BaseDirectory);
            else
                Console.Out.WriteLine("Failed to withdraw");
        }//end deposit
        static async Task receipt()
        {
            if (receiptHolder == null)
                Console.Out.WriteLine("There has not been a recent deposit. So no receipt can be shown.");
            else
            {
                await receiptHolder.getReceipt();
                Console.Out.WriteLine(receiptHolder.interpretReceipt().interpretation);

            }   
        }//end deposit

        static async Task writeCheck()
        {
            Console.Out.WriteLine("How many CloudCoins are you withdrawing?");
            int amount = reader.readInt();
            Console.Out.WriteLine("Who are you Paying?");
            string payto = reader.readString();
            Console.Out.WriteLine("Who is being Emailed?");
            string emailto = reader.readString();
            var request = await cli.GetAsync("https://"+url+"/service/write_check?pk=" + privateKey + "&action=email&amount="+amount+"&emailto="+emailto+"&payto="+payto+"&from="+account+"&signby="+sign);
            string response = await request.Content.ReadAsStringAsync();
            Console.Out.WriteLine(response);
        }

        static async Task GetCheck()
        {
            Console.Out.WriteLine("What is the Check's Id?");
            string id = reader.readString();
            var request = await cli.GetAsync("https://" + url + "/service/checks?id="+id+"&receive=json");
            string response = await request.Content.ReadAsStringAsync();
            Console.Out.WriteLine(response);
        }

        
    }
}
