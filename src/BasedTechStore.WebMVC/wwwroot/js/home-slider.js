document.addEventListener('DOMContentLoaded', function () {
    // Отримуємо елемент карусельки
    const carousel = document.getElementById('homeCarousel');

    if (carousel) {
        // Перевіряємо, чи є активний елемент
        const activeItems = carousel.querySelectorAll('.carousel-item.active');
        if (activeItems.length === 0) {
            const firstItem = carousel.querySelector('.carousel-item');
            if (firstItem) {
                firstItem.classList.add('active');
            }
        } else if (activeItems.length > 1) {
            // Залишаємо активним тільки перший елемент
            Array.from(activeItems).slice(1).forEach(item => {
                item.classList.remove('active');
            });
        }

        // Створюємо об'єкт карусельки з опціями
        try {
            const carouselInstance = new bootstrap.Carousel(carousel, {
                interval: 5000,
                pause: 'hover',
                ride: 'carousel',
                wrap: true
            });

            console.log('Карусель успішно ініціалізована');
        } catch (error) {
            console.error('Помилка ініціалізації карусельки:', error);
        }
    }
});