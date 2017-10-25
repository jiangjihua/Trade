using CTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiangJihua.SlippageHunter
{
    class Trader : ISubscriber, IDisposable
    {
        private double targetPrice;
        private string instrumentID;
        private ThostFtdcInstrumentField instrument;
        private TradeFramework frame;
        private ThostFtdcDepthMarketDataField lastQuote;

        public int Volume { get; set; }



        public Trader(string instrumentID, double targetPrice, TradeFramework framework)
        {
            if (framework == null)
            {
                throw new ArgumentNullException();
            }
            this.Volume = 1;
            this.instrumentID = instrumentID;
            this.targetPrice = targetPrice;
            this.frame = framework;

            this.frame.Subscribe(this, instrumentID);
            this.instrument = this.frame.GetInstrument(instrumentID);
        }

        public void Dispose()
        {
            if (this.frame != null)
            {
                this.frame.Unsubscribe(this);
            }
        }

        public void OnQuote(CTP.ThostFtdcDepthMarketDataField e)
        {
            if (e.InstrumentID == instrumentID)
            {
                lastQuote = e;
            }
        }

        public void Run()
        {
            while (lastQuote == null)
            {
                System.Threading.Thread.Sleep(500);
            }

            if (lastQuote.LastPrice > targetPrice)
            {
                if (Buy(targetPrice))
                {
                    if (Sell(targetPrice + instrument.PriceTick, 500))
                    {
                        SellImmediately();
                    }
                }
            }
            else
            {
                if (SellShort(targetPrice))
                {
                    if (BuyToCover(targetPrice - instrument.PriceTick, 500))
                    {
                        BuyToCoverImmediatley();
                    }
                }
            }

        }

        private void BuyToCoverImmediatley()
        {
            throw new NotImplementedException();
        }

        private bool BuyToCover(double p1, int p2)
        {
            throw new NotImplementedException();
        }

        private bool SellShort(double targetPrice)
        {
            throw new NotImplementedException();
        }

        private void SellImmediately()
        {
            throw new NotImplementedException();
        }

        private bool Sell(double p1, int timeout)
        {
            throw new NotImplementedException();
        }

        private bool Buy(double targetPrice)
        {
            ThostFtdcInputOrderField order = frame.CreateOrder();
            order.CombOffsetFlag_0 = EnumOffsetFlagType.Open;
            order.Direction = EnumDirectionType.Buy;
            order.InstrumentID = instrumentID;
            order.VolumeTotalOriginal = Volume;
            order.LimitPrice = targetPrice;
            order.StopPrice = targetPrice;
            order.ContingentCondition = EnumContingentConditionType.Immediately;
            order.VolumeCondition = EnumVolumeConditionType.CV;

            System.Threading.AutoResetEvent autoResetEvent = new System.Threading.AutoResetEvent(false);
            frame.CtpTrader.OnRtnOrder += (pOrder) =>
            {
            };
            frame.CtpTrader.ReqOrderInsert(order);
            autoResetEvent.WaitOne();
        }

        void CtpTrader_OnRtnOrder(ThostFtdcOrderField pOrder)
        {
            throw new NotImplementedException();
        }
    }
}
