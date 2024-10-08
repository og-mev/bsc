
const { ethers } = require("ethers");
const exchanges = { "pancakeswap": 1, "biswap": 2 };
class ERC20 {
    constructor(symbol, fullname, address, decimals) {
        this.symbol = symbol;
        this.fullname = fullname;
        this.address = address;
        this.decimals = decimals;
    }
}
class SwapPair {
    constructor(symbol, exchange, token0, token1, reverse0, reverse1) {
        this.symbol = symbol;
        this.exchange = exchange;
        this.token0 = token0;
        this.token1 = token1;
        this.reverse0 = reverse0;
        this.reverse1 = reverse1;
    }

    async up_reverse(reverse0, reverse1) {
        this.reverse0 = reverse0;
        this.reverse1 = reverse1;
    }
}

class SwapBridge {

    constructor(chain, address = "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", rpcUrl = null, swapAbi = null) {
        this.chain = chain;
        this.address = address;
        this.#set_swap_abi(swapAbi);
        this.#init_provider(rpcUrl);
        this.#attach_swap_contract();
        this.wallets = [];

    }
    #set_swap_abi(abi) {
        if (abi == null) {
            this.bridgeAbi = this.#get_default_abi();
        } else {
            this.bridgeAbi = bridgeAbi;
        }
    }
    #init_provider(url = "") {
        let provider;
        if (url) {
            if (url.startsWith("ipc")) {
                provider = new ethers.providers.IpcProvider(url);
            } else if (url.startsWith("http")) {
                provider = new ethers.providers.JsonRpcBatchProvider(url);
            } else if (url.startsWith("ws")) {
                provider = new ethers.providers.WebSocketProvider(url);
            }
        } else {
            provider = new ethers.providers.JsonRpcBatchProvider();
        }
        this.provider = provider;
    }

    #get_default_abi() {
        return require("../../contract/artifacts/contracts/flashswap.sol/Flashswap.json").abi;
    }
    async #attach_swap_contract() {
        let contract = new ethers.Contract(this.address, this.bridgeAbi, this.provider);
        let name = await contract.name()
        console.log("contract name: " + name);
        this.contract = contract;
    }
    async import_wallets(keys = []) {
        await Promise.all(keys.map(async (key) => {
            await this.#import_wallet(key);
        }));
    }
    async #import_wallet(key) {
        let wallet = new ethers.Wallet(key);
        let address = await wallet.getAddress();
        wallet = wallet.connect(this.provider);
        let nonce = await wallet.getTransactionCount();
        wallet.nonce = nonce;
        this.wallets.push(wallet);
        console.log("wallet nonce: " + nonce);
        console.log("wallet address: " + address);

    }
    #get_random_int(min, max) {
        min = Math.ceil(min);
        max = Math.floor(max);
        return Math.floor(Math.random() * (max - min) + min); //The maximum is exclusive and the minimum is inclusive
    }
    #get_random_wallet() {
        let index = this.#get_random_int(0, this.wallets.length);
        let wallet = this.wallets[index];
        return wallet;
    }
    // 调用参数规范
    // let swapArr = [{
    //     symbol: "pancakeswap",
    //     amountIn: 10,
    //     amountOutMin: 10,
    //     path: ["0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56"]
    // }];
    async swap(arr = []) {
        let symbols = [];
        let amountIns = [];
        let amountOutMins = [];
        let paths = [];
        let pathSlices = [];
        arr.forEach(element => {
            symbols.push(exchanges[element.symbol]);
            amountIns.push(element.amountIn);
            amountOutMins.push(element.amountOutMin);
            paths = paths.concat(element.path);
            pathSlices.push(element.path.length);
        });
        // console.log(paths);
        // console.log(pathSlices);
        let wallet = this.#get_random_wallet();
        let overrides = {
            nonce: wallet.nonce,
            gasLimit: 1500000,
            gasPrice: ethers.utils.parseUnits("5.0", "gwei"),
            value: 0
        };
        wallet.nonce = wallet.nonce + 1;
        let unsignedTx = await this.contract.populateTransaction["multiSwap"](symbols, amountIns, amountOutMins, pathSlices, paths, overrides);
        let signedTx = await wallet.signTransaction(unsignedTx);
        let provider = new ethers.providers.JsonRpcBatchProvider();//TODO 需要调用本地的高速IPC
        let tx = await provider.sendTransaction(signedTx);
        console.log("tx hash:", tx.hash);
    }

}

module.exports = { ERC20, SwapPair, SwapBridge };