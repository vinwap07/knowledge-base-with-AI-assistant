class HeaderAuthChecker {
    constructor() {
        this.isAuthenticated = false;
        this.init();
    }

    async init() {
        this.checkAuthStatus();
        this.updateUI();
        this.bindEvents();
    }

    checkAuthStatus() {
        const hasSessionCookie = this.getCookie('SessionID');
        if (hasSessionCookie) {
            this.isAuthenticated = true;
            console.log('Пользователь авторизован (куки найдены):', hasSessionCookie);
        } else {
            this.isAuthenticated = false;;
            console.log('Пользователь не авторизован (куки не найдены)');
        }
    }

    getCookie(name) {
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [cookieName, cookieValue] = cookie.trim().split('=');
            if (cookieName === name) {
                return cookieValue;
            }
        }
        return null;
    }

    updateUI() {
        const authButtons = document.getElementById('authButtons');
        const userMenu = document.getElementById('userMenu');
        const userName = document.getElementById('userName');
        const userAvatar = document.getElementById('userAvatar');
    
        console.log('Обновление UI. Авторизован:', this.isAuthenticated);
    
        if (this.isAuthenticated) {
            const name = this.getCookie('Name');
            authButtons.style.display = 'none';
            userMenu.style.display = 'flex';
            userName.textContent = name || 'Пользователь';
            userAvatar.textContent = this.getInitials(name);
            console.log('Показано меню пользователя');
        } else {
            authButtons.style.display = 'flex';
            userMenu.style.display = 'none';
            console.log('Показаны кнопки входа/регистрации');
        }
    }

    getInitials(name) {
        if (!name) return '?';
        return name.split(' ').map(n => n[0]).join('').toUpperCase();
    }

    bindEvents() {
        // Обработчик клика по аватару для открытия dropdown
        const userAvatar = document.getElementById('userAvatar');
    
        if (userAvatar) {
            userAvatar.addEventListener('click', (e) => {
                window.location.href = 'http://localhost:5000/profile.html';  
            });
        }
    
        // Периодическая проверка авторизации (каждые 30 секунд)
        setInterval(() => {
            this.checkAuthStatus();
            this.updateUI();
        }, 30000);
    }
}

    // Добавляем CSS стили для user menu
    const userMenuStyles = `
    .user-menu {
        display: flex;
        align-items: center;
        gap: 1rem;
        position: relative;
    }
    
    .username {
        font-weight: 500;
        color: #2d3748;
    }
    
    .user-avatar {
        width: 40px;
        height: 40px;
        background: linear-gradient(135deg, #667eea, #764ba2);
        color: white;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 600;
        cursor: pointer;
        transition: transform 0.3s ease;
    }
    
    .user-avatar:hover {
        transform: scale(1.1);
    }`;

    // Добавляем стили в документ
    const styleSheet = document.createElement('style');
    styleSheet.textContent = userMenuStyles;
    document.head.appendChild(styleSheet);

    // Для отладки: выводим все куки в консоль
    console.log('Текущие куки:', document.cookie);
    console.log('Данные в localStorage:', {
    userData: localStorage.getItem('userData'),
    authToken: localStorage.getItem('authToken')
});
    
// // Инициализация когда DOM загружен
// document.addEventListener('DOMContentLoaded', function() {
//     new HeaderAuthChecker();
// });