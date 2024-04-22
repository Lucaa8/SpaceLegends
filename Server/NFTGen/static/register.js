function handleWalletOptionChange(value) {
    const walletAddressContainer = document.getElementById('walletAddressContainer');
    const walletInfoText = document.getElementById('walletInfoText');
    if (value === 'yes') {
        walletAddressContainer.style.display = 'block';
        walletInfoText.style.display = 'none';
    } else {
        walletAddressContainer.style.display = 'none';
        walletInfoText.style.display = 'block';
    }
}

function generateWallet() {
    const wallet = ethers.Wallet.createRandom();
    return [wallet.address, wallet.privateKey];
}

function generateRandomString(length) {
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    const charactersLength = characters.length;
    for (let i = 0; i < length; i++) {
        result += characters.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
}

function downloadPrivateKeyFile(privateKey) {
    // Generate a random string between 10 and 15 char length (random name to avoid some programs to scan the download directory of the user if he forgets to delete the file)
    const filename = generateRandomString(Math.floor(Math.random() * 5) + 10) + '.txt';
    const blob = new Blob([privateKey], { type: 'text/plain' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(link.href);
}

document.addEventListener("DOMContentLoaded", function() {
    const form = document.getElementById('registerForm');

    form.addEventListener('submit', function(event) {
        event.preventDefault();

        const confirmPassword = document.getElementById('confirmPassword');
        if (document.getElementById('password').value !== confirmPassword.value) {
            confirmPassword.setCustomValidity('Passwords do not match.');
            confirmPassword.reportValidity();
            return;
        }

        let walletPrivate = '';
        if(document.getElementById('no').checked)
        {
            const wallet = generateWallet();
            document.getElementById('walletAddress').value = wallet[0];
            walletPrivate = wallet[1];
        }

        const formData = new FormData(form);
        fetch(form.getAttribute("data-url"), {
            method: 'POST',
            body: formData
        })
        .then(response => {
            if(!response.ok)
            {
                return response.json().then(errorContent => Promise.reject(errorContent));
            }
            if(walletPrivate !== '')
            {
                downloadPrivateKeyFile(walletPrivate);
            }
        })
        .catch((error) => {
            //afficher msg derreur
            console.error('Error:', error);
        });

    });
});
