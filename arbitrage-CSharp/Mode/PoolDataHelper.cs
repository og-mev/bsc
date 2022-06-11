using System;
using System.Collections.Generic;
using System.Text;

namespace arbitrage_CSharp.Mode
{
    static class PoolDataHelper
    {
        /// <summary>
        /// <token0_token1,pairData>
        /// </summary>
        private static Dictionary<string, PoolPairs> pairsTokenDic = new Dictionary<string, PoolPairs>();

        private static Dictionary<string, PoolPairs> poolPairsDic;


        public static void Init(Dictionary<string, PoolPairs> poolPairs)
        {
            poolPairsDic = poolPairs;
            pairsTokenDic.Clear();
            foreach (var poolPair in poolPairsDic)
            {
                pairsTokenDic.Add(poolPair.Value.poolToken0.tokenAddress + "_" + poolPair.Value.poolToken1.tokenAddress,poolPair.Value);
            }
        }
        /// <summary>
        /// 通过 两个币种返回 对应的 poolpair 数据
        /// 
        /// </summary>
        /// <param name="token0Address"></param>
        /// <param name="token1address"></param>
        /// <returns> isReverse  表示 token0 和 token1 在 池子里面是否相反 </returns>
        public static  PoolPairs GetPoolPair(string token0Address,string token1address)
        {
            if (poolPairsDic == null)
            {
                throw new Exception("请先调用 init 方法");
            }
            if (pairsTokenDic.TryGetValue(token0Address+"_"+token1address,out PoolPairs poolPairs))
            {
                return poolPairs;
            }
            else if (pairsTokenDic.TryGetValue(token1address + "_" + token0Address, out  poolPairs))
            {
                return new PoolPairs(poolPairs.poolToken1, poolPairs.poolToken0);
            }
            return null;
        }

        public static PoolPairs GetPoolPair(string poollAddress)
        {
            if (poolPairsDic == null)
            {
                throw new Exception("请先调用 init 方法");
            }
            return poolPairsDic[poollAddress];
        }
    }
}
