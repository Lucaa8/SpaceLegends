const btn = document.getElementById('btnSearchNFT')
const tbx = document.getElementById('tbxSearchNFT')
const alert = document.getElementById('alertSearchNft')

function searchNFT()
{
    alert.hidden = true;
    const searchValue = tbx.value;
    if(searchValue === '')
    {
        alert.innerText = "Cannot search empty id"
        alert.hidden = false;
        return;
    }
    btn.setAttribute("disabled", "");
    const url = btn.getAttribute("data-url").replace('0', searchValue)
    fetch(url, {
        method: 'GET',
    })
    .then(response => {
        btn.removeAttribute("disabled");
        if (!response.ok) {
            throw "Unknown token id. This NFT does not exist."
        }
        return response.json();
    })
    .then(data => {
        tbx.value = ''
        displaySearchResults(data);
    })
    .catch(error => {
        alert.innerText = error
        alert.hidden = false;
    });
}

const imgNFT = document.getElementById('imgNFT')
const txtTitleNFT = document.getElementById('txtTitleNFT')
const txtRarityNFT = document.getElementById('txtRarityNFT')
const txtDescriptionNFT = document.getElementById('txtDescriptionNFT')
const txtCreationDateNFT = document.getElementById('txtCreationDateNFT')
const txtCollectionNFT = document.getElementById('txtCollectionNFT')
const txtRowNFT = document.getElementById('txtRowNFT')
const txtColNFT = document.getElementById('txtColNFT')
const ulOwnershipHistoryNFT = document.getElementById('ulOwnershipHistoryNFT')
const rarityClasses = ['r-legendary', 'r-epic', 'r-rare', 'r-common']

function displaySearchResults(result) {
    if("id" in result)
    {
        txtTitleNFT.children[0].innerText = '#'+result['id']
    }
    if("image" in result)
    {
        imgNFT.src = result['image']
    }
    if("name" in result)
    {
        txtTitleNFT.firstChild.nodeValue = result['name']
    }
    if("description" in result)
    {
        txtDescriptionNFT.innerText = result["description"]
    }
    if("rarity" in result)
    {
        txtRarityNFT.innerText = result['rarity'][0]
        txtRarityNFT.classList.remove(...rarityClasses);
        txtRarityNFT.classList.add(result['rarity'][1]);
    }
    if("ownership_history" in result)
    {
        ulOwnershipHistoryNFT.innerHTML = ''
        for(let owner of result['ownership_history'])
        {
            const li = document.createElement('li')
            li.appendChild(document.createTextNode(`${owner[0]} - `))
            const txt = document.createElement('span')
            txt.className = 'text-muted'
            txt.textContent = owner[1]
            li.appendChild(txt)
            ulOwnershipHistoryNFT.appendChild(li)
        }
    }
    for(let attr of result['attributes'])
    {
        if(attr['trait_type'] === 'Creation')
        {
            const date = new Date(attr['value'] * 1000).toISOString()
            txtCreationDateNFT.innerText = date.substring(0, date.length-5).replace('T', ' at ')
        }
        if(attr['trait_type'] === 'Collection')
        {
            txtCollectionNFT.innerText = attr['value']
        }
        if(attr['trait_type'] === 'Image Row')
        {
            txtRowNFT.innerText = attr['value']
        }
        if(attr['trait_type'] === 'Image Column')
        {
            txtColNFT.innerText = attr['value']
        }
    }
}
