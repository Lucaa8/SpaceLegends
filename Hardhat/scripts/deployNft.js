async function main() {
  const CosmicRelic = await ethers.getContractFactory("CosmicRelic");
  const cosmicRelic = await CosmicRelic.deploy(); //baseTokenURI argument?

  console.log("CosmicRelic deployed to:", cosmicRelic.address);
}

main()
  .then(() => process.exit(0))
  .catch(error => {
    console.error(error);
    process.exit(1);
  });
