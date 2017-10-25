using CTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JiangJihua.SlippageHunter
{
    public class CTPMarketDataProvider : IDisposable
    {
        private CTPMDAdapter ctpReader;
        public bool IsConntected { get; private set; }
        public bool IsLogin { get; private set; }
        public string[] SubscribedInstruments { get; private set; }
        public AccountInfo AccountInfo { get; private set; }

        public event EventHandler<ThostFtdcDepthMarketDataField> OnMarketDataReceived;
        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;

        public CTPMarketDataProvider(AccountInfo accountInfo)
        {
            this.AccountInfo = accountInfo;

            ctpReader = new CTPMDAdapter();
            Initialize(ctpReader);

            Connect(accountInfo.MarketDataAddress);
        }

        public CTPMarketDataProvider(AccountInfo accountInfo, string pszFlowPath)
        {
            this.AccountInfo = accountInfo;

            var dataFlowPath = CreateDirectory(pszFlowPath);
            ctpReader = new CTPMDAdapter(dataFlowPath, false);
            Initialize(ctpReader);

            Connect(accountInfo.MarketDataAddress);
        }

        public void Dispose()
        {
            Release(ctpReader);
        }

        private void Connect(string address)
        {
            var url = string.Format("tcp://{0}", address);
            this.ctpReader.RegisterFront(url);
            this.ctpReader.Init();
        }

        private void Login()
        {
            ctpReader.ReqUserLogin(new ThostFtdcReqUserLoginField() { BrokerID = AccountInfo.BrokerID, UserID = AccountInfo.InvestorID, Password = AccountInfo.Password });
        }

        public void SubstribeQuotations(string[] instrumentIDs)
        {
            if (instrumentIDs == null || instrumentIDs.Length == 0)
            {
                return;
            }

            SubscribedInstruments = instrumentIDs;
            SubscribeMarketData();
        }

        private void SubscribeMarketData()
        {
            if (SubscribedInstruments == null || SubscribedInstruments.Length == 0)
            {
                return;
            }

            try
            {
                this.ctpReader.SubscribeMarketData(SubscribedInstruments);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("订阅行情失败。{0}", ex.Message));
            }
        }

        private void Initialize(CTPMDAdapter ctpReader)
        {
            ctpReader.OnFrontConnected += ReaderOnFrontConnected;
            ctpReader.OnFrontDisconnected += ReaderOnFrontDisconnected;
            ctpReader.OnRspUserLogin += ReaderOnRspUserLogin;
            ctpReader.OnRspUserLogout += ReaderOnRspUserLogout;
            ctpReader.OnRtnDepthMarketData += ReaderOnRtnDepthMarketData;
            ctpReader.OnRspError += ReaderOnRspError;
        }

        private void Release(CTPMDAdapter ctpReader)
        {
            ctpReader.OnFrontConnected -= ReaderOnFrontConnected;
            ctpReader.OnFrontDisconnected -= ReaderOnFrontDisconnected;
            ctpReader.OnRspUserLogin -= ReaderOnRspUserLogin;
            ctpReader.OnRspUserLogout -= ReaderOnRspUserLogout;
            ctpReader.OnRtnDepthMarketData -= ReaderOnRtnDepthMarketData;
            ctpReader.OnRspError -= ReaderOnRspError;

            ctpReader.Dispose();
        }

        private void ReaderOnFrontConnected()
        {
            this.IsConntected = true;
            Console.WriteLine(string.Format("CTP行情服务连接成功。{0}", AccountInfo.MarketDataAddress));
            Login();

            if (OnConnected != null)
            {
                OnConnected(this, EventArgs.Empty);
            }
        }

        private void ReaderOnFrontDisconnected(int nReason)
        {
            this.IsConntected = false;

            Console.WriteLine(string.Format("CTP行情服务断开。{0} ", AccountInfo.MarketDataAddress));
            if (OnDisconnected != null)
            {
                OnDisconnected(this, EventArgs.Empty);
            }
        }

        private void ReaderOnRspUserLogin(ThostFtdcRspUserLoginField pRspUserLogin, ThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            this.IsLogin = true;
            Console.WriteLine(string.Format("CTP行情服务登录成功。{0}", AccountInfo.MarketDataAddress));

            SubscribeMarketData();
        }

        private void ReaderOnRspUserLogout(ThostFtdcUserLogoutField pUserLogout, ThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            this.IsLogin = false;
            Console.WriteLine(string.Format("CTP行情服务登出成功。{0}", AccountInfo.MarketDataAddress));
        }

        private void ReaderOnRspError(ThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            Console.WriteLine(string.Format("{0} OnRspError", AccountInfo.MarketDataAddress));
        }

        private void ReaderOnRtnDepthMarketData(ThostFtdcDepthMarketDataField pDepthMarketData)
        {
            if (OnMarketDataReceived != null)
            {
                OnMarketDataReceived(this, pDepthMarketData);
            }
        }

        private static string CreateDirectory(string pszFlowPath)
        {
            var dataDirectory = pszFlowPath;

            if (!System.IO.Directory.Exists(dataDirectory))
            {
                System.IO.Directory.CreateDirectory(dataDirectory);
            }
            if (!dataDirectory.EndsWith(@"\"))
            {
                dataDirectory = dataDirectory + @"\";
            }
            return dataDirectory;
        }

    }
}
