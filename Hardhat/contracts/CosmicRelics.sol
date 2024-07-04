// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721Burnable.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

struct OwnershipHistory {
    address owner;
    string userId;
}

contract CosmicRelic is ERC721, ERC721Burnable, Ownable {

    // Base URL for token metadata (API backend)
    string private _baseTokenURI;
	
	// Stores the total crel supply. Incremented on mint and decremented on burn.
	uint256 private _totalSupply;

	// Stores the creation (minting) of every NFT
	mapping(uint256 => uint256) private _tokenCreationTimestamps;

    // Stores every owner of every NFT
    mapping(uint256 => OwnershipHistory[]) private _ownershipHistory;
	
	// Stores which image (piece of any collection) the NFTs will hold. Also stores the rarity of the piece
	// The mask is the following (collection << 27) | (row << 11) | (column << 3) | rarity
	mapping(uint256 => uint256) private _tokenType;

    constructor(string memory baseTokenURI) ERC721("Cosmic Relic", "CREL") Ownable(msg.sender) {
        setBaseURI(baseTokenURI);
    }
	
	function setBaseURI(string memory baseTokenURI) public onlyOwner {
        _baseTokenURI = baseTokenURI;
    }

    function _baseURI() internal view override returns (string memory) {
        return _baseTokenURI;
    }
	
	// Return true if the given tokenId is linked to an owner, false otherwise (the NFT does not exist)
	function exists(uint256 tokenId) public view returns (bool) {
		try this.ownerOf(tokenId) returns (address) {
			return true;
		} catch {
			return false;
		}
	}

	// Build a dynamic token URL for metadata. E.g. https://space-legends.luca-dc.ch/token/12
    function tokenURI(uint256 tokenId) public view override(ERC721) returns (string memory) {
        require(exists(tokenId), "ERC721URI: URI query for nonexistent token");
        return string(abi.encodePacked(_baseURI(), Strings.toString(tokenId)));
    }
	
	// Generate a new NFT, add the first account in the history list, set the token creation to now and set the token type.
	function mint(address addrTo, string memory uidTo, uint256 tokenId, uint256 tokenType) public onlyOwner {
        _mint(addrTo, tokenId);
		_totalSupply++;
		_ownershipHistory[tokenId].push(OwnershipHistory(addrTo, uidTo));
		_tokenCreationTimestamps[tokenId] = block.timestamp;
		_tokenType[tokenId] = tokenType;
    }
	
    // This method should clear _ownershipHistory for this tokenId. Otherwise, when a new token is created with this id, it would carry the older token's history
	// This method override the ERC721Burnable#burn method to decrement _totalSupply is burn call is successfull
    function burn(uint256 tokenId) public override {
		require(exists(tokenId), "ERC721Burn: burn query for nonexistent token");
		super.burn(tokenId); // The ERC721Burnable#burn is calling the _update method of ERC721 which already checks if the caller is the owner or approved before burning the token.
		_totalSupply--;
    }
    
    // Custom transfer function that doesn't require approve + transferFrom
    // Also adds the new owner inside the history list
    function safeTransfer(uint256 tokenId, address newOwnerAddr, string memory newOwnerUid) public {
        require(_isAuthorized(ownerOf(tokenId), msg.sender, tokenId), "ERC721: transfer caller is not owner nor approved");
        _safeTransfer(msg.sender, newOwnerAddr, tokenId);
        _ownershipHistory[tokenId].push(OwnershipHistory(newOwnerAddr, newOwnerUid));
    }

    // Get the history list
    function getOwnershipHistory(uint256 tokenId) public view returns (OwnershipHistory[] memory) {
		require(exists(tokenId), "ERC721Metadata: History query for nonexistent token");
        return _ownershipHistory[tokenId];
    }
	
	// Get the token creation date
	function getTokenCreationTimestamp(uint256 tokenId) public view returns (uint256) {
		require(exists(tokenId), "ERC721Metadata: Timestamp query for nonexistent token");
		return _tokenCreationTimestamps[tokenId];
	}
	
	// Get the token type (collection, row, col and rarity packed)
	function getTokenType(uint256 tokenId) public view returns (uint256) {
		require(exists(tokenId), "ERC721Metadata: Token type query for nonexistent token");
		return _tokenType[tokenId];
	}
	
	// Return the current totalSupply of crel
	function totalSupply() public view returns (uint256) {
        return _totalSupply;
    }
	
}