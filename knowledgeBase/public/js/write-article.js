document.addEventListener('DOMContentLoaded', function() {
    console.log('Запуск простого редактора...');

    const titleInput = document.getElementById('articleTitle');
    const description = document.getElementById('articleDescription');
    const contentDiv = document.getElementById('articleContent');
    const wordCountElement = document.getElementById('wordCount');
    const wordCountElementFoot = document.getElementById('wordCountFoot');
    const charCountElement = document.getElementById('charCount');
    const readingTimeElement = document.getElementById('readingTime');
    const publishBtn = document.getElementById('publishBtn');
    const toolbarBtns = document.querySelectorAll('.toolbar-btn');
    const CategoryElement = document.getElementById('category');

    // Функция загрузки категорий с сервера
    async function loadCategories() {
        try {
            console.log('Загрузка категорий с сервера...');
            const response = await fetch('http://localhost:5000/categories/slugs');

            if (!response.ok) {
                throw new Error(`Ошибка загрузки категорий: ${response.status} ${response.statusText}`);
            }

            const categories = await response.json();
            console.log('Загруженные категории:', categories);

            // Очищаем существующие опции (кроме первой "Выберите категорию")
            while (CategoryElement.children.length > 1) {
                CategoryElement.removeChild(CategoryElement.lastChild);
            }

            // Проверяем, что categories - массив и не пустой
            if (!Array.isArray(categories) || categories.length === 0) {
                console.warn('Категории не найдены или пустой массив');
                showNotification('Категории не найдены', 'error');
                return;
            }

            // Добавляем категории из сервера
            categories.forEach(category => {
                // Проверяем наличие необходимых полей
                if (category.slug && category.name) {
                    const option = document.createElement('option');
                    option.value = category.slug;
                    option.textContent = category.name;
                    CategoryElement.appendChild(option);
                } else {
                    console.warn('Некорректная категория:', category);
                }
            });

            console.log(`Успешно загружено ${categories.length} категорий`);

        } catch (error) {
            console.error('Ошибка при загрузке категорий:', error);
            showNotification('Не удалось загрузить категории', 'error');
        }
    }

    // Функция обновления статистики
    function updateStatistics() {
        const fullText = contentDiv ? contentDiv.textContent || contentDiv.innerText : '';
        // Подсчет слов
        const words = fullText.trim().split(/\s+/).filter(word => word.length > 0);
        const wordCount = words.length;

        // Подсчет символов
        const charCount = fullText.length;

        // Время чтения
        const readingTime = Math.max(1, Math.ceil(wordCount / 200));

        console.log('Статистика:', { wordCount, charCount, readingTime });

        // Обновление элементов
        if (wordCountElement) wordCountElement.textContent = wordCount;
        if (wordCountElementFoot) wordCountElementFoot.textContent = 'Слов: ' + wordCount;
        if (charCountElement) charCountElement.textContent = charCount;
        if (readingTimeElement) readingTimeElement.textContent = `~${readingTime} мин`;
    }

    // Обновление статистики при вводе в редакторе
    if (contentDiv) {
        contentDiv.addEventListener('input', updateStatistics);
    }

    // Обработчики кнопок форматирования
    toolbarBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            const command = this.getAttribute('data-command');
            document.execCommand(command, false, null);
            contentDiv.focus(); // Возвращаем фокус в редактор
        });
    });

    // Обработчик кнопки "Опубликовать"
    if (publishBtn) {
        publishBtn.addEventListener('click', async function(){
            const title = titleInput.value.trim();
            const content = contentDiv ? contentDiv.innerHTML : '';
            const descriptionValue = description.value.trim();
            const categoryValue = CategoryElement.value;

            if (!title) {
                alert('Пожалуйста, введите заголовок статьи.');
                return;
            }

            if (!content.trim()) {
                alert('Статья не может быть пустой.');
                return;
            }

            if (!categoryValue) {
                alert('Пожалуйста, выберите категорию.');
                return;
            }

            const readingTime = Math.max(1, Math.ceil((content.trim().split(/\s+/).filter(word => word.length > 0).length) / 200));

            const articleData = {
                Title: title,
                Description: descriptionValue,
                Content: content,
                ReadingTime: readingTime,
                Category: categoryValue
            };

            console.log('Отправка данных статьи:', articleData);

            try {
                let response = await fetch('http://localhost:5000/article/create', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(articleData),
                });

                let text = await response.text();

                if (response.status === 401) {
                    showNotification('Вы не имеете права доступа', 'error');
                    setTimeout(() => {
                        window.location.href = '/index.html';
                    }, 2000);
                    return;
                }

                if (!response.ok) {
                    document.open();
                    document.write(text);
                    document.close();
                    return;
                }

                if (text === "False") {
                    console.error('Ошибка при публикации');
                    alert('Ошибка при публикации');
                } else {
                    console.log('Статья успешно опубликована', text);
                    showNotification('Статья успешно опубликована', 'success');
                    setTimeout(() => {
                        window.location.href = 'http://localhost:5000/article/' + text;
                    }, 2000);
                }
            } catch (err) {
                console.error('Ошибка сети:', err);
                showNotification('Ошибка сети при публикации', 'error');
            }
        });
    }

    // Загружаем категории при загрузке страницы
    loadCategories();

    // Первоначальное обновление статистики
    updateStatistics();

    console.log('Простой редактор запущен');
});

// Остальной код (showNotification и стили) остается без изменений
function showNotification(message, type = 'info') {
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