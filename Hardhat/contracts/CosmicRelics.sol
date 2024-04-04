// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

struct OwnershipHistory {
    address owner;
    string userId;
}

contract CosmicRelic is ERC721, Ownable {

    // Base URL for token metadata (if static: IPFS, else some API backend)
    string private _baseTokenURI;

	// Stores the creation (minting) of every NFT
	mapping(uint256 => uint256) private _tokenCreationTimestamps;

    // Stores every owner of every NFT
    mapping(uint256 => OwnershipHistory[]) private _ownershipHistory;
	
	// Stores which image (piece of any collection) the NFTs will hold. E.g 010202 for the Mars collection, center piece (row 2, col 2), then packed into a single uint: 66050
	mapping(uint256 => uint256) private _tokenType;

    constructor(string memory baseTokenURI) ERC721("CosmicRelic", "CREL") Ownable(msg.sender) {
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

	// Build a dynamic token URL for metadata. E.g. https://api.space-legends/token/12
    function tokenURI(uint256 tokenId) public view override(ERC721) returns (string memory) {
        require(exists(tokenId), "ERC721URI: URI query for nonexistent token");
        return string(abi.encodePacked(_baseURI(), Strings.toString(tokenId)));
    }
	
	// Adding the a new owner inside the history list. Needs to be called after transfer event
	function pushNewOwner(uint256 tokenId, address newOwnerAddr, string memory newOwnerUid) public onlyOwner {
		require(exists(tokenId), "ERC721Metadata: History query for nonexistent token");
		_ownershipHistory[tokenId].push(OwnershipHistory(newOwnerAddr, newOwnerUid));
	}
	
	// Generate a new NFT, add the first account in the history list, set the token creation to now and set the token type.
	function mint(address addrTo, string memory uidTo, uint256 tokenId, uint256 tokenType) public onlyOwner {
        _mint(addrTo, tokenId);
		_ownershipHistory[tokenId].push(OwnershipHistory(addrTo, uidTo));
		_tokenCreationTimestamps[tokenId] = block.timestamp;
		_tokenType[tokenId] = tokenType;
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
	
	// Get the token type (collection, row and col packed)
	function getTokenType(uint256 tokenId) public view returns (uint256) {
		require(exists(tokenId), "ERC721Metadata: Token type query for nonexistent token");
		return _tokenType[tokenId];
	}
	
}
