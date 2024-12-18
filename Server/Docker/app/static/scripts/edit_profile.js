async function _displayResult(response) {

    if(!response.headers.has("Content-Type") || response.headers.get("Content-Type") !== 'application/json') {
        showErrorToast('An unexcepted error occurred, please retry later.');
        return false;
    }

    const jrep = await response.json();

    if(response.ok) {
        showInfoToast(jrep.message);
    } else {
        showErrorToast(jrep.message);
    }

    return response.ok;

}

async function changeDisplayname(url) {

    const response = await authenticatedRequest(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({'display_name': document.getElementById('displayname').value})
    });

    await _displayResult(response);

}

const currentPass = document.getElementById('currpassword');
const pass1 = document.getElementById('pass1')
const pass2 = document.getElementById('pass2')

async function changePassword(url) {

    if(pass1.value !== pass2.value) {
        showErrorToast('Passwords do not match.');
        return;
    }

    const response = await authenticatedRequest(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            'current': currentPass.value,
            'pass1': pass1.value,
            'pass2': pass2.value
        })
    });

    const isOk = await _displayResult(response);

    if(isOk) {
        currentPass.value = "";
        pass1.value = "";
        pass2.value = "";
    }

}

async function confirmSync(url)
{
    f = async function()
    {
        const response = await authenticatedRequest(url, { method: 'POST' });
        await _displayResult(response);
    }
    setConfirmationNote('(Please note that it may take up to 24 hours for the updated SDT amount to be visible on Etherscan or any block explorator)');
}