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
        private int closeTimeout = 1000;

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
                    if (Sell(targetPrice + instrument.PriceTick, closeTimeout))
                    {
                        SellImmediately();
                    }
                }
            }
            else
            {
                if (SellShort(targetPrice))
                {
                    if (BuyToCover(targetPrice - instrument.PriceTick, closeTimeout))
                    {
                        BuyToCoverImmediatley();
                    }
                }
            }

        }


        private bool SellShort(double targetPrice)
        {
            ThostFtdcInputOrderField order = frame.CreateOrder();
            order.CombOffsetFlag_0 = EnumOffsetFlagType.Open;
            order.Direction = EnumDirectionType.Sell;
            order.InstrumentID = instrumentID;
            order.VolumeTotalOriginal = Volume;
            order.LimitPrice = targetPrice;
            order.StopPrice = targetPrice;
            order.ContingentCondition = EnumContingentConditionType.Immediately;
            //order.VolumeCondition = EnumVolumeConditionType.CV;

            return PlaceOrder(order);
        }
        private bool BuyToCover(double price, int timeout)
        {
            ThostFtdcInputOrderField order = frame.CreateOrder();
            order.CombOffsetFlag_0 = EnumOffsetFlagType.CloseToday;
            order.Direction = EnumDirectionType.Buy;
            order.InstrumentID = instrumentID;
            order.VolumeTotalOriginal = Volume;
            order.LimitPrice = price;
            order.StopPrice = price;
            order.ContingentCondition = EnumContingentConditionType.Immediately;
            //order.VolumeCondition = EnumVolumeConditionType.CV;

            return PlaceOrder(order, timeout);

        }
        private bool BuyToCoverImmediatley()
        {
            ThostFtdcInputOrderField order = frame.CreateOrder();
            order.CombOffsetFlag_0 = EnumOffsetFlagType.CloseToday;
            order.Direction = EnumDirectionType.Buy;
            order.InstrumentID = instrumentID;
            order.VolumeTotalOriginal = Volume;
            order.LimitPrice = lastQuote.UpperLimitPrice;
            order.StopPrice = lastQuote.UpperLimitPrice;
            order.ContingentCondition = EnumContingentConditionType.Immediately;
            //order.VolumeCondition = EnumVolumeConditionType.CV;

            return PlaceOrder(order);
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
            //order.VolumeCondition = EnumVolumeConditionType.CV;

            return PlaceOrder(order);
        }
        private bool Sell(double price, int timeout)
        {
            ThostFtdcInputOrderField order = frame.CreateOrder();
            order.CombOffsetFlag_0 = EnumOffsetFlagType.CloseToday;
            order.Direction = EnumDirectionType.Sell;
            order.InstrumentID = instrumentID;
            order.VolumeTotalOriginal = Volume;
            order.LimitPrice = price;
            order.StopPrice = price;
            order.ContingentCondition = EnumContingentConditionType.Immediately;
            //order.VolumeCondition = EnumVolumeConditionType.CV;

            return PlaceOrder(order, timeout);
        }
        private bool SellImmediately()
        {
            ThostFtdcInputOrderField order = frame.CreateOrder();
            order.CombOffsetFlag_0 = EnumOffsetFlagType.CloseToday;
            order.Direction = EnumDirectionType.Sell;
            order.InstrumentID = instrumentID;
            order.VolumeTotalOriginal = Volume;
            order.LimitPrice = lastQuote.LowerLimitPrice;
            order.StopPrice = lastQuote.LowerLimitPrice;
            order.ContingentCondition = EnumContingentConditionType.Immediately;
            //order.VolumeCondition = EnumVolumeConditionType.CV;

            return PlaceOrder(order);
        }

        /// <summary>
        /// 下单、超时撤单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private bool PlaceOrder(ThostFtdcInputOrderField order, int timeout)
        {
            using (var helper = new OrderPlacer(this.frame))
            {
                return helper.PlaceOrder(order, timeout);
            }
        }

        private bool PlaceOrder(ThostFtdcInputOrderField order)
        {
            using (var helper = new OrderPlacer(this.frame))
            {
                return helper.PlaceOrder(order);
            }
        }

    }
}
