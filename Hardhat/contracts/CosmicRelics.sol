// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract CosmicRelic is ERC721, Ownable {
    // Base URL for token metadata (if static: IPFS, else some API backend)
    string private _baseTokenURI;

	// Stores the creation (minting) of every NFToken
	mapping(uint256 => uint256) private _tokenCreationTimestamps;

    // Stores every owner address of every NFToken (this system does not stores the player's name/id, useless?)
    mapping(uint256 => address[]) private _ownershipHistory;

    constructor(string memory baseTokenURI) ERC721("CosmicRelic", "CREL") {
        setBaseURI(baseTokenURI);
    }
	
	function setBaseURI(string memory baseTokenURI) public onlyOwner {
        _baseTokenURI = baseTokenURI;
    }

    function _baseURI() internal view override returns (string memory) {
        return _baseTokenURI;
    }

	// Build a dynamic token URL for metadata. E.g. https://api.space-legends/token/12
    function tokenURI(uint256 tokenId) public view override(ERC721) returns (string memory) {
        require(_exists(tokenId), "ERC721URI: URI query for nonexistent token");
        return string(abi.encodePacked(_baseURI(), Strings.toString(tokenId)));
    }

	// TDB after or before?
    function _beforeTokenTransfer(address from, address to, uint256 tokenId) internal override(ERC721) {
        super._beforeTokenTransfer(from, to, tokenId);
        if (from != address(0)) {
            _ownershipHistory[tokenId].push(to);
        }
    }
	
	function mint(address to, uint256 tokenId) public onlyOwner {
        _mint(to, tokenId);
		_ownershipHistory[tokenId].push(to);
		_tokenCreationTimestamps[tokenId] = block.timestamp;
    }

    // Fonction pour récupérer l'historique des propriétaires d'un NFT
    function getOwnershipHistory(uint256 tokenId) public view returns (address[] memory) {
		require(_exists(tokenId), "ERC721Metadata: History query for nonexistent token");
        return _ownershipHistory[tokenId];
    }
	
	function getTokenCreationTimestamp(uint256 tokenId) public view returns (uint256) {
		require(_exists(tokenId), "ERC721Metadata: Timestamp query for nonexistent token");
		return _tokenCreationTimestamps[tokenId];
	}
	
}
