class ArticlesPage {
    constructor() {
        this.allArticles = [];
        this.filteredArticles = [];
        this.categories = [];
        this.filters = {
            search: '',
            category: '',
            sort: 'newest'
        };
        this.init();
    }

    async init() {
        await this.loadArticles();
        this.bindEvents();
    }

    // 
    async loadArticles() {
        try {
            this.showLoading(true);

            const response = await fetch('http://localhost:5000/article', {
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

            const articlesData = await response.json();
            console.log('Получены данные статей:', articlesData);

            this.allArticles = Array.isArray(articlesData) ? articlesData : (articlesData.articles || []);

            // Проверяем наличие isLikedByUser
            this.allArticles.forEach(article => {
                console.log(`Статья ${article.id}:`, {
                    title: article.title,
                    likesCount: article.likesCount,
                    isLikedByUser: article.isLikedByUser
                });
            });

            this.extractCategories();
            this.applyFilters();

        } catch (error) {
            console.error('Ошибка загрузки статей:', error);
            this.showError('Не удалось загрузить статьи');
        } finally {
            this.showLoading(false);
        }
    }

    extractCategories() {
        const categoriesSet = new Set();
        this.allArticles.forEach(article => {
            if (article.category) {
                categoriesSet.add(article.category);
            }
        });

        this.categories = Array.from(categoriesSet);
        this.populateCategoryFilter();
    }

    populateCategoryFilter() {
        const categoryFilter = document.getElementById('categoryFilter');
        if (!categoryFilter) {
            console.error('Элемент categoryFilter не найден');
            return;
        }

        while (categoryFilter.children.length > 1) {
            categoryFilter.removeChild(categoryFilter.lastChild);
        }

        this.categories.forEach(category => {
            const option = document.createElement('option');
            option.value = category;
            option.textContent = category;
            categoryFilter.appendChild(option);
        });
    }

    applyFilters() {
        let filtered = [...this.allArticles];

        // Фильтрация по поиску
        if (this.filters.search) {
            const searchTerm = this.filters.search.toLowerCase();
            filtered = filtered.filter(article =>
                (article.title && article.title.toLowerCase().includes(searchTerm)) ||
                (article.summary && article.summary.toLowerCase().includes(searchTerm)) ||
                (article.author && article.author.toLowerCase().includes(searchTerm))
            );
        }

        // Фильтрация по категории
        if (this.filters.category) {
            filtered = filtered.filter(article =>
                article.category === this.filters.category
            );
        }

        // Сортировка
        filtered = this.sortArticles(filtered);

        this.filteredArticles = filtered;
        this.displayArticles();
        this.updateFilterTags();
    }

    sortArticles(articles) {
        switch (this.filters.sort) {
            case 'newest':
                return articles.sort((a, b) => new Date(b.publishDate) - new Date(a.publishDate));
            case 'oldest':
                return articles.sort((a, b) => new Date(a.publishDate) - new Date(b.publishDate));
            case 'popular':
                return articles.sort((a, b) => (b.likesCount || 0) - (a.likesCount || 0));
            default:
                return articles;
        }
    }

    displayArticles() {
        const grid = document.getElementById('articlesGrid');
        const noResultsMessage = document.getElementById('noResultsMessage');

        if (!grid) {
            console.error('Элемент articlesGrid не найден');
            return;
        }

        console.log('Отображаемые статьи:', this.filteredArticles);

        if (!this.filteredArticles || this.filteredArticles.length === 0) {
            if (grid) grid.style.display = 'none';
            if (noResultsMessage) noResultsMessage.style.display = 'block';
            return;
        }

        if (grid) grid.style.display = 'grid';
        if (noResultsMessage) noResultsMessage.style.display = 'none';

        grid.innerHTML = this.filteredArticles.map(article => createArticleCard(article)).join('');
    }

    updateFilterTags() {
        const filterTags = document.getElementById('filterTags');
        const searchTag = document.getElementById('searchTag');
        const categoryTag = document.getElementById('categoryTag');
        const searchTerm = document.getElementById('searchTerm');
        const categoryName = document.getElementById('categoryName');

        if (!filterTags || !searchTag || !categoryTag || !searchTerm || !categoryName) {
            console.error('Один из элементов фильтров не найден');
            return;
        }

        const hasSearch = this.filters.search !== '';
        const hasCategory = this.filters.category !== '';

        if (hasSearch) {
            searchTerm.textContent = this.filters.search;
            searchTag.style.display = 'flex';
        } else {
            searchTag.style.display = 'none';
        }

        if (hasCategory) {
            categoryName.textContent = this.filters.category;
            categoryTag.style.display = 'flex';
        } else {
            categoryTag.style.display = 'none';
        }

        if (hasSearch || hasCategory) {
            filterTags.style.display = 'flex';
        } else {
            filterTags.style.display = 'none';
        }
    }

    bindEvents() {
        // Поиск
        const searchBtn = document.getElementById('searchBtn');
        const searchInput = document.getElementById('searchInput');
        const categoryFilter = document.getElementById('categoryFilter');
        const sortFilter = document.getElementById('sortFilter');

        if (searchBtn && searchInput) {
            searchBtn.addEventListener('click', () => {
                this.setSearch(searchInput.value.trim());
            });

            searchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    this.setSearch(e.target.value.trim());
                }
            });
        }

