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
    downloadPrivateKeyFile(wallet.privateKey)
    return wallet.address;
}

function register()
{
    alert(document.getElementById('username').checkValidity()) //cheeeck

    //alert(generateWallet())
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
    // Generate a random string between 10 and 15 char length
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