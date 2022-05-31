using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using Nethereum.Contracts;
using Nethereum.HdWallet;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.IpcClient;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tools;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts.Extensions;
using Nethereum.Contracts.Standards.ENS.Registrar.ContractDefinition;
using Nethereum.RPC.TransactionManagers;

namespace arbitrage_CSharp
{
    public static class Constants
    {
        public static readonly Dictionary<string, int> exchanges = new Dictionary<string, int> { { "pancakeswap" ,1 }, { "biswap", 2 } };
    }

    public class ERC20
    {
        public string symbol { get; set; }
        public string fullname { get; set; }
        public string address { get; set; }
        public decimal decimals { get; set; }
        public ERC20(string symbol, string fullname, string address, decimal decimals)
        {
            this.symbol = symbol;
            this.fullname = fullname;
            this.address = address;
            this.decimals = decimals;
        }

    }

    public class SwapPair
    {


        public string symbol { get; set; }
        public string exchange { get; set; }
        public string token0 { get; set; }
        public string token1 { get; set; }
        public string reverse0 { get; set; }
        public string reverse1 { get; set; }
        public string address { get; set; }
        public decimal decimals { get; set; }
        public SwapPair(string symbol, string exchange, string token0, string token1, string reverse0, string reverse1, string address, decimal decimals)
        {
            this.symbol = symbol;
            this.exchange = exchange;
            this.token0 = token0;
            this.token1 = token1;
            this.reverse0 = reverse0;
            this.reverse1 = reverse1;
            this.address = address;
            this.decimals = decimals;
        }

        public async Task up_reverse(string reverse0, string reverse1)
        {
            this.reverse0 = reverse0;
            this.reverse1 = reverse1;
        }
    }

    public class SwapBridge
    {
       

        public string symbol { get; set; }
        public string address { get; set; }


        public ClientBase provider { get; set; }

        public Contract contract { get; set; }

        public string bridgeAbi
        {
            get { return _bridgeAbi; }
            set {
                if (value!=null)
                {
                    _bridgeAbi = value;
                }
            } 
        }

        private string _bridgeAbi;
        

        public List<(Account,BigInteger)> wallets { get; set; }

        public SwapBridge(string symbol, string address= "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56",string rpcUrl = "http://127.0.0.1:8545/", string swapAbi = null)
        {
            this._bridgeAbi = JObject.Parse(File.ReadAllText("../../contract/artifacts/contracts/flashswap.sol/Flashswap.json"))["abi"].ToString();

            this.symbol = symbol;
            this.address = address;
            this.bridgeAbi = swapAbi;
            this.provider = init_provider(rpcUrl);
            this.contract = attach_swap_contract(rpcUrl);//使用自己的高速通道创建
            this.wallets = new List<(Account, BigInteger)>();
        }


        private ClientBase init_provider(string url)
        {
            ClientBase provider = null;
            if (!string.IsNullOrEmpty(url))
            {
                if (url.StartsWith("ipc"))
                {
                    provider = new IpcClient(url);
                }
                else if (url.StartsWith("http"))
                {
                    provider = new RpcClient(new Uri(url));
                }
                else if (url.StartsWith("ws"))
                {
                    provider = new WebSocketClient(url);
                }
                else
                {
                    //provider = new RpcClient(new Uri("https://data-seed-prebsc-1-s1.binance.org:8545/"));
                    provider = new RpcClient(new Uri("http://127.0.0.1:8545/"));
                }
            }
            return provider;
        }

        private Contract attach_swap_contract(string url)
        {
            var web3 = new Web3( url);
            //var abi = @"[{""constant"":false,""inputs"":[{""name"":""_spender"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""approve"",""outputs"":[{""name"":""success"",""type"":""bool""}],""type"":""function""},{""constant"":true,""inputs"":[],""name"":""totalSupply"",""outputs"":[{""name"":""supply"",""type"":""uint256""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_from"",""type"":""address""},{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""transferFrom"",""outputs"":[{""name"":""success"",""type"":""bool""}],""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""name"":""success"",""type"":""bool""}],""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""},{""name"":""_spender"",""type"":""address""}],""name"":""allowance"",""outputs"":[{""name"":""remaining"",""type"":""uint256""}],""type"":""function""},{""inputs"":[{""name"":""_initialAmount"",""type"":""uint256""}],""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""_from"",""type"":""address""},{""indexed"":true,""name"":""_to"",""type"":""address""},{""indexed"":false,""name"":""_value"",""type"":""uint256""}],""name"":""Transfer"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""_owner"",""type"":""address""},{""indexed"":true,""name"":""_spender"",""type"":""address""},{""indexed"":false,""name"":""_value"",""type"":""uint256""}],""name"":""Approval"",""type"":""event""}]";
            var contract = web3.Eth.GetContract(bridgeAbi, address);
            Logger.Debug($"contract {contract.ToString()}");
            return contract;
        }

