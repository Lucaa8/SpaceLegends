// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/extensions/ERC20Burnable.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract StarDust is ERC20, ERC20Burnable, Ownable {

    constructor() ERC20("StarDust", "SDT") Ownable(msg.sender) {
        // Mint initial supply
        _mint(msg.sender, 1000000 * 10 ** decimals());
    }

    // Function to mint tokens - only the owner of the contract can call this function
    function mint(address to, uint256 amount) public onlyOwner {
        _mint(to, amount * 10 ** decimals());
    }
	
}
