﻿using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;

namespace Nethereum.Client
{
    public enum EthereumClient
    {
        Geth,
        OpenEthereum,
        Ganache,
        Infura,
        External,
        Hardhat
    }

    public enum InfuraNetwork
    {
        Ropsten, //faucet https://faucet.dimensions.network/
        Rinkeby,
        Kovan,
        Mainnet
    }

    public class EthereumClientIntegrationFixture : IDisposable
    {
        public const string ETHEREUM_CLIENT_COLLECTION_DEFAULT = "Ethereum client Test";
        public static string GethClientPath { get; set; } = @"..\..\..\..\testchain\gethclique\geth.exe";
        public static string ParityClientPath { get; set; } = @"..\..\..\..\testchain\openethereumpoa\openethereum.exe";

        public static string HardhatClientPath { get; set; } = @"..\..\..\..\testchain\hardhat";
        public static string AccountPrivateKey { get; set; } = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";
        public static string AccountAddress { get; set; } = "0x12890d2cce102216644c59daE5baed380d84830c";

        public static string HardhatParams { get; set; } =
            "--fork https://eth-mainnet.alchemyapi.io/v2/{apikey} --fork-block-number 11998887";

        public static string ManagedAccountPassword { get; set; } = "password";
        public static string InfuraId { get; set; } = "7238211010344719ad14a89db874158c";
        public static InfuraNetwork InfuraNetwork { get; set; } = InfuraNetwork.Ropsten;
        public static string HttpUrl { get; set; } = "http://localhost:8545";
        public static System.Numerics.BigInteger ChainId { get; set; } = 444444444500;

        public string DeployStr { get; set; }

        public string flashswap { get; set; }

        public static Account GetAccount()
        {
            return new Account(AccountPrivateKey, ChainId);
        }

        public static ManagedAccount GetManagedAccount()
        {
            return new ManagedAccount(AccountAddress, ManagedAccountPassword);
        }

        private readonly Process _process;
        private readonly string _exePath;

        public string GetInfuraUrl(InfuraNetwork infuraNetwork)
        {
            return "https://" + Enum.GetName(typeof(InfuraNetwork), infuraNetwork).ToLower() + ".infura.io/v3/" + InfuraId;
        }

        public Web3.Web3 GetInfuraWeb3(InfuraNetwork infuraNetwork)
        {
            return new Web3.Web3(new Account(AccountPrivateKey), GetInfuraUrl(infuraNetwork));
        }

        private string GetHttpUrl()
        {
            if (EthereumClient == EthereumClient.Infura)
            {
                return GetInfuraUrl(InfuraNetwork);
            }
            else
            {
                return HttpUrl;
            }
        }

        private Web3.Web3 _web3;
        public Web3.Web3 GetWeb3()
        {
            if (_web3 == null)
            {
                _web3 = new Web3.Web3(GetAccount(), GetHttpUrl());
            }

            return _web3;
        }

        public EthereumClient EthereumClient { get; private set; } = EthereumClient.Geth;

        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json", true)
                .Build();
            return config;
        }

        public class EthereumTestSettings
        {
            public string GethPath { get; set; }
            public string ParityPath { get; set; }

            public string HardhatPath { get; set; }

            public string HardhatParams { get; set; }

            public string AccountAddress { get; set; }

            public string AccountPrivateKey { get; set; }
            public string ManagedAccountPassword { get; set; }

            public string ChainId { get; set; }

            public string Client { get; set; }

            public string InfuraNetwork { get; set; }
            public string InfuraId { get; set; }

            public string HttpUrl { get; set; }

            public string DeployStr { get; set; }

            public string flashswap { get; set; }
        }

