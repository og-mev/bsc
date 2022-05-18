
const { ethers } = require("ethers");
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
    constructor(symbol, address = "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", rpcUrl = null, swapAbi = null) {
        this.symbol = symbol;
        this.address = address;
        this.#set_swap_abi(swapAbi);
        this.#init_provider(rpcUrl);
        this.#attach_swap_contract();

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
        // let abi = require("../contract/artifacts/contracts/flashswap.sol/Flashswap.json").abi;
        let abi = [
            // Some details about the token
            "function name() view returns (string)",
            "function symbol() view returns (string)",

            // Get the account balance
            "function balanceOf(address) view returns (uint)"
        ];

        return abi;
    }
    async #attach_swap_contract() {
        let contract = new ethers.Contract(this.address, this.bridgeAbi, this.provider);
        let name = await contract.name()
        console.log("contract name: " + name);
        this.contract = contract;

    }

    swap(arr = []) {
        arr.forEach(element => {
            console.log(element);
        });
        // let symbols = [1, 2];
        // let amountIns = [hre.ethers.utils.parseEther("1.0"), hre.ethers.utils.parseEther("290.0")];
        // let amountOutMins = [hre.ethers.utils.parseEther("290"), hre.ethers.utils.parseEther("0.8")];
        // let paths = ["0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0x55d398326f99059ff775485246999027b3197955", "0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c","0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56"];
        // let pathSlices = [2, 4];
        // const swapTx = await greeter.multiSwap(symbols, amountIns, amountOutMins, pathSlices, paths);
    }

}

module.exports = { ERC20, SwapPair, SwapBridge };