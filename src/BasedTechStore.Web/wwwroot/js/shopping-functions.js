document.addEventListener('DOMContentLoaded', function () {

    ShoppingSystem.init();
});

class ShoppingSystem {
    constructor() {
        this.saveTimeout = null;
        this.beforeUnloadHandlerAdded = false;
        this.SAVE_DELAY = 1000; // 1 second

        this.listTypes = {
            cart: {
                storageKey: 'cart-items',
                counterSelector: '.cart-count',
                buttonSelector: '.add-to-cart',
                displayName: 'кошика',
                serverEndpoints: {
                    add: '/Cart/AddToCart',
                    remove: '/Cart/RemoveFromCart',
                    get: '/Cart/GetCartItems',
                    update: '/Cart/UpdateCartItem',
                    save: '/Cart/SaveCartChanges',
                    saveBeacon: '/Cart/SaveCartChangesBeacon',
                    clear: '/Cart/ClearCart'
                },
                requiresServer: true
            },
            wishlist: {
                storageKey: 'wishlist-items',
                counterSelector: '.wishlist-count',
                buttonSelector: '.add-to-wishlist',
                displayName: 'улюблених',
                serverEndpoints: {
                    add: '/Wishlist/AddToWishlist',
                    remove: '/Wishlist/RemoveFromWishlist',
                    get: '/Wishlist/GetWishlistItems',
                    clear: '/Wishlist/ClearWishlist'
                },
                requiresServer: false
            },
            compare: {
                storageKey: 'compare-items',
                counterSelector: '.compare-count',
                buttonSelector: '.add-to-compare',
                displayName: 'порівняння',
                serverEndpoints: {
                    add: '/Compare/AddToCompare',
                    remove: '/Compare/RemoveFromCompare',
                    get: '/Compare/GetCompareItems',
                    clear: '/Compare/ClearCompare'
                },
                requiresServer: false,
                maxItems: 4 // Обмеження для порівняння
            }
        };

        this.pendingChanges = {
            cart: { cartId: null, createdItems: [], updatedItems: [], deletedItems: [] },
            wishlist: { items: [] },
            compare: { items: [] }
        };
    }

    static init() {
        if (!window.shoppingSystem) {
            window.shoppingSystem = new ShoppingSystem();
        }
        return window.shoppingSystem.initialize();
    }

    async initialize() {
        try {
            await this.syncAllWithServer();
            this.updateAllCounters();
            this.setupAllButtons();
            this.initializePages();

            console.log('Shopping system initialized successfully');
        } catch (error) {
            console.error('Failed to initialize shopping system:', error);
        }
    }

    async syncAllWithServer() {
        const syncPromises = Object.keys(this.listTypes).map(type =>
            this.syncWithServer(type).catch(error => {
                console.warn(`Failed to sync ${type}:`, error);
                return null;
            })
        );

        await Promise.all(syncPromises);
    }

    async syncWithServer(type) {
        const config = this.listTypes[type];
        if (!config.serverEndpoints.get) return null;

        try {
            const response = await fetch(config.serverEndpoints.get, {
                method: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                credentials: 'same-origin'
            });

            if (!response.ok) throw new Error(`HTTP error: ${response.status}`);

            const data = await response.json();

            if (data.success) {
                const items = data.items.map(item =>
                    typeof item === 'object' ? item.productId : item
                );
                localStorage.setItem(config.storageKey, JSON.stringify(items));
                console.log(`${type} synchronized:`, items.length, 'items');
                return data;
            }
        } catch (error) {
            console.error(`Error syncing ${type}:`, error);
            return null;
        }
    }
    getItems(type) {
        const config = this.listTypes[type];
        try {
            const items = JSON.parse(localStorage.getItem(config.storageKey) || '[]');
            return Array.isArray(items) ? items : [];
        } catch (error) {
            console.error(`Error loading ${type} items:`, error);
            return [];
        }
    }

