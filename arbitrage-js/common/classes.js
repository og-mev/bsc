
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

class SwapHelper {
    // 调用参数规范
    // let swapArr = [{
    //     symbol: "pancakeswap",
    //     amountIn: 10,
    //     amountOutMin: 10,
    //     path: ["0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56"]
    // }];
    static convert2calldata(arr = []) {
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
        let params = [symbols, amountIns, amountOutMins, pathSlices, paths];
        return params;

        // let symbols = [1, 2];
        // let amountIns = [hre.ethers.utils.parseEther("1.0"), hre.ethers.utils.parseEther("290.0")];
        // let amountOutMins = [hre.ethers.utils.parseEther("290"), hre.ethers.utils.parseEther("0.8")];
        // let paths = ["0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0x55d398326f99059ff775485246999027b3197955", "0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c","0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56"];
        // let pathSlices = [2, 4];
        // const swapTx = await greeter.multiSwap(symbols, amountIns, amountOutMins, pathSlices, paths);
    }

}

module.exports = { ERC20, SwapPair, SwapHelper };