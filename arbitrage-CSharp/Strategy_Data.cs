using Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using arbitrage_CSharp.Tools;
using Nethereum.Contracts;
using Nethereum.Web3;

using System.Numerics;
using Nethereum.Util;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using System.Linq;
using Nethereum.RPC.Eth.DTOs;

namespace arbitrage_CSharp
{
    public partial class Strategy
    {
        /// <summary>
        /// 当前所有本金
        /// </summary>
        public Dictionary<string, BigDecimal> CurrTokenAmountDic = new Dictionary<string, BigDecimal>();
        /// <summary>
        /// 获取所有的 交易对 通过合约
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, PoolPairs>> GetPoolDatasByContractAsync()
        {
            //通过合约拉取
            var web3 = _ethereumClientIntegrationFixture.GetWeb3();
            /*
            var factoryAddress = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";
            var factoryService = new UniswapV2FactoryService(web3, factoryAddress);
            var weth = "0xc02aaa39b223fe8d0a0e5c4f27ead9083c756cc2";
            var dai = "0x6b175474e89094c44da98b954eedeac495271d0f";
            var pair = await factoryService.GetPairQueryAsync(weth, dai);
            */

            //获取全部paris
            //1 获取allPairsLength
            //2 按长度循环调用 allPairs
            List<string> symbolAddressList = new List<string>();

            string factoryAbiStr = File.ReadAllText(config.uniswapV3_factoryAbi);
            string tokenAbiStr = File.ReadAllText(config.tokenAbi);


            string pairAbiStr = File.ReadAllText(config.uniswapV3_pairAbi);
            var factoryContract = web3.Eth.GetContract(factoryAbiStr, config.unswapV2_FactoryAddress);
            Logger.Debug($"contract {factoryContract.ToString()}");
            int count = await factoryContract.GetFunction("allPairsLength")
                .CallAsync<int>();
            Logger.Debug($"交易对数量 {count}");

            //存放所有的 池里面的数据
            Dictionary<string, PoolPairs> allPoolDic = new Dictionary<string, PoolPairs>();
            //先只要100个
            var allPairs = factoryContract.GetFunction("allPairs");
            List<Task<(string pairAddress, PoolPairs pairs)>> tasks = new List<Task<(string pairAddress, PoolPairs pairs)>>();
            for (int i = 0; i < 1000; i++)
            {
                if (i % 10 == 0)
                {
                    await Task.Delay(5000);
                }
                if (i % 12 == 0)
                {
                    await Task.WhenAll(tasks.ToArray());
                    tasks.Clear();
                }
                tasks.Add(GetPoolData(web3, symbolAddressList, tokenAbiStr, pairAbiStr, allPoolDic, allPairs, i, "PancakeSwap"));

                //string strs = JsonConvert.SerializeObject(allPoolDic, Formatting.Indented);

                //Logger.Debug(strs);
                //File.WriteAllText("./allPairs.json", strs);
            }
            await Task.WhenAll(tasks.ToArray());


            string str = JsonConvert.SerializeObject(allPoolDic, Formatting.Indented);
            Logger.Debug(str);
            File.WriteAllText(config.pairsDataPath, str);

            return allPoolDic;

        }
        /// <summary>
        /// 拉取节点合约数据
        /// </summary>
        /// <param name="web3"></param>
        /// <param name="symbolAddressList"></param>
        /// <param name="tokenAbiStr"></param>
        /// <param name="pairAbiStr"></param>
        /// <param name="allPoolDic"></param>
        /// <param name="allPairs"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static async Task<(string pairAddress, PoolPairs pairs)> GetPoolData(Web3 web3, List<string> symbolAddressList, string tokenAbiStr, string pairAbiStr, Dictionary<string, PoolPairs> allPoolDic, Function allPairs, int i,string exchangeName)
        {

            try
            {
                string pairAddress = (await allPairs.CallAsync<string>(i)).ToLower();
                symbolAddressList.Add(pairAddress);
                Logger.Debug($"pairAddress {pairAddress}");

                //获取每个交易对的 数量和地址
                var pairContract = web3.Eth.GetContract(pairAbiStr, pairAddress);
                var reserveData = await pairContract.GetFunction("getReserves")
                .CallAsync<ReservesDto>();

                string addressT0 = await pairContract.GetFunction("token0")
                .CallAsync<string>();
                string addressT1 = await pairContract.GetFunction("token1")
                .CallAsync<string>();

                //                 string symbol = await pairContract.GetFunction("symbol")
                //                 .CallAsync<string>();
                //                 var symbolP = symbol.Split('-');
                Logger.Debug($"addressT0 {addressT0} addressT1 {addressT1} ");

                //var ss = reserveData.Reserve0 / reserveData.Reserve1;



                var token0Contract = web3.Eth.GetContract(tokenAbiStr, addressT0);
                int dec0 = await token0Contract.GetFunction("decimals")
                    .CallAsync<int>();
                var token1Contract = web3.Eth.GetContract(tokenAbiStr, addressT1);
                int dec1 = await token1Contract.GetFunction("decimals")
                    .CallAsync<int>();
                string symbol0 = await token0Contract.GetFunction("symbol")
                                .CallAsync<string>();
                string symbol1 = await token1Contract.GetFunction("symbol")
            .CallAsync<string>();
                BigDecimal r0 = new BigDecimal(reserveData.Reserve0, -dec0);
                BigDecimal r1 = new BigDecimal(reserveData.Reserve1, -dec1);

                PoolToken t0 = new PoolToken(symbol0, r0, addressT0, dec0);
                PoolToken t1 = new PoolToken(symbol1, r1, addressT1, dec1);


                allPoolDic.Add(pairAddress, new PoolPairs(t0, t1, exchangeName));
                Logger.Debug($"address {pairAddress} addressT0 {addressT0} {reserveData.Reserve0}  addressT1 {addressT1} {reserveData.Reserve1} symbol {symbol0} {symbol1}");
                return (pairAddress, new PoolPairs(t0, t1, exchangeName));
            }
            catch (Exception)
            {

                Logger.Error($"这个不是erc20 ！！！！！！！！！！！！idx {i}");
            }
            return (null, null);

        }

