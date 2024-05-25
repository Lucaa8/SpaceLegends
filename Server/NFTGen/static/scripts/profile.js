//Add the logic to rotate the cards on click
const cards = document.getElementsByClassName("flip-card");
for(let card of cards) {
    card.addEventListener('click', function () {
        card.children[0].classList.toggle('flipped');
    });
}

async function _displayMessageAndUpdate(response) {

    if(!response.headers.has("Content-Type") || response.headers.get("Content-Type") !== 'application/json') {
        showErrorToast('An unexcepted error occurred, please retry later.');
        return;
    }

    const jrep = await response.json();

    if(response.ok) {
        showInfoToast(jrep.message);
        document.getElementById('profilePic').src = jrep.path;
    } else {
        showErrorToast(jrep.message);
    }
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
    await _displayMessageAndUpdate(response);
}

async function _uploadFile() {
    const response = await authenticatedRequest(uploadPicUrl, {
        method: 'POST',
        body: new FormData(profilePicForm)
    });
    await _displayMessageAndUpdate(response);
}