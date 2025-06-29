document.addEventListener('DOMContentLoaded', function () {

    initAuthForms();
    handle401Error();

    if (localStorage.getItem('redirectAfterLogin')) {
        const redirectUrl = localStorage.getItem('redirectAfterLogin');
        localStorage.removeItem('redirectAfterLogin'); // Clear the item after use

        console.log(`Redirecting to: ${redirectUrl}`);
        if (window.location.pathname === '/' && redirectUrl !== '/') {
            window.location.href = redirectUrl;
        }
    }
});

function initAuthForms() {
    const signUpForm = document.getElementById('signUpForm');
    if (signUpForm) {
        console.log('SignUp form found, initializing...');
        signUpForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const formData = new FormData(signUpForm);

            fetch('/Auth/SignUp', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        console.log('Registration successful, checking token...');
                        // Перевірити чи токен збережений
                        fetch('/Profile/TestAuth', {
                            method: 'GET',
                            credentials: 'include'
                        })
                            .then(response => response.json())
                            .then(authData => {
                                console.log('Auth status:', authData);
                                if (authData.isAuthenticated) {
                                    window.location.href = data.redirectUrl || '/Profile';
                                } else {
                                    console.error('User not authenticated after registration');
                                }
                            });
                    } else {
                        showErrors('signUpErrors', data.errors || ['Помилка реєстрації']);
                    }
                })
                .catch(error => {
                    console.error('Помилка:', error);
                    showErrors('signUpErrors', ['Сталася помилка під час реєстрації. Спробуйте ще раз.']);
                });
        });
    } else {
        console.warn('SignUp form not found.');
    }

    const signInForm = document.getElementById('signInForm');
    if (signInForm) {
        console.log('SignIn form found, initializing...');
        signInForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const formData = new FormData(signInForm);
            console.log(Object.fromEntries(formData.entries())); // Коректний вивід даних

            fetch('/Auth/SignIn', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(response => {
                    console.log(`HTTP Status: ${response.status}`);
                    if (!response.ok) {
                        throw new Error(`Помилка HTTP: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    console.log('Отримано дані:', data);

                    if (data.success) {
                        console.log('Вхід успішний, токен отримано');

                        localStorage.setItem('isAuthenticated', 'true');

                        setTimeout(() => {
                            window.location.href = data.redirectUrl || '/Profile';
                        }, 500);
                    } else {
                        showErrors('signInErrors', data.errors || ['Помилка входу']);
                    }
                })
                .catch(error => {
                    console.error('Помилка:', error);
                    showErrors('signInErrors', ['Сталася помилка під час входу. Спробуйте ще раз.']);
                });
        });
    } else {
        console.warn('SignIn form not found.');
    }
}

function showErrors(containerId, errors) {
    const container = document.getElementById(containerId);
    if (container) {
        container.innerHTML = ''; // Clear previous errors
        container.classList.add('alert', 'alert-danger');

        errors.forEach(error => {
            const errorElement = document.createElement('div');
            errorElement.textContent = error;
            container.appendChild(errorElement);
        });

        container.classList.remove('d-none'); // Show the error container
    } else {
        console.warn(`Error container with ID ${containerId} not found.`);
    }
}

function handle401Error() {
    $(document).ajaxError(function (event, xhr, settings) {
        if (xhr.status === 401) {
            $('#authModal').modal('show');

            localStorage.setItem('redirectAfterLogin', window.location.pathname);
        }
    });
}