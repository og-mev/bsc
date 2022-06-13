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

        private static Dictionary<string, string> pairsTokenAddressDic = new Dictionary<string, string>();

        private static Dictionary<string, PoolPairs> poolPairsDic = new Dictionary<string, PoolPairs>();


        public static void Init(Dictionary<string, PoolPairs> poolPairs)
        {
            poolPairsDic = poolPairs;
            pairsTokenDic.Clear();
            foreach (var poolPair in poolPairsDic)
            {
                string key = poolPair.Value.poolToken0.tokenAddress + "_" + poolPair.Value.poolToken1.tokenAddress;
                pairsTokenDic.Add(key, poolPair.Value);
                pairsTokenAddressDic.Add(key, poolPair.Key);
            }
        }
        /// <summary>
        /// 通过 两个币种返回 对应的 poolpair 数据
        /// 
        /// </summary>
        /// <param name="token0Address"></param>
        /// <param name="token1address"></param>
        /// <returns> isReverse  表示 token0 和 token1 在 池子里面是否相反 </returns>
        public static  (PoolPairs pairs, string poolAddress) GetPoolPair(string token0Address,string token1address)
        {
            if (poolPairsDic == null)
            {
                throw new Exception("请先调用 init 方法");
            }
            if (pairsTokenDic.TryGetValue(token0Address+"_"+token1address,out PoolPairs poolPairs))
            {
                return (poolPairs, pairsTokenAddressDic[token0Address + "_" + token1address]);
            }
            else if (pairsTokenDic.TryGetValue(token1address + "_" + token0Address, out  poolPairs))
            {
                return (new PoolPairs(poolPairs.poolToken1, poolPairs.poolToken0), pairsTokenAddressDic[token1address + "_" + token0Address]);
            }
            return (null,null);
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
