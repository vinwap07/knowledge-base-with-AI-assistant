document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('showSummaryBtn').addEventListener('click', showAISummary);
    document.getElementById('closeModalBtn').addEventListener('click', closeAISummary);

    // Закрытие модального окна при клике вне его области
    document.getElementById('summaryModal').addEventListener('click', function(e) {
        if (e.target === this) {
            closeAISummary();
        }
    });

    // Закрытие модального окна по клавише Escape
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            closeAISummary();
        }
    });

    document.getElementById('like-btn1').addEventListener('click', function(e) {
        handleLike(e.target.closest('.like-btn'));
    });

    document.getElementById('like-btn2').addEventListener('click', function(e) {
        handleLike(e.target.closest('.like-btn'));
    });
});

// Показать модальное окно
function showAISummary() {
    const modal = document.getElementById('summaryModal');

    // Показываем модалку
    modal.style.display = 'block';

    // Добавляем анимацию появления
    modal.classList.add('modal-visible');

    // Блокируем прокрутку body
    document.body.style.overflow = 'hidden';
}

// Закрыть модальное окно
function closeAISummary() {
    const modal = document.getElementById('summaryModal');

    // Добавляем анимацию исчезновения
    modal.classList.remove('modal-visible');
    modal.classList.add('modal-hiding');

    // Ждем окончания анимации и скрываем
    setTimeout(() => {
        modal.style.display = 'none';
        modal.classList.remove('modal-hiding');

        // Восстанавливаем прокрутку
        document.body.style.overflow = '';
    }, 300);
}

// Копировать резюме
function copySummary() {
    const summaryContent = document.querySelector('.ai-summary-content');

    navigator.clipboard.writeText(summaryContent.textContent).then(() => {
        const btn = document.querySelector('.btn-primary');
        const originalText = btn.innerHTML;

        // Меняем текст кнопки
        btn.innerHTML = '✅ Скопировано!';
        btn.classList.add('copied');

        // Анимация успешного копирования
        summaryContent.classList.add('summary-copied');

        // Возвращаем исходное состояние через 2 секунды
        setTimeout(() => {
            btn.innerHTML = originalText;
            btn.classList.remove('copied');
            summaryContent.classList.remove('summary-copied');
        }, 2000);
    }).catch(err => {
        console.error('Ошибка копирования:', err);
        alert('Не удалось скопировать текст');
    });
}

async function handleLike(likeBtn) {
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