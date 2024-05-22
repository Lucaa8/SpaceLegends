function getAccessToken() {
    return localStorage.getItem('a');
}

function _setAccessToken(token) {
    localStorage.setItem('a', token);
}

function getRefreshToken() {
    return localStorage.getItem('b');
}

function _setRefreshToken(token) {
    localStorage.setItem('b', token);
}

function updateTokens(data) {

    if("access_token" in data) {
        _setAccessToken(data.access_token);
    }

    if("refresh_token" in data) {
        _setRefreshToken(data.refresh_token);
    }

}

function _clearTokens() {

    if('a' in localStorage) {
        localStorage.removeItem('a');
    }

    if('b' in localStorage) {
        localStorage.removeItem('b');
    }

}

function addAuthorizationHeader(reqParams, token) {

    if (!reqParams.headers) {
        reqParams.headers = {};
    }
    reqParams.headers['Authorization'] = `Bearer ${token}`;

    return reqParams;

}

async function _refreshToken() {

    let params = addAuthorizationHeader({ method: 'POST' }, getRefreshToken());
    const response = await fetch('/auth/refresh', params);
    if(response.ok) {
        const data = await response.json();
        if("access_token" in data) {
            _setAccessToken(data.access_token);
            return data.access_token;
        }
    }

    _clearTokens();
    return null;

}

async function authenticatedRequest(url, params) {

    addAuthorizationHeader(params, getAccessToken());
    let response = await fetch(url, params);

    if(response.status === 401) {
        const newAccess = await _refreshToken();
        if(newAccess) {
            addAuthorizationHeader(params, newAccess);
            response = await fetch(url, params);
        } else {
            window.location.href = '/login';
        }
    }

    return response;

}

function logout() {

    authenticatedRequest('/auth/logout', {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refresh: getRefreshToken() })
    }).then(response => {
        _clearTokens();
        window.location.href = '/';
    });

}