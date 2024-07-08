// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/extensions/ERC20Burnable.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract StarDust is ERC20, ERC20Burnable, Ownable {

    constructor() ERC20("StarDust", "SDT") Ownable(msg.sender) {
        // Mint initial supply
        _mint(msg.sender, 1 * 10 ** decimals());
    }

    // Function to mint tokens - only the owner of the contract can call this function
    function mint(address to, uint256 amount) public onlyOwner {
        _mint(to, amount);
    }
    
    // Allows the owner of the contract to burn tokens from a specified address
    // This is to avoid any account to pay gas fee for a SDT sync between their player account and their wallet
    // If the game shuts down, the owner must renounceOwnership on this contract and this function cant be used anymore :)
    function burnFor(address account, uint256 amount) public onlyOwner {
        require(balanceOf(account) >= amount, "Insufficient balance to burn");
        _burn(account, amount);
    }
	
}