    async addItem(type, productId) {
        const config = this.listTypes[type];
        let items = this.getItems(type);

        // Перевірка на існування товару
        if (items.includes(productId)) {
            this.showNotification(`Товар вже є в ${config.displayName}`, 'info');
            return false;
        }

        // Перевірка обмежень для порівняння
        if (type === 'compare' && config.maxItems && items.length >= config.maxItems) {
            this.showNotification(`Максимальна кількість товарів для ${config.displayName}: ${config.maxItems}`, 'warning');
            return false;
        }

        this.showNotification(`Додаємо товар до ${config.displayName}...`, 'info');

        // Для кошика - відправляємо на сервер
        if (config.requiresServer && config.serverEndpoints.add) {
            return await this.addItemToServer(type, productId);
        }

        // Для інших типів - тільки localStorage
        items.push(productId);
        localStorage.setItem(config.storageKey, JSON.stringify(items));
        this.updateCounter(type);
        this.showNotification(`Товар додано до ${config.displayName}`, 'success');
        return true;
    }

    async addItemToServer(type, productId) {
        const config = this.listTypes[type];
        const token = this.getCSRFToken();

        const formData = new FormData();
        formData.append('productId', productId);
        formData.append('quantity', 1);
        if (token) formData.append('__RequestVerificationToken', token);

        try {
            const response = await fetch(config.serverEndpoints.add, {
                method: 'POST',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                body: formData,
                credentials: 'same-origin'
            });

            if (!response.ok) throw new Error(`HTTP error: ${response.status}`);

            const data = await response.json();

            if (data.success) {
                // Оновлюємо localStorage
                const items = this.getItems(type);
                items.push(productId);
                localStorage.setItem(config.storageKey, JSON.stringify(items));

                // Оновлюємо лічильник
                this.updateCounterFromServer(type, data);
                this.showNotification(`Товар додано до ${config.displayName}`, 'success');
                return true;
            } else {
                throw new Error(data.message || 'Unknown server error');
            }
        } catch (error) {
            console.error(`Error adding item to ${type}:`, error);
            this.showNotification(`Помилка додавання товару: ${error.message}`, 'danger');
            return false;
        }
    }

    removeItem(type, productId) {
        const config = this.listTypes[type];
        let items = this.getItems(type);

        const index = items.indexOf(productId);
        if (index > -1) {
            items.splice(index, 1);
            localStorage.setItem(config.storageKey, JSON.stringify(items));
            this.updateCounter(type);

            // Якщо потрібно, відправляємо на сервер
            if (config.requiresServer && config.serverEndpoints.remove) {
                this.removeItemFromServer(type, productId);
            }

            return true;
        }
        return false;
    }

