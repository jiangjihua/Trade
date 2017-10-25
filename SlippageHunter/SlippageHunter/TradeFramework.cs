using CTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiangJihua.SlippageHunter
{
    class TradeFramework
    {
        private CTPMarketDataProvider dataPrivoder;
        private Dictionary<ISubscriber, List<string>> subscription = new Dictionary<ISubscriber, List<string>>();
        private List<string> subscribedInstruments = new List<string>();
        private List<ThostFtdcInstrumentField> instrumentList = new List<ThostFtdcInstrumentField>();
        private int orderRef = 1000;

        public AccountInfo AccountInfo { get; private set; }
        public CTPTrader CtpTrader { get; private set; }

        public TradeFramework(AccountInfo accountInfo)
        {
            this.AccountInfo = accountInfo;

            InitCTPTrader();

            InitMarketDataProvider();
        }

        public void Subscribe(ISubscriber subscribler, string instrumentID)
        {
            lock (subscription)
            {
                List<string> list;
                if (subscription.TryGetValue(subscribler, out list))
                {
                    if (!list.Contains(instrumentID))
                    {
                        list.Add(instrumentID);
                    }
                }
                else
                {
                    list = new List<string>();
                    subscription.Add(subscribler, list);
                    list.Add(instrumentID);
                }
            }

            if (!subscribedInstruments.Contains(instrumentID))
            {
                subscribedInstruments.Add(instrumentID);
                dataPrivoder.SubstribeQuotations(subscribedInstruments.ToArray());
            }
        }

        public void Unsubscribe(ISubscriber subscribler)
        {
            lock (subscription)
            {
                subscription.Remove(subscribler);
            }
        }

        public ThostFtdcInstrumentField GetInstrument(string instrumentID)
        {
            return instrumentList.FirstOrDefault(p => p.InstrumentID == instrumentID);
        }

        private void InitMarketDataProvider()
        {
            dataPrivoder = new CTPMarketDataProvider(AccountInfo);
            dataPrivoder.OnMarketDataReceived += dataPrivoder_OnMarketDataReceived;
        }

        private void InitCTPTrader()
        {
            CtpTrader = new CTPTrader();
            ConnectCtpTraderServer();
            if (Login())
            {
                GetInstruments();
            }
        }

        private bool GetInstruments()
        {
            System.Threading.AutoResetEvent autoResetEvent = new System.Threading.AutoResetEvent(false);
            CtpTrader.OnRspQryInstrument += (pInstrument, pRspInfo, nRequestID, bIsLast) =>
            {
                instrumentList.Add(pInstrument);

                if (bIsLast)
                {
                    autoResetEvent.Set();
                }
            };
            CtpTrader.ReqQryInstrument();

            if (autoResetEvent.WaitOne(5000))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool Login()
        {
            System.Threading.AutoResetEvent autoResetEvent = new System.Threading.AutoResetEvent(false);
            CtpTrader.OnRspUserLogin += (p1, p2, p3, p4) => { autoResetEvent.Set(); };
            CtpTrader.ReqUserLogin(new ThostFtdcReqUserLoginField());
            if (autoResetEvent.WaitOne(5000))
            {
                return true;
            }
            else
            {
                Console.WriteLine("login time out");
                return false;
            }
        }

        private bool ConnectCtpTraderServer()
        {
            try
            {
                CtpTrader.SubscribePrivateTopic(EnumTeResumeType.THOST_TERT_QUICK);
                CtpTrader.SubscribePublicTopic(EnumTeResumeType.THOST_TERT_QUICK);
                CtpTrader.RegisterFront(string.Format("tcp://{0}", AccountInfo.TradeAddress));
                CtpTrader.Init();

                Log(string.Format("资金账户【{0}】连接CTP交易柜台成功.", this.AccountInfo.InvestorID));
                return true;
            }
            catch (Exception ex)
            {
                var msg = string.Format("资金账户【{0}】连接CTP交易柜台失败.", this.AccountInfo.InvestorID);
                Log(msg);
                LogError(msg, ex);
                return false;
            }
        }

        private void LogError(string msg, Exception ex)
        {
            Log(msg);
        }

        private void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        private void dataPrivoder_OnMarketDataReceived(object sender, CTP.ThostFtdcDepthMarketDataField e)
        {
            lock (subscription)
            {
                foreach (var item in subscription)
                {
                    if (item.Value.Contains(e.InstrumentID))
                    {
                        item.Key.OnQuote(e);
                    }
                }
            }
        }



        public ThostFtdcInputOrderField CreateOrder()
        {
            var orderField = new ThostFtdcInputOrderField()
            {
                BrokerID = AccountInfo.BrokerID,
                InvestorID = AccountInfo.InvestorID,
                CombHedgeFlag_0 = EnumHedgeFlagType.Speculation,
                OrderPriceType = EnumOrderPriceTypeType.LimitPrice,
                IsAutoSuspend = 0,
                IsSwapOrder = 0,
                MinVolume = 1,
                TimeCondition = EnumTimeConditionType.GFD,
                VolumeCondition = EnumVolumeConditionType.AV,
                UserForceClose = 0,
                OrderRef = (++orderRef).ToString(),
                ForceCloseReason = EnumForceCloseReasonType.NotForceClose,
            };

            return orderField;
        }
    }
}
