document.getElementById('get-articles-btn').addEventListener('click', function(event) {
    if (getCookie('SessionID')) {
        window.location.href = 'articles.html';
    } else {
        window.location.href = 'register.html';
    }
});

function getCookie(name) {
    const cookies = document.cookie.split(';');
    for (let cookie of cookies) {
        const [cookieName, cookieValue] = cookie.trim().split('=');
        if (cookieName === name) {
            return cookieValue;
        }
    }
    return null;
}

// TODO: переместить всех обработчиков кнопок шаблоне страницы сайта сюда с проверкой куки 