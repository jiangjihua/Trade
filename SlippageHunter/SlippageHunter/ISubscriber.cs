using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiangJihua.SlippageHunter
{
    interface ISubscriber
    {

        void OnQuote(CTP.ThostFtdcDepthMarketDataField e);
    }
}
