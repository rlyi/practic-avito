document.addEventListener('DOMContentLoaded', function() {
    // Элементы DOM
    const authButtons = document.querySelector('.home-auth-buttons');
    const addButton = document.querySelector('.home-add-button');
    const modal = document.getElementById('auth-modal');
    const closeBtn = document.querySelector('.close');
    const loginForm = document.getElementById('login-form');
    const registerForm = document.getElementById('register-form');
    const showRegister = document.getElementById('show-register');
    const showLogin = document.getElementById('show-login');
    
    // Состояние авторизации (в реальном проекте будет проверяться через API)
    let isAuthenticated = false;
    let currentUser = null;
    
    // Проверка авторизации при загрузке (заглушка)
    checkAuth();
    
    // Обработчики событий
    document.querySelector('.home-auth-button:last-child').addEventListener('click', openAuthModal);
    closeBtn.addEventListener('click', closeAuthModal);
    showRegister.addEventListener('click', showRegisterForm);
    showLogin.addEventListener('click', showLoginForm);
    loginForm.addEventListener('submit', handleLogin);
    registerForm.addEventListener('submit', handleRegister);
    addButton.addEventListener('click', handleAddButtonClick);
    
    // Функции
    function openAuthModal() {
        modal.style.display = 'block';
        showLoginForm();
    }
    
    function closeAuthModal() {
        modal.style.display = 'none';
    }
    
    function showLoginForm() {
        loginForm.style.display = 'block';
        registerForm.style.display = 'none';
    }
    
    function showRegisterForm(e) {
        e.preventDefault();
        loginForm.style.display = 'none';
        registerForm.style.display = 'block';
    }
    
async function handleLogin(e) {
    e.preventDefault();
    const formData = {
        Username: e.target.elements[0].value,
        Password: e.target.elements[1].value
    };

    try {
        const response = await fetch('/User/Login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });

        if (!response.ok) {
            throw new Error('Ошибка авторизации');
        }

        const result = await response.json();
        
        if (result.success) {
            currentUser = { name: result.username };
            localStorage.setItem('currentUser', JSON.stringify(currentUser));
            updateUI();
            closeAuthModal();
        } else {
            alert(result.error || 'Неверные данные для входа');
        }
    } catch (error) {
        console.error('Ошибка:', error);
        alert('Ошибка при входе');
    }
}
    
    function handleRegister(e) {
        e.preventDefault();
        // Здесь будет запрос к API
        const name = e.target.elements[0].value;
        const email = e.target.elements[1].value;
        const password = e.target.elements[2].value;
        
        // Заглушка для демонстрации
        setTimeout(() => {
            isAuthenticated = true;
            currentUser = { name: name, email: email };
            updateUI();
            closeAuthModal();
            alert('Регистрация успешна!');
        }, 500);
    }
    
    function handleAddButtonClick() {
        if (!isAuthenticated) {
            openAuthModal();
            return false;
        }
        // Перенаправление на страницу создания объявления
        window.location.href = '/Ad/Create';
    }
    
    function checkAuth() {
        // В реальном проекте: запрос к API для проверки авторизации
        // Здесь - заглушка
        const token = localStorage.getItem('authToken');
        if (token) {
            isAuthenticated = true;
            currentUser = { name: 'Пользователь' };
        }
        updateUI();
    }
    
    function updateUI() {
        if (isAuthenticated) {
            authButtons.innerHTML = `
                <button class="home-auth-button">Избранное</button>
                <button class="home-auth-button">${currentUser.name}</button>
            `;
            addButton.textContent = '+ Разместить объявление';
        } else {
            authButtons.innerHTML = `
                <button class="home-auth-button">Избранное</button>
                <button class="home-auth-button">Вход и регистрация</button>
            `;
        }
    }
    
    // Закрытие модалки при клике вне её
    window.addEventListener('click', function(e) {
        if (e.target === modal) {
            closeAuthModal();
        }
    });
});