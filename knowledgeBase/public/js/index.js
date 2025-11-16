async function loadPopularArticles() {
    try {
        const response = await fetch('http://localhost:5000/article/popular/3', {
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

        const popularArticles = await response.json();
        this.displayArticles(popularArticles, 'popularArticles');

    } catch (error) {
        console.error('Ошибка загрузки популярных статей:', error);
        this.showErrorMessage('popular', 'Не удалось загрузить популярные статьи');
    }
}

function displayArticles(articles, containerId) {
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
                    <span class="date">📅 ${formatDate(article.publishDate)}</span>
                    <span class="reading-time">⏱️ ${calculateReadingTime(article.readingTime)}</span>
                </div>
                <p class="article-excerpt">${article.summary}</p>
                <div class="article-stats">
                    <span class="likes">❤️ ${article.likesCount}</span>
                </div>
                <div class="article-actions">
                    <button class="btn btn-primary btn-sm" onclick="readArticle(${article.id})">
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

function formatDate(dateString) {
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

function calculateReadingTime(minutes) {
    if (minutes === 1) {
        return '1 минута';
    } else if (minutes < 5) {
        return `${minutes} минуты`;
    } else {
        return `${minutes} минут`;
    }
}

function readArticle(articleId) {
    try {
        window.location.href = 'http://localhost:5000/article/' + articleId;
    } catch (error) {
        console.error('Ошибка чтения статьи:', error);
    }
}
document.addEventListener('DOMContentLoaded', function() {
    loadPopularArticles();
});
