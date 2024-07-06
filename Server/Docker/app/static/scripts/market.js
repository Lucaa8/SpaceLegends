let currentListings = {}
let userSDT = -1;
let collec = -1;
let row = -1;
let col = -1;
let rarity = "All Rarities";
let maxPrice = 500.0;

function openConfirmationModel(nftId) {
    f = async function()
    {
        buy(nftId);
    }
    setConfirmationNote('(Please note that while this NFT will appear instantly on your profile, it may take up to 24 hours to be visible in Etherscan or on your OpenSea page. Additionally, you will not be able to relist this NFT for sale on the market until this period has elapsed)');
    let myModal = new bootstrap.Modal(document.getElementById('confirmationModal'), {});
    myModal.show();
}

async function buy(nftId)
{
    let myModal = bootstrap.Modal.getInstance(document.getElementById('nftModal'));
    myModal.hide();
    let r = await authenticatedRequest("/api/buy-nft/"+nftId.substring(1), { method: "POST" });
    if(r.ok)
    {
        currentListings = currentListings.filter(item => item['nft']['id'] !== nftId);
        document.getElementById('listing-'+nftId.substring(1)).remove();
        updateListings();
        userSDT = -1;
        showInfoToast("You successfully bought the NFT " + nftId + "!");
    }
    else
    {
        if(!r.headers.has("Content-Type") || r.headers.get("Content-Type") !== 'application/json') {
            showErrorToast('An unknown error occurred while buying the NFT. Please retry later.');
        }
        else
        {
            const jrep = await r.json();
            showErrorToast(jrep.message);
        }
    }
}

function initListings(base64String)
{
    // Decode base64 string to get the json content and then parse it!
    currentListings = JSON.parse(atob(base64String));
}

function _listingStatut(nftId, show)
{
    document.getElementById("listing-"+nftId.substring(1)).style.display = show ? "block" : "none";
    return show;
}

function updateListings()
{
    let counter = 0;
    for(let listing of currentListings)
    {
        const nft = listing["nft"];
        const isCollec = collec === -1 || collec === parseInt(nft["Collection"]);
        const isRow = row === -1 || row === nft["Image Row"];
        const isCol = col === -1 || col === nft["Image Column"];
        const isRarity = rarity === "All Rarities" || rarity === nft["Rarity"][0];
        const isPrice = listing["price"] <= maxPrice;
        if(_listingStatut(nft["id"], isCollec && isRow && isCol && isRarity && isPrice))
        {
            counter++;
        }
    }
    document.getElementById('listings-count').innerText = "There are " + counter.toString() + " relics listed for sale.";
    document.getElementById('empty').style.display = counter === 0 ? "block" : "none";
}

function findNft(nftId)
{
    for(let listing of currentListings)
    {
        if(listing["nft"]["id"] === nftId)
        {
            return listing;
        }
    }
    return null;
}

// Fetch for the first time to the server, then stores it in the NFT's object so next times the user opens this NFT's details, I do not need to re-fetch it
async function fetchHistory(nft)
{
    if(nft.history === undefined)
    {
        let r = await fetch("/api/history/"+nft.id.substring(1));
        if(!r.headers.has("Content-Type") || r.headers.get("Content-Type") !== 'application/json') {
            showErrorToast('An unknown error occurred while fetching NFT\'s history. You can still buy the NFT.');
        }
        else
        {
            const jrep = await r.json();
            if(r.ok) {
                nft.history = jrep;
            } else {
                showErrorToast("An error occurred while fetching NFT's history. ("+jrep.message+"). You can still buy the NFT.");
            }
        }
    }
    return nft.history;
}

// Fetch for the first time to the server, then stores it in userSDT so next times the user opens this NFT's details, I do not need to re-fetch it
async function fetchSDT()
{
    if(userSDT === -1)
    {
        let r = await authenticatedRequest("/api/money-sdt", { headers: {} });
        if(r.ok)
        {
            userSDT = (await r.json())["money"];
        }
    }
    return userSDT;
}

