package ch.luca.spacelegends.Controllers;

import org.springframework.web.bind.annotation.PathVariable;
import org.web3j.protocol.Web3j;
import org.web3j.protocol.http.HttpService;
import org.springframework.web.bind.annotation.GetMapping;

import java.util.List;

public class NftMetadataController {

    private final Web3j web3j = Web3j.build(new HttpService("https://<votre-noeud-ethereum>"));
    private final String contractAddress = "<adresse-de-votre-contrat>";
    private final String contractABI = "<ABI-de-votre-contrat>";

    @GetMapping("/token/{tokenId}")
    public NftMetadata getTokenMetadata(@PathVariable String tokenId) throws Exception {
        return new NftMetadata("Token #" + tokenId, "Earth N°1", "https://bafybeie4n7ygwhczxutgdtkjvr4mmdbbwdgejf6jrwz3d47xonhir6lnja.ipfs.w3s.link/00_Earth_r1c1.png", List.of("Luca"));
    }

    public static class NftMetadata {
        private String name;
        private String description;
        private String image;
        private List<String> ownershipHistory;

        public NftMetadata(String name, String description, String image, List<String> ownershipHistory) {
            this.name = name;
            this.description = description;
            this.image = image;
            this.ownershipHistory = ownershipHistory;
        }

        public String getName() {
            return name;
        }

        public String getDescription() {
            return description;
        }

        public String getImage() {
            return image;
        }

        public List<String> getOwnershipHistory() {
            return ownershipHistory;
        }
    }

}
