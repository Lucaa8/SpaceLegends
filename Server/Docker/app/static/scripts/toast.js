function showErrorToast(text)
{
    document.getElementById('error-toast-text').innerText = text;
    bootstrap.Toast.getOrCreateInstance(document.getElementById('toastNotificationError')).show();
}

function showInfoToast(text)
{
    document.getElementById('info-toast-text').innerText = text;
    bootstrap.Toast.getOrCreateInstance(document.getElementById('toastNotificationInfo')).show();
}