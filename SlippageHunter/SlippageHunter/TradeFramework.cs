using CTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JiangJihua.SlippageHunter
{
    class TradeFramework
    {
        private CTPMarketDataProvider dataPrivoder;
        private Dictionary<ISubscriber, List<string>> subscription = new Dictionary<ISubscriber, List<string>>();
        private List<string> subscribedInstruments = new List<string>();
        private List<ThostFtdcInstrumentField> instrumentList = new List<ThostFtdcInstrumentField>();
        private long orderRef = 1000;
        private AutoResetEvent autoResetEventLogin = new AutoResetEvent(false);

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

        private void InitMarketDataProvider()
        {
            dataPrivoder = new CTPMarketDataProvider(AccountInfo);
            dataPrivoder.OnMarketDataReceived += dataPrivoder_OnMarketDataReceived;
        }

        private void InitCTPTrader()
        {
            CtpTrader = new CTPTrader();
            CtpTrader.OnRspUserLogin += CtpTrader_OnRspUserLogin;
            CtpTrader.OnRspError += CtpTrader_OnRspError;

            ConnectCtpTraderServer();


            if (Login())
            {
                GetInstruments();
            }
        }



        private bool Login()
        {
            Task.Run(new Action(() =>
            {
                CtpTrader.ReqUserLogin(new ThostFtdcReqUserLoginField()
                {
                    BrokerID = AccountInfo.BrokerID,
                    UserID = AccountInfo.InvestorID,
                    Password = AccountInfo.Password,
                    UserProductInfo = "ChenShi",
                });
            }));

            if (autoResetEventLogin.WaitOne(5 * 1000))
            {
                Console.WriteLine("登录成功");
                return true;
            }
            else
            {
                Console.WriteLine("登录超时");
                return false;
            }
        }

        private void CtpTrader_OnRspUserLogin(ThostFtdcRspUserLoginField pRspUserLogin, ThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            autoResetEventLogin.Set();

            orderRef = Math.Max(orderRef, long.Parse(pRspUserLogin.MaxOrderRef.Trim()));
        }

       

        private void CtpTrader_OnRspError(ThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            Console.WriteLine(pRspInfo.ErrorMsg);
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

        private bool ConnectCtpTraderServer()
        {
            try
            {
                CtpTrader.SubscribePrivateTopic(EnumTeResumeType.THOST_TERT_QUICK);
                CtpTrader.SubscribePublicTopic(EnumTeResumeType.THOST_TERT_QUICK);
                CtpTrader.RegisterFront(string.Format("tcp://{0}", AccountInfo.TradeAddress));
                CtpTrader.Init();
                Thread.Sleep(1000);

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



    }
}
