using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace arbitrage_CSharp.Mode
{
    /// <summary>
    /// 数量变化
    /// </summary>
    class TX
    {
        /// <summary>
        /// 池子地址
        /// </summary>
        public string poolAddress;
        /// <summary>
        /// token地址
        /// </summary>
        public string tokenAddress;
        /// <summary>
        /// 变化数量,是发给我有小数的还是没有小数的
        /// </summary>
        public BigDecimal changeAmount;
        /// <summary>
        /// 价格
        /// </summary>
        public BigInteger gasPrice;
    }

    class TXDatas
    {
        public TX tx;
    }
}