async function listingClick(nftId)
{
    const listing = findNft(nftId);
    if(listing === null)
    {
        showErrorToast('This NFT seems to not exist anymore!');
        await new Promise(r => setTimeout(r, 500)); // Waits that the nft modal window is opened
        bootstrap.Modal.getInstance(document.getElementById('nftModal')).hide();
        return;
    }
    const nft = listing["nft"];
    document.getElementById('listing-nft-name').innerText = nft["name"] + " " + nft["id"];
    document.getElementById('listing-nft-description').innerText = nft["description"];
    let rarityModal = document.getElementById('listing-nft-rarity');
    rarityModal.innerText = nft['Rarity'][0]
    rarityModal.classList.remove(...['r-legendary', 'r-epic', 'r-rare', 'r-common']);
    rarityModal.classList.add(nft['Rarity'][1]);
    document.getElementById('listing-nft-img').src = nft["image"];
    document.getElementById('listing-nft-collection').innerText = nft["Collection"];
    document.getElementById('listing-nft-row').innerText = nft["Image Row"];
    document.getElementById('listing-nft-col').innerText = nft["Image Column"];
    document.getElementById('listing-nft-created').innerText = nft["created"].replace("T", " at ");

    document.getElementById('listing-seller').innerHTML = "<a target='_blank' href="+listing["author"]["profile"]+">"+listing["author"]["name"]+"</a>";
    document.getElementById('listing-price').innerText = listing["price"] + " SDT";

    let ulOwnershipHistoryNFT = document.getElementById('listing-nft-history');
    ulOwnershipHistoryNFT.innerHTML = '<li>Fetching history...</li>'
    let history = await fetchHistory(nft);
    if(history !== undefined && history.length > 0)
    {
        ulOwnershipHistoryNFT.innerHTML = '';
        for(let owner of history)
        {
            const li = document.createElement('li');
            const addr = document.createElement('a');
            addr.href = 'https://testnets.opensea.io/' + owner[0];
            addr.target = '_blank';
            addr.textContent = owner[0].slice(0, 7) + "..." + owner[0].slice(-5);
            li.appendChild(addr);
            li.appendChild(document.createTextNode(' - '));
            const user = document.createElement('a');
            user.href = '/profile/' + owner[1];
            user.target = '_blank';
            user.textContent = owner[1];
            li.appendChild(user);
            ulOwnershipHistoryNFT.appendChild(li)
        }
    }
    const sdt = await fetchSDT();
    if(sdt !== -1)
    {
        document.getElementById('user-sdt').innerText = `(You have ${sdt} SDT)`;
        document.getElementById('listing-buy-btn').onclick = () => openConfirmationModel(nftId);
    }
}

/* Max price filter */
document.getElementById('maxPrice').addEventListener('input', function() {
    document.getElementById('priceValue').textContent = this.value;
});
// The update listings is done in the change event which is triggered when the user release the mouse/finger from the slider. To avoid spamming update while dragging
document.getElementById('maxPrice').addEventListener('change', function() {
    maxPrice = parseFloat(this.value);
    updateListings();
});

/* Dropdown filters */
document.querySelectorAll('#locationDropdown + .dropdown-menu .dropdown-item').forEach(item => {
    item.addEventListener('click', function() {
        document.getElementById('locationDropdown').textContent = this.textContent;
        row = parseInt(item.getAttribute("data-r"));
        col = parseInt(item.getAttribute("data-c"));
        updateListings();
    });
});

document.querySelectorAll('#collectionDropdown + .dropdown-menu .dropdown-item').forEach(item => {
    item.addEventListener('click', function() {
        document.getElementById('collectionDropdown').textContent = this.textContent;
        collec = parseInt(item.getAttribute("data-c"));
        updateListings();
    });
});

document.querySelectorAll('#rarityDropdown + .dropdown-menu .dropdown-item').forEach(item => {
    item.addEventListener('click', function() {
        document.getElementById('rarityDropdown').textContent = this.textContent;
        rarity = this.textContent;
        updateListings();
    });
});