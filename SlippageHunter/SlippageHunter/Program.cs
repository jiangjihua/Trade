using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiangJihua.SlippageHunter
{
    class Program
    {
        static void Main(string[] args)
        {
            AccountInfo account = new AccountInfo();
            account.InvestorName = System.Configuration.ConfigurationManager.AppSettings["InvestorName"];
            account.BrokerID = System.Configuration.ConfigurationManager.AppSettings["BrokerID"];
            account.InvestorID = System.Configuration.ConfigurationManager.AppSettings["InvestorID"];
            account.Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
            account.MarketDataAddress = System.Configuration.ConfigurationManager.AppSettings["MarketDataAddress"];
            account.TradeAddress = System.Configuration.ConfigurationManager.AppSettings["TradeAddress"];

            TradeFramework framework = new TradeFramework(account);

            string instrumentID = System.Configuration.ConfigurationManager.AppSettings["InstrumentID"];

            var input = GetPrice();
            while (input.ToLower() != "exit")
            {
                double price;
                if (double.TryParse(input, out price))
                {
                    var trader = new Trader(instrumentID, price, framework);
                    trader.Run();
                }
                input = GetPrice();
            }
        }

        private static string GetPrice()
        {
            Console.WriteLine("Enter a price:");
            var input = Console.ReadLine();
            return input;
        }
    }
}
