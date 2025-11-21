function createArticleCard(article) {
    const readingTime = formatReadingTime(article.readingTime);
    const category = article.category || 'Общее';
    const likesCount = article.likesCount || 0;

    // Безопасное получение isLikedByUser
    const isLiked = article.hasOwnProperty('isLikedByUser') ? article.isLikedByUser : false;

    console.log(`Создание карточки ${article.id}:`, { likesCount, isLiked }); // Для отладки

    const likeBtnClass = isLiked ? 'like-btn liked' : 'like-btn';

    return `
        <div class="article-card" data-article-id="${article.id}"">
            <div class="article-image">${article.icon}</div>
            <div class="article-content">
                <div class="article-category">${category}</div>
                <h3 class="article-title">${article.title || 'Без названия'}</h3>
                <p class="article-summary">${article.description || 'Описание отсутствует'}</p>
                
                <div class="article-meta">
                    <div class="article-author">${article.author || 'Неизвестный автор'}</div>
                    <div class="article-stats">
                        <span class="article-stat">📅 ${formatDate(article.publishDate)}</span>
                        <span class="article-stat">⏱️ ${readingTime}</span>
                    </div>
                </div>
                <div class="article-actions">
                    <a href="http://localhost:5000/article/${article.id}" class="read-more">
                        Читать →
                    </a>
                    <button class="${likeBtnClass}" 
                            data-article-id="${article.id}"
                            data-likes-count="${likesCount}"
                            data-is-liked="${isLiked}">
                        ${isLiked ? '💖' : '❤️'} ${likesCount}
                    </button>
                </div>
            </div>
        </div>
    `;
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

function formatReadingTime(minutes) {
    if (!minutes) return '1 мин';
    if (minutes < 60) {
        return `${minutes} мин`;
    } else {
        const hours = Math.floor(minutes / 60);
        const remainingMinutes = minutes % 60;
        return remainingMinutes > 0 ? `${hours}ч ${remainingMinutes}мин` : `${hours}ч`;
    }
}

// Обработчик для клика по карточке
document.addEventListener('click', (e) => {
    // Если клик не по кнопке лайка и не по ссылке "Читать"
    if (e.target.closest('.article-card') &&
        !e.target.closest('.like-btn')) {

        const articleCard = e.target.closest('.article-card');
        const articleId = articleCard.querySelector('.like-btn').dataset.articleId;
        window.location.href = `http://localhost:5000/article/${articleId}`;
    }
});

document.addEventListener('click', (e) => {
    if (e.target.closest('.like-btn')) {
        e.preventDefault();
        e.stopPropagation();
        this.handleLike(e.target.closest('.like-btn'));
    }
});
