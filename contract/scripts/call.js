const hre = require('hardhat');

async function main() {
    const Greeter = await hre.ethers.getContractFactory("Greeter");
    const greeter = await Greeter.attach("0xf2767Ac35Eb088cB44dF17C2ffF37A6ceEdd5676");
    const setGreetingTx = await greeter.setGreeting("Hola, sadfkjalskjfdsa!");
    await setGreetingTx.wait();
    console.log("end!");
}

main()
  .then(() => process.exit(0))
  .catch((error) => {
    console.error(error);
    process.exit(1);
  });