        public EthereumClientIntegrationFixture()
        {

            var config = InitConfiguration();
            if (config != null)
            {

                var ethereumTestSection = config.GetSection("EthereumTestSettings");

                if (ethereumTestSection != null)
                {
                    var ethereumTestSettings = new EthereumTestSettings();
                    ethereumTestSection.Bind(ethereumTestSettings);
                    if (!string.IsNullOrEmpty(ethereumTestSettings.GethPath)) GethClientPath = ethereumTestSettings.GethPath;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.ParityPath)) ParityClientPath = ethereumTestSettings.ParityPath;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.HardhatPath)) HardhatClientPath = ethereumTestSettings.HardhatPath;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.HardhatParams)) HardhatParams = ethereumTestSettings.HardhatParams;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.AccountAddress)) AccountAddress = ethereumTestSettings.AccountAddress;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.AccountPrivateKey)) AccountPrivateKey = ethereumTestSettings.AccountPrivateKey;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.ChainId)) ChainId = BigInteger.Parse(ethereumTestSettings.ChainId);
                    if (!string.IsNullOrEmpty(ethereumTestSettings.ManagedAccountPassword)) ManagedAccountPassword = ethereumTestSettings.ManagedAccountPassword;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.Client)) EthereumClient = (EthereumClient)Enum.Parse(typeof(EthereumClient), ethereumTestSettings.Client);
                    if (!string.IsNullOrEmpty(ethereumTestSettings.InfuraNetwork)) InfuraNetwork = (InfuraNetwork)Enum.Parse(typeof(InfuraNetwork), ethereumTestSettings.InfuraNetwork); ;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.InfuraId)) InfuraId = ethereumTestSettings.InfuraId;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.HttpUrl)) HttpUrl = ethereumTestSettings.HttpUrl;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.DeployStr)) DeployStr = ethereumTestSettings.DeployStr;
                    if (!string.IsNullOrEmpty(ethereumTestSettings.flashswap)) flashswap = ethereumTestSettings.flashswap;
                }
            }

            var client = Environment.GetEnvironmentVariable("ETHEREUM_CLIENT");

            if (client == null)
            {
                Console.WriteLine("**************TEST CLIENT NOT CONFIGURED IN ENVIRONMENT USING DEFAULT");
            }
            else
            {
                Console.WriteLine("************ENVIRONMENT CONFIGURED WITH CLIENT: " + client.ToString());
            }

            if (string.IsNullOrEmpty(client))
            {

            }
            else if (client == "geth")
            {
                EthereumClient = EthereumClient.Geth;
                Console.WriteLine("********TESTING WITH GETH****************");
            }
            else if (client == "parity")
            {
                EthereumClient = EthereumClient.OpenEthereum;
                Console.WriteLine("******* TESTING WITH PARITY ****************");
            }
            else if (client == "ganache")
            {
                EthereumClient = EthereumClient.Ganache;
                Console.WriteLine("******* TESTING WITH GANACHE ****************");
            }
            else if (client == "hardhat")
            {
                EthereumClient = EthereumClient.Hardhat;
                Console.WriteLine("******* TESTING WITH HARDHat ****************");
            }

            if (EthereumClient == EthereumClient.Geth)
            {

                var location = typeof(EthereumClientIntegrationFixture).GetTypeInfo().Assembly.Location;
                var dirPath = Path.GetDirectoryName(location);
                _exePath = Path.GetFullPath(Path.Combine(dirPath, GethClientPath));

                DeleteData();

                var psiSetup = new ProcessStartInfo(_exePath,
                    @" --datadir=devChain init genesis_clique.json ")
                {
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(_exePath)

                };

                Process.Start(psiSetup);
                Thread.Sleep(3000);

                var psi = new ProcessStartInfo(_exePath,
                    @" --nodiscover --rpc --datadir=devChain  --rpccorsdomain "" * "" --mine --rpcapi ""eth, web3, personal, net, miner, admin, debug"" --rpcaddr ""0.0.0.0"" --allow-insecure-unlock --unlock 0x12890d2cce102216644c59daE5baed380d84830c --password ""pass.txt""  --ws  --wsaddr ""0.0.0.0"" --wsapi ""eth, web3, personal, net, miner, admin, debug"" --wsorigins "" * "" --verbosity 0 console  ")
                {
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(_exePath)

                };
                _process = Process.Start(psi);
            }
            else if (EthereumClient == EthereumClient.OpenEthereum)
            {

                var location = typeof(EthereumClientIntegrationFixture).GetTypeInfo().Assembly.Location;
                var dirPath = Path.GetDirectoryName(location);
                _exePath = Path.GetFullPath(Path.Combine(dirPath, ParityClientPath));

                DeleteData();

                var psi = new ProcessStartInfo(_exePath,
                    @" --config node0.toml") // --logging debug")
                {
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(_exePath)

                };
                _process = Process.Start(psi);
                Thread.Sleep(10000);
            }
            else if (EthereumClient == EthereumClient.Ganache)
            {
                var psi = new ProcessStartInfo("ganache-cli")
                {
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = true,
                    WorkingDirectory = _exePath,
                    Arguments = " --account=" + AccountPrivateKey + ",10000000000000000000000"

                };
                _process = Process.Start(psi);
                Thread.Sleep(10000);
            }
            else if (EthereumClient == EthereumClient.Hardhat)
            {
                var location = typeof(EthereumClientIntegrationFixture).GetTypeInfo().Assembly.Location;
                var dirPath = Path.GetDirectoryName(location);
                _exePath = Path.GetFullPath(Path.Combine(dirPath, HardhatClientPath));
                string path = "./hardhatPro.txt";
                if (!File.Exists(path))
                {
                    openPro();
                    
                }
                else
                {
                    string id = File.ReadAllText(path);

                    var prs = Process.GetProcesses();
                    int pId = int.Parse(id);
                    bool isOpen = false;
                    foreach (var item in prs)
                    {
                        if (item.Id == pId)
                        {
                            isOpen = true;
                            break;
                        }
                        
                    }
                    if (!isOpen)
                    {
                        openPro();
                    }
                   
                    
                }
                void openPro()
                {
                    var psi = new ProcessStartInfo("npx")
                    {
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal,
                        UseShellExecute = true,
                        WorkingDirectory = _exePath,
                        Arguments = "hardhat node " + HardhatParams


                    };
                    var _process = Process.Start(psi);
                    var txt = File.CreateText(path);
                    txt.Close();
                    File.WriteAllText(path, _process.Id.ToString());

                    Thread.Sleep(8000);
                    //添加测试参数

                    var deployP = new ProcessStartInfo("npx")
                    {
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal,
                        UseShellExecute = true,
                        WorkingDirectory = _exePath,
                        Arguments = DeployStr

                    };
                    //Process.Start(deployP);
                    var flashswapP = new ProcessStartInfo("npx")
                    {
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal,
                        UseShellExecute = true,
                        WorkingDirectory = _exePath,
                        Arguments = flashswap

                    };
                    Process.Start(flashswapP);
                }

            }


            Thread.Sleep(3000);
        }

        public void Dispose()
        {
            if (!_process.HasExited)
            {
                _process.Kill();
            }

            Thread.Sleep(2000);
            DeleteData();
        }

        private void DeleteData()
        {
            var attempts = 0;
            var success = false;

            while (!success && attempts < 2)
            {
                try
                {
                    InnerDeleteData();
                    success = true;
                }
                catch
                {
                    Thread.Sleep(1000);
                    attempts = attempts + 1;
                }
            }
        }

        private void InnerDeleteData()
        {
            if (EthereumClient == EthereumClient.Geth)
            {
                var pathData = Path.Combine(Path.GetDirectoryName(_exePath), @"devChain\geth");

                if (Directory.Exists(pathData))
                {
                    Directory.Delete(pathData, true);
                }

            }
            else if (EthereumClient == EthereumClient.OpenEthereum)
            {
                var pathData = Path.Combine(Path.GetDirectoryName(_exePath), @"parity0\chains");

                if (Directory.Exists(pathData))
                {
                    Directory.Delete(pathData, true);
                }
            }

        }
    }
}
