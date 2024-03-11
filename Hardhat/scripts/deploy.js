async function main() {
  const StarDust = await ethers.getContractFactory("StarDust");
  const starDust = await StarDust.deploy();

  await starDust.deployed();

  console.log("StarDust deployed to:", starDust.address);
}

main()
  .then(() => process.exit(0))
  .catch(error => {
    console.error(error);
    process.exit(1);
  });
