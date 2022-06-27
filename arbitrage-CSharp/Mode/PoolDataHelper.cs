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



        public static void Init(Dictionary<string, PoolPairs> poolPairs)
        {
            poolPairsDic = poolPairs;
            pairsTokenDic.Clear();
            foreach (var poolPair in poolPairsDic)
            {
                (string k0,string k1) = GetPoolKeys(poolPair.Value.poolToken0.tokenAddress, poolPair.Value.poolToken1.tokenAddress, poolPair.Value.exchangeName);
                string key = k0;
                pairsTokenDic.Add(key, poolPair.Value);
                pairsTokenAddressDic.Add(key, poolPair.Key);
            }
        }
        /// <summary>
        /// 获取两种key，正向和反向
        /// </summary>
        /// <param name="addr0"></param>
        /// <param name="addr1"></param>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        public static (string k0,string k1)GetPoolKeys(string addr0,string addr1,string exchangeName)
        {
            string key0 = addr0 + "_" + addr1 + ":" + exchangeName;
            string key1 = addr1 + "_" + addr0 + ":" + exchangeName;
            return (key0, key1);
        }
        /// <summary>
        /// 通过 两个币种返回 对应的 poolpair 数据
        /// Dictionary<string 交易所 , (PoolPairs pairs, string poolAddress) 对应的交易对和 pair地址>   
        /// </summary>
        /// <param name="token0Address"></param>
        /// <param name="token1address"></param>
        /// <param name="tokenChangeNumDic"> 如果不为 null 表示获取 改变后的值</param>
        /// <returns> isReverse  表示 token0 和 token1 在 池子里面是否相反 </returns>
        public static Dictionary<string , (PoolPairs pairs, string poolAddress)> GetPoolPair(string token0Address,string token1address,List<string> exchangeNames, Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic = null)
        {
            if (poolPairsDic == null)
            {
                throw new Exception("请先调用 init 方法");
            }
            Dictionary<string, (PoolPairs pairs, string poolAddress)> exchangePairDic = new Dictionary<string, (PoolPairs pairs, string poolAddress)>();
            foreach (var exchangeName in exchangeNames)
            {
                PoolPairs pairs = null;
                string poolAddress = null;
                (string key0, string key1) = GetPoolKeys(token0Address, token1address, exchangeName);
                if (pairsTokenDic.TryGetValue(key0, out PoolPairs poolPairs))
                {
                    pairs = new PoolPairs(poolPairs.poolToken0.Clone(), poolPairs.poolToken1.Clone(), poolPairs.exchangeName); ;
                    poolAddress = pairsTokenAddressDic[key0];
                }
                else if (pairsTokenDic.TryGetValue(key1, out poolPairs))
                {
                    pairs = new PoolPairs(poolPairs.poolToken1.Clone(), poolPairs.poolToken0.Clone(), poolPairs.exchangeName);
                    poolAddress = pairsTokenAddressDic[key1];
                    //return (new PoolPairs(poolPairs.poolToken1, poolPairs.poolToken0), pairsTokenAddressDic[token1address + "_" + token0Address]);
                }
                if (tokenChangeNumDic != null && pairs != null)
                {
                    var dic = tokenChangeNumDic.ContainsKey(key0) ? tokenChangeNumDic[key0] : tokenChangeNumDic[key1];

                    pairs.poolToken0.tokenReverse += Util.ParseBiginteger(dic[pairs.poolToken0.tokenAddress], pairs.poolToken0.decimalNum);
                    pairs.poolToken1.tokenReverse += Util.ParseBiginteger(dic[pairs.poolToken1.tokenAddress], pairs.poolToken1.decimalNum);
                }
                if (pairs!=null)
                {
                    exchangePairDic.Add(exchangeName,(pairs, poolAddress));
                }
            }
            return exchangePairDic;

            
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
