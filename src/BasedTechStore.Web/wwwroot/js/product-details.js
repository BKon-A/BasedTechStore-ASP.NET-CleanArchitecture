document.addEventListener('DOMContentLoaded', function () {

    // Init func for category details
    if (document.querySelector('.product-actions')) {
        console.log('Product details page initialized');
        setupProductDetailsEvents();
    }
});

// Налаштовуємо обробники подій для сторінки деталей
function setupProductDetailsEvents() {
    // Запобігаємо подвійним кліками
    document.querySelectorAll('.product-actions button').forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault(); // Запобігаємо стандартній поведінці кнопки

            // Перевіряємо, чи не була кнопка вже натиснута (запобігання подвійним кліками)
            if (this.dataset.clicked === 'true') {
                return;
            }

            // Позначаємо кнопку як натиснуту
            this.dataset.clicked = 'true';

            // Викликаємо відповідну функцію
            if (this.classList.contains('btn-primary')) {
                addToCart(this.dataset.productId);
            } else if (this.classList.contains('btn-outline-danger')) {
                addToWishlist(this.dataset.productId);
            }

            // Через деякий час знімаємо позначку (щоб можна було натиснути знову)
            setTimeout(() => {
                this.dataset.clicked = 'false';
            }, 1000);
        });
    });
}

// Додавання товару в кошик
function addToCart(productId) {
    console.log(`Adding product to cart: ${productId}`);

    // Тут буде запит до API для додавання товару в кошик
    // Поки що просто показуємо повідомлення
    showNotification('Товар додано в кошик!', 'success');

    // Анімація кнопки для візуального фідбеку
    animateButton(document.querySelector('.btn-primary'));
}

// Додавання товару в обрані
function addToWishlist(productId) {
    console.log(`Adding product to wishlist: ${productId}`);

    // Тут буде запит до API для додавання товару в обрані
    // Поки що просто показуємо повідомлення
    showNotification('Товар додано в обрані!', 'info');

    // Анімація кнопки для візуального фідбеку
    animateButton(document.querySelector('.btn-outline-danger'));
}

// Функція для відображення повідомлення
function showNotification(message, type = 'info') {
    // Створюємо елемент повідомлення
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} notification-toast`;
    notification.innerHTML = message;

    // Додаємо на сторінку
    document.body.appendChild(notification);

    // Показуємо з анімацією
    setTimeout(() => {
        notification.classList.add('show');

        // Видаляємо через деякий час
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => {
                notification.remove();
            }, 300);
        }, 2000);
    }, 100);
}

// Анімація кнопки при натисканні
function animateButton(button) {
    button.classList.add('btn-clicked');
    setTimeout(() => {
        button.classList.remove('btn-clicked');
    }, 300);
}