        // Фильтр по категориям
        if (categoryFilter) {
            categoryFilter.addEventListener('change', (e) => {
                this.setCategory(e.target.value);
            });
        }

        // Сортировка
        if (sortFilter) {
            sortFilter.addEventListener('change', (e) => {
                this.setSort(e.target.value);
            });
        }

        // Обработчики для тегов фильтров
        const clearSearchBtn = document.querySelector('#searchTag .tag-remove');
        const clearCategoryBtn = document.querySelector('#categoryTag .tag-remove');
        const clearAllBtn = document.querySelector('.clear-all-filters');

        if (clearSearchBtn) {
            clearSearchBtn.addEventListener('click', () => this.clearSearch());
        }

        if (clearCategoryBtn) {
            clearCategoryBtn.addEventListener('click', () => this.clearCategory());
        }

        if (clearAllBtn) {
            clearAllBtn.addEventListener('click', () => this.clearAllFilters());
        }

        // Обработчик лайков
        document.addEventListener('click', (e) => {
            if (e.target.closest('.like-btn')) {
                this.handleLike(e.target.closest('.like-btn'));
            }
        });
    }

    setSearch(searchTerm) {
        this.filters.search = searchTerm;
        this.applyFilters();
    }

    setCategory(category) {
        this.filters.category = category;
        this.applyFilters();
    }

    setSort(sortType) {
        this.filters.sort = sortType;
        this.applyFilters();
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
            likeBtn.dataset.isLiked = newIsLiked;

            // Вычисляем новое количество лайков
            let newLikesCount;
            if (newIsLiked) {
                newLikesCount = currentLikes + 1;
            } else {
                newLikesCount = Math.max(0, currentLikes - 1); // Защита от отрицательных значений
            }

            console.log('Новое состояние:', {
                articleId,
                currentLikes,
                newLikesCount,
                isCurrentlyLiked,
                newIsLiked
            });

            // Обновляем данные в массивах
            this.updateArticleLikeState(articleId, newLikesCount, newIsLiked);

            // Перерисовываем статьи
            this.applyFilters();

        } catch (error) {
            console.error('Ошибка при лайке:', error);
            this.showLikeError(likeBtn, currentLikes, isCurrentlyLiked);
        } finally {
            likeBtn.disabled = false;
        }
    }

    updateArticleLikeState(articleId, newLikesCount, isLiked) {
        console.log('Обновление состояния лайка:', { articleId, newLikesCount, isLiked });

        // Обновляем данные во всех массивах статей
        const updateArticle = (article) => {
            if (article.id == articleId) { // Используем нестрогое сравнение на случай разных типов
                const updatedArticle = {
                    ...article,
                    likesCount: newLikesCount,
                    isLikedByUser: isLiked
                };
                console.log('Статья обновлена:', updatedArticle);
                return updatedArticle;
            }
            return article;
        };

        this.allArticles = this.allArticles.map(updateArticle);
        this.filteredArticles = this.filteredArticles.map(updateArticle);

        // Проверяем, что данные обновились
        console.log('allArticles после обновления:', this.allArticles.find(a => a.id == articleId));
        console.log('filteredArticles после обновления:', this.filteredArticles.find(a => a.id == articleId));
    }

    clearSearch() {
        const searchInput = document.getElementById('searchInput');
        if (searchInput) searchInput.value = '';
        this.filters.search = '';
        this.applyFilters();
    }

    clearCategory() {
        const categoryFilter = document.getElementById('categoryFilter');
        if (categoryFilter) categoryFilter.value = '';
        this.filters.category = '';
        this.applyFilters();
    }

    clearAllFilters() {
        this.clearSearch();
        this.clearCategory();

        const sortFilter = document.getElementById('sortFilter');
        if (sortFilter) sortFilter.value = 'newest';
        this.filters.sort = 'newest';

        this.applyFilters();
    }

    getCategoryIcon(categoryName) {
        const icons = {
            'Руководства': '📚',
            'Техническая поддержка': '🔧',
            'Аналитика': '📊',
            'Обучающие материалы': '🎓',
            'Новости': '📰',
            'Советы': '💡',
            'Инструкции': '📝',
            'Безопасность': '🔒',
            'Графика': '📈',
            'Общее': '📄'
        };
        return icons[categoryName] || '📁';
    }

    updateArticleLikeState(articleId, newLikesCount, isLiked) {
        console.log('Обновление состояния лайка:', { articleId, newLikesCount, isLiked });

        // Обновляем данные во всех массивах статей
        const updateArticle = (article) => {
            if (article.id == articleId) { // Используем нестрогое сравнение на случай разных типов
                const updatedArticle = {
                    ...article,
                    likesCount: newLikesCount,
                    isLikedByUser: isLiked
                };
                console.log('Статья обновлена:', updatedArticle);
                return updatedArticle;
            }
            return article;
        };

        this.allArticles = this.allArticles.map(updateArticle);
        this.filteredArticles = this.filteredArticles.map(updateArticle);

        // Проверяем, что данные обновились
        console.log('allArticles после обновления:', this.allArticles.find(a => a.id == articleId));
        console.log('filteredArticles после обновления:', this.filteredArticles.find(a => a.id == articleId));
    }
    showLikeError(likeBtn, originalLikes, wasLiked) {
        // Восстанавливаем предыдущее состояние при ошибке
        const icon = wasLiked ? '💖' : '❤️';
        likeBtn.innerHTML = `${icon} ${originalLikes}`;
        likeBtn.classList.toggle('liked', wasLiked);
    }
    showLoading(show) {
        const grid = document.getElementById('articlesGrid');
        if (grid) {
            if (show) {
                grid.innerHTML = '<div class="loading-message">Загрузка статей...</div>';
            }
        }
    }

    showError(message) {
        const grid = document.getElementById('articlesGrid');
        if (grid) {
            grid.innerHTML = `
                <div class="error-message">
                    <h3>Ошибка</h3>
                    <p>${message}</p>
                    <button class="read-more" onclick="location.reload()" style="margin-top: 1rem;">
                        Попробовать снова
                    </button>
                </div>
            `;
        }
    }
}

// Глобальная переменная для доступа из HTML
let articlesPage;

// Инициализация когда DOM загружен
document.addEventListener('DOMContentLoaded', function() {
    articlesPage = new ArticlesPage();
});