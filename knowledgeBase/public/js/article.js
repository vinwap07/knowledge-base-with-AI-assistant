class ArticlePage {
    constructor() {
        this.articleId = this.getArticleIdFromUrl();
        this.init();
    }

    init() {
        this.loadArticle();
        this.bindEvents();
    }

    getArticleIdFromUrl() {
        const path = window.location.pathname;
        const parts = path.split('/');
        return parts[parts.length - 1] || null;
    }

    async loadArticle() {
        if (!this.articleId) {
            this.showError('ID статьи не указан');
            return;
        }
    }

    displayArticle(article) {
        // Обновляем заголовок страницы
        document.title = `${article.Title} - KnowBase`;

        // Заполняем данные статьи
        document.getElementById('articleTitle').textContent = article.Title;
        document.getElementById('articleAuthor').textContent = article.Author;
        document.getElementById('articleCategory').textContent = article.Category;
        document.getElementById('articlePublishDate').textContent = this.formatDate(article.PublishDate);
        document.getElementById('articleSummary').textContent = article.Summary;
        document.getElementById('articleContent').innerHTML = article.Content;
        document.getElementById('articleReadingTime').textContent = this.calculateReadingTime(article.ReadingTime);
    }

    formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('ru-RU', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
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

    bindEvents() {
        // Обработчик для кнопки "Нравится"
        const likeBtn = document.getElementById('likeBtn');
        if (likeBtn) {
            likeBtn.addEventListener('click', () => {
                this.handleLike();
            });
        }
    }

    async handleLike() {
        try {
            const likeBtn = document.getElementById('likeBtn');
            likeBtn.disabled = true;
            likeBtn.innerHTML = '❤️ ...';

            // В реальном приложении здесь будет запрос к API
            await new Promise(resolve => setTimeout(resolve, 500));

            likeBtn.innerHTML = '❤️ Понравилось!';
            likeBtn.style.background = '#f56565';

            setTimeout(() => {
                likeBtn.innerHTML = '❤️ Нравится';
                likeBtn.style.background = '';
                likeBtn.disabled = false;
            }, 2000);

        } catch (error) {
            console.error('Ошибка при лайке:', error);
            likeBtn.innerHTML = '❤️ Нравится';
            likeBtn.disabled = false;
        }
    }

    showLoading(show) {
        const container = document.querySelector('.article-container');
        if (show) {
            container.classList.add('loading');
        } else {
            container.classList.remove('loading');
        }
    }

    showError(message) {
        const container = document.querySelector('.article-container');
        container.innerHTML = `
            <div style="text-align: center; padding: 3rem;">
                <h2 style="color: #e53e3e; margin-bottom: 1rem;">Ошибка</h2>
                <p style="color: #718096; margin-bottom: 2rem;">${message}</p>
                <button class="btn btn-primary" onclick="window.location.href='/'">На главную</button>
            </div>
        `;
    }

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
}
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


// Инициализация когда DOM загружен
document.addEventListener('DOMContentLoaded', function() {
    new ArticlePage();
});