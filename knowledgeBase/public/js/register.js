document.getElementById('registration-form').addEventListener('submit', async function(event) {
    event.preventDefault();
    console.log('start registration');
    const email = document.getElementById('email').value.trim();
    const name = document.getElementById('username').value.trim();
    const password = document.getElementById('password').value;

    clearErrors();

    let isValid = true;

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        showError('emailError', 'Введите корректный email');
        isValid = false;
    }

    const nameRegex = /^[a-zA-Zа-яА-ЯёЁ\s\-]+$/;
    if (name.length < 2 || name.length > 15 || !nameRegex.test(name)) {
        showError('nameError', 'Имя должно иметь длину от 2 до 15 символов и содержать только буквы');
        isValid = false;
    }

    if (password.length < 2 || password.length > 15) {
        showError('passwordError', 'Пароль должен содержать от 2 до 15 символов');
        isValid = false;
    }

    if (isValid) {
        const userData = {
            Email: email,
            Name: name,
            Password: password
        };

        try {
            let response = await fetch(`http://localhost:5000/user/register`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;charset=utf-8'
                },
                body: JSON.stringify(userData)
            });

            if (!response.ok) {
                return response.text().then(html => {
                    document.open();
                    document.write(html);
                    document.close();
                });
            }
    
            let responseText = await response.text();
            console.log(responseText);
            
            if (responseText === '""') {
                alert('Регистрация прошла успешно!');
                window.location.href = 'http://localhost:5000/profile.html';
            } else {
                showError('wrong-data-text', responseText);
            }
        } catch (error) {
            console.error("Ошибка:", error)
        }

        this.reset();
    }
});

function showError(elementId, message) {
    document.getElementById(elementId).textContent = message;
}

function clearErrors() {
    const errorElements = document.querySelectorAll('.error');
    errorElements.forEach(element => {
        element.textContent = '';
    });
}