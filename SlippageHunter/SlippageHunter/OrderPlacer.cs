using CTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JiangJihua.SlippageHunter
{
    class OrderPlacer : IDisposable
    {
        private bool isSuccess = false;
        private AutoResetEvent autoResetEventOrderCompleted = new AutoResetEvent(false);
        private AutoResetEvent autoResetEventDelete = new AutoResetEvent(false);
        private ThostFtdcOrderField orderOnWay;
        private TradeFramework frame;

        public OrderPlacer(TradeFramework framework)
        {
            this.frame = framework;

            this.frame.CtpTrader.OnRspOrderInsert += CtpTrader_OnRspOrderInsert;
            this.frame.CtpTrader.OnErrRtnOrderInsert += CtpTrader_OnErrRtnOrderInsert;
            this.frame.CtpTrader.OnRspOrderAction += CtpTrader_OnRspOrderAction;
            this.frame.CtpTrader.OnErrRtnOrderAction += CtpTrader_OnErrRtnOrderAction;

        }

        public bool PlaceOrder(ThostFtdcInputOrderField order)
        {
            isSuccess = false;

            autoResetEventOrderCompleted = new AutoResetEvent(false);
            frame.CtpTrader.OnRtnOrder += CtpTrader_OnRtnOrder;

            Console.WriteLine("Send order.({0})", order.ToString2());
            frame.CtpTrader.ReqOrderInsert(order);

            autoResetEventOrderCompleted.WaitOne();
            frame.CtpTrader.OnRtnOrder -= CtpTrader_OnRtnOrder;

            return isSuccess;
        }

        /// <summary>
        /// 下单、超时撤单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool PlaceOrder(ThostFtdcInputOrderField order, int timeout)
        {
            isSuccess = false;

            autoResetEventOrderCompleted = new AutoResetEvent(false);
            frame.CtpTrader.OnRtnOrder += CtpTrader_OnRtnOrder;

            Console.WriteLine("Send order. ({0})", order.ToString2());
            frame.CtpTrader.ReqOrderInsert(order);

            var isTimeout = !autoResetEventOrderCompleted.WaitOne(timeout);
            frame.CtpTrader.OnRtnOrder -= CtpTrader_OnRtnOrder;

            if (isTimeout)
            {
                isSuccess = false;
                frame.CtpTrader.OnRspOrderAction += CtpTrader_OnRspOrderAction;
                frame.CtpTrader.ReqOrderAction(new ThostFtdcInputOrderActionField()
                {
                    ActionFlag = EnumActionFlagType.Delete,
                    BrokerID = orderOnWay.BrokerID,
                    InvestorID = orderOnWay.InvestorID,
                    InstrumentID = orderOnWay.InstrumentID,
                    FrontID = orderOnWay.FrontID,
                    SessionID = orderOnWay.SessionID,
                    OrderSysID = orderOnWay.OrderSysID,
                    OrderRef = orderOnWay.OrderRef,
                    ExchangeID = orderOnWay.ExchangeID,
                });

                //autoResetEventOrderCompleted.WaitOne();
            }
            return isSuccess;
        }
        private void CtpTrader_OnRtnOrder(ThostFtdcOrderField pOrder)
        {
            orderOnWay = pOrder;

            if (pOrder.OrderStatus == EnumOrderStatusType.AllTraded)
            {
                isSuccess = true;
                autoResetEventOrderCompleted.Set();
            }
            else if (pOrder.OrderStatus == EnumOrderStatusType.Canceled)
            {
                isSuccess = false;
                autoResetEventOrderCompleted.Set();
            }
        }

        private void CtpTrader_OnRspOrderInsert(ThostFtdcInputOrderField pInputOrder, ThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            if (pRspInfo != null)
            {
                Console.WriteLine(pRspInfo.ErrorMsg);
            }

            isSuccess = false;
            autoResetEventOrderCompleted.Set();
        }
        private void CtpTrader_OnErrRtnOrderInsert(ThostFtdcInputOrderField pInputOrder, ThostFtdcRspInfoField pRspInfo)
        {
            if (pRspInfo != null)
            {
                Console.WriteLine(pRspInfo.ErrorMsg);
            }
        }

        private void CtpTrader_OnRspOrderAction(ThostFtdcInputOrderActionField pInputOrderAction, ThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            if (pRspInfo != null)
            {
                Console.WriteLine(pRspInfo.ErrorMsg);
            }
        }
        private void CtpTrader_OnErrRtnOrderAction(ThostFtdcOrderActionField pOrderAction, ThostFtdcRspInfoField pRspInfo)
        {
            if (pRspInfo != null)
            {
                Console.WriteLine(pRspInfo.ErrorMsg);
            }
        }

        public void Dispose()
        {
            frame.CtpTrader.OnRtnOrder -= CtpTrader_OnRtnOrder;
        }
    }
}
