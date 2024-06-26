async function main() {
  const CosmicRelic = await ethers.getContractFactory("CosmicRelic");
  
  const baseTokenURI = "https://space-legends.luca-dc.ch/api/token/";
  
  const cosmicRelic = await CosmicRelic.deploy(baseTokenURI);

  console.log("CosmicRelic deployed to:", cosmicRelic);
}

main()
  .then(() => process.exit(0))
  .catch(error => {
    console.error(error);
    process.exit(1);
  });
