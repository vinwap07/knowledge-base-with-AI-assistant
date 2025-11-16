class CategoriesPage {
    constructor() {
        this.categories = [];
        this.init();
    }

    async init() {
        await this.loadCategories();
        this.bindEvents();
    }

    async loadCategories() {
        try {
            this.showLoading(true);

            const response = await fetch('http://localhost:5000/categories', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                return response.text().then(html => {
                    // Заменяем текущую страницу на HTML-ответ
                    document.open();
                    document.write(html);
                    document.close();
                });
            }

            const categoriesData = await response.json();
            this.categories = categoriesData;
            this.displayCategories();
            this.updateStats();

        } catch (error) {
            console.error('Ошибка загрузки категорий:', error);
            this.showError('Не удалось загрузить категории');
        } finally {
            this.showLoading(false);
        }
    }

    displayCategories() {
        const grid = document.getElementById('categoriesGrid');

        if (!this.categories || this.categories.length === 0) {
            grid.innerHTML = '<div class="error-message">Категории не найдены</div>';
            return;
        }

        grid.innerHTML = this.categories.map(category => `
            <div class="category-card">
                <div class="category-icon">${category.icon || '📁'}</div>
                <h3 class="category-title">${category.name}</h3>
                <p class="category-description">${category.description || 'Статьи по данной теме'}</p>
                
                <div class="category-stats">
                    <span class="articles-count">${category.articlesCount || 0} статей</span>
                </div>
                
                <a href="/articles.html?category=${category.id}" class="category-link">
                    Смотреть статьи →
                </a>
            </div>
        `).join('');
    }

    updateStats() {
        const statsContainer = document.getElementById('categoriesStats');
        const totalCategories = document.getElementById('totalCategories');
        const totalArticles = document.getElementById('totalArticles');

        if (this.categories && this.categories.length > 0) {
            const totalArticlesCount = this.categories.reduce((sum, category) => {
                return sum + (category.articlesCount || 0);
            }, 0);

            totalCategories.textContent = this.categories.length;
            totalArticles.textContent = totalArticlesCount;
            statsContainer.style.display = 'flex';
        }
    }

    showLoading(show) {
        const grid = document.getElementById('categoriesGrid');
        if (show) {
            grid.innerHTML = '<div class="loading-message">Загрузка категорий...</div>';
        }
    }

    showError(message) {
        const grid = document.getElementById('categoriesGrid');
        grid.innerHTML = `
            <div class="error-message">
                <h3>Ошибка</h3>
                <p>${message}</p>
                <button class="category-link" onclick="location.reload()" style="margin-top: 1rem;">
                    Попробовать снова
                </button>
            </div>
        `;
    }

    bindEvents() {
        // Можно добавить обработчики для фильтрации или поиска
    }
}

// Инициализация когда DOM загружен
document.addEventListener('DOMContentLoaded', function() {
    new CategoriesPage();
});