async function main() {
  const CosmicRelic = await ethers.getContractFactory("CosmicRelic");
  
  const baseTokenURI = "http://localhost/token/";
  
  const cosmicRelic = await CosmicRelic.deploy(baseTokenURI);

  console.log("CosmicRelic deployed to:", cosmicRelic.address);
}

main()
  .then(() => process.exit(0))
  .catch(error => {
    console.error(error);
    process.exit(1);
  });
