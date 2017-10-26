using CTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiangJihua.SlippageHunter
{
    public static class CTPExtension
    {
        public static string ToString2(this ThostFtdcTradeField field)
        {
            return string.Format("OrderRef:{0},{1}, {2} {3} {4} {5}",
                field.OrderRef,
                field.UserID,
                field.Direction,
                field.Volume,
                field.InstrumentID,
                field.Price
                );
        }

        public static string ToString2(this ThostFtdcOrderField field)
        {
            return string.Format("OrderRef:{0},{1},{2} {3}, {4} {5} {6} {7} {8}",
                field.OrderRef,
                field.UserID,
                field.OrderStatus,
                field.StatusMsg,
                field.CombOffsetFlag_0,
                field.Direction,
                field.VolumeTotalOriginal,
                field.InstrumentID,
                field.LimitPrice
                );
        }

        public static string ToString2(this ThostFtdcInputOrderField field)
        {
            return string.Format("OrderRef:{0},{1},{2} {3} {4} {5} {6}",
                field.OrderRef,
                field.UserID,
                field.CombOffsetFlag_0,
                field.Direction,
                field.VolumeTotalOriginal,
                field.InstrumentID,
                field.LimitPrice
                );
        }

        public static string ToString2(this ThostFtdcParkedOrderField field)
        {
            return string.Format("OrderRef:{0},{1},{2} {3} {4} {5} {6}",
                field.OrderRef,
                field.UserID,
                field.CombOffsetFlag_0,
                field.Direction,
                field.VolumeTotalOriginal,
                field.InstrumentID,
                field.LimitPrice
                );
        }

        public static string ToString2(this ThostFtdcRemoveParkedOrderField field)
        {
            return string.Format("ParkedOrderID:{0},{1},{2}", field.ParkedOrderID, field.BrokerID, field.InvestorID);
        }

        public static string ToString2(this ThostFtdcInputOrderActionField field)
        {
            return string.Format("OrderRef:{0},{1},{2}",
                field.OrderRef,
                field.UserID,
                field.InstrumentID
                );
        }

        public static string ToString2(this ThostFtdcDepthMarketDataField field)
        {
            return string.Format("{0} {1},{2} {3},{4},{5},{6}",
                field.TradingDay,
                field.UpdateTime,
                field.UpdateMillisec,
                field.InstrumentID,
                field.LastPrice,
                field.AskPrice1,
                field.BidPrice1
                );
        }

        public static string ToString2(this ThostFtdcInstrumentStatusField field)
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                field.InstrumentID,
                field.ExchangeInstID,
                field.ExchangeID,
                field.InstrumentStatus,
                field.SettlementGroupID,
                field.EnterReason,
                field.EnterTime,
                field.TradingSegmentSN
                );
        }

        public static DateTime? TryGetDate(this string date)
        {
            return new DateTime(
                int.Parse(date.Substring(0, 4)),
                int.Parse(date.Substring(4, 2)),
                int.Parse(date.Substring(6, 2)));
        }

        public static TimeSpan? TryGetTime(this string time)
        {
            return new TimeSpan(
                int.Parse(time.Substring(0, 2)),
                int.Parse(time.Substring(3, 2)),
                int.Parse(time.Substring(6, 2))
                );
        }
    }
}
