document.addEventListener('DOMContentLoaded', function() {
    // Загрузка header
    fetch('http://localhost:5000/header.html')
        .then(response => {
            if (!response.ok) {
                throw new Error('Header not found');
            }
            return response.text();
        })
        .then(data => {
            document.getElementById('header').innerHTML = data;
            // Инициализируем HeaderAuthChecker после загрузки header
            new HeaderAuthChecker();
        })
        .catch(error => console.error('Ошибка загрузки header:', error));
});