    async removeItemFromServer(type, productId) {
        const config = this.listTypes[type];
        const token = this.getCSRFToken();

        try {
            const response = await fetch(config.serverEndpoints.remove, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    productId: productId,
                    __RequestVerificationToken: token
                }),
                credentials: 'same-origin'
            });

            if (!response.ok) throw new Error(`HTTP error: ${response.status}`);

            const data = await response.json();
            if (!data.success) {
                console.error(`Server error removing ${type} item:`, data.message);
            }
        } catch (error) {
            console.error(`Error removing ${type} item from server:`, error);
        }
    }

    // ================ ЛІЧИЛЬНИКИ ================

    updateAllCounters() {
        Object.keys(this.listTypes).forEach(type => this.updateCounter(type));
    }

    updateCounter(type) {
        const config = this.listTypes[type];
        const items = this.getItems(type);
        const countElement = document.querySelector(config.counterSelector);

        if (countElement) {
            const count = items.length;
            countElement.textContent = count.toString();
            this.animateCounter(countElement);
        }
    }

    updateCounterFromServer(type, data) {
        const config = this.listTypes[type];
        const countElement = document.querySelector(config.counterSelector);

        if (countElement && data.cartItemCount !== undefined) {
            countElement.textContent = data.cartItemCount.toString();
            this.animateCounter(countElement);
        } else {
            this.updateCounter(type);
        }
    }

    animateCounter(element) {
        element.classList.remove('badge-pulse');
        setTimeout(() => element.classList.add('badge-pulse'), 10);
    }

    // ================ НАЛАШТУВАННЯ КНОПОК ================

    setupAllButtons() {
        Object.keys(this.listTypes).forEach(type => this.setupButtons(type));
    }

    setupButtons(type) {
        const config = this.listTypes[type];

        document.querySelectorAll(config.buttonSelector).forEach(button => {
            button.addEventListener('click', (e) => {
                e.preventDefault();
                const productId = button.dataset.productId;
                this.addItem(type, productId);
                this.animateButton(button);
            });
        });
    }

    animateButton(button) {
        button.classList.add('btn-clicked');
        setTimeout(() => button.classList.remove('btn-clicked'), 300);
    }

    // ================ ІНІЦІАЛІЗАЦІЯ СТОРІНОК ================

    initializePages() {
        // Ініціалізація сторінки кошика
        if (document.querySelector('.cart-item')) {
            this.initCartPage();
        }

        // Ініціалізація сторінки улюблених
        if (document.querySelector('.wishlist-item')) {
            this.initWishlistPage();
        }

        // Ініціалізація сторінки порівняння
        if (document.querySelector('.compare-item')) {
            this.initComparePage();
        }
    }

    initCartPage() {
        this.setupCartPendingChanges();
        this.setupQuantityButtons();
        this.setupRemoveButtons('cart');
        this.setupClearButton('cart');
    }

    initWishlistPage() {
        this.setupRemoveButtons('wishlist');
        this.setupClearButton('wishlist');
        this.setupMoveToCartButtons();
    }

    initComparePage() {
        this.setupRemoveButtons('compare');
        this.setupClearButton('compare');
        this.setupAddToCartFromCompare();
    }

    // ================ ФУНКЦІОНАЛ КОШИКА ================

    setupCartPendingChanges() {
        const cartIdElement = document.querySelector('[data-cart-id]');
        if (cartIdElement) {
            this.pendingChanges.cart.cartId = cartIdElement.dataset.cartId;
        }
    }

    setupQuantityButtons() {
        // Збільшення кількості
        document.querySelectorAll('.increment-quantity').forEach(button => {
            button.addEventListener('click', () => {
                const input = button.parentNode.querySelector('.item-quantity');
                const newValue = Math.min(parseInt(input.value) + 1, 99);
                input.value = newValue;
                this.updateCartItemQuantity(button.closest('.cart-item'), newValue);
            });
        });

        // Зменшення кількості
        document.querySelectorAll('.decrement-quantity').forEach(button => {
            button.addEventListener('click', () => {
                const input = button.parentNode.querySelector('.item-quantity');
                const newValue = Math.max(parseInt(input.value) - 1, 1);
                input.value = newValue;
                this.updateCartItemQuantity(button.closest('.cart-item'), newValue);
            });
        });

        // Ручна зміна кількості
        document.querySelectorAll('.item-quantity').forEach(input => {
            input.addEventListener('change', () => {
                let value = Math.max(1, Math.min(99, parseInt(input.value) || 1));
                input.value = value;
                this.updateCartItemQuantity(input.closest('.cart-item'), value);
            });
        });
    }

    setupRemoveButtons(type) {
        const selector = type === 'cart' ? '.remove-item' : `.remove-${type}-item`;

        document.querySelectorAll(selector).forEach(button => {
            button.addEventListener('click', () => {
                const itemId = button.dataset.id;
                const confirmMessage = `Ви впевнені, що хочете видалити цей товар з ${this.listTypes[type].displayName}?`;

                if (confirm(confirmMessage)) {
                    if (type === 'cart') {
                        this.removeCartItem(itemId);
                    } else {
                        this.removeListItem(type, itemId);
                    }
                }
            });
        });
    }

    setupClearButton(type) {
        const clearBtn = document.getElementById(`clear-${type}-btn`);
        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                const confirmMessage = `Ви впевнені, що хочете очистити ${this.listTypes[type].displayName}?`;
                if (confirm(confirmMessage)) {
                    this.clearList(type);
                }
            });
        }
    }

    setupMoveToCartButtons() {
        document.querySelectorAll('.move-to-cart').forEach(button => {
            button.addEventListener('click', () => {
                const productId = button.dataset.productId;
                this.moveToCart(productId);
            });
        });
    }

    setupAddToCartFromCompare() {
        document.querySelectorAll('.add-to-cart-from-compare').forEach(button => {
            button.addEventListener('click', () => {
                const productId = button.dataset.productId;
                this.addItem('cart', productId);
            });
        });
    }

    // ================ ОПЕРАЦІЇ З КОШИКОМ ================

    async updateCartItemQuantity(cartItemElement, quantity) {
        const itemId = cartItemElement.dataset.id;
        const price = parseFloat(cartItemElement.dataset.price);
        const totalElement = cartItemElement.querySelector('.item-total');
        const totalPrice = price * quantity;

        totalElement.textContent = this.formatCurrency(totalPrice);

        // Оновлення списку змін
        const existingIndex = this.pendingChanges.cart.updatedItems.findIndex(i => i.id === itemId);
        if (existingIndex > -1) {
            this.pendingChanges.cart.updatedItems[existingIndex].quantity = quantity;
        } else {
            this.pendingChanges.cart.updatedItems.push({ id: itemId, quantity: quantity });
        }

        try {
            const response = await fetch('/Cart/UpdateCartItem', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ cartItemId: itemId, quantity: quantity })
            });

            const data = await response.json();
            if (data.success && data.cartTotalPrice) {
                this.updateCartTotals(data.cartTotalPrice);
            }
        } catch (error) {
            console.error('Error updating cart item:', error);
            this.showNotification('Помилка оновлення кількості', 'danger');
        }

        this.scheduleSaveCartChanges();
    }

    removeCartItem(itemId) {
        this.pendingChanges.cart.deletedItems.push({ id: itemId });

        const updatedIndex = this.pendingChanges.cart.updatedItems.findIndex(i => i.id === itemId);
        if (updatedIndex > -1) {
            this.pendingChanges.cart.updatedItems.splice(updatedIndex, 1);
        }

        const cartItem = document.querySelector(`.cart-item[data-id="${itemId}"]`);
        if (cartItem) cartItem.style.opacity = '0.5';

        this.scheduleSaveCartChanges();
    }

    removeListItem(type, itemId) {
        const element = document.querySelector(`.${type}-item[data-id="${itemId}"]`);
        if (element) {
            element.style.opacity = '0.5';
            setTimeout(() => element.remove(), 300);
        }

        // Видалення з localStorage
        const productId = element?.dataset.productId;
        if (productId) {
            this.removeItem(type, productId);
        }

        this.showNotification(`Товар видалено з ${this.listTypes[type].displayName}`, 'success');
    }

    async clearList(type) {
        const config = this.listTypes[type];

        if (type === 'cart') {
            const allCartItems = document.querySelectorAll('.cart-item');
            allCartItems.forEach(item => {
                this.pendingChanges.cart.deletedItems.push({ id: item.dataset.id });
                item.style.opacity = '0.5';
            });

            this.pendingChanges.cart.updatedItems = [];
            this.pendingChanges.cart.createdItems = [];
            this.scheduleSaveCartChanges();
        } else {
            // Очищення localStorage
            localStorage.setItem(config.storageKey, JSON.stringify([]));
            this.updateCounter(type);

            // Видалення елементів з DOM
            document.querySelectorAll(`.${type}-item`).forEach(item => {
                item.style.opacity = '0.5';
                setTimeout(() => item.remove(), 300);
            });

            // Відправка на сервер, якщо потрібно
            if (config.serverEndpoints.clear) {
                this.clearListOnServer(type);
            }
        }

        this.showNotification(`${this.capitalize(config.displayName)} очищено`, 'success');
    }

    async clearListOnServer(type) {
        const config = this.listTypes[type];
        const token = this.getCSRFToken();

        try {
            await fetch(config.serverEndpoints.clear, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ __RequestVerificationToken: token }),
                credentials: 'same-origin'
            });
        } catch (error) {
            console.error(`Error clearing ${type} on server:`, error);
        }
    }

    async moveToCart(productId) {
        const success = await this.addItem('cart', productId);
        if (success) {
            this.removeItem('wishlist', productId);
            this.showNotification('Товар переміщено до кошика', 'success');
        }
    }

    // ================ ЗБЕРЕЖЕННЯ ЗМІН ================

    scheduleSaveCartChanges() {
        if (this.saveTimeout) clearTimeout(this.saveTimeout);

        this.saveTimeout = setTimeout(() => {
            this.saveCartChanges();
            this.saveTimeout = null;
        }, this.SAVE_DELAY);

        this.ensureBeforeUnloadHandler();
    }

    ensureBeforeUnloadHandler() {
        if (!this.beforeUnloadHandlerAdded) {
            window.addEventListener('beforeunload', (event) => this.beforeUnloadHandler(event));
            this.beforeUnloadHandlerAdded = true;
        }
    }

    beforeUnloadHandler(event) {
        const changes = this.pendingChanges.cart;
        if (changes.createdItems.length > 0 || changes.updatedItems.length > 0 || changes.deletedItems.length > 0) {
            if (this.saveTimeout) clearTimeout(this.saveTimeout);

            const payload = {
                cartId: changes.cartId,
                createdItems: changes.createdItems,
                updatedItems: changes.updatedItems,
                deletedItems: changes.deletedItems,
                __RequestVerificationToken: this.getCSRFToken()
            };

            const blob = new Blob([JSON.stringify(payload)], { type: 'application/json' });
            navigator.sendBeacon('/Cart/SaveCartChangesBeacon', blob);
        }
    }

    async saveCartChanges() {
        const token = this.getCSRFToken();
        const changes = this.pendingChanges.cart;

        if (!changes.cartId) {
            console.error('Cart ID not found');
            return;
        }

        const payload = {
            cartId: changes.cartId,
            createdItems: changes.createdItems,
            updatedItems: changes.updatedItems,
            deletedItems: changes.deletedItems,
            __RequestVerificationToken: token
        };

        try {
            const response = await fetch('/Cart/SaveCartChanges', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify(payload)
            });

            const data = await response.json();
            if (data.success) {
                this.updatePageAfterSave(data);
                this.resetPendingChanges();
                this.showNotification('Кошик успішно оновлено', 'success');
            } else {
                throw new Error(data.message || 'Unknown error');
            }
        } catch (error) {
            console.error('Error saving cart changes:', error);
            this.showNotification('Помилка збереження змін', 'danger');
        }
    }

    updatePageAfterSave(data) {
        // Оновлення загальної суми
        if (data.cartTotalPrice !== undefined) {
            this.updateCartTotals(data.cartTotalPrice);
        }

        // Оновлення лічильника
        if (data.cartItemCount !== undefined) {
            const cartCountBadge = document.querySelector('.cart-count');
            if (cartCountBadge) {
                cartCountBadge.textContent = data.cartItemCount;
            }
        }

        // Видалення елементів з DOM
        this.pendingChanges.cart.deletedItems.forEach(item => {
            const element = document.querySelector(`.cart-item[data-id="${item.id}"]`);
            if (element) element.remove();
        });

        // Перезавантаження сторінки, якщо кошик порожній
        if (data.cartItemCount === 0) {
            location.reload();
        }
    }

    updateCartTotals(totalPrice) {
        const selectors = ['.cart-subtotal', '.cart-total'];
        selectors.forEach(selector => {
            const element = document.querySelector(selector);
            if (element) {
                element.textContent = this.formatCurrency(totalPrice);
            }
        });
    }

    resetPendingChanges() {
        this.pendingChanges.cart = {
            cartId: this.pendingChanges.cart.cartId,
            createdItems: [],
            updatedItems: [],
            deletedItems: []
        };
    }

    // ================ ДОПОМІЖНІ ФУНКЦІЇ ================

    getCSRFToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : null;
    }

    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} notification-toast`;
        notification.innerHTML = message;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.classList.add('show');
            setTimeout(() => {
                notification.classList.remove('show');
                setTimeout(() => notification.remove(), 300);
            }, 2000);
        }, 100);
    }

    formatCurrency(value) {
        return new Intl.NumberFormat('uk-UA', {
            style: 'currency',
            currency: 'UAH'
        }).format(value);
    }

    capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }
}

// Глобальні функції для зворотної сумісності
window.shoppingFunctions = {
    addToCart: (productId) => window.shoppingSystem?.addItem('cart', productId),
    addToWishlist: (productId) => window.shoppingSystem?.addItem('wishlist', productId),
    addToCompare: (productId) => window.shoppingSystem?.addItem('compare', productId),
    removeFromCart: (productId) => window.shoppingSystem?.removeItem('cart', productId),
    removeFromWishlist: (productId) => window.shoppingSystem?.removeItem('wishlist', productId),
    removeFromCompare: (productId) => window.shoppingSystem?.removeItem('compare', productId),
    showNotification: (message, type) => window.shoppingSystem?.showNotification(message, type),
    formatCurrency: (value) => window.shoppingSystem?.formatCurrency(value)
};