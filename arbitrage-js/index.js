const { ethers } = require("ethers");
const { ERC20, SwapPair, SwapHelper } = require("./common/classes");
async function main() {
    console.log("hello world!");
    await sleep(2000);
    //demo params array
    let swapArr = [{
        symbol: "pancakeswap",
        amountIn: 10,
        amountOutMin: 10,
        path: ["0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56"]
    }, {
        symbol: "pancakeswap",
        amountIn: 10,
        amountOutMin: 10,
        path: ["0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56"]
    }];
    let params = SwapHelper.convert2calldata(swapArr);
    console.log(params);
}

function sleep(ms) {
    return new Promise((resolve) => {
        setTimeout(resolve, ms);
    });
}

// We recommend this pattern to be able to use async/await everywhere
// and properly handle errors.
main()
    .then(() => process.exit(0))
    .catch((error) => {
        console.error(error);
        process.exit(1);
    });
