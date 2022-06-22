using arbitrage_CSharp.Tools;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
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

        /// <summary>
        /// 临时变化 数量
        /// </summary>
        private static Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic;


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
        /// <param name="tokenChangeNumDic"> 如果不为 null 表示获取 改变后的值</param>
        /// <returns> isReverse  表示 token0 和 token1 在 池子里面是否相反 </returns>
        public static  (PoolPairs pairs, string poolAddress) GetPoolPair(string token0Address,string token1address, Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic = null)
        {
            if (poolPairsDic == null)
            {
                throw new Exception("请先调用 init 方法");
            }

            PoolPairs pairs =null;
            string poolAddress = null;
            string key0 = token0Address + "_" + token1address;
            string key1 = token1address + "_" + token0Address;
            if (pairsTokenDic.TryGetValue(key0, out PoolPairs poolPairs))
            {
                pairs = new PoolPairs(poolPairs.poolToken0.Clone(), poolPairs.poolToken1.Clone()); ;
                poolAddress = pairsTokenAddressDic[token0Address + "_" + token1address];
            }
            else if (pairsTokenDic.TryGetValue(key1, out  poolPairs))
            {
                pairs = new PoolPairs(poolPairs.poolToken1.Clone(), poolPairs.poolToken0.Clone());
                poolAddress = pairsTokenAddressDic[token1address + "_" + token0Address];
                //return (new PoolPairs(poolPairs.poolToken1, poolPairs.poolToken0), pairsTokenAddressDic[token1address + "_" + token0Address]);
            }
            if (tokenChangeNumDic!=null  && pairs !=null)
            {
                var dic = tokenChangeNumDic.ContainsKey(key0) ? tokenChangeNumDic[key0] : tokenChangeNumDic[key1];
               
                pairs.poolToken0.tokenReverse += Util.ParseBiginteger(dic[pairs.poolToken0.tokenAddress], pairs.poolToken0.decimalNum);
                pairs.poolToken1.tokenReverse += Util.ParseBiginteger(dic[pairs.poolToken1.tokenAddress], pairs.poolToken1.decimalNum);
            }


            return (pairs, poolAddress);
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