        public async Task import_wallets(params string[] keys)
        {
            foreach (var key in keys)
            {
                await import_wallet(key);
            }
        }

        private async Task import_wallet(string privateKey)
        {
            var account = new Account(privateKey);
            var address = account.Address;
            var web3 = new Web3(account,provider);
            var nonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.PublicKey);
            wallets.Add((account,nonce));
            
            Logger.Debug($"nonce {nonce} address{address}");
            return ;
        }

        private (Account,BigInteger) get_random_wallet()
        {
            int idx = new Random().Next(0, wallets.Count);
            return wallets[idx];
        }

        public void swap(List<(string symbol, decimal amountIn, decimal amountOutMin, string[] path)> arrs)//(string symbols, decimal amountIns, decimal amountOutMins,List<string> paths)
        {
            List<string> symbols = new List<string>();
            List<decimal> amountIns = new List<decimal>();
            List<decimal> amountOutMins = new List<decimal>();
            List<string> paths = new List<string>();
            List<int> pathSlices = new List<int>();

            foreach (var item in arrs)
            {
                symbols.Add(item.symbol);
                //symbols.Add(Constants.exchanges[symbol]);
                amountIns.Add(item.amountIn);
                amountOutMins.Add(item.amountOutMin);
                paths.AddRange(paths);
                pathSlices.Add(item.path.Length);
                Logger.Debug($"item.symbol {item.symbol} item.amountIn {item.amountIn} item.amountOutMin {item.amountOutMin} ");
                string pathStr = "";
                foreach (var path in paths)
                {
                    pathStr += "->"+ path;
                    
                }
                Logger.Debug($"pathStr {pathStr}");
            }
            var wallet = get_random_wallet();
            wallet.Item2 += 1;
            var transactionInput = EtherTransferTransactionInputBuilder.CreateTransactionInput("", "", 0m, 5m, 1500000, wallet.Item2);
            var multiSwap = this.contract.GetFunction("multiSwap");
            
            //CallAsync<string>(symbols, amountIns, amountOutMins, pathSlices, paths, overrides);
            var task = multiSwap.SendTransactionAsync(transactionInput, symbols, amountIns, amountOutMins, pathSlices, paths);
            Task.WaitAll(task);
            Logger.Debug($"tx:{task.Result}");
        }


    }
    /// <summary>
    /// 交易池中的交易对
    /// </summary>
    public class PoolPairs
    {
        public PoolToken poolTokenA;

        public PoolToken poolTokenB;

        public PoolPairs(PoolToken poolTokenA, PoolToken poolTokenB)
        {
            this.poolTokenA = poolTokenA;
            this.poolTokenB = poolTokenB;
        }
    }

    public class PoolToken
    {
        public PoolToken(string tokenSymbol, BigInteger tokenReverse ,string tokenAddress)
        {
            this.tokenSymbol = tokenSymbol;
            this.tokenReverse = tokenReverse;
            this.tokenAddress = tokenAddress;
        }

        /// <summary>
        /// 币种名称
        /// </summary>
        public string tokenSymbol { get; set; }
        /// <summary>
        /// 池子中数量
        /// </summary>
        public BigInteger tokenReverse { get; set; }
        /// <summary>
        /// 币种地址
        /// </summary>
        public string tokenAddress { get; set; }
    }

    public class PathDataAll
    {

        public List<(string exchange, List<(string address,decimal reverse)> tokenAddresses)> paths;

        public PathDataAll(string str)
        //exchangeName:USDT&234-BNB&234,exchangeName:USDT&234-BNB&234
        {
            paths = new List<(string exchange, List<(string address, decimal reverse)> tokenAddresses)>();
            //里面包含多个交易所
            var strs = str.Split(',');
            foreach (var item in strs)
            {
                (string exchange, List<(string address, decimal reverse)> tokenAddresses) path = ("", new List<(string address, decimal reverse)>());
                var pathStr = item.Split(':');
                path.exchange = pathStr[0];
                var tokenAddresses = new List<string>( pathStr[1].Split('-'));
                foreach (var p in tokenAddresses)
                {
                    var pp = p.Split('&');
                    var tokenAddresse = (pp[0], decimal.Parse(pp[1]));
                    path.tokenAddresses.Add(tokenAddresse);
                }
                paths.Add(path);   
            }
        }
        /// <summary>
        /// 返回利润
        /// </summary>
        /// <param name="maxBalance"></param>
        /// <returns></returns>
        public decimal GetProfit(decimal maxBalance)
        {


            return 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var path in paths)
            {
                sb.Append($"{path.exchange}:");
                foreach (var item in path.tokenAddresses)
                {
                    sb.Append($"{item.address}-{item.reverse}->");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
