document.addEventListener('DOMContentLoaded', function() {
    // Загрузка header
    fetch('header.html')
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

    // Загрузка footer
    fetch('footer.html')
        .then(response => {
            if (!response.ok) {
                throw new Error('Footer not found');
            }
            return response.text();
        })
        .then(data => {
            document.getElementById('footer').innerHTML = data;
        })
        .catch(error => console.error('Ошибка загрузки footer:', error));
});