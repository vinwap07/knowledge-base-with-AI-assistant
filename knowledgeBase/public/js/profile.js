class ProfileManager {
    constructor() {
        this.currentUser = null;
        this.userRole = 'user';

        // Инициализируем с обработкой ошибок
        this.init().catch(error => {
            console.error('Ошибка инициализации ProfileManager:', error);
            showError('Не удалось загрузить профиль');
        });
    }

    async init() {
        await this.loadUserProfile();
        this.setupEventListeners();
        this.updateUI();
        this.bindEvents();
    }

    async loadUserProfile() {
        try {
            const response = await fetch('http://localhost:5000/user/getProfile', {
                method: 'GET',
            });

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
            showError('Не удалось загрузить профиль: ' + error.message);
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
            showError('Не удалось загрузить ваши статьи')
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

            container.innerHTML = articles.map(article => createArticleCard(article)).join('');
            
        } catch (error) {
            console.error('Ошибка отображения статей:', error);
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

            showNotification(`Роль изменена на: ${this.getRoleDisplayName(role)}`, 'success');
        } catch (error) {
            console.error('Ошибка переключения роли:', error);
            showError('Ошибка при изменении роли');
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
            showNotification('Пользователь успешно назначен модератором!', 'success');
            hideCreateModeratorModal();

        } catch (error) {
            console.error('Ошибка назначения модератора:', error);
            showNotification('Ошибка при назначении модератора', 'error');
        }
    }

    editProfile() {
        try {
            showNotification('Редактирование профиля', 'info');
            // window.location.href = '/edit-profile.html';
        } catch (error) {
            console.error('Ошибка редактирования профиля:', error);
        }
    }

    async handleLike(likeBtn) {
        const articleId = likeBtn.dataset.articleId;
        const currentLikes = parseInt(likeBtn.dataset.likesCount) || 0;
        const isCurrentlyLiked = likeBtn.dataset.isLiked === 'true';

        try {
            likeBtn.disabled = true;
            likeBtn.innerHTML = '💫 ...';

            const method = isCurrentlyLiked ? "DELETE" : "POST";

            const response = await fetch(`http://localhost:5000/article/like/${articleId}`, {
                method: method
            });

            if (!response.ok) {
                return response.text().then(html => {
                    document.open();
                    document.write(html);
                    document.close();
                });
            }

            const result = await response.text();
            console.log('Ответ от сервера:', result);

            const newIsLiked = !isCurrentlyLiked;
            

            // Вычисляем новое количество лайков
            let newLikesCount;
            if (newIsLiked) {
                newLikesCount = currentLikes + 1;
                likeBtn.innerHTML = '💫 ...';
            } else {
                newLikesCount = Math.max(0, currentLikes - 1); // Защита от отрицательных значений
            }

            likeBtn.dataset.likesCount = newLikesCount;
            likeBtn.dataset.isLiked = newIsLiked;
            likeBtn.className = newIsLiked ? 'like-btn liked' : 'like-btn';
            likeBtn.innerHTML = `${newIsLiked ? '💖' : '❤️'} ${newLikesCount}`;
            this.updateArticles();

            console.log('Новое состояние:', {
                articleId,
                currentLikes,
                newLikesCount,
                isCurrentlyLiked,
                newIsLiked
            });

        } catch (error) {
            console.error('Ошибка при лайке:', error);
        } finally {
            likeBtn.disabled = false;
        }
    }
    
    async updateArticles(){
        try {
            // Очищаем контейнеры
            const myArticlesContainer = document.getElementById('myArticles');
            const likedArticlesContainer = document.getElementById('likedArticles');

            if (myArticlesContainer) myArticlesContainer.innerHTML = '<div class="loading-message">Загрузка...</div>';
            if (likedArticlesContainer) likedArticlesContainer.innerHTML = '<div class="loading-message">Загрузка...</div>';

            this.loadLikedArticles();
            this.loadMyArticles();
        } catch (error) {
            console.error('Ошибка перезагрузки:', error);
            // Показываем ошибку в контейнерах
            const errorHTML = '<div class="error">Ошибка загрузки</div>';
            document.getElementById('myArticles').innerHTML = errorHTML;
            document.getElementById('likedArticles').innerHTML = errorHTML;
        }
    }

    bindEvents() {
        document.addEventListener('click', (e) => {
            if (e.target.closest('.like-btn')) {
                this.handleLike(e.target.closest('.like-btn'));
            }
        });
    }
}

// Инициализация
let profileManager;

document.addEventListener('DOMContentLoaded', function() {
    profileManager = new ProfileManager();
});

