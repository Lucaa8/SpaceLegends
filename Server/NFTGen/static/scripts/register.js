document.addEventListener("DOMContentLoaded", function() {
    const form = document.getElementById('registerForm');

    form.addEventListener('submit', function(event) {
        event.preventDefault();

        const errorAlert = document.getElementById('alertMessage');
        errorAlert.classList.add('d-none');
        errorAlert.innerText = "";

        const confirmPassword = document.getElementById('confirmPassword');
        if (document.getElementById('password').value !== confirmPassword.value) {
            confirmPassword.setCustomValidity('Passwords do not match.');
            confirmPassword.reportValidity();
            return;
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
            return response.json().then(r => {
                updateTokens(r);
                window.location.href = '/profile';
            });
        })
        .catch((error) => {
            errorAlert.classList.remove('d-none');
            if("message" in error) {
                if(error["message"] instanceof Array) {
                    for(let err of error["message"]) {
                        errorAlert.innerText += err + "\n";
                    }
                } else {
                    errorAlert.innerText = error["message"];
                }
            } else {
                errorAlert.innerText = "Oops, something went wrong. Retry later.";
            }
        });

    });
});
