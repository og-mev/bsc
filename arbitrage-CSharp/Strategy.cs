﻿using Tools;
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

using Nethereum.Uniswap.Contracts.UniswapV2Factory;

using Nethereum.Client;
using Nethereum.RPC.Eth.DTOs;
using arbitrage_CSharp.Mode;
using System.Numerics;
using Nethereum.Util;
using WBNB;
using Nethereum.Web3.Accounts;
using static Nethereum.Util.UnitConversion;
using static arbitrage_CSharp.Tools.Util;

namespace arbitrage_CSharp
{
    public partial class Strategy
    {
        private readonly EthereumClientIntegrationFixture _ethereumClientIntegrationFixture;

        
        /// <summary>
        /// 当前区块高度
        /// </summary>
        private int blockNum = 0;
        Config config;
        private Web3 web3;

        /// <summary>
        /// 存放所有token的兑换路径
        /// Dictionary<string（token id）, Dictionary<int（path长度）, List（相同长度多条路径）<List<string（依次兑换的 token id）>>>>
        /// </summary>
        Dictionary<string, Dictionary<int, List<List<string>>>> tokensSwapPathsDic = new Dictionary<string, Dictionary<int, List<List<string>>>>();
        /// <summary>
        /// token 的可兑换地址
        /// </summary>
        Dictionary<string, HashSet<string>> adjacencyList;
        /// <summary>
        /// 所有池子里面的数据 ，测试用，正式情况通过redis获取
        /// key 是
        /// </summary>
        Dictionary<string, PoolPairs> poolPairsDic;
        /// <summary>
        /// 集合一次区块 里面所有的 池子里面的token数量变化
        /// 动态加到 里面去<poolAddress , PoolPairs 变化数量>
        /// </summary>
        Dictionary<string, PoolPairs> poolChangeDic = new Dictionary<string, PoolPairs>();
        /// <summary>
        /// 币种对应小数点 字典
        /// </summary>
        Dictionary<string, string> tokenDecimlDic = new Dictionary<string, string>();

        /// <summary>
        /// 调用 信息的接口
        /// <命令号，数据>
        /// </summary>
        Action<string,object> SenMsg;
        public Strategy(string ConfigPath, Action<string, object> SenMsg)
        {
            Config config = null;
            if (File.Exists(ConfigPath))
            {
                string text = File.ReadAllText(ConfigPath);
                if (!string.IsNullOrEmpty(text))
                {
                    config = JsonConvert.DeserializeObject<Config>(text);
                }
                else
                {
                    Logger.Debug("data is empty :" + ConfigPath);
                }
            }
            else
            {
                Logger.Error("配置表路径错误！！");
                return;
            }
            if (config != null)
            {
                this.config = config;
            }
            this.SenMsg = SenMsg;
            //string str = JsonConvert.SerializeObject(config,Formatting.Indented);
            //Logger.Debug(str);
            _ethereumClientIntegrationFixture = new EthereumClientIntegrationFixture();
        }


        public async void StartAsync()
        {
            web3 = new Web3(config.NodeUrl);
            //await GetPoolDatasByContractAsync();
            //poolDatas

            //1 拉取 redis 获取 全路径，并且监听更新
            RedisDB.Init(config.RedisConfig);
            //RedisDB.Instance.StringGet<T>(DBKey);
            //poolPairsDic = await GetPoolDatasByContractAsync();//GetPoolDatasByFile();
            //poolPairsDic = GetPoolDatasByFile();
            poolPairsDic = GetPoolDatasByRedis();
            PoolDataHelper.Init(poolPairsDic);
            //test 兑换 wbnb
            await TestGetBaseTokenAsync();



            //获取所有路径,和 每个token 的可以兑换tokens;
            var (tokensSwapPathsDic, adjacencyList) = GetAllPaths(poolPairsDic,4,false);
            this.tokensSwapPathsDic = tokensSwapPathsDic;
            //RedisDB.Init(config.RedisConfig);
            //2 监听 peending  tx
            var tx = new TX()
            {

            };
            //test 
            try
            {
                //await OnTxChangeAsync("");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }




        /// <summary>
        /// 监听tx消息
        /// </summary>
        /// <param name="tokenChangeNumDic"> <reserves key  <token adrr, changeNum >></param>
        public async Task OnTxChangeAsync(Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic,List <string> sortPath)
        {
            //3 根据 tx 的交易对 获取所有对应路径
            //解析tx,获取到的tx是什么样子的,有可能同一个 区块中有多笔 tx改变？ 不考虑

            /* test
            string poolId = config.testConfig.poolId;
            string adressFrom = config.testConfig.adressFrom;//DAI
            decimal amountFrom = 0;
            string addressTo = config.testConfig.adressTo;//USDC
            */
            //test下需要计算出能兑换多少，实际上通过服务器传送
            

            BigDecimal changeAmountTo = 0;
            //循环所有的 有改变的 token，寻找 其中有利润的路径
            foreach (var changeToken in sortPath)
            {
                string poolId = config.testConfig.poolId;
                string adressFrom = config.testConfig.adressFrom;//DAI
                decimal amountFrom = 0;
                string addressTo = config.testConfig.adressTo;//USDC


                var (poolPair, addr) = PoolDataHelper.GetPoolPair(adressFrom, addressTo);
                var token0 = poolPair.GetToken(adressFrom, poolId);
                var token1 = poolPair.GetToken(addressTo, poolId);
                if (config.CurrTokenAmountDic.TryGetValue(token0.tokenAddress, out decimal balance))//有token 才进行计算
                {
                    CFMM cFMM = new CFMM(token0.tokenReverse, token1.tokenReverse);
                    //test ______________________实际上会收到 值
                    changeAmountTo = CFMM.GetDeltaB(cFMM, config.uniswapV2_fee, amountFrom);
                    //test______________________________
                    //根据tx 修改池子里面的数量
                    token0.tokenReverse += amountFrom;
                    token1.tokenReverse -= changeAmountTo;

                    //4 根据盈利比例计算出所有可兑换的路径，以及最大兑换数量
                    //获取 两个token的路径
                    var tokenPaths = GetRandomPath(token0.tokenAddress, 3);
                    var (bestPath, bestAmount, amountOut) = GetPathsWithAmount(tokenPaths, token0, true);

                    if (bestAmount <= 0)
                    {
                        Logger.Debug("没有好的路径！！");
                    }
                    else
                    {
                        Logger.Debug($"兑换率最高 bestAmount {bestAmount} amountOut{amountOut}");
                    }
                    //如果超过最大现金，设置为最大现金
                    if (bestAmount > balance)
                    {
                        Logger.Debug($"超过最大现金 balance {balance} bestAmount{bestAmount}");
                        amountOut = (amountOut / bestAmount) * balance;
                        bestAmount = balance;

                    }
                    //                 else if (bestAmount < 1 )
                    //                 {
                    //                     Logger.Debug($"小于1了 balance {balance} bestAmount{bestAmount}");
                    //                     amountOut = 1;
                    //                     bestAmount = 5;
                    //                 }
                    //由bigdecimal转换为bignumber
                    //取余，避免兑换小数差别
                    amountOut = amountOut * (1 - config.IgnoreRate);

                    var bestAmount_0 = bestAmount.Mantissa * BigDecimal.Pow(10, token0.decimalNum + bestAmount.Exponent);
                    var bestAmount_int = (BigInteger)((decimal)(bestAmount_0));
                    var amountOut_0 = amountOut.Mantissa * BigDecimal.Pow(10, token0.decimalNum + amountOut.Exponent);
                    var amountOut_int = (BigInteger)((decimal)(amountOut_0));

                    //TestAllTokens(token0, token1);

                    //5 签名后发给 ray
                    var flashswapAddr = JObject.Parse(File.ReadAllText(config.contractPath + "deploy.json"))["address"].ToString();
                    var swapAbi = JObject.Parse(File.ReadAllText(config.contractPath + "artifacts/contracts/flashswap.sol/Flashswap.json"))["abi"].ToString();
                    var bridge = new SwapBridge("BSC", flashswapAddr.ToLower(), swapAbi: swapAbi);

                    //根据算法来交换 币
                    var swapArr = new List<(string symbol, BigInteger amountIn, BigInteger amountOutMin, string[] path)>();



                    var v0 = bestAmount_int - (bestAmount_int % 10000000);
                    var v1 = amountOut_int - (amountOut_int % 10000000);

                    (string symbol, BigInteger amountIn, BigInteger amountOutMin, string[] path) swap = ("pancakeswap", v0, v1, bestPath.ToArray());
                    swapArr.Add(swap);
                    await bridge.import_wallets(config.privateKeys.ToArray());


                    try
                    {
                        await bridge.swap(swapArr);
                    }
                    catch (Exception ex)
                    {

                        Logger.Error(ex);
                    }
                }




            }
           
           
            Logger.Debug("完成了！！！！！！！！！！！");
        }

        /// <summary>
        /// 获取 路径对应的盈利 数量 和路径 
        /// </summary>
        /// <param name="tokenPaths"></param>
        /// <param name="token0"> 表示我们拥有要兑换的</param>
        private (List<string> backPath, BigDecimal bestAmountT0ALL,BigDecimal bestAmountOut) GetPathsWithAmount(List<List<string>> tokenPaths, PoolToken token0, bool islog = true)
        {
            List<string> backPath = new List<string>();
            BigDecimal bestAmountT0ALL = 0;
            BigDecimal bestProfit = 0;
            BigDecimal bestAmountOut = 0;

            List<string> bestPairPaths = new List<string>();
            //循环计算 所有路径的 最大盈利
            foreach (var path in tokenPaths)
            {
                //把 所有路径合成 一个CFMM
                List<PoolPairs> cFMMPaths = new List<PoolPairs>();
                List<string> pairPaths = new List<string>();
                for (int i = 0; i < path.Count-1; i++)
                {
                    var (_poolPair,addr) = PoolDataHelper.GetPoolPair(path[i],path[i+1]);
                    if (islog)
                    {
                        Logger.Debug($"path[i]_path[+1]  {path[i]}_{path[i + 1]}");
                    }
                    
                    //t0表示我们其实拥有的token，t1是要兑换的
                    if (_poolPair!=null)
                    {
                        if (islog)
                            Logger.Debug(_poolPair.ToString());
                        cFMMPaths.Add(_poolPair);
                        pairPaths.Add(addr);
                    }
                }
                try
                {
                    CFMM endCFMM = CFMM.GetVisualCFMM(config.uniswapV2_fee, cFMMPaths.ToArray());
                    BigDecimal bestAmountT0 = CFMM.GetBestChangeAmount(endCFMM.R0, endCFMM.R1, config.uniswapV2_fee);
                    if (islog)
                        Logger.Debug($" bestAmountT0 {bestAmountT0} 路径最近兑换数量 {string.Join("-->", path.ToArray()) }");
                    //计算利润
                    if (bestAmountT0> 0)
                    {
                        //test 测试
                        //bestAmountT0 = 10;
                        BigDecimal amountOut = CFMM.GetDeltaB(endCFMM, config.uniswapV2_fee, (decimal)bestAmountT0);
                        BigDecimal profit = amountOut - bestAmountT0;
                        if (profit>bestProfit)
                        {
                            bestProfit = profit;
                            backPath = path;
                            bestAmountT0ALL = bestAmountT0;
                            bestAmountOut = amountOut;
                            bestPairPaths = pairPaths;
                            //if (islog)
                            
                                Logger.Debug($"有利润 bestProfit {bestProfit} bestAmountT0ALL {bestAmountT0ALL} amountOut{amountOut}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            Logger.Debug("path pairAddr: " + string.Join(" ", bestPairPaths));
            Logger.Debug("backPath : " + string.Join(" ", backPath));
            Logger.Debug($" bestProfit {bestProfit} ");
            return (backPath, bestAmountT0ALL, bestAmountOut);

        }
        /// <summary>
        /// 根据 token 返回 一定数量的随机路径
        /// </summary>
        /// <param name="tokenAddress"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        private List<List<string>> GetRandomPath(string tokenAddress, int num)
        {
            List<List<string>> paths = new List<List<string>>();
            foreach (var item in tokensSwapPathsDic[tokenAddress])
            {
                paths.AddRange(item.Value);
            }
            return paths;
        }


        /// <summary>
        /// 返回根据 路径长度添装的 路径list字典
        /// </summary>
        /// <param name="poolPairsDic"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        private static (Dictionary<string, Dictionary<int, List<List<string>>>>,Dictionary<string,HashSet<string>>) GetAllPaths(Dictionary<string, PoolPairs> poolPairsDic,int maxHosp=5,bool log = true)
        {
            //存放所有token 对应的兑换路径
            Dictionary<string, Dictionary<int, List<List<string>>>> _tokensSwapPathsDic = new Dictionary<string, Dictionary<int, List<List<string>>>>();
            List<string> allTokens = new List<string>();
            List<string> vertices = new List<string>();
            List<Tuple<string, string>> edges = new List<Tuple<string, string>>();
            //获取有多少种类
            foreach (var item in poolPairsDic)
            {
                if (!allTokens.Contains(item.Value.poolToken0.tokenAddress))
                {
                    allTokens.Add(item.Value.poolToken0.tokenAddress);
                }
                if (!allTokens.Contains(item.Value.poolToken1.tokenAddress))
                {
                    allTokens.Add(item.Value.poolToken1.tokenAddress);
                }
                
            }
            foreach (var item in allTokens)
            {
                vertices.Add(item);
            }
            // 构成图，把相同的 地址能连接的放到一起
            foreach (var poolPair in poolPairsDic)
            {
                edges.Add(new Tuple<string, string>(poolPair.Value.poolToken0.tokenAddress, poolPair.Value.poolToken1.tokenAddress));
            }
            Logger.Debug("获得全部的 token");
            var graph = new Graph<string>(vertices, edges);
            //循环获得所有tokens的兑换路径
            for (int i = 0; i < allTokens.Count; i++)
            {
                var token = allTokens[i];
                var allPaths = Algorithms.DFSAllPaths<string>(graph, vertices[i], maxHosp);
                _tokensSwapPathsDic.Add(token, allPaths);
                if (log)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var edge in edges)
                    {
                        sb.AppendLine($"{edge.Item1} {edge.Item2} 100");
                    }

                    for (int j = 0; j < vertices.Count; j++)
                    {
                        sb.AppendLine($"{vertices[j]}");
                    }
                    Logger.Debug(sb.ToString());
                }
                if (i%200==0)
                {
                    Logger.Debug($"当前计算到了{i}");
                }
            }
            return (_tokensSwapPathsDic,graph.AdjacencyList);
        }


        #region 通信方法
        /// <summary>
        /// 添加tx
        /// </summary>
        /// <param name="txList"></param>
        public async Task AddTxAsync(string transaction)
        {
            var transactionRpc = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transaction);

            var txPares = Util.DecodeTransaction(transactionRpc);

            var (tokenChangeNumDic , sortPath) = GetTokenChangeAmount(txPares,config.uniswapV2_fee,tokenDecimlDic);
            OnTxChangeAsync(tokenChangeNumDic, sortPath);

        }
        /// <summary>
        /// 提交签名给服务器
        /// </summary>
        /// <param name="sign"></param>
        public void SendExchange(string sign ,int blockNum=0)
        {
            Logger.Debug($"发送tx {sign}");
            SenMsg(Commands.Send_Sign, sign);
        }

        #endregion

        #region 工具


        /// <summary>
        /// 计算出 本次交易 所有币种变化的数量
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="fee"></param>
        /// <param name="tokenDecimlDic"></param>
        /// <param name="exchangeName"></param>
        /// <returns>  <reserves key  <token adrr, changeNum >> </returns>
        public static (Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic, List<string> sortPath) GetTokenChangeAmount(PathData pathData,decimal fee,Dictionary<string,string> tokenDecimlDic, string exchangeName = "PancakeSwap")
        {
            //CFMM cFMM = new CFMM(poolP.poolToken0.tokenReverse, poolP.poolToken1.tokenReverse);
            //正向计算
            List<string> sortPath = new List<string>();
            BigInteger amoutIn = 0;
            if (pathData.amountKonw == AmountEnum.KnowIn)
            {
                sortPath = pathData.paths;
                amoutIn = pathData.amountIns;

            }//反向计算 ，把  path 翻转，依次计算值
            else if (pathData.amountKonw == AmountEnum.KnowOut)
            {
                pathData.paths.Reverse();
                sortPath = pathData.paths;
                amoutIn = pathData.amountOutMins;

            }
            //拉取 数据库对应数据参数
            //<air addr  <token addr , changeNum>>
            Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic = new Dictionary<string, Dictionary<string, BigInteger>>();
            List<(string name, BigDecimal changeNum)> tokenChangeNum = new List<(string name, BigDecimal changeNum)>();
            var paths = sortPath;
            for (int i = 0; i < paths.Count - 1; i++)
            {
                var addr0 = paths[i];
                var addr1 = paths[i + 1];
                
                bool isReverse = false;
                
                string keyTab = $"reserves";
                string keyName = $"{addr0}:{addr1}:{exchangeName}";
                string value = RedisDB.Instance.HashGet(keyTab, keyName);
                if (string.IsNullOrEmpty(value))
                {
                    keyTab = $"reserves";
                    keyName = $"{addr1}:{addr0}:{exchangeName}";
                    value = RedisDB.Instance.HashGet(keyTab, keyName);
                    isReverse = true;
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception($" 找不到地址：reserves {addr0}:{addr1}:{exchangeName}");
                }
                else
                {
                    (PoolPairs poolP, string pairAddr) = PraseRedis2Pair(keyName, value, tokenDecimlDic, null, isReverse);
                    BigDecimal amountOut =0;
                    //需要计算 是增加还是减少
                    //正向 是已知a增加 求b减少
                    CFMM cFMM = new CFMM(poolP.poolToken0.tokenReverse, poolP.poolToken1.tokenReverse);
                    Dictionary<string, BigInteger> keyValues ;
                    string pairKey0 = GetReservesKey( poolP.poolToken0.tokenAddress, poolP.poolToken1.tokenAddress , exchangeName);
                    string pairKey1 = GetReservesKey(poolP.poolToken0.tokenAddress, poolP.poolToken1.tokenAddress, exchangeName);

                    if (!tokenChangeNumDic.TryGetValue(pairKey0, out keyValues))
                    {
                        if (!tokenChangeNumDic.TryGetValue(pairKey1, out keyValues))
                        {
                            keyValues = new Dictionary<string, BigInteger>();
                            tokenChangeNumDic.Add(pairKey0, keyValues);
                        }
                    }
                    
                    if (i==0)
                    {//判断是否已经有这个池子，如果有直接添加值
                     //判断池子是否有这个 token 如果有直接改变 值
                        if (keyValues.ContainsKey(poolP.poolToken0.tokenAddress))
                        {
                            keyValues[poolP.poolToken0.tokenAddress] += amoutIn;
                        }
                        else
                        {
                            keyValues.Add(poolP.poolToken0.tokenAddress, amoutIn);
                        }
                        amountOut = CFMM.GetDeltaB(cFMM, fee,new BigDecimal(amoutIn,poolP.poolToken0.decimalNum));
                    }
                    else
                    {//反向的话 是已知b 减少 a求增加
                        BigDecimal nextOut = tokenChangeNum[i - 1].changeNum;
                        amountOut = CFMM.GetDeltaB(cFMM, fee, nextOut);
                        //判断池子是否有这个 token 如果有直接改变 值
                        BigInteger nextOut_int = nextOut.ParseBigDecimal(poolP.poolToken0.decimalNum);
                        if (keyValues.ContainsKey(poolP.poolToken0.tokenAddress))
                        {
                            keyValues[poolP.poolToken0.tokenAddress] += nextOut_int;
                        }
                        else
                        {
                            keyValues.Add(poolP.poolToken0.tokenAddress, nextOut_int);
                        }
                    }
                    //token1 减少 数量
                    tokenChangeNum.Add((poolP.poolToken1.tokenAddress, amountOut));
                    if (keyValues.ContainsKey(poolP.poolToken1.tokenAddress))
                    {

                        keyValues[poolP.poolToken1.tokenAddress] -= amountOut.ParseBigDecimal(poolP.poolToken1.decimalNum);
                    }
                    else
                    {
                        keyValues.Add(poolP.poolToken1.tokenAddress, -amountOut.ParseBigDecimal(poolP.poolToken1.decimalNum));
                    }
                }
            }
            return (tokenChangeNumDic,sortPath);
        }
        /// <summary>
        /// 获取 某个池子里面 token变换后数量
        /// </summary>
        /// <param name="tokenChangeNumDic"> Dictionary<string, Dictionary<string, BigInteger>> </param>
        /// <param name="dbKey"></param>
        public Dictionary<string, PoolPairs> GetTokenAfterTxChange(Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic , string exchangeName= "PancakeSwap")
        {
            Dictionary<string, PoolPairs> changeDic = new Dictionary<string, PoolPairs>();
            foreach ( var changePair in tokenChangeNumDic)
            {
                string addr0 = "";
                string addr1 = "";
                int idx = 0;
                foreach (var item in changePair.Value)
                {
                    if (idx==0)
                    {
                        addr0 = item.Key;
                    }
                    else
                    {
                        addr1 = item.Key;
                    }
                    idx++;
                }

                string dbTab = "tokens";
                string pairKey0 = GetReservesKey(addr0,addr1, exchangeName);
                string pairKey1 = GetReservesKey(addr1,addr0, exchangeName);
                bool isRe = false;//池子的 token顺序
                string realKey="";
                var value = RedisDB.Instance.HashGet(dbTab, pairKey0);
                realKey = pairKey0;
                if (string.IsNullOrEmpty(value))
                {
                    isRe = true;
                    value = RedisDB.Instance.HashGet(dbTab, pairKey1);
                    realKey = pairKey1;
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception($"找不到池子 pairKey0 {pairKey0}  pairKey1 {pairKey1} ");
                }

                (PoolPairs poolP, string pairAddr) = PraseRedis2Pair(realKey, value, tokenDecimlDic, null);

                PoolToken t0, t1;
                Dictionary<string, BigInteger> keyValues;

                if (!tokenChangeNumDic.TryGetValue(pairKey0, out keyValues))
                {
                    if (!tokenChangeNumDic.TryGetValue(pairKey1, out keyValues))
                    {
                        throw new Exception($"没找到对应池子 pairKey1 {pairKey1} pairKey1 {pairKey1} ");
                    }
                }
                t0 = poolP.poolToken0.Clone();
                t1 = poolP.poolToken1.Clone();

                t0.tokenReverse += keyValues[t0.tokenAddress];
                t1.tokenReverse += keyValues[t1.tokenAddress];
                changeDic.Add(realKey, new PoolPairs(t0, t1));
            }
            return changeDic;

        }
        /// <summary>
        /// 获取 db key
        /// </summary>
        /// <param name="addr0"></param>
        /// <param name="addr1"></param>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        public static string GetReservesKey(string addr0,string addr1,string exchangeName)
        {
            string pairKey = addr0 + ":" + addr1 + ":" + exchangeName;
            return pairKey;
        }

        /// <summary>
        /// 返回 没有小数位数和Symbol
        /// </summary>
        /// <param name="reservesKey"></param>
        /// <param name="reservesValue"></param>
        /// <returns></returns>
        private static (PoolPairs poolP, string pairAddr) PraseRedis2Pair(string reservesKey, string reservesValue, Dictionary<string, string> tokenDecimlDic, StringBuilder sb,bool isReverse = false)
        {
            

            string[] tokenAddr = reservesKey.Split(':');//0x93d5a19a993D195cfC75AcdD736A994428290a59:0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c
            string[] tokenDatas = reservesValue.ToString().Split('|');//r0|r1|pairAddr|update_timestamp   PancakeSwap: 3|3136980|0x696d16539Dd3eB00C19103f7d8cfA2cb32d66086|1654968667
            string addr0 = tokenAddr[0];
            string addr1 = tokenAddr[1];

            PoolToken t0 = new PoolToken("", BigDecimal.Parse(tokenDatas[0]), addr0, 0);
            PoolToken t1 = new PoolToken("", BigDecimal.Parse(tokenDatas[1]), addr1, 0);
            string pairAddr = tokenDatas[2];


            (int Decimal, string Symbol) tokenData0 = (0, null);
            (int Decimal, string Symbol) tokenData1 = (0, null);


            bool can = true;
            if (tokenDecimlDic.TryGetValue(addr0, out string data0Str))
            {
                tokenData0 = JsonConvert.DeserializeObject<(int Decimal, string Symbol)>(data0Str);

            }
            else
            {
                if (sb!=null)
                    sb.AppendLine(addr0);
                can = false;
            }
            if (tokenDecimlDic.TryGetValue(addr1, out string data1Str))
            {
                tokenData1 = JsonConvert.DeserializeObject<(int Decimal, string Symbol)>(data1Str);
            }
            else
            {
                if (sb != null)
                    sb.AppendLine(addr1);
                can = false;
            }
            PoolPairs poolP = null;
            if (can)
            {
                t0.tokenSymbol = tokenData0.Symbol;
                t0.decimalNum = tokenData0.Decimal;
                t1.tokenSymbol = tokenData1.Symbol;
                t1.decimalNum = tokenData1.Decimal;
                if (isReverse)
                {
                    poolP = new PoolPairs(t1, t0);

                }
                else
                {
                    poolP = new PoolPairs(t0, t1);
                }


            }

            return (poolP, pairAddr);
        }
        #endregion

        #region test
        private void TestAllTokens(PoolToken token0, PoolToken token1)
        {
            foreach (var item in tokensSwapPathsDic)
            {
                var tokenPaths = GetRandomPath(item.Key, 4);
                var (bestPath, bestAmountIn, amountOut) = GetPathsWithAmount(tokenPaths, token0,  false);
                if (bestAmountIn > 0)
                {
                    Logger.Debug($"有利润！！ {item.Key}  {bestAmountIn}  {string.Join("->", bestPath)}");
                }
            }
        }
        /// <summary>
        /// 添加默认token
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contractAddress"></param>
        /// <returns></returns>
        private async Task TestGetBaseTokenAsync(string url = "http://127.0.0.1:8545/",string contractAddress= "0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c")
        {
            await Task.Delay(2000);
            //var flashswapAddr = JObject.Parse(File.ReadAllText(config.contractPath + "deploy.json"))["address"].ToString();
            //contractAddress = flashswapAddr;
            var account = new Account(config.privateKeys[0]);
            var address = account.Address;
            var web3 = new Web3(account, url);
            string wnnbAbi = File.ReadAllText(config.wbnbAbi);
            var contractHandler = web3.Eth.GetContractHandler(contractAddress);

            BigInteger amount = UnitConversion.Convert.ToWei(1);
            //质押
            DepositFunction deposit = new DepositFunction()
            {
                AmountToSend = amount,
                Gas = 30000000,
                GasPrice = 5000000000,
                FromAddress = account.Address,
            };

            var depositFunctionTxnReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync<DepositFunction>(deposit);
            //转账
            var transferFunction = new TransferFunction()
            {
                AmountToSend = 0,
                Gas = 30000000,
                GasPrice = 5000000000,
                FromAddress = account.Address,
            };
            transferFunction.Dst = address;
            transferFunction.Wad = amount;
            var transferFunctionTxnReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync(transferFunction);

            Logger.Debug($"change {amount} to bnb");
        }
        #endregion
    }



    public class Config
    {
        
        public List<string> privateKeys = new List<string> { "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80" };//真实测试  0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80
        //158.247.203.163
        public string NodeUrl = "http://158.247.203.163:8545/";//"http://localhost:8545/";

        public string RedisConfig= "localhost,password=l3h2p1w0*";

        public string contractPath = "D://_Work//_mev//bsc//contract//";

        public string pairsDataPath = "./allPairs.json";

        public string tokenAbi = "./tokenAbi.json";

        public string uniswapV3_factoryAbi;

        public string unswapV2_FactoryAddress = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";

        public string uniswapV3_pairAbi;

        public string wbnbAbi = "./wbnbAbi.json";

        public decimal uniswapV2_fee = 0.0m;
        /// <summary>
        ///忽略比例 ，忽略兑换比例,取整
        /// </summary>
        public decimal IgnoreRate = 0.00001m;
        /// <summary>
        /// 当前各种币的数量的字典
        /// </summary>
        public Dictionary<string, decimal> CurrTokenAmountDic = new Dictionary<string, decimal>() { { "0x0e09fabb73bd3ade0a17ecc321fd13a19e81ce82", 100 } };

        public Dictionary<string, string> allPaths = new Dictionary<string, string>() { {"BNB-USDT", "exchangeName:USDT&231-BNB&232,exchangeName:USDT&233-BNB&234" } };

        public List<PoolPairs> testPoolPairs = new List<PoolPairs>() ;

        public TestConfig testConfig = new TestConfig();
    }

    public class TestConfig
    {
        public string poolId = "0x74e4716e431f45807dcf19f284c7aa99f18a4fbc";//"0xae461ca67b15dc8dc81ce7615e0320da1a9ab8d5";

        public string adressFrom = "0x2170ed0880ac9a755fd29b2688956bd959f933f8";//"0x6b175474e89094c44da98b954eedeac495271d0f";

        public string adressTo = "0xbb4cdb9cbd36b01bd1cbaebf2de08d9173bc095c";//"0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48";
    }
    //https://mainnet.infura.io/v3/f7d3ed56ffc1466bbfa4d23738fc0a87
    //npx hardhat node --fork https://mainnet.infura.io/v3/f7d3ed56ffc1466bbfa4d23738fc0a87
    //npx hardhat node --fork https://eth-mainnet.alchemyapi.io/v2/uyl_NYbVcmhfPETCGr7CW0_JWCVkYh2v
    /*
     *  {
        "token0": {
          "id": "0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48",
          "symbol": "USDC",
          "name": "USD//C",
          "derivedETH": "0.0005704799642292971753121751257002743"
        },
        "token1": {
          "id": "0xe336ac63cf871a66e7fbe74d1dc5c6774fbed281",
          "symbol": "DGT",
          "name": "DGT",
          "derivedETH": "0"
        },
        "reserve0": "0.000001",
        "reserve1": "0.000000140968211369",
        "reserveUSD": "0.0000009991948502158373759798111774433184",
        "trackedReserveETH": "0.00000000110176478891768812064172103849706",
        "token0Price": "7.093797887400220887738431272455595",
        "token1Price": "0.140968211369",
        "volumeUSD": "0",
        "txCount": "4"
      },
     */

}