        /// <summary>
        /// 通过存档获取所有交易对
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, PoolPairs> GetPoolDatasByFile()
        {//需要解析
            string str = File.ReadAllText(config.pairsDataPath);

            JObject Info = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(str);
            Dictionary<string, PoolPairs> allPoolDic = JsonConvert.DeserializeObject<Dictionary<string, PoolPairs>>(str);
            foreach (var pool in allPoolDic)
            {
                var item = Info[pool.Key];
                var v = item["poolToken0"]["tokenReverse"];
                BigInteger s = BigInteger.Parse(v["Mantissa"].ToString());
                int de = int.Parse(v["Exponent"].ToString());
                pool.Value.poolToken0.tokenReverse = new BigDecimal(s, de);

                var v1 = item["poolToken1"]["tokenReverse"];
                BigInteger s1 = BigInteger.Parse(v1["Mantissa"].ToString());
                int de1 = int.Parse(v1["Exponent"].ToString());
                pool.Value.poolToken1.tokenReverse = new BigDecimal(s1, de1);
            }
            return allPoolDic;
        }

        /// <summary>
        /// 通过redis获取数据
        /// </summary>
        /// <returns>Dictionary<string, PoolPairs> : <pair addr:exchangeName ></returns>
        public Dictionary<string, PoolPairs> GetPoolDatasByRedis()
        {//需要解析

            Dictionary<string, PoolPairs> allPoolDic = new Dictionary<string, PoolPairs>();

            string keyR = "reserves";
            string keyT = "tokens";
            int pageSize = 250;
            List<StackExchange.Redis.HashEntry> kp = new List<StackExchange.Redis.HashEntry>();
            List<StackExchange.Redis.HashEntry> kT = new List<StackExchange.Redis.HashEntry>();
            //int len = RedisDB.Instance.StringGet<int>(lenKey );
            //             long len = RedisDB.Instance.HashLength(keyR);
            //             long allPage = len / pageSize ;
            //             if (len%pageSize>0)
            //             {
            //                 allPage += 1;
            //             }
            //获取所有池子信息
            //for (int page = 0; page < allPage; page++)
            {
                var res = RedisDB.Instance.HashScan(keyR, "*", pageSize, 0 * pageSize);
                
                kp.AddRange(res);
                //Test  删除超过数量的部分 交易对
                if (kp.Count>config.testConfig.maxPairCount)
                {
                    kp.RemoveRange(config.testConfig.maxPairCount-1, kp.Count- config.testConfig.maxPairCount);
                }
                Logger.Debug(res.ToString());
            }
            //获取所有的 token小数点信息
            long lenT = RedisDB.Instance.HashLength(keyT);
            var resT = RedisDB.Instance.HashScan(keyT, "*", pageSize, 0 * pageSize);
            kT.AddRange(resT);
            StringBuilder sb = new StringBuilder();
            foreach (var item in kT)
            {
                string s = item.Value;
                //Logger.Debug(s);
                //tokenDecimlDic.Add(item.Name.ToString(), JsonConvert.DeserializeObject<(int Decimal, string Symbol)>(s) );
                TokenD tokenD = JsonConvert.DeserializeObject<TokenD>(s);
                tokenDecimlDic.Add(item.Name.ToString(), (tokenD.Decimal,tokenD.Symbol));
            }

            foreach (var item in kp)
            {
                prase(item);
            }

            void prase(StackExchange.Redis.HashEntry s)
            {
                string key = s.Name;
                //先只考虑 单个交易所
                //if (key.EndsWith(":PancakeSwap"))
                {
                    try
                    {
                        (PoolPairs poolP, string pairAddr) = PraseRedis2Pair(key, s.Value, tokenDecimlDic, sb);

                        if (poolP != null)
                        {
                            if (!allPoolDic.ContainsKey(pairAddr))
                            {
                                allPoolDic.Add(pairAddr, poolP);
                            }
                            else
                            {
                                Logger.Debug($"same key {key}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        Logger.Error(ex);
                    }
                }
            }

            return allPoolDic;
        }

        public static async Task<Dictionary<string, BigDecimal>> GetBalanceAsync(Web3 web3, string addr,List<Token> tokens)
        {

            var tokensOwned = await web3.Eth.ERC20.GetAllTokenBalancesUsingMultiCallAsync(
                    new string[] { addr }, tokens.Where(x => x.ChainId == 1),
                    BlockParameter.CreateLatest());


            //Filtering only the tokens from the token list that have a positive balance
            var tokensWithBalance = tokensOwned.Where(x => x.GetTotalBalance() > 0);

            Dictionary<string, BigDecimal> balanceDic = new Dictionary<string, BigDecimal>();
            foreach (var tokenWithBalance in tokensWithBalance)
            {
                
                Logger.Debug("-----------------------------");
                Logger.Debug("Name:" + tokenWithBalance.Token.Name);
                Logger.Debug("Token:" + tokenWithBalance.Token.Address);
                Logger.Debug("ChainId:" + tokenWithBalance.Token.ChainId);
                Logger.Debug("Decimals:" + tokenWithBalance.Token.Decimals);
                //Getting the balance of the owner as we could have queried for multiple owners
                var balance = tokenWithBalance.OwnersBalances.FirstOrDefault(x => x.Owner.IsTheSameAddress(addr)).Balance;
                //Converting the balance to a whole unit using the decimal places
                var balanceReal = Nethereum.Util.UnitConversion.Convert.FromWei(balance, tokenWithBalance.Token.Decimals);
                Logger.Debug("Balance:" + balanceReal);
                Logger.Debug("-----------------------------");
                balanceDic[tokenWithBalance.Token.Address] = balanceReal;
            }

            return balanceDic;
        }
    }

    class TokenD
    {
        public int Decimal;
        public string Symbol;
    }
}
