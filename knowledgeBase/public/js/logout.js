document.getElementById('logout').addEventListener('click', async function (event)  {
    try {
        let response = await fetch(`http://localhost:5000/user/logout`, {
            method: 'POST',
        });

        if (!response.ok) {
            return response.text().then(html => {
                // Заменяем текущую страницу на HTML-ответ
                document.open();
                document.write(html);
                document.close();
            });
        }

        let responseText = await response.text();

        const isFailed = responseText === "False";
        if (isFailed) {
            alert('Что-то пошло не так(')
        } else {
            window.location.href = 'http://localhost:5000/index.html';
        }

    } catch (error) {
        console.error("Ошибка:", error);
        document.getElementById("result").textContent = `Ошибка: ${error.message}`;
    }
});

