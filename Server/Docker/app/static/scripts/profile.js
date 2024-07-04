//Add the logic to rotate the cards on click
const cards = document.getElementsByClassName("flip-card");
for(let card of cards) {
    card.addEventListener('click', function (e) {
        if(e.target.closest('.btn') || e.target.closest('input'))
        {
            return;
        }
        card.children[0].classList.toggle('flipped');
    });
}

async function _displayMessageAndUpdate(response, okCallback) {

    if(response.status === 204)
    {
        okCallback(null);
        return;
    }

    if(!response.headers.has("Content-Type") || response.headers.get("Content-Type") !== 'application/json') {
        showErrorToast('An unexcepted error occurred, please retry later.');
        return;
    }

    const jrep = await response.json();

    if(response.ok) {
        okCallback(jrep);
    } else {
        showErrorToast(jrep.message);
    }
}

function confirmRemoveListing(nftId, url)
{
    f = async function()
    {
        await removeListing(nftId, url);
    }
}

function confirmListing(nftId, url)
{
    f = async function()
    {
        await addListing(nftId, url);
    }
}

async function addListing(nftId, url)
{
    const response = await authenticatedRequest(url, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ price: document.getElementById('priceInput-'+nftId).value })
    });
    await _displayMessageAndUpdate(response, (jrep) => {
        showInfoToast("Your listing has been added on the market successfully");
        document.getElementById('nft-list-'+nftId).style.display = 'none';
        document.getElementById('nft-listed-'+nftId).style.display = 'block';
    });
}

async function removeListing(nftId, url)
{
    const response = await authenticatedRequest(url, { method: 'DELETE' });
    await _displayMessageAndUpdate(response, (jrep) => {
        showInfoToast("Your listing has been removed from the market successfully");
        document.getElementById('nft-listed-'+nftId).style.display = 'none';
        document.getElementById('nft-list-'+nftId).style.display = 'block';
    });
}

const profilePicForm = document.getElementById('profilePicForm');
const profilePicInput = document.getElementById('profilePicInput');
const uploadPicUrl = profilePicForm.getAttribute('data-update-url');
const deletePicUrl = profilePicForm.getAttribute('data-delete-url');

profilePicInput.addEventListener('change', async function (e) {
    const file = e.target.files[0];
    if (file) {
        if (file.size > 5 * 1024 * 1024) { // 5 MB in bytes
            showErrorToast('File size must be less than 5MB.');
        } else if(file.type !== 'image/png' && file.type !== 'image/jpeg') {
            showErrorToast('File type must be image/png or image/jpeg.');
        } else {
            await _uploadFile();
            profilePicForm.reset();
        }
    } else {
        showErrorToast('No file selected.');
    }
});

function changeProfilePic() {
    profilePicInput.click();
}

async function deleteProfilePic() {
    const response = await authenticatedRequest(deletePicUrl, { method: 'DELETE' });
    await _displayMessageAndUpdate(response, (jrep) => {
        showInfoToast(jrep.message);
        document.getElementById('profilePic').src = jrep.path;
    });
}

async function _uploadFile() {
    const response = await authenticatedRequest(uploadPicUrl, {
        method: 'POST',
        body: new FormData(profilePicForm)
    });
    await _displayMessageAndUpdate(response, (jrep) => {
        showInfoToast(jrep.message);
        document.getElementById('profilePic').src = jrep.path;
    });
}

async function copyWallet(element) {
    await navigator.clipboard.writeText(document.getElementById("walletAddr").innerText);

    element.classList.add("clicked");
    setTimeout(() => {
        element.classList.remove("clicked");
    }, 300);

}