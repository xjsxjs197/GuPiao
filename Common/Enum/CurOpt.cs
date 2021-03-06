﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// 当前操作的枚举
    /// </summary>
    public enum CurOpt
    {
        Init = 0,
        InitEvent = 1,
        ConnServer = 2,
        LoginEvent = 3,
        CloseServer = 4,
        TradeRelease = 5,
        GetStockInfo = 6,
        BuyStock = 7,
        SellStock = 8,
        GetGuPiaoInfo = 9,
        OrderOKEvent = 10,
        OrderSuccessEvent = 11,
        OrderErrEvent = 12,
        StockQuoteEvent = 13,
        ServerErrEvent = 14,
        CancelOrder = 15,
        GetTodayPiaoInfo = 16,

        GetStockData = 17,
        ResetProcessBar = 18,
        ProcessBarStep = 19,
        CloseProcessBar = 20,

        EmuTradeEnd = 21
    }
}
