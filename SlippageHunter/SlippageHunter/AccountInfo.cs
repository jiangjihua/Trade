using JiangJihua.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiangJihua.SlippageHunter
{
    [Serializable]
    public class AccountInfo : NotifyPropertyChangedObject
    {
        private string brokerID, investorID, investorName, password, tradeAddress, marketDataAddress;

        public string BrokerID
        {
            get { return brokerID; }
            set
            {
                this.brokerID = value;
                OnPropertyChanged("BrokerID");
            }
        }

        public string InvestorID
        {
            get
            {
                return investorID;
            }
            set
            {
                investorID = value;
                OnPropertyChanged("InvestorID");
            }
        }

        public string InvestorName
        {
            get { return investorName; }
            set
            {
                investorName = value;
                OnPropertyChanged("InvestorName");
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }

        public string TradeAddress
        {
            get { return tradeAddress; }
            set
            {
                tradeAddress = value;
                OnPropertyChanged("TradeAddress");
            }
        }

        public string MarketDataAddress
        {
            get { return marketDataAddress; }
            set
            {
                marketDataAddress = value;
                OnPropertyChanged("MarketDataAddress");
            }
        }


        public AccountInfo()
        {

        }

        public override string ToString()
        {
            return string.Format("{0}（{1}）", InvestorName, InvestorID);
        }
    }
}
