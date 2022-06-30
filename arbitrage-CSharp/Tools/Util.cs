using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Uniswap.Contracts.UniswapV2Pair.ContractDefinition;
using Nethereum.Uniswap.Contracts.UniswapV2Router02.ContractDefinition;
//using Nethereum.Uniswap.Contracts.UniswapV2Router01.ContractDefinition;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace arbitrage_CSharp.Tools
{
    public static class Util
    {
        public static BigDecimal Sqrt(this BigDecimal x, BigDecimal? guess = null)
        {
            var ourGuess = guess.GetValueOrDefault(x / 2m);
            var result = x / ourGuess;
            var average = (ourGuess + result) / 2m;

            if (average == ourGuess) // This checks for the maximum precision possible with a decimal.
                return average;
            else
                return Sqrt(x, average);
        }
        public static BigDecimal Sqrt2(this BigDecimal c, decimal epsilon = 0.0M)
        {
            if (c < 0)
            {
                return 0;
            }
            BigDecimal e = 1e-50m;
            BigDecimal x = c;
            BigDecimal y = (x + c / x) / 2;

            var v = x - y;

            bool canDo = true;
            canDo = (v > 0 ? v : -v) > epsilon;
            while (canDo)
            {
                x = y;
                y = (x + c / x) / 2;

                v = x - y;
                canDo = (v > 0 ? v : -v) > epsilon;
            }
            return x;
        }

        public static BigInteger ParseBigDecimal(this BigDecimal c,int pow)
        {
            c = c*BigDecimal.Pow(10, pow);
            string cs = c.ToString().Split('.')[0];
            return BigInteger.Parse(cs);
        } 
        public static BigDecimal ParseBiginteger(BigInteger bestAmount,int decimalNum)
        {
            BigDecimal dec = new BigDecimal(bestAmount, -decimalNum);
            return dec;
        }

        //         public static BigDecimal.ParseBiginteger(this BigInteger,int nub)
        //         {
        //             bestAmount.Mantissa* BigDecimal.Pow(10, decimalNum + bestAmount.Exponent);
        //         }

        public static PathData DecodeTransaction(Transaction txn)
        {
            PathData data = null;
            // pairs 合约 =========================================
            //             if (txn.IsTransactionForFunctionMessage<SwapFunction>())
            //             {
            //                 SwapFunction transfer = new SwapFunction().DecodeTransaction(txn);
            //                 Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
            //                 Console.WriteLine(transfer.AmountToSend);
            //             }
            //router 合约 ==========================================
            //swapETHForExactTokens
            if (txn.IsTransactionForFunctionMessage<SwapETHForExactTokensFunction>())
            {//0xbcdc1dc8719512a03eb48ff5c544179779f05204ef2b39684240c9bb89233c4a
                SwapETHForExactTokensFunction transfer = new SwapETHForExactTokensFunction().DecodeTransaction(txn);
                data = new PathData(transfer.AmountToSend, transfer.AmountOut, AmountEnum.KnowOut,transfer.Path, transfer.GasPrice.Value);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactETHForTokens
            else if (txn.IsTransactionForFunctionMessage<SwapExactETHForTokensFunction>())
            {//0x91f60ab52184468a4ba6a223470395861488d9717a7a7d9dfc3ab238afe21f35
                SwapExactETHForTokensFunction transfer = new SwapExactETHForTokensFunction().DecodeTransaction(txn);
                data = new PathData(transfer.AmountToSend, transfer.AmountOutMin, AmountEnum.KnowIn, transfer.Path, transfer.GasPrice.Value);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactETHForTokensSupportingFeeOnTransferTokens
            else if (txn.IsTransactionForFunctionMessage<SwapExactETHForTokensSupportingFeeOnTransferTokensFunction>())
            {//0xa7dd000089213d7b96e8f2588b2f9710573d37eb7cf10631be18d0912015bb7c
                SwapExactETHForTokensSupportingFeeOnTransferTokensFunction transfer = new SwapExactETHForTokensSupportingFeeOnTransferTokensFunction().DecodeTransaction(txn);
                data = new PathData(transfer.AmountToSend, transfer.AmountOutMin, AmountEnum.KnowIn, transfer.Path, transfer.GasPrice.Value);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactTokensForETH
            else if (txn.IsTransactionForFunctionMessage<SwapExactTokensForETHFunction>())
            {//0x81ab040033cd65e1c3826d8162643d9e626629105a63a6ab55ed1c7b92154f64
                SwapExactTokensForETHFunction transfer = new SwapExactTokensForETHFunction().DecodeTransaction(txn);
                data = new PathData(transfer.AmountIn, transfer.AmountOutMin, AmountEnum.KnowIn, transfer.Path, transfer.GasPrice.Value);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactTokensForETHSupportingFeeOnTransferTokens
            else if (txn.IsTransactionForFunctionMessage<SwapExactTokensForETHSupportingFeeOnTransferTokensFunction>())
            {//0x0467476f2d681fefbccd8944e6263930863f1f189343420ee3d8482ed2ba7113
                SwapExactTokensForETHSupportingFeeOnTransferTokensFunction transfer = new SwapExactTokensForETHSupportingFeeOnTransferTokensFunction().DecodeTransaction(txn);
                data = new PathData(transfer.AmountIn, transfer.AmountOutMin, AmountEnum.KnowIn, transfer.Path, transfer.GasPrice.Value);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactTokensForTokens
            else if (txn.IsTransactionForFunctionMessage<SwapExactTokensForTokensFunction>())
            {//0x8006c0926b3a8fbb46bb33ca33568196b2a1800c5815fda1617933744420d6ba
                SwapExactTokensForTokensFunction transfer = new SwapExactTokensForTokensFunction().DecodeTransaction(txn);
                data = new PathData(transfer.AmountIn, transfer.AmountOutMin, AmountEnum.KnowIn, transfer.Path, transfer.GasPrice.Value);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactTokensForTokensSupportingFeeOnTransferTokens
            else if (txn.IsTransactionForFunctionMessage<SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction>())
            {//0xb61d3a904bbdc684e918c98551e7db49b50043500e7f66b166538d7b9332adcc
                SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction transfer = new SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction().DecodeTransaction(txn);
                data = new PathData(transfer.AmountIn, transfer.AmountOutMin, AmountEnum.KnowIn, transfer.Path, transfer.GasPrice.Value);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapTokensForExactETH
            else if (txn.IsTransactionForFunctionMessage<SwapTokensForExactETHFunction>())
            {//0x2847f6bd8f0cd9b75e22b8de25572a385ae6aec98f0b2add25a28142b6dac7c7
                SwapTokensForExactETHFunction transfer = new SwapTokensForExactETHFunction().DecodeTransaction(txn);
                data = new PathData(transfer.AmountInMax, transfer.AmountOut, AmountEnum.KnowOut, transfer.Path, transfer.GasPrice.Value);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapTokensForExactTokens
            else if (txn.IsTransactionForFunctionMessage<SwapTokensForExactTokensFunction>())
            {//0x7907604b5ab27ff76529975102a32f58f9454fc6336a701dc6de940ff0c875da
                SwapTokensForExactTokensFunction transfer = new SwapTokensForExactTokensFunction().DecodeTransaction(txn);
                data = new PathData(transfer.AmountInMax, transfer.AmountOut, AmountEnum.KnowOut, transfer.Path, transfer.GasPrice.Value);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }
            if (data!=null)
            {
                for (int i = 0; i < data.paths.Count; i++)
                {
                    data.paths[i] = data.paths[i].ToLower();
                }
                data.amountIns *= 10;
                data.amountOutMins *= 10;
            }
          
            return data;
        }
        /// <summary>
        /// Clones the specified list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="List">The list.</param>
        /// <returns>List{``0}.</returns>
        public static List<T> Clone<T>(this List<T> List)
        {

            var str = JsonConvert.SerializeObject(List);
            return JsonConvert.DeserializeObject<List<T>>(str);

        }
        public static List<List<T>> CloneBigDecmic<T>(this List<List<T>> mList) 
        {
            var list = new List<List<T>>();
            foreach (var item in mList)
            {
                var list1 = new List<T>();
                foreach (var item1 in item)
                {
                    list1.Add(item1);
                }
                list.Add(list1);
            }
            if (list==mList)
            {
                list = mList;
            }
            return list;

        }
        public static void Sync(this Task task)
        {
            
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public class PathData
        {
            public PathData(BigInteger amountIns, BigInteger amountOutMins, AmountEnum amountKonw ,List<string> paths, BigInteger gasPrice)
            {
                this.amountIns = amountIns;
                this.amountOutMins = amountOutMins;
                this.amountKonw = amountKonw;
                this.paths = paths;
                this.gasPrice = gasPrice;
            }

            public BigInteger amountIns { get; set; }
            public BigInteger amountOutMins { get; set; }

            public AmountEnum amountKonw = AmountEnum.None;
            public List<string> paths { get; set; }

            public BigInteger gasPrice { get; set; }

            public override string ToString()
            {
                return $"{{{nameof(amountIns)}={amountIns.ToString()}, {nameof(amountOutMins)}={amountOutMins.ToString()}, {nameof(paths)}={paths}, {nameof(gasPrice)}={gasPrice.ToString()}}}";
            }
        }
        /// <summary>
        /// 一直数量是 in 还是out，none 表示没有赋值
        /// </summary>
        public enum AmountEnum
        {
            None,
            KnowIn,
            KnowOut
        }

    }

   


}
