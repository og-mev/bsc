// We require the Hardhat Runtime Environment explicitly here. This is optional
// but useful for running the script in a standalone fashion through `node <script>`.
//
// When running the script with `npx hardhat run <script>` you'll find the Hardhat
// Runtime Environment's members available in the global scope.
const hre = require("hardhat");

async function main() {
    // Hardhat always runs the compile task when running scripts with its command
    // line interface.
    //
    // If this script is run directly using `node` you may want to call compile
    // manually to make sure everything is compiled
    // await hre.run('compile');

    // We get the contract to deploy
    const Greeter = await hre.ethers.getContractFactory("Flashswap");
    const greeter = await Greeter.deploy();
    await greeter.deployed();
    const swapTx = await greeter.swap([{
        symbol: 1,
        pair: "0xf2767ac35eb088cb44df17c2fff37a6ceedd5676",
        amount: 2,
        amountOutMin: 3,
        fromToken: "0xf2767ac35eb088cb44df17c2fff37a6ceedd5676",
        toToken: "0xf2767ac35eb088cb44df17c2fff37a6ceedd5676"
    }]);
    await swapTx.wait();
    console.log("Greeter deployed to:", greeter.address);
}

// We recommend this pattern to be able to use async/await everywhere
// and properly handle errors.
main()
    .then(() => process.exit(0))
    .catch((error) => {
        console.error(error);
        process.exit(1);
    });
