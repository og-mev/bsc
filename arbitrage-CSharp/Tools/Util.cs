using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Uniswap.Contracts.UniswapV2Pair.ContractDefinition;
using Nethereum.Uniswap.Contracts.UniswapV2Router02.ContractDefinition;
//using Nethereum.Uniswap.Contracts.UniswapV2Router01.ContractDefinition;
using Nethereum.Util;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

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
        public static void DecodeTransaction(Transaction txn)
        {
            //(BigInteger)
            // pairs 合约
            if (txn.IsTransactionForFunctionMessage<SwapFunction>())
            {
                SwapFunction transfer = new SwapFunction().DecodeTransaction(txn);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }
            //router 合约
            //swapETHForExactTokens
            else if (txn.IsTransactionForFunctionMessage<SwapETHForExactTokensFunction>())
            {
                SwapETHForExactTokensFunction transfer = new SwapETHForExactTokensFunction().DecodeTransaction(txn);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactETHForTokens
            else if (txn.IsTransactionForFunctionMessage<SwapExactETHForTokensFunction>())
            {
                SwapExactETHForTokensFunction transfer = new SwapExactETHForTokensFunction().DecodeTransaction(txn);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactETHForTokensSupportingFeeOnTransferTokens
            else if (txn.IsTransactionForFunctionMessage<SwapExactETHForTokensSupportingFeeOnTransferTokensFunction>())
            {
                SwapExactETHForTokensSupportingFeeOnTransferTokensFunction transfer = new SwapExactETHForTokensSupportingFeeOnTransferTokensFunction().DecodeTransaction(txn);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactTokensForETH
            else if (txn.IsTransactionForFunctionMessage<SwapExactTokensForETHFunction>())
            {
                SwapExactTokensForETHFunction transfer = new SwapExactTokensForETHFunction().DecodeTransaction(txn);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactTokensForETHSupportingFeeOnTransferTokens
            else if (txn.IsTransactionForFunctionMessage<SwapExactTokensForETHSupportingFeeOnTransferTokensFunction>())
            {
                SwapExactTokensForETHSupportingFeeOnTransferTokensFunction transfer = new SwapExactTokensForETHSupportingFeeOnTransferTokensFunction().DecodeTransaction(txn);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactTokensForTokens
            else if (txn.IsTransactionForFunctionMessage<SwapExactTokensForTokensFunction>())
            {
                SwapExactTokensForTokensFunction transfer = new SwapExactTokensForTokensFunction().DecodeTransaction(txn);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapExactTokensForTokensSupportingFeeOnTransferTokens
            else if (txn.IsTransactionForFunctionMessage<SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction>())
            {
                SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction transfer = new SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction().DecodeTransaction(txn);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapTokensForExactETH
            else if (txn.IsTransactionForFunctionMessage<SwapTokensForExactETHFunction>())
            {
                SwapTokensForExactETHFunction transfer = new SwapTokensForExactETHFunction().DecodeTransaction(txn);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }//SwapTokensForExactTokens
            else if (txn.IsTransactionForFunctionMessage<SwapTokensForExactTokensFunction>())
            {
                SwapTokensForExactTokensFunction transfer = new SwapTokensForExactTokensFunction().DecodeTransaction(txn);
                Console.WriteLine(Web3.Convert.FromWei(transfer.AmountToSend));
                Console.WriteLine(transfer.AmountToSend);
            }
        }

    }

   


}
