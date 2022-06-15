using System;
using System.Collections.Generic;
using System.Text;

namespace arbitrage_CSharp.Mode
{
    static  class  Commands
    {
        /// <summary>
        /// 停止程序
        /// </summary>
        public  const  string Stop = "Stop";
        /// <summary>
        /// 区块结束
        /// </summary>
        public const string End_Block = "End_Block";
        /// <summary>
        /// 区块开始
        /// 后面可以跟 tx
        /// </summary>
        public const string Start_Block = "Start_Block";
        /// <summary>
        /// 添加交易信息
        /// 后面可以跟 tx
        /// </summary>
        public const string Add_Tx = "Add_Tx";
        /// <summary>
        /// 发生 签名
        /// </summary>
        public const string Send_Sign = "Send_Sign";
    }
}
