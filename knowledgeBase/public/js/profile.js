class ProfileManager {
    constructor() {
        this.currentUser = null;
        this.userRole = 'user';

        // Инициализируем с обработкой ошибок
        this.init().catch(error => {
            console.error('Ошибка инициализации ProfileManager:', error);
            this.showError('Не удалось загрузить профиль');
        });
    }

    async init() {
        await this.loadUserProfile();
        this.setupEventListeners();
        this.updateUI();
    }

    async loadUserProfile() {
        try {
            const response = await fetch('http://localhost:5000/user/getProfile', {
                method: 'GET',
            });

            if (response.status === 401) {
                this.showNotification('Вы не имеете права доступа', 'error');
                setTimeout(() => {
                    window.location.href = '/index.html';
                }, 2000);
                return;
            }

            if (!response.ok) {
                return response.text().then(html => {
                    document.open();
                    document.write(html);
                    document.close();
                });
            }
            
            const responseData = await response.json();
            if (typeof responseData === 'string') {
                try {
                    this.currentUser = JSON.parse(responseData);
                } catch (parseError) {
                    console.error('Ошибка парсинга JSON:', parseError);
                    throw new Error('Неверный формат данных от сервера');
                }
            } else if (typeof responseData === 'object' && responseData !== null) {
                this.currentUser = responseData;
            } else {
                throw new Error('Неверный формат данных пользователя');
            }

            if (!this.currentUser.email || !this.currentUser.name) {
                console.warn('Неполные данные пользователя:', this.currentUser);
                throw new Error('Неполные данные пользователя');
            }

            this.normalizeUserData();
            console.log('Нормализованные данные пользователя:', this.currentUser);

            this.userRole = this.currentUser.role;

            this.displayUserInfo();
            this.loadLikedArticles();
            this.loadMyArticles();

        } catch (error) {
            console.error('Ошибка загрузки профиля:', error);
            this.showError('Не удалось загрузить профиль: ' + error.message);
        }
    }

    normalizeUserData() {
        if (this.currentUser.role) {
            this.currentUser.role = this.currentUser.role.toLowerCase();
        } else {
            this.currentUser.role = 'user';
        }
        
        this.currentUser.name = this.currentUser.name || 'Пользователь';
        this.currentUser.email = this.currentUser.email || 'email@example.com';
    }
    displayUserInfo() {
        try {
            const userName = this.currentUser?.name || 'Пользователь';
            const userEmail = this.currentUser?.email || 'email@example.com';
            const userRole = this.currentUser?.role || 'user'
            
            document.getElementById('userName').textContent = userName;
            document.getElementById('userAvatar').textContent = this.getInitials(userName);
            document.getElementById('profileName').textContent = userName;
            document.getElementById('profileEmail').textContent = userEmail;
            document.getElementById('profileAvatar').textContent = this.getInitials(userName);

            const roleBadge = document.getElementById('profileRole');
            roleBadge.innerHTML = `<span class="role-badge ${userRole}">${this.getRoleDisplayName(userRole)}</span>`;

        } catch (error) {
            console.error('Ошибка отображения информации пользователя:', error);
        }
    }

    getInitials(name) {
        if (!name) return '?';
        return name.split(' ').map(n => n[0]).join('').toUpperCase();
    }

    getRoleDisplayName(role) {
        const roles = {
            'user': 'Пользователь',
            'moderator': 'Модератор',
            'admin': 'Администратор'
        };
        return roles[role] || 'Пользователь';
    }

    updateUI() {
        try {
            const myArticlesSection = document.getElementById('myArticlesSection');
            const adminSection = document.getElementById('createModeratorBtn');
            const roleSwitcher = document.getElementById('roleSwitcher');

            if (roleSwitcher) {
                roleSwitcher.style.display = 'block';
            }

            if (this.userRole === 'user') {
                if (myArticlesSection) myArticlesSection.style.display = 'none';
                if (adminSection) adminSection.style.display = 'none';
            } else if (this.userRole === 'moderator') {
                if (myArticlesSection) myArticlesSection.style.display = 'block';
                if (adminSection) adminSection.style.display = 'none';
            } else if (this.userRole === 'admin') {
                if (myArticlesSection) myArticlesSection.style.display = 'block';
                if (adminSection) adminSection.style.display = 'block';
            }
        } catch (error) {
            console.error('Ошибка обновления UI:', error);
        }
    }

    async loadLikedArticles() {
        try {
            const response = await fetch('http://localhost:5000/article/favorite', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                return response.text().then(html => {
                    document.open();
                    document.write(html);
                    document.close();
                });
            }

            const likedArticles = await response.json();
            this.displayArticles(likedArticles, 'likedArticles');

        } catch (error) {
            console.error('Ошибка загрузки понравившихся статей:', error);
            this.showErrorMessage('likedArticles', 'Не удалось загрузить понравившиеся статьи');
        }
    }

    async loadMyArticles() {
        try {
            const response = await fetch('http://localhost:5000/article/myArticles', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                return response.text().then(html => {
                    document.open();
                    document.write(html);
                    document.close();
                });
            }

            const myArticles = await response.json();
            this.displayArticles(myArticles, 'myArticles');

        } catch (error) {
            console.error('Ошибка загрузки ваших статей:', error);
            this.showErrorMessage('myArticles', 'Не удалось загрузить ваши статьи');
        }
    }

    displayArticles(articles, containerId) {
        try {
            const container = document.getElementById(containerId);
            if (!container) {
                console.error(`Контейнер ${containerId} не найден`);
                return;
            }

            if (!articles || articles.length === 0) {
                container.innerHTML = '<div class="no-articles">Пока тут пусто</div>';
                return;
            }

            container.innerHTML = articles.map(article => `
            <div class="article-card" data-article-id="${article.id}">
                <h3 class="article-title">${article.title}</h3>
                <div class="article-meta">
                    <span class="author">👤 ${article.author}</span>
                    <span class="date">📅 ${this.formatDate(article.publishDate)}</span>
                    <span class="reading-time">⏱️ ${this.calculateReadingTime(article.readingTime)}</span>
                </div>
                <p class="article-excerpt">${article.summary}</p>
                <div class="article-stats">
                    <span class="likes">❤️ ${article.likesCount}</span>
                </div>
                <div class="article-actions">
                    <button class="btn btn-primary btn-sm" onclick="profileManager.readArticle(${article.id})">
                        Читать
                    </button>
                </div>
            </div>
        `).join('');

        } catch (error) {
            console.error('Ошибка отображения статей:', error);
            this.showErrorMessage(containerId, 'Ошибка при отображении статей');
        }
    }
    formatDate(dateString) {
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString('ru-RU', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
            });
        } catch {
            return dateString;
        }
    }

    setupEventListeners() {
        try {
            // Закрытие модальных окон по клику вне области
            document.querySelectorAll('.modal').forEach(modal => {
                modal.addEventListener('click', (e) => {
                    if (e.target === modal) {
                        modal.classList.remove('active');
                    }
                });
            });

            // ESC для закрытия модальных окон
            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape') {
                    this.hideAllModals();
                }
            });
        } catch (error) {
            console.error('Ошибка настройки обработчиков событий:', error);
        }
    }

    // Методы для переключения ролей (для демонстрации)
    switchRole(role) {
        try {
            this.userRole = role;
            this.currentUser.role = role;
            localStorage.setItem('demoRole', role);
            this.displayUserInfo();
            this.updateUI();
            this.loadMyArticles();

            this.showNotification(`Роль изменена на: ${this.getRoleDisplayName(role)}`, 'success');
        } catch (error) {
            console.error('Ошибка переключения роли:', error);
            this.showError('Ошибка при изменении роли');
        }
    }

    // Модальные окна

    showCreateModeratorModal() {
        try {
            document.getElementById('createModeratorModal').classList.add('active');
        } catch (error) {
            console.error('Ошибка открытия модального окна:', error);
        }
    }

    hideCreateModeratorModal() {
        try {
            document.getElementById('createModeratorModal').classList.remove('active');
            document.getElementById('createModeratorForm').reset();
        } catch (error) {
            console.error('Ошибка закрытия модального окна:', error);
        }
    }

    hideAllModals() {
        try {
            document.querySelectorAll('.modal').forEach(modal => {
                modal.classList.remove('active');
            });
        } catch (error) {
            console.error('Ошибка закрытия модальных окон:', error);
        }
    }

    async createModerator(event) {
        try {
            event.preventDefault();
            const formData = new FormData(event.target);
            const moderatorData = {
                email: formData.get('email'),
                permissions: {
                    canCreateArticles: formData.get('canCreateArticles') === 'on',
                    canEditArticles: formData.get('canEditArticles') === 'on',
                    canDeleteArticles: formData.get('canDeleteArticles') === 'on',
                    canManageComments: formData.get('canManageComments') === 'on'
                }
            };

            // В реальном приложении здесь был бы запрос к API
            console.log('Назначение модератора:', moderatorData);
            this.showNotification('Пользователь успешно назначен модератором!', 'success');
            this.hideCreateModeratorModal();

        } catch (error) {
            console.error('Ошибка назначения модератора:', error);
            this.showNotification('Ошибка при назначении модератора', 'error');
        }
    }

    readArticle(articleId) {
        try {
            window.location.href = 'http://localhost:5000/article/' + articleId;
        } catch (error) {
            console.error('Ошибка чтения статьи:', error);
        }
    }

    editProfile() {
        try {
            this.showNotification('Редактирование профиля', 'info');
            // window.location.href = '/edit-profile.html';
        } catch (error) {
            console.error('Ошибка редактирования профиля:', error);
        }
    }

    // Утилиты
    showNotification(message, type = 'info') {
        try {
            // Создаем уведомление
            const notification = document.createElement('div');
            notification.className = `notification notification-${type}`;
            notification.innerHTML = `
                <span>${message}</span>
                <button class="notification-close" onclick="this.parentElement.remove()">&times;</button>
            `;

            // Стили для уведомления
            notification.style.cssText = `
                position: fixed;
                top: 100px;
                right: 20px;
                background: ${type === 'error' ? '#fed7d7' : type === 'success' ? '#c6f6d5' : '#bee3f8'};
                color: ${type === 'error' ? '#9b2c2c' : type === 'success' ? '#276749' : '#2c5aa0'};
                padding: 12px 20px;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
                z-index: 10001;
                display: flex;
                align-items: center;
                gap: 10px;
                max-width: 400px;
                border-left: 4px solid ${type === 'error' ? '#e53e3e' : type === 'success' ? '#38a169' : '#3182ce'};
                animation: slideIn 0.3s ease-out;
            `;

            document.body.appendChild(notification);

            // Автоматическое скрытие через 5 секунд
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 5000);
        } catch (error) {
            console.error('Ошибка показа уведомления:', error);
        }
    }

    showError(message) {
        this.showNotification(message, 'error');
    }

    calculateReadingTime(minutes){
        if (minutes === 1) {
            return '1 минута';
        } else if (minutes < 5) {
            return `${minutes} минуты`;
        } else {
            return `${minutes} минут`;
        }
    }
}

// Добавляем стили для анимации уведомлений
const notificationStyles = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    .notification-close {
        background: none;
        border: none;
        font-size: 18px;
        cursor: pointer;
        padding: 0;
        width: 20px;
        height: 20px;
        display: flex;
        align-items: center;
        justify-content: center;
    }
`;

// Добавляем стили только если их еще нет
if (!document.querySelector('style[data-profile-notifications]')) {
    const styleSheet = document.createElement('style');
    styleSheet.textContent = notificationStyles;
    styleSheet.setAttribute('data-profile-notifications', 'true');
    document.head.appendChild(styleSheet);
}

// Инициализация
let profileManager;

document.addEventListener('DOMContentLoaded', function() {
    profileManager = new ProfileManager();
});

