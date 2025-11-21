function getCookie(name) {
    const cookies = document.cookie.split(';');
    for (let cookie of cookies) {
        const [cookieName, cookieValue] = cookie.trim().split('=');
        if (cookieName === name) {
            return cookieValue;
        }
    }
    return null;
}

function showLoading(show) {
    const grid = document.getElementById('articlesGrid');
    if (grid) {
        if (show) {
            grid.innerHTML = '<div class="loading-message">Загрузка статей...</div>';
        }
    }
}

function showError(message) {
    showNotification(message, 'error');
}

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

// TODO: переместить всех обработчиков кнопок шаблоне страницы сайта сюда с проверкой куки 