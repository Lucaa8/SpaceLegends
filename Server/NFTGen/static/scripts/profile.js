document.addEventListener('DOMContentLoaded', function () {
    //Display tooltip with level xp on top of level div
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

//Add the logic to rotate the cards on click
const cards = document.getElementsByClassName("flip-card");
for(let card of cards) {
    card.addEventListener('click', function () {
        card.children[0].classList.toggle('flipped');
    });
}

const profilePicForm = document.getElementById('profilePicForm');
const profilePicInput = document.getElementById('profilePicInput');
const uploadPicUrl = profilePicForm.getAttribute('data-url');

profilePicInput.addEventListener('change', async function (e) {
    const file = e.target.files[0];
    if (file) {
        if (file.size > 5 * 1024 * 1024) { // 5 MB in bytes
            showErrorToast('File size must be less than 3MB.');
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
    const response = await authenticatedRequest(uploadPicUrl, {
        method: 'POST',
        body: new FormData(profilePicForm)
    });
    //todo response
}

async function _uploadFile() {
    const response = await authenticatedRequest('deleteurl', { method: 'DELETE' });
    //todo response
}