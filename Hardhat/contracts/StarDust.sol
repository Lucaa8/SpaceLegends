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
    
    // Recover tokens accidentally sent to the contract
    // This function allows the contract owner to recover ERC20 tokens that were sent to this contract address by mistake.
    // This can be crucial in preventing accidental loss of tokens and provides a way to return them to their rightful owner.
    function recoverToken(address tokenAddress, uint256 tokenAmount) public onlyOwner {
        require(tokenAmount > 0, "Amount must be greater than 0");
        IERC20(tokenAddress).transfer(owner(), tokenAmount);
    }
	
}
