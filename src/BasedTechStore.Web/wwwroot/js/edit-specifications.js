// BasedTechStore.Web/wwwroot/js/edit-specifications.js
// Додаємо глобальний об'єкт для зберігання початкових даних
window.originalSpecificationsData = {
    categories: [],
    types: []
};

document.addEventListener('DOMContentLoaded', function () {
    // Tracking changes variables
    let localSpecCategoriesCache = {};
    let localSpecTypesCache = {};
    let pendingChanges = {
        createdCategories: [],
        updatedCategories: [],
        deletedCategories: [],
        createdTypes: [],
        updatedTypes: [],
        deletedTypes: []
    };

    const specCategoryForm = document.getElementById('specCategoryForm');
    const specTypeForm = document.getElementById('specTypeForm');
    const productSpecsForm = document.getElementById('productSpecsForm');

    const productCategorySelect = document.getElementById('categoryForSpecSelect');
    const specCategoriesTable = document.getElementById('specCategoriesTable')?.querySelector('tbody');
    const specTypesTable = document.getElementById('specTypesTable')?.querySelector('tbody');

    const productForSpecSelect = document.getElementById('productForSpecSelect');
    const productSpecsContainer = document.getElementById('productSpecsContainer');

    // Buttons
    const addSpecCategoryBtn = document.getElementById('addSpecCategoryBtn');
    const addSpecTypeBtn = document.getElementById('addSpecTypeBtn');
    const saveSpecsBtn = document.getElementById('saveSpecsBtn');
    const saveProductSpecsBtn = document.getElementById('saveProductSpecsBtn');

    const originalLoadSpecCategories = loadSpecCategories;
    loadSpecCategories = function (categoryId) {
        originalLoadSpecCategories(categoryId);

        if (categoryId) {
            loadProductsForCategory(categoryId);
        } else {
            document.querySelector('.product-selector-container').classList.add('d-none');
            productSpecsContainer.innerHTML = '';
        }
    };

    // Current selected category and spec category
    let currentCategoryId = '';
    let currentSpecCategoryId = '';
    let hasChanges = false;

    // Current selected product
    let currentProductId = '';

    // Modals
    let specCategoryModal = null;
    if (document.getElementById('specCategoryModal')) {
        specCategoryModal = new bootstrap.Modal(document.getElementById('specCategoryModal'));
    }

    let specTypeModal = null;
    if (document.getElementById('specTypeModal')) {
        specTypeModal = new bootstrap.Modal(document.getElementById('specTypeModal'));
    }

    // =============== START local storage functions =============
    // Функція для збереження стану
    function saveStateToLocalStorage() {
        try {
            // Перевіряємо, чи є реальні зміни перед збереженням
            if (hasRealChanges(pendingChanges)) {
                localStorage.setItem('pendingSpecificationsChanges', JSON.stringify(pendingChanges));
                console.log('State saved to localStorage');
            } else {
                console.log('No changes to save to localStorage');
                localStorage.removeItem('pendingSpecificationsChanges');
            }
        } catch (error) {
            console.error('Failed to save state to localStorage:', error);
        }
    }

    function loadStateFromLocalStorage() {
        try {
            const savedState = localStorage.getItem('pendingSpecificationsChanges');
            if (savedState) {
                const parsedState = JSON.parse(savedState);
                // Перевірка чи є реальні зміни
                if (parsedState && typeof parsedState === 'object' && hasRealChanges(parsedState)) {
                    console.log('Loaded state contains real changes:', parsedState);
                    return parsedState;
                } else {
                    // Видаляємо збережений стан, якщо немає реальних змін
                    console.log('No real changes in saved state, removing it');
                    localStorage.removeItem('pendingSpecificationsChanges');
                }
            }
        } catch (error) {
            console.error('Failed to load state from localStorage:', error);
            // Clear local storage
            localStorage.removeItem('pendingSpecificationsChanges');
        }
        return null;
    }

    // Нова функція для перевірки наявності реальних змін
    function hasRealChanges(state) {
        if (!state) return false;

        // Перевіряємо наявність даних у всіх масивах змін
        const hasCreated = state.createdCategories && state.createdCategories.length > 0;
        const hasUpdated = state.updatedCategories && state.updatedCategories.length > 0;
        const hasDeleted = state.deletedCategories && state.deletedCategories.length > 0;
        const hasCreatedTypes = state.createdTypes && state.createdTypes.length > 0;
        const hasUpdatedTypes = state.updatedTypes && state.updatedTypes.length > 0;
        const hasDeletedTypes = state.deletedTypes && state.deletedTypes.length > 0;

        return hasCreated || hasUpdated || hasDeleted ||
            hasCreatedTypes || hasUpdatedTypes || hasDeletedTypes;
    }

    // Функція очищення стану
    function clearSavedState() {
        console.log('Clearing saved state from localStorage');
        localStorage.removeItem('pendingSpecificationsChanges');
        pendingChanges = {
            createdCategories: [],
            updatedCategories: [],
            deletedCategories: [],
            createdTypes: [],
            updatedTypes: [],
            deletedTypes: []
        };
    }
    // =============== END local storage functions ===============

    // Ініціалізація при завантаженні сторінки (використовуючи jQuery)
    $(document).ready(function () {
        const savedState = loadStateFromLocalStorage();
        if (savedState) {
            // Використовуємо Bootstrap для відображення модального вікна підтвердження
            const confirmDialog = new bootstrap.Modal(
                $('<div class="modal fade" tabindex="-1">' +
                    '<div class="modal-dialog">' +
                    '<div class="modal-content">' +
                    '<div class="modal-header">' +
                    '<h5 class="modal-title">Відновлення незбережених змін</h5>' +
                    '<button type="button" class="btn-close" data-bs-dismiss="modal"></button>' +
                    '</div>' +
                    '<div class="modal-body">' +
                    '<p>Виявлено незбережені зміни. Бажаєте їх відновити?</p>' +
                    '</div>' +
                    '<div class="modal-footer">' +
                    '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal" id="discardChanges">Відхилити</button>' +
                    '<button type="button" class="btn btn-primary" id="restoreChanges">Відновити</button>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>').appendTo('body')[0]
            );

            $('#discardChanges').on('click', function () {
                clearSavedState();
                confirmDialog.hide();
            });

            $('#restoreChanges').on('click', function () {
                pendingChanges = savedState;
                setHasChanges(true);

                if (currentCategoryId) {
                    loadSpecCategories(currentCategoryId);
                }
                confirmDialog.hide();
            });

            confirmDialog.show();
        }
    });

    // If specifications tab is not loaded, return
    if (!productCategorySelect || !specCategoriesTable || !specTypesTable) {
        return;
    }

    // Перевірка і запуск вибору категорії при завантаженні сторінки
    if (productCategorySelect && productCategorySelect.value) {
        currentCategoryId = productCategorySelect.value;
        console.log('Selected category ID:', currentCategoryId);
        loadSpecCategories(currentCategoryId);
        addSpecCategoryBtn.disabled = false;
    }

    // Modal handler for changes tracking
    const specModalHandler = window.modalHandlers ? window.modalHandlers["specificationsWarningOnLeave"] : null;
    // Покращена функція для встановлення прапорця змін
    function setHasChanges(value) {
        hasChanges = value;

        // Оновлюємо прапорець у ModalHandler
        if (specModalHandler) {
            specModalHandler.hasChanges = value;
        }

        // Оновлюємо localStorage, якщо є зміни
        if (value) {
            saveStateToLocalStorage();

            // Візуально показуємо, що є незбережені зміни
            $('#saveSpecsBtn').addClass('btn-warning').removeClass('btn-primary');
        } else {
            clearSavedState();
            $('#saveSpecsBtn').addClass('btn-primary').removeClass('btn-warning');
        }
    }

    // Додамо функцію для перевірки наявності змін
    function checkForChanges() {
        const hasCreatedCategories = pendingChanges.createdCategories.length > 0;
        const hasUpdatedCategories = pendingChanges.updatedCategories.length > 0;
        const hasDeletedCategories = pendingChanges.deletedCategories.length > 0;
        const hasCreatedTypes = pendingChanges.createdTypes.length > 0;
        const hasUpdatedTypes = pendingChanges.updatedTypes.length > 0;
        const hasDeletedTypes = pendingChanges.deletedTypes.length > 0;

        // Якщо є будь-які зміни, встановлюємо прапорець
        const newHasChanges = hasCreatedCategories || hasUpdatedCategories || hasDeletedCategories ||
            hasCreatedTypes || hasUpdatedTypes || hasDeletedTypes;

        // Виводимо інформацію про зміни для діагностики
        console.log('Changes detected:',
            hasCreatedCategories ? `${pendingChanges.createdCategories.length} created categories` : '',
            hasUpdatedCategories ? `${pendingChanges.updatedCategories.length} updated categories` : '',
            hasDeletedCategories ? `${pendingChanges.deletedCategories.length} deleted categories` : '',
            hasCreatedTypes ? `${pendingChanges.createdTypes.length} created types` : '',
            hasUpdatedTypes ? `${pendingChanges.updatedTypes.length} updated types` : '',
            hasDeletedTypes ? `${pendingChanges.deletedTypes.length} deleted types` : ''
        );

        setHasChanges(newHasChanges);
        return newHasChanges;
    }

    // Функція для коректного додавання елемента до pendingChanges
    function addToPendingChanges(item, type) {
        let targetArray;
        let compareFunction;

        switch (type) {
            case 'createdCategory':
                targetArray = pendingChanges.createdCategories;
                compareFunction = (a, b) => a.id === b.id;
                break;
            case 'updatedCategory':
                targetArray = pendingChanges.updatedCategories;
                compareFunction = (a, b) => a.id === b.id;
                break;
            case 'deletedCategory':
                targetArray = pendingChanges.deletedCategories;
                compareFunction = (a, b) => a.id === b.id;
                break;
            case 'createdType':
                targetArray = pendingChanges.createdTypes;
                compareFunction = (a, b) => a.id === b.id;
                break;
            case 'updatedType':
                targetArray = pendingChanges.updatedTypes;
                compareFunction = (a, b) => a.id === b.id;
                break;
            case 'deletedType':
                targetArray = pendingChanges.deletedTypes;
                compareFunction = (a, b) => a.id === b.id;
                break;
            default:
                console.error('Unknown pending changes type:', type);
                return;
        }

        // Перевіряємо, чи елемент вже є в масиві
        const existingIndex = targetArray.findIndex(existing => compareFunction(existing, item));

        if (existingIndex >= 0) {
            // Оновлюємо існуючий елемент
            targetArray[existingIndex] = item;
        } else {
            // Додаємо новий елемент
            targetArray.push(item);
        }

        // Оновлюємо прапорець змін
        checkForChanges();
    }

    // Load spec categories when product category changes
    if (productCategorySelect) {
        // Налаштовуємо обробник зміни категорії
        productCategorySelect.addEventListener('change', function () {
            currentCategoryId = this.value;
            console.log('Selected category ID:', currentCategoryId);

            if (currentCategoryId) {
                loadSpecCategories(currentCategoryId);
                loadProductsForCategory(currentCategoryId);
                addSpecCategoryBtn.disabled = false;
            } else {
                specCategoriesTable.innerHTML = '';
                specTypesTable.innerHTML = '';
                productForSpecSelect.innerHTML = '<option value="">Оберіть продукт</option>';
                productSpecsContainer.innerHTML = '';
                addSpecCategoryBtn.disabled = true;
                addSpecTypeBtn.disabled = true;
            }
        });

        // Активація при початковому завантаженні, якщо категорія вже вибрана
        if (productCategorySelect.value) {
            currentCategoryId = productCategorySelect.value;
            loadSpecCategories(currentCategoryId);
            addSpecCategoryBtn.disabled = false;
        }
    }

    // Load products
    if (productForSpecSelect) {
        productForSpecSelect.addEventListener('change', function () {
            currentProductId = this.value;
            if (currentProductId) {
                loadProductSpecs(currentProductId);
            } else {
                productSpecsContainer.innerHTML = '';
            }
        });
    }

    document.addEventListener('click', function (e) {
        if (e.target && e.target.id === 'saveProductSpecsBtn' ||
            (e.target.parentElement && e.target.parentElement.id === 'saveProductSpecsBtn')) {
            saveProductSpecs();
        }
    });

    // Add spec category button
    addSpecCategoryBtn.addEventListener('click', function () {
        document.getElementById('specCategoryId').value = '00000000-0000-0000-0000-000000000000';
        document.getElementById('specCategoryName').value = '';

        // Auto indexing
        if (localSpecCategoriesCache[currentCategoryId]) {
            reindexDisplayOrder(localSpecCategoriesCache[currentCategoryId]);
            const nextOrder = getNextDisplayOrder(localSpecCategoriesCache[currentCategoryId]);
            document.getElementById('specCategoryOrder').value = nextOrder;
        } else {
            document.getElementById('specCategoryOrder').value = '1';
        }

        document.getElementById('specCategoryProductId').value = currentCategoryId;

        specCategoryModal.show();
    });

    // Handle specification category form submission
    specCategoryForm.addEventListener('submit', function (e) {
        e.preventDefault();

        const name = $('#specCategoryName').val().trim();
        let displayOrder = parseInt(document.getElementById('specCategoryOrder').value, 10);

        if (!name) {
            alert('Введіть назву категорії характеристик');
            return;
        }

        if (isNaN(displayOrder) || displayOrder < 1) {
            alert('Порядок повинен бути числом більшим за 0');
            return;
        }

        // Getting form data
        const id = document.getElementById('specCategoryId').value;
        const isNew = id === '00000000-0000-0000-0000-000000000000';

        // Для існуючого елемента використовуємо обмін порядковими номерами
        if (!isNew && localSpecCategoriesCache[currentCategoryId]) {
            // Знаходимо поточний елемент, щоб отримати його старий порядковий номер
            const currentCategory = localSpecCategoriesCache[currentCategoryId].find(c => c.id === id);

            // Якщо є елемент з таким же порядковим номером, пропонуємо обмін
            const categoryWithSameOrder = localSpecCategoriesCache[currentCategoryId]
                .find(c => c.id !== id && c.displayOrder === displayOrder);

            if (categoryWithSameOrder && currentCategory) {
                // Option: swap display order
                if (confirm(`Категорія "${categoryWithSameOrder.name}" вже має порядковий номер ${displayOrder}. Поміняти їх місцями?`)) {

                    localSpecCategoriesCache[currentCategoryId] = swapDisplayOrder(
                        localSpecCategoriesCache[currentCategoryId],
                        id,
                        displayOrder
                    );
                } else {
                    // Option: reindex all categories
                    if (confirm('Автоматично перенумерувати всі категорії?')) {
                        // Copy current categories without the current one
                        let categories = localSpecCategoriesCache[currentCategoryId].filter(c => c.id !== id);

                        // Add this category to the list
                        categories.push({
                            id: id,
                            name: name,
                            displayOrder: displayOrder,
                            productCategoryId: currentCategoryId,
                            productCategoryName: productCategorySelect.options[productCategorySelect.selectedIndex].text || ''
                        });

                        localSpecCategoriesCache[currentCategoryId] = reindexDisplayOrder(categories);
                    } else {
                        // Option: revert to old display order
                        document.getElementById('specCategoryOrder').value = currentCategory.displayOrder;
                        return;
                    }
                }
            } else if (displayOrder > localSpecCategoriesCache[currentCategoryId].length + 1) {
                // If the display order exceeds the maximum, ask for confirmation to set it to the last position
                if (confirm(`Порядковий номер ${displayOrder} перевищує максимально можливий. Встановити номер ${localSpecCategoriesCache[currentCategoryId].length + 1}?`)) {
                    displayOrder = localSpecCategoriesCache[currentCategoryId].length + 1;
                }
            }
        }

        const selectedOption = productCategorySelect.options[productCategorySelect.selectedIndex];
        const categoryName = selectedOption ? selectedOption.text : '';

        const category = {
            id: isNew ? crypto.randomUUID() : id,
            name: name,
            displayOrder: displayOrder,
            productCategoryId: currentCategoryId,
            productCategoryName: categoryName
        };

        // Use "local cache" for pending changes
        if (isNew) {
            // Check if we already have a pending category with this ID
            const existingIndex = pendingChanges.createdCategories.findIndex(c => c.id === category.id);
            if (existingIndex >= 0) {
                pendingChanges.createdCategories[existingIndex] = category; // Update existing category
            } else {
                pendingChanges.createdCategories.push(category);
            }
        } else {
            // Check if we already have a pending update for this category
            const existingUpdateIndex = pendingChanges.updatedCategories.findIndex(c => c.id === category.id);
            if (existingUpdateIndex >= 0) {
                pendingChanges.updatedCategories[existingUpdateIndex] = category; // Update existing category
            } else {
                pendingChanges.updatedCategories.push(category); // Add new update
            }
        }

        specCategoryModal.hide();

        // Renew UI
        if (!localSpecCategoriesCache[currentCategoryId]) {
            localSpecCategoriesCache[currentCategoryId] = [];
        }

        // Delete old version, if update
        if (!isNew) {
            localSpecCategoriesCache[currentCategoryId] = localSpecCategoriesCache[currentCategoryId]
                .filter(c => c.id !== id);
        }

        // Add new version
        localSpecCategoriesCache[currentCategoryId].push(category);

        // Reindex
        localSpecCategoriesCache[currentCategoryId] = reindexDisplayOrder(localSpecCategoriesCache[currentCategoryId]);

        // Синхронізуємо displayOrder між кешем і pendingChanges
        if (isNew) {
            localSpecCategoriesCache[currentCategoryId] = reindexDisplayOrder(localSpecCategoriesCache[currentCategoryId]);

            console.log('Reindexing categories...');
            console.log('Reindexed categories:', localSpecCategoriesCache[currentCategoryId]);

            // Sync all items in cache with pending changes for consistency
            syncCacheWithPendingChanges(currentCategoryId);
        }

        renderSpecCategories(localSpecCategoriesCache[currentCategoryId]);
        setHasChanges(true);
    });

    // Handle specification type form submission
    specTypeForm.addEventListener('submit', function (e) {
        e.preventDefault();

        const name = document.getElementById('specTypeName').value.trim();
        let displayOrder = parseInt(document.getElementById('specTypeOrder').value, 10);
        const unit = document.getElementById('specTypeUnit').value.trim();
        const isFilterable = document.getElementById('specTypeFilterable').checked;

        if (!name) {
            alert('Введіть назву типу характеристики');
            return;
        }
        if (isNaN(displayOrder) || displayOrder < 1) {
            alert('Порядок повинен бути числом більшим за 0');
            return;
        }

        const id = document.getElementById('specTypeId').value;
        const isNew = !id || id === '';

        // Для існуючого елемента використовуємо обмін порядковими номерами
        if (!isNew && localSpecTypesCache[currentSpecCategoryId]) {
            // Знаходимо поточний елемент, щоб отримати його старий порядковий номер
            const currentType = localSpecTypesCache[currentSpecCategoryId].find(t => t.id === id);

            // Якщо є елемент з таким же порядковим номером, пропонуємо обмін
            const typeWithSameOrder = localSpecTypesCache[currentSpecCategoryId]
                .find(t => t.id !== id && t.displayOrder === displayOrder);

            if (typeWithSameOrder && currentType) {
                // Option: swap display order
                if (confirm(`Тип характеристики "${typeWithSameOrder.name}" вже має порядковий номер ${displayOrder}. Поміняти їх місцями?`)) {

                    localSpecTypesCache[currentSpecCategoryId] = swapDisplayOrder(
                        localSpecTypesCache[currentSpecCategoryId],
                        id,
                        displayOrder
                    );
                } else {
                    // Option: reindex all types
                    if (confirm('Автоматично перенумерувати всі типи?')) {
                        // Copy current types without the current one
                        let types = localSpecTypesCache[currentSpecCategoryId].filter(t => t.id !== id);

                        const specCategoryName = localSpecCategoriesCache[currentCategoryId]
                            .find(c => c.id === currentSpecCategoryId)?.name || '';

                        // Add this type to the list
                        types.push({
                            id: id,
                            name: name,
                            unit: unit,
                            isFilterable: isFilterable,
                            displayOrder: displayOrder,
                            specificationCategoryId: currentSpecCategoryId,
                            specificationCategoryName: specCategoryName
                        });

                        localSpecTypesCache[currentSpecCategoryId] = reindexDisplayOrder(types);
                    } else {
                        // Option: revert to old display order
                        document.getElementById('specTypeOrder').value = currentType.displayOrder;
                        return;
                    }
                }
            } else if (displayOrder > localSpecTypesCache[currentSpecCategoryId].length + 1) {
                // If the display order exceeds the maximum, ask for confirmation to set it to the last position
                if (confirm(`Порядковий номер ${displayOrder} перевищує максимально можливий. Встановити номер ${localSpecTypesCache[currentSpecCategoryId].length + 1}?`)) {
                    displayOrder = localSpecTypesCache[currentSpecCategoryId].length + 1;
                }
            }
        }

        const specCategoryName = localSpecCategoriesCache[currentCategoryId]
            .find(c => c.id === currentSpecCategoryId)?.name || '';

        const type = {
            id: isNew ? crypto.randomUUID() : id,
            name: name,
            unit: unit,
            isFilterable: isFilterable,
            displayOrder: displayOrder,
            specificationCategoryId: currentSpecCategoryId,
            specificationCategoryName: specCategoryName
        };

        console.log('Saving type:', type);

        // Use "local cache" for pending changes
        if (isNew) {
            // Check if we already have a pending type with this ID
            const existingIndex = pendingChanges.createdTypes.findIndex(t => t.id === type.id);
            if (existingIndex >= 0) {
                pendingChanges.createdTypes[existingIndex] = type; // Update existing type
            } else {
                pendingChanges.createdTypes.push(type);
            }
        } else {
            // Check if we already have a pending update for this type
            const existingUpdateIndex = pendingChanges.updatedTypes.findIndex(t => t.id === type.id);
            if (existingUpdateIndex >= 0) {
                pendingChanges.updatedTypes[existingUpdateIndex] = type; // Update existing type
            } else {
                pendingChanges.updatedTypes.push(type); // Add new update
            }
        }

        // Hide modal
        specTypeModal.hide();

        // Renew UI
        if (!localSpecTypesCache[currentSpecCategoryId]) {
            localSpecTypesCache[currentSpecCategoryId] = [];
        }

        // Delete old version, if update
        if (!isNew) {
            localSpecTypesCache[currentSpecCategoryId] = localSpecTypesCache[currentSpecCategoryId]
                .filter(t => t.id !== id);
        }

        // Add new version
        localSpecTypesCache[currentSpecCategoryId].push(type);

        // Reindex display order for types
        localSpecTypesCache[currentSpecCategoryId] = reindexDisplayOrder(localSpecTypesCache[currentSpecCategoryId]);

        console.log('Reindexing types...');
        console.log('spectypeBtn If(isNew): Reindexed types:', localSpecTypesCache[currentSpecCategoryId]
            .map(t => `${t.name}: ${t.displayOrder}`).join(', '));

        // For new items
        if (isNew) {
            // Sync all items in cache with pending changes for consistency
            syncTypeCacheWithPendingChanges(currentSpecCategoryId);
        }
        // For existing items, update and pending changes directly
        else {
            const existingUpdateIndex = pendingChanges.updatedTypes.findIndex(t => t.id === type.id);
            if (existingUpdateIndex >= 0) {
                pendingChanges.updatedTypes[existingUpdateIndex].displayOrder =
                    localSpecTypesCache[currentSpecCategoryId].find(t => t.id === type.id)?.displayOrder || type.displayOrder;
            }
        }

        // Display updated types
        renderSpecTypes(localSpecTypesCache[currentSpecCategoryId]);
        setHasChanges(true);
    });

    // Add spec type button
    addSpecTypeBtn.addEventListener('click', function () {
        if (!currentSpecCategoryId) {
            alert('Спочатку оберіть категорію характеристик');
            return;
        }

        document.getElementById('specTypeId').value = '';
        document.getElementById('specTypeName').value = '';
        document.getElementById('specTypeUnit').value = '';
        document.getElementById('specTypeFilterable').checked = false;

        // Auto indexing
        if (localSpecTypesCache[currentSpecCategoryId]) {
            reindexDisplayOrder(localSpecTypesCache[currentSpecCategoryId]);
            const nextOrder = getNextDisplayOrder(localSpecTypesCache[currentSpecCategoryId]);
            document.getElementById('specTypeOrder').value = nextOrder;
        } else {
            document.getElementById('specTypeOrder').value = '1';
        }

        document.getElementById('specTypeCategoryId').value = currentSpecCategoryId;

        specTypeModal.show();
    });

    // Save all specifications
    saveSpecsBtn.addEventListener('click', function () {
        // Check if there are any changes to save
        if (!hasRealChanges(pendingChanges)) {
            window.createInfoModal({
                id: 'noChangesModal',
                title: 'Немає змін',
                message: 'Немає змін для збереження.',
                type: 'info',
                showCancelButton: false,
                showSaveButton: false,
                confirmText: 'OK'
            });
            return;
        }

        try {
            console.log('Preparing to save specifications...');

            // Check for unique orders before saving
            ensureUniqueOrdersBeforeSave();

            // Diagnostic output for orders
            debugDisplayOrders();

            // Synchronize all cache with pending changes
            syncAllCacheWithPendingChanges();

            // Забезпечуємо унікальність та консистентність даних
            ensureUniquePendingChanges();
            ensureUniqueEntries();

            // Валідуємо дані перед відправкою
            const validation = validatePendingChanges();
            if (!validation.valid) {
                window.createInfoModal({
                    id: 'validationErrorModal',
                    title: 'Помилка валідації',
                    message: validation.message,
                    type: 'warning',
                    showCancelButton: false,
                    showSaveButton: false,
                    confirmText: 'OK'
                });
                return;
            }

            // Виводимо для діагностики
            console.log('Prepared pendingChanges:', JSON.parse(JSON.stringify(pendingChanges)));

            // Формуємо дані для відправки
            const formData = new FormData();
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            formData.append('__RequestVerificationToken', token);

            // Додаємо всі категорії та типи до formData
            appendArrayToFormData(formData, pendingChanges.createdCategories, 'PendingChanges.CreatedCategories');
            appendArrayToFormData(formData, pendingChanges.updatedCategories, 'PendingChanges.UpdatedCategories');
            appendArrayToFormData(formData, pendingChanges.deletedCategories, 'PendingChanges.DeletedCategories');
            appendArrayToFormData(formData, pendingChanges.createdTypes, 'PendingChanges.CreatedTypes');
            appendArrayToFormData(formData, pendingChanges.updatedTypes, 'PendingChanges.UpdatedTypes');
            appendArrayToFormData(formData, pendingChanges.deletedTypes, 'PendingChanges.DeletedTypes');

            // Для діагностики виводимо всі пари ключ-значення
            console.log('FormData entries:');
            for (const pair of formData.entries()) {
                console.log(pair[0], ':', pair[1]);
            }

            // Відправка даних на сервер з індикатором завантаження
            //const saveButton = document.getElementById('saveSpecsBtn');
            const originalText = saveSpecsBtn.innerHTML;
            saveSpecsBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Збереження...';
            saveSpecsBtn.disabled = true;

            fetch('/AdminPanel/SaveAllSpecifications', {
                method: 'POST',
                body: formData
            })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(errorData => {
                            throw new Error(errorData.message || 'Не вдалося зберегти характеристики');
                        });
                    }
                    return response.json();
                })
                .then(result => {
                    console.log('Save result:', result);

                    // Очищаємо всі масиви змін
                    clearPendingChanges();

                    // Очищаємо localStorage
                    localStorage.removeItem('pendingSpecificationsChanges');

                    // Скидаємо прапорець змін
                    setHasChanges(false);

                    // Показуємо повідомлення про успіх
                    window.createInfoModal({
                        id: 'saveSuccessModal',
                        title: 'Збережено',
                        message: 'Всі зміни характеристик успішно збережено.',
                        type: 'success',
                        showCancelButton: false,
                        showSaveButton: false,
                        confirmText: 'OK'
                    });

                    // Перезавантажуємо категорії, якщо є обрана категорія
                    if (currentCategoryId) {
                        loadSpecCategories(currentCategoryId);
                    }
                })
                .catch(error => {
                    console.error('Error saving specifications:', error);

                    // Показуємо повідомлення про помилку
                    window.createInfoModal({
                        id: 'saveErrorModal',
                        title: 'Помилка',
                        message: 'Не вдалося зберегти зміни: ' + error.message,
                        type: 'danger',
                        showCancelButton: false,
                        showSaveButton: false,
                        confirmText: 'OK'
                    });
                })
                .finally(() => {
                    // Відновлюємо вигляд кнопки
                    saveSpecsBtn.innerHTML = originalText;
                    saveSpecsBtn.disabled = false;
                });
        } catch (error) {
            console.error('Error preparing data for save:', error);

            // Показуємо повідомлення про помилку
            window.createInfoModal({
                id: 'prepareErrorModal',
                title: 'Помилка',
                message: 'Помилка при підготовці даних: ' + error.message,
                type: 'danger',
                showCancelButton: false,
                showSaveButton: false,
                confirmText: 'OK'
            });
        }
    });

    function saveProductSpecs() {
        const productSpecsForm = document.getElementById('productSpecsForm');
        if (!productSpecsForm) {
            console.error('Форму з характеристиками продукту не знайдено');
            return;
        }

        const productId = productSpecsForm.dataset.productId;
        if (!productId) {
            console.error('ID продукту не знайдено в даних форми');
            return;
        }

        const formData = new FormData();
        formData.append('productId', productId);

        productSpecsForm.querySelectorAll('.product-spec-input').forEach(input => {
            const value = input.value.trim();
            if (value) {
                formData.append(`specifications[${input.dataset.specTypeId}]`, value);
            }
        });

        const token = productSpecsForm.querySelector('input[name="__RequestVerificationToken"]').value;
        formData.append('__RequestVerificationToken', token);

        const saveBtn = document.getElementById('saveProductSpecsBtn');
        const originalText = saveBtn.innerHTML;
        saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Збереження...';
        saveBtn.disabled = true;

        fetch('/AdminPanel/SaveProductSpecifications', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    // Successfully saved
                    showNotification('Характеристики продукту успішно збережено', 'success');
                } else {
                    throw new Error(result.message || 'Не вдалося зберегти характеристики продукту');
                }
            })
            .catch(error => {
                console.error('Помилка збереження характеристик:', error);
                // Error message
                showNotification('Помилка при збереженні характеристик: ' + error.message, 'danger');
            })
            .finally(() => {
                saveBtn.innerHTML = originalText;
                saveBtn.disabled = false;
            });
    }

    // ===================== Functions ====================
    // Функція для отримання існуючих категорій для групи
    function getExistingCategoriesForGroup(productCategoryId) {
        const originalCategories = window.originalSpecificationsData?.categories || [];
        return originalCategories.filter(category =>
            category.productCategoryId === productCategoryId &&
            !pendingChanges.deletedCategories.some(deleted => deleted.id === category.id)
        );
    }

    // Функція для отримання існуючих типів для категорії
    function getExistingTypesForCategory(specificationCategoryId) {
        const originalTypes = window.originalSpecificationsData?.types || [];
        return originalTypes.filter(type =>
            type.specificationCategoryId === specificationCategoryId &&
            !pendingChanges.deletedTypes.some(deleted => deleted.id === type.id)
        );
    }

    function loadSpecCategories(categoryId) {
        fetch(`/AdminPanel/GetSpecificationCategories?categoryId=${categoryId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to load spec categories');
                }
                return response.json()
            })
            .then(categories => {
                // Зберігаємо оригінальні дані
                window.originalSpecificationsData.categories = JSON.parse(JSON.stringify(categories));

                // Зберігаємо в локальний кеш
                localSpecCategoriesCache[categoryId] = categories;

                // Рендеримо категорії
                renderSpecCategories(categories);

                // Вибираємо перший рядок
                const firstRow = specCategoriesTable.querySelector('tr[data-id]');
                if (firstRow) {
                    firstRow.click();
                }
            })
            .catch(error => {
                console.error('Error loading spec categories:', error)
                specCategoriesTable.innerHTML = `
            <tr>
                <td colspan="3" class="text-center">
                    Помилка при завантаженні категорій характеристик: ${error.message}.
                </td>
            <tr>`;
            });
    }

    function loadSpecTypes(specCategoryId) {
        fetch(`/AdminPanel/GetSpecificationTypes?specCategoryId=${specCategoryId}`)
            .then(response => response.json())
            .then(types => {
                // Зберігаємо оригінальні дані
                window.originalSpecificationsData.types =
                    window.originalSpecificationsData.types.filter(t => t.specificationCategoryId !== specCategoryId);

                window.originalSpecificationsData.types =
                    window.originalSpecificationsData.types.concat(JSON.parse(JSON.stringify(types)));

                // Зберігаємо в локальний кеш
                localSpecTypesCache[specCategoryId] = types;

                // Рендеримо типи
                renderSpecTypes(types);
            })
            .catch(error => console.error('Error loading spec types:', error));
    }

    // Load list of products for the selected category
    function loadProductsForCategory(categoryId) {
        fetch(`/AdminPanel/GetProductsByCategoryId?categoryId=${categoryId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to load products for category');
                }
                return response.json();
            })
            .then(products => {
                productForSpecSelect.innerHTML = '<option value="">Оберіть продукт</option>';

                if (products && products.length > 0) {
                    products.forEach(product => {
                        const option = document.createElement('option');
                        option.value = product.id;
                        option.textContent = product.name;
                        productForSpecSelect.appendChild(option);
                    });

                    //document.querySelector('.product-selector-container').classList.remove('d-none');
                } else {
                    //document.querySelector('.product-selector-container').classList.add('d-none');
                    productSpecsContainer.innerHTML = `
                        <div class="alert alert-info">
                            Не знайдено продуктів для вибраної категорії.
                        </div>`;
                }
            })
            .catch(error => {
                console.error('Error loading products for category:', error);
                productForSpecSelect.innerHTML = '<option value="">!Помилка завантаження!</option>';
                productForSpecSelect.innerHTML = `
                <div class="alert alert-danger">
                    Помилка при завантаженні продуктів для категорії: ${error.message}
                </div>`;
            });
    }

    // Load product specifications
    function loadProductSpecs(productId) {
        fetch(`/AdminPanel/GetProductSpecificationsForm?productId=${productId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to load product specifications');
                }
                return response.text();
            })
            .then(html => {
                productSpecsContainer.innerHTML = html;

                setupProductSpecsEvents();
            })
            .catch(error => {
                console.error('Error loading product specifications:', error);
                productSpecsContainer.innerHTML = `
                    <div class="alert alert-danger">
                        Помилка при завантаженні характеристик продукту: ${error.message}
                    </div>`;
            });
    }

    function editSpecCategory(categoryId) {

        // Check category in local cache
        const cachedCategory = localSpecCategoriesCache[currentCategoryId]?.find(c => c.id === categoryId);

        if (cachedCategory) {
            document.getElementById('specCategoryId').value = cachedCategory.id;
            document.getElementById('specCategoryName').value = cachedCategory.name;
            document.getElementById('specCategoryOrder').value = cachedCategory.displayOrder;
            document.getElementById('specCategoryProductId').value = currentCategoryId;

            specCategoryModal.show();
        } else {
            // If not in local cache, fetch from server
            fetch(`/AdminPanel/GetSpecificationCategory?id=${categoryId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Failed to get spec category');
                    }
                    return response.json();
                })
                .then(category => {
                    // Заповнюємо форму безпосередньо
                    document.getElementById('specCategoryId').value = category.id;
                    document.getElementById('specCategoryName').value = category.name;
                    document.getElementById('specCategoryOrder').value = category.displayOrder;
                    document.getElementById('specCategoryProductId').value = currentCategoryId;

                    specCategoryModal.show();
                })
                .catch(error => {
                    console.error('Error getting spec category:', error);
                    alert('Помилка при отриманні категорії характеристик');
                });
        }
    }

    function editSpecType(typeId) {
        // Check type in local cache
        const cachedType = localSpecTypesCache[currentSpecCategoryId]?.find(t =>
            t.id === typeId);

        if (cachedType) {
            document.getElementById('specTypeId').value = cachedType.id;
            document.getElementById('specTypeName').value = cachedType.name;
            document.getElementById('specTypeUnit').value = cachedType.unit || '';
            document.getElementById('specTypeFilterable').checked = cachedType.isFilterable;
            document.getElementById('specTypeOrder').value = cachedType.displayOrder;
            document.getElementById('specTypeCategoryId').value = currentSpecCategoryId;

            specTypeModal.show();
        } else {
            // If not in local cache, fetch from server
            fetch(`/AdminPanel/GetSpecificationType?id=${typeId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Failed to get spec type');
                    }
                    return response.json();
                })
                .then(type => {
                    document.getElementById('specTypeId').value = type.id;
                    document.getElementById('specTypeName').value = type.name;
                    document.getElementById('specTypeUnit').value = type.unit || '';
                    document.getElementById('specTypeFilterable').checked = type.isFilterable;
                    document.getElementById('specTypeOrder').value = type.displayOrder;
                    document.getElementById('specTypeCategoryId').value = currentSpecCategoryId;

                    specTypeModal.show();
                })
                .catch(error => {
                    console.error('Error getting spec type:', error);
                    alert('Помилка при отриманні типу характеристики');
                });
        }
    }

    //function saveProductSpecs() {
    //    if (!productSpecsForm) return;

    //    const productId = productSpecsForm.dataset.productId;
    //    if (!productId) {
    //        console.error('Product ID not found in form data');
    //        return;
    //    }

    //    const formData = new FormData();
    //    formData.append('productId', productId);

    //    productSpecsForm.querySelectorAll('.product-spec-input').forEach(input => {
    //        const value = input.value.trim();
    //        if (value) {
    //            formData.append(`specifications[${input.dataset.specTypeId}]`, value);
    //        }
    //    });

    //    const token = productSpecsForm.querySelector('input[name="__RequestVerificationToken"]').value;
    //    formData.append('__RequestVerificationToken', token);

    //    fetch('/AdminPanel/SaveProductSpecifications', {
    //        method: 'POST',
    //        body: formData
    //    })
    //        .then(response => response.json())
    //        .then(result => {
    //            if (result.success) {
    //                window.createInfoModal({
    //                    id: 'saveSpecsSuccessModal',
    //                    title: 'Збережено',
    //                    message: 'Характеристики продукту успішно збережено.',
    //                    type: 'success',
    //                    showCancelButton: false,
    //                    showSaveButton: false,
    //                    confirmText: 'OK'
    //                });
    //                setHasChanges(false);
    //            } else {
    //                throw new Error(result.message || 'Не вдалося зберегти характеристики продукту');
    //            }
    //        })
    //        .catch(error => {
    //            console.error('Error saving product specifications:', error);
    //            window.createInfoModal({
    //                id: 'saveSpecsErrorModal',
    //                title: 'Помилка',
    //                message: 'Не вдалося зберегти характеристики продукту: ' + error.message,
    //                type: 'danger',
    //                showCancelButton: false,
    //                showSaveButton: false,
    //                confirmText: 'OK'
    //            });
    //        });
    //}

    function deleteSpecCategory(categoryId) {
        window.createInfoModal({
            id: 'confirmDeleteSpecCategory',
            title: 'Видалення категорії характеристик',
            message: 'Ви впевнені, що хочете видалити цю категорію характеристик?',
            type: 'danger',
            showSaveButton: false,
            onConfirm: function () {
                // Add to pending changes for deletion
                const categoryToDelete = localSpecCategoriesCache[currentCategoryId]
                    .find(c => c.id === categoryId);
                if (categoryToDelete) {
                    pendingChanges.deletedCategories.push(categoryToDelete);

                    // Після видалення категорії треба перенумерувати displayOrder
                    const remainingCategories = localSpecCategoriesCache[currentCategoryId]
                        .filter(c => c.id !== categoryId);

                    // Перенумеровуємо
                    reindexDisplayOrder(remainingCategories);

                    // Оновлюємо кеш
                    localSpecCategoriesCache[currentCategoryId] = remainingCategories;
                }

                // Delete from local cache cascade
                localSpecCategoriesCache[currentCategoryId] = localSpecCategoriesCache[currentCategoryId]
                    .filter(c => c.id !== categoryId);
                if (localSpecTypesCache[categoryId]) {
                    pendingChanges.deletedTypes.push(...localSpecTypesCache[categoryId]);
                    delete localSpecTypesCache[categoryId];
                }

                // Render updated categories and types
                renderSpecCategories(localSpecCategoriesCache[currentCategoryId]);

                // Clear spec types
                if (categoryId === currentSpecCategoryId) {
                    currentSpecCategoryId = '';
                    specTypesTable.innerHTML = '';
                    addSpecTypeBtn.disabled = true;
                }

                setHasChanges(true);
            }
        });
    }

    function deleteSpecType(typeId) {
        window.createInfoModal({
            id: 'confirmDeleteSpecType',
            title: 'Видалення типу характеристик',
            message: 'Ви впевнені, що хочете видалити цей тип характеристики?',
            type: 'danger',
            showSaveButton: false,
            onConfirm: function () {
                // Add to pending changes for deletion
                const typeToDelete = localSpecTypesCache[currentSpecCategoryId]
                    .find(t => t.id === typeId);
                if (typeToDelete) {
                    pendingChanges.deletedTypes.push(typeToDelete);
                }

                // Delete from local cache
                localSpecTypesCache[currentSpecCategoryId] = localSpecTypesCache[currentSpecCategoryId]
                    .filter(t => t.id !== typeId);

                renderSpecTypes(localSpecTypesCache[currentSpecCategoryId]);
                setHasChanges(true);
            }
        });
    }

    function renderSpecCategories(categories) {
        // Clear existing categories
        specCategoriesTable.innerHTML = '';

        if (!categories || categories.length === 0) {
            specCategoriesTable.innerHTML = `
            <tr>
                <td colspan="3" class="text-center">
                    Для цієї категорії товарів ще не додано категорій характеристик.
                </td>
            <tr>`;
            return;
        }

        // Sort by display order
        const sortedCategories = [...categories].sort((a, b) => a.displayOrder - b.displayOrder);

        sortedCategories.forEach(category => {
            const row = document.createElement('tr');
            row.setAttribute('data-id', category.id);

            row.innerHTML = `
                <td>${category.name}</td>
                <td>${category.displayOrder}</td>
                <td class="text-end">
                    <div class="btn-group btn-group-sm">
                        <button type="button" class="btn btn-outline-primary edit-spec-category-btn" data-spec-category-id="${category.id}" title="Редагувати">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button type="button" class="btn btn-outline-danger delete-spec-category-btn" data-spec-category-id="${category.id}" title="Видалити">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </td>`;
            specCategoriesTable.appendChild(row);
        });

        // Add event listeners for edit and delete buttons
        setupSpecCategoryEvents();

        // Select the first row if exists
        const firstRow = specCategoriesTable.querySelector('tr[data-id]');
        if (firstRow) {
            firstRow.click();
        } else {
            specTypesTable.innerHTML = '';
            currentSpecCategoryId = '';
            addSpecTypeBtn.disabled = true;
        }

        setHasChanges(true);
    }

    function renderSpecTypes(types) {
        // Clear existing types
        specTypesTable.innerHTML = '';

        if (!types || types.length === 0) {
            specTypesTable.innerHTML = `
            <tr>
                <td colspan="5" class="text-center">
                    Для цієї категорії характеристик ще не додано типів характеристик.
                </td>
            <tr>`;
            return;
        }
        // Sort by display order
        const sortedTypes = [...types].sort((a, b) => a.displayOrder - b.displayOrder);

        sortedTypes.forEach(type => {
            const row = document.createElement('tr');

            row.innerHTML = `
                <td>${type.name}</td>
                <td>${type.unit}</td>
                <td class="text-center">
                    ${type.isFilterable
                    ? '<i class="bi bi-check-lg text-success"></i>'
                    : '<i class="bi bi-x-lg text-danger"></i>'}
                </td>
                <td>${type.displayOrder}</td>
                <td>
                    <div class="btn-group btn-group-sm">
                        <button type="button" class="btn btn-outline-primary edit-spec-type-btn" data-spec-type-id="${type.id}">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button type="button" class="btn btn-outline-danger delete-spec-type-btn" data-spec-type-id="${type.id}">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </td>
                `;

            specTypesTable.appendChild(row);
        });

        // Add event listeners for edit and delete buttons
        setupSpecTypeEvents();
        setHasChanges(true);
    }

    function setupSpecCategoryEvents() {
        // Check, if a add button is enabled after rendering categories
        if (currentCategoryId) {
            addSpecCategoryBtn.disabled = false;
            console.log('Add button enabled');
        }

        // Buttons for edit and delete
        specCategoriesTable.querySelectorAll('.edit-spec-category-btn').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                const categoryId = this.getAttribute('data-spec-category-id');
                editSpecCategory(categoryId);
            });
        });
        specCategoriesTable.querySelectorAll('.delete-spec-category-btn').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                const categoryId = this.getAttribute('data-spec-category-id');
                deleteSpecCategory(categoryId);
            });
        });

        // Row click to load spec types
        specCategoriesTable.querySelectorAll('tr[data-id]').forEach(row => {
            row.addEventListener('click', function () {
                specCategoriesTable.querySelectorAll('tr').forEach(r => r.classList.remove('table-primary'));
                this.classList.add('table-primary');

                currentSpecCategoryId = this.getAttribute('data-id');
                loadSpecTypes(currentSpecCategoryId);
                addSpecTypeBtn.disabled = false;
            });
        });
    }

    function setupSpecTypeEvents() {
        // Buttons for edit and delete
        specTypesTable.querySelectorAll('.edit-spec-type-btn').forEach(btn => {
            btn.addEventListener('click', function () {
                const typeId = this.getAttribute('data-spec-type-id');
                editSpecType(typeId);
            });
        });
        specTypesTable.querySelectorAll('.delete-spec-type-btn').forEach(btn => {
            btn.addEventListener('click', function () {
                const typeId = this.getAttribute('data-spec-type-id');
                deleteSpecType(typeId);
            });
        });
    }

    function setupProductSpecsEvents() {
        //if (saveProductSpecsBtn) {
        //    saveProductSpecsBtn.addEventListener('click', saveProductSpecs);
        //}

        document.querySelectorAll('.product-spec-input').forEach(input => {
            input.addEventListener('change', function () {
                setHasChanges(true);
            });
        });
    }

    function reindexDisplayOrder(items) {
        if (!items || !items.length) return items;

        // Remove duplicates by ID and keep the first occurrence
        const uniqueItems = [];
        const seenIds = new Set();

        // Delete duplicates and keep the first occurrence
        for (const item of items) {
            if (!seenIds.has(item.id)) {
                uniqueItems.push(item);
                seenIds.add(item.id);
            }
        }

        // Sort existing items by displayOrder and name(olny for the same displayOrder)
        const sortedItems = [...uniqueItems].sort((a, b) => {
            const orderDiff = (a.displayOrder || 0) - (b.displayOrder || 0);
            return orderDiff !== 0 ? orderDiff : (a.name || '').localeCompare(b.name || '');
        });

        // Reindex starting from 1
        for (let i = 0; i < sortedItems.length; i++) {
            sortedItems[i].displayOrder = i + 1;
        }

        console.log("Reindexed items:", sortedItems.map(i => `${i.name}: ${i.displayOrder}`).join(", "));

        const orderCounts = {};
        for (const item of sortedItems) {
            orderCounts[item.displayOrder] = (orderCounts[item.displayOrder] || 0) + 1;
        }

        const duplicates = Object.entries(orderCounts).filter(([_, count]) => count > 1);
        if (duplicates.length > 0) {
            console.warn("Увага! Присутні дублікати порядкових номерів:",
                duplicates.map(([order, count]) => `${order}: ${count} разів`).join(", "));
        }

        return sortedItems;
    }

    // Append array to FormData with prefix with garantee of correct serialization
    function appendArrayToFormData(formData, array, prefix) {
        if (!array || array.length === 0) return;

        // Array for created, updated, or deleted items
        let preparedItems;

        if (prefix.includes('Created')) {
            preparedItems = prepareItemsForFormData(array, prefix);
        } else {
            preparedItems = [...array];
        }

        preparedItems.forEach((item, idx) => {
            for (const key in item) {
                if (item.hasOwnProperty(key)) {
                    let value = item[key];

                    if (value === null || value === undefined) {
                        value = ''; // Ensure no null or undefined values
                    } else if (typeof value === 'boolean') {
                        value = value.toString(); // Convert boolean to string
                    }
                    formData.append(`${prefix}[${idx}].${key}`, value);
                }
            }
        });
    }

    // Нова функція для підготовки даних
    function prepareItemsForFormData(array, prefix) {
        if (!array || array.length === 0) return [];

        const items = [...array];

        if (prefix.includes('Categories')) {
            // Group by productCategoryId for categories
            const groupedItems = {};
            items.forEach(item => {
                if (!groupedItems[item.productCategoryId]) {
                    groupedItems[item.productCategoryId] = [];
                }
                groupedItems[item.productCategoryId].push({ ...item });
            });

            // Reindex each group
            for (const groupId in groupedItems) {
                const group = groupedItems[groupId];

                const existingCategories = getExistingCategoriesForGroup(groupId);
                const maxExistingOrder = existingCategories.length > 0 ? Math.max(...existingCategories
                    .map(c => c.displayOrder || 0)) : 0;

                group.sort((a, b) => (a.displayOrder || 0) - (b.displayOrder || 0));

                group.forEach((item, index) => {
                    if (prefix.includes('Created')) {
                        item.displayOrder = maxExistingOrder + index + 1;
                    }
                    //else if (prefix.includes('Updated')) {
                    //    item.displayOrder = displayOrder;
                    //}
                });
                console.log(`prepareItemsForFormData, categories: Normalized ${prefix} for group ${groupId}:`,
                    group.map(item => `${item.name}: ${item.displayOrder}`).join(', '));
            }

            return Object.values(groupedItems).flat();
        }
        else if (prefix.includes('Types')) {
            const groupedItems = {};
            items.forEach(item => {
                if (!groupedItems[item.specificationCategoryId]) {
                    groupedItems[item.specificationCategoryId] = [];
                }
                groupedItems[item.specificationCategoryId].push({ ...item });
            });

            for (const groupId in groupedItems) {
                const group = groupedItems[groupId];

                const existingTypes = getExistingTypesForCategory(groupId);
                const maxExistingOrder = existingTypes.length > 0 ? Math.max(...existingTypes
                    .map(t => t.displayOrder || 0)) : 0;

                group.sort((a, b) => (a.displayOrder || 0) - (b.displayOrder || 0));

                group.forEach((item, index) => {
                    if (prefix.includes('Created')) {
                        item.displayOrder = maxExistingOrder + index + 1;
                    }
                });
                console.log(`prepareItemsForFormData, types: Normalized ${prefix} for group ${groupId}:`,
                    group.map(item => `${item.name}: ${item.displayOrder}`).join(', '));
            }

            return Object.values(groupedItems).flat();
        }

        return items;
    }

    // Get next display order for new category or type
    function getNextDisplayOrder(items) {
        if (!items || items.length === 0) return 1;

        // Indexing existing items by displayOrder
        const reindexedItems = reindexDisplayOrder([...items]);

        // Return the last displayOrder
        return reindexedItems.length + 1;
    }

    // Helper function to sync cached categories with pendingChanges
    function syncCacheWithPendingChanges(categoryId) {
        console.log('Syncing cache with pending changes for category:', categoryId);

        if (!localSpecCategoriesCache[categoryId]) {
            console.warn(`No cached categories found for product category ${categoryId}`);
            return;
        }

        // Save original state for comparison
        const originalCategories = window.originalSpecificationsData?.categories || [];
        console.log('Original categories:', originalCategories.length)

        const createdCategoryIds = new Set(pendingChanges.createdCategories.map(c => c.id));
        const updatedCategoryIds = new Set(pendingChanges.updatedCategories.map(c => c.id));
        const deletedCategoryIds = new Set(pendingChanges.deletedCategories.map(c => c.id));

        const categoriesToProcess = localSpecCategoriesCache[categoryId].splice();
        console.log(`Processing ${categoriesToProcess.length} cached categories`);

        let hasChanges = false;

        for (const cachedCategory of categoriesToProcess) {
            // Skip if already in pending changes
            if (createdCategoryIds.has(cachedCategory.id) ||
                updatedCategoryIds.has(cachedCategory.id) ||
                deletedCategoryIds.has(cachedCategory.id)) {
                console.log(`Category ${cachedCategory.name} is already in pending changes, skipping`);
                continue;
            }

            const originalCategory = originalCategories.find(c => c.id === cachedCategory.id);

            if (originalCategory) {
                // Compare with original data and add to updatedCategories if changed
                if (originalCategory.name !== cachedCategory.name ||
                    originalCategory.displayOrder !== cachedCategory.displayOrder) {
                    console.log(`Category ${cachedCategory.name} has changed, adding to updatedCategories`);
                    pendingChanges.updatedCategories.push({ ...cachedCategory });
                    hasChanges = true;
                } else {
                    console.log(`Category ${cachedCategory.name} has not changed, skipping`);
                }
            } else {
                // If category not found in original data, it is new
                console.log(`Category ${cachedCategory.name} is new, adding to createdCategories`);
                pendingChanges.createdCategories.push({ ...cachedCategory });
                hasChanges = true;
            }
        }

        if (hasChanges) {
            setHasChanges(true);
            checkForChanges();
        }
    }

    function syncTypeCacheWithPendingChanges(specCategoryId) {
        console.log('Syncing type cache with pending changes for spec category:', specCategoryId);

        if (!localSpecTypesCache[specCategoryId]) {
            console.warn(`No cached types found for spec category ${specCategoryId}`);
            return;
        }

        // Save original state for comparison
        const originalTypes = window.originalSpecificationsData?.types || [];
        console.log('Original types:', originalTypes.length)

        const createdTypeIds = new Set(pendingChanges.createdTypes.map(t => t.id));
        const updatedTypeIds = new Set(pendingChanges.updatedTypes.map(t => t.id));
        const deletedTypeIds = new Set(pendingChanges.deletedTypes.map(t => t.id));

        const typesToProcess = localSpecTypesCache[specCategoryId].splice();
        console.log(`Processing ${typesToProcess.length} cached types`);

        let hasChanges = false;

        for (const cachedType of typesToProcess) {
            // Skip if already in pending changes
            if (createdTypeIds.has(cachedType.id) ||
                updatedTypeIds.has(cachedType.id) ||
                deletedTypeIds.has(cachedType.id)) {
                console.log(`Type ${cachedType.name} is already in pending changes, skipping`);
                continue;
            }

            const originalType = originalTypes.find(t => t.id === cachedType.id);

            if (originalType) {
                // Compare with original data and add to updatedTypes if changed
                if (originalType.name !== cachedType.name ||
                    originalType.unit !== cachedType.unit ||
                    originalType.isFilterable !== cachedType.isFilterable ||
                    originalType.displayOrder !== cachedType.displayOrder) {
                    console.log(`Type ${cachedType.name} has changed, adding to updatedTypes`);
                    pendingChanges.updatedTypes.push({ ...cachedType });
                    hasChanges = true;
                } else {
                    console.log(`Type ${cachedType.name} has not changed, skipping`);
                }
            } else {
                // If type not found in original data, it is new
                console.log(`Type ${cachedType.name} is new, adding to createdTypes`);
                pendingChanges.createdTypes.push({ ...cachedType });
                hasChanges = true;
            }
        }

        if (hasChanges) {
            setHasChanges(true);
            checkForChanges();
        }
    }
    // Додайте цю функцію для синхронізації всіх елементів кешу з pendingChanges
    function syncAllCacheWithPendingChanges() {
        console.log('Синхронізація кешу з pendingChanges...');

        try {
            // Sync all categories in the cache
            for (const categoryId in localSpecCategoriesCache) {
                if (Object.prototype.hasOwnProperty.call(localSpecCategoriesCache, categoryId)) {
                    syncCacheWithPendingChanges(categoryId);
                }
            }

            // Sync all types in the cache
            for (const specCategoryId in localSpecTypesCache) {
                if (Object.prototype.hasOwnProperty.call(localSpecTypesCache, specCategoryId)) {
                    syncTypeCacheWithPendingChanges(specCategoryId);
                }
            }

            console.log('Sync completed successfully.');
            setHasChanges(checkForChanges());
        } catch (error) {
            console.error('Error during cache synchronization:', error);
        }
    }

    // Function to swap display order of items
    function swapDisplayOrder(items, itemId, newDisplayOrder) {
        if (!Array.isArray(items) || items.length === 0) return items;

        const itemsCopy = [...items];
        const currentItemIndex = itemsCopy.findIndex(item => item.id === itemId);

        if (currentItemIndex === -1) {
            console.warn(`Item with id ${itemId} not found`);
            return itemsCopy;
        }

        const currentItem = itemsCopy[currentItemIndex];
        const oldDisplayOrder = currentItem.displayOrder;

        const itemsWithSameOrder = itemsCopy.filter(item => item.id !== itemId && item.displayOrder === newDisplayOrder);

        if (itemsWithSameOrder.length > 0) {

            // Move all conflicting items to the old place of the current item
            itemsWithSameOrder.forEach(item => {
                item.displayOrder = oldDisplayOrder;
            });
        }

        // Update the display order of the current item
        currentItem.displayOrder = newDisplayOrder;

        return reindexDisplayOrder(itemsCopy);
    }

    // Function to ensure unique pending changes
    function ensureUniquePendingChanges() {
        // Crate ids sets for created, updated and deleted categories and types
        const createdCategoryIds = new Set(pendingChanges.createdCategories.map(c => c.id));
        const updatedCategoryIds = new Set(pendingChanges.updatedCategories.map(c => c.id));
        const deletedCategoryIds = new Set(pendingChanges.deletedCategories.map(c => c.id));

        const createdTypeIds = new Set(pendingChanges.createdTypes.map(t => t.id));
        const updatedTypeIds = new Set(pendingChanges.updatedTypes.map(t => t.id));
        const deletedTypeIds = new Set(pendingChanges.deletedTypes.map(t => t.id));

        // Видаляємо з updatedCategories ті, що є у createdCategories або deletedCategories
        pendingChanges.updatedCategories = pendingChanges.updatedCategories.filter(c =>
            !createdCategoryIds.has(c.id) && !deletedCategoryIds.has(c.id)
        );

        // Видаляємо з updatedTypes ті, що є у createdTypes або deletedTypes
        pendingChanges.updatedTypes = pendingChanges.updatedTypes.filter(t =>
            !createdTypeIds.has(t.id) && !deletedTypeIds.has(t.id)
        );

        // Видаляємо з createdCategories ті, що є у deletedCategories
        pendingChanges.createdCategories = pendingChanges.createdCategories.filter(c =>
            !deletedCategoryIds.has(c.id)
        );

        // Видаляємо з createdTypes ті, що є у deletedTypes
        pendingChanges.createdTypes = pendingChanges.createdTypes.filter(t =>
            !deletedTypeIds.has(t.id)
        );
    }

    // Додаємо функцію, яка гарантує унікальність усіх записів за ID
    function ensureUniqueEntries() {
        // Видалення дублікатів у всіх масивах змін
        pendingChanges.createdCategories = removeDuplicatesById(pendingChanges.createdCategories);
        pendingChanges.updatedCategories = removeDuplicatesById(pendingChanges.updatedCategories);
        pendingChanges.deletedCategories = removeDuplicatesById(pendingChanges.deletedCategories);
        pendingChanges.createdTypes = removeDuplicatesById(pendingChanges.createdTypes);
        pendingChanges.updatedTypes = removeDuplicatesById(pendingChanges.updatedTypes);
        pendingChanges.deletedTypes = removeDuplicatesById(pendingChanges.deletedTypes);

        // Забезпечуємо консистентність (не можна одночасно створювати і оновлювати один запис)
        // Видаляємо з updatedCategories ті, що є у createdCategories
        pendingChanges.updatedCategories = pendingChanges.updatedCategories
            .filter(updated => !pendingChanges.createdCategories.some(created => created.id === updated.id));

        // Видаляємо з updatedTypes ті, що є у createdTypes
        pendingChanges.updatedTypes = pendingChanges.updatedTypes
            .filter(updated => !pendingChanges.createdTypes.some(created => created.id === updated.id));

        // Якщо запис є у deletedCategories, його не повинно бути у createdCategories чи updatedCategories
        const deletedCategoryIds = pendingChanges.deletedCategories.map(c => c.id);
        pendingChanges.createdCategories = pendingChanges.createdCategories
            .filter(c => !deletedCategoryIds.includes(c.id));
        pendingChanges.updatedCategories = pendingChanges.updatedCategories
            .filter(c => !deletedCategoryIds.includes(c.id));

        // Те ж саме для типів
        const deletedTypeIds = pendingChanges.deletedTypes.map(t => t.id);
        pendingChanges.createdTypes = pendingChanges.createdTypes
            .filter(t => !deletedTypeIds.includes(t.id));
        pendingChanges.updatedTypes = pendingChanges.updatedTypes
            .filter(t => !deletedTypeIds.includes(t.id));

        // Забезпечуємо унікальні displayOrder для кожної категорії/групи типів
        ensureUniqueDisplayOrdersByGroup();
    }

    // Функція для видалення дублікатів за ID
    function removeDuplicatesById(items) {
        const uniqueItems = [];
        const seenIds = new Set();

        for (const item of items) {
            if (!seenIds.has(item.id)) {
                seenIds.add(item.id);
                uniqueItems.push(item);
            }
        }

        return uniqueItems;
    }

    // Забезпечуємо унікальні displayOrder в межах групи
    function ensureUniqueDisplayOrdersByGroup() {
        // Групуємо категорії за ProductCategoryId
        const categoriesByProductId = {};

        // Обробляємо createdCategories
        for (const category of pendingChanges.createdCategories) {
            if (!categoriesByProductId[category.productCategoryId]) {
                categoriesByProductId[category.productCategoryId] = [];
            }
            categoriesByProductId[category.productCategoryId].push(category);
        }

        // Обробляємо updatedCategories
        for (const category of pendingChanges.updatedCategories) {
            if (!categoriesByProductId[category.productCategoryId]) {
                categoriesByProductId[category.productCategoryId] = [];
            }
            categoriesByProductId[category.productCategoryId].push(category);
        }

        // Для кожної групи забезпечуємо унікальні displayOrder
        for (const productCategoryId in categoriesByProductId) {
            const categories = categoriesByProductId[productCategoryId];

            // Сортуємо категорії за displayOrder
            categories.sort((a, b) => a.displayOrder - b.displayOrder);

            // Переіндексуємо з 1
            for (let i = 0; i < categories.length; i++) {
                categories[i].displayOrder = i + 1;
            }
        }

        // Аналогічно для типів
        const typesBySpecCategoryId = {};

        // Обробляємо createdTypes
        for (const type of pendingChanges.createdTypes) {
            if (!typesBySpecCategoryId[type.specificationCategoryId]) {
                typesBySpecCategoryId[type.specificationCategoryId] = [];
            }
            typesBySpecCategoryId[type.specificationCategoryId].push(type);
        }

        // Обробляємо updatedTypes
        for (const type of pendingChanges.updatedTypes) {
            if (!typesBySpecCategoryId[type.specificationCategoryId]) {
                typesBySpecCategoryId[type.specificationCategoryId] = [];
            }
            typesBySpecCategoryId[type.specificationCategoryId].push(type);
        }

        // Для кожної групи забезпечуємо унікальні displayOrder
        for (const specCategoryId in typesBySpecCategoryId) {
            const types = typesBySpecCategoryId[specCategoryId];

            // Сортуємо типи за displayOrder
            types.sort((a, b) => a.displayOrder - b.displayOrder);

            // Переіндексуємо з 1
            for (let i = 0; i < types.length; i++) {
                types[i].displayOrder = i + 1;
            }
        }
    }

    function ensureOrderUniqueness(items) {
        if (!items || items.length <= 1) return true;

        const orderCounts = {};
        for (const item of items) {
            orderCounts[item.displayOrder] = (orderCounts[item.displayOrder] || 0) + 1;
        }

        // Check for duplicates
        const duplicates = Object.entries(orderCounts).filter(([_, count]) => count > 1);
        if (duplicates.length > 0) {
            console.warn("Duplicated display orders:", duplicates.map(([order, count]) => `${order} (${count} items)`).join(", "));
            return false;
        }

        return true;
    }

    function ensureUniqueOrdersBeforeSave() {
        console.log('Перевірка та виправлення порядкових номерів перед збереженням...');
        // Check unique indexes in all collections
        for (const categoryId in localSpecCategoriesCache) {
            if (localSpecCategoriesCache[categoryId]?.length > 0) {
                localSpecCategoriesCache[categoryId] = reindexDisplayOrder(localSpecCategoriesCache[categoryId]);
            }
        }
        for (const categoryId in localSpecTypesCache) {
            if (localSpecTypesCache[categoryId]?.length > 0) {
                localSpecTypesCache[categoryId] = reindexDisplayOrder(localSpecTypesCache[categoryId]);
            }
        }

        // Handle createdCategories they need to be reindexed
        if (pendingChanges.createdCategories.length > 0) {
            const categoryGroups = {};

            for (const category of pendingChanges.createdCategories) {
                if (!categoryGroups[category.productCategoryId]) {
                    categoryGroups[category.productCategoryId] = [];
                }
                categoryGroups[category.productCategoryId].push(category);
            }

            for (const groupId in categoryGroups) {
                const existingCategories = getExistingCategoriesForGroup(groupId);
                const maxOrder = existingCategories.length > 0 ? Math.max(...existingCategories
                    .map(c => c.displayOrder || 0)) : 0;
                const group = categoryGroups[groupId];

                group.sort((a, b) => (a.displayOrder || 0) - (b.displayOrder || 0));
                group.forEach((item, index) => {
                    item.displayOrder = maxOrder + index + 1;
                });
            }
        }

        // Handle createdTypes they need to be reindexed
        if (pendingChanges.createdTypes.length > 0) {
            const typeGroups = {};

            for (const type of pendingChanges.createdTypes) {
                if (!typeGroups[type.specificationCategoryId]) {
                    typeGroups[type.specificationCategoryId] = [];
                }
                typeGroups[type.specificationCategoryId].push(type);
            }

            // For each group, reindex displayOrder
            for (const groupId in typeGroups) {
                const existingTypes = getExistingTypesForCategory(groupId);
                const maxOrder = existingTypes.length > 0 ? Math.max(...existingTypes
                    .map(t => t.displayOrder || 0)) : 0;
                const group = typeGroups[groupId];

                group.sort((a, b) => (a.displayOrder || 0) - (b.displayOrder || 0));
                group.forEach((item, index) => {
                    item.displayOrder = maxOrder + index + 1;
                });
            }
        }

        // Handle updatedTypes
        if (pendingChanges.updatedTypes.length > 0) {
            pendingChanges.updatedTypes.forEach(updatedType => {
                const cachedTypes = localSpecTypesCache[updatedType.specificationCategoryId] || [];
                const cachedType = cachedTypes.find(t => t.id === updatedType.id);
                if (cachedType) {
                    updatedType.displayOrder = cachedType.displayOrder;
                }
            });
        }

        console.log("Перевірка та виправлення порядкових номерів завершена.");
    }

    // Допоміжна функція для видалення дублікатів за ключем
    function removeDuplicates(array, key) {
        return array.filter((item, index, self) =>
            index === self.findIndex(t => t[key] === item[key])
        );
    }

    function trackChanges(item, originalItem, type) {
        if (!originalItem) {
            // Якщо це новий елемент
            addToPendingChanges(item, `created${type}`);
            return;
        }

        // Перевірка на зміни
        let changed = false;

        if (type === 'Category') {
            changed = originalItem.name !== item.name ||
                originalItem.displayOrder !== item.displayOrder;
        } else if (type === 'Type') {
            changed = originalItem.name !== item.name ||
                originalItem.displayOrder !== item.displayOrder ||
                originalItem.unit !== item.unit ||
                originalItem.isFilterable !== item.isFilterable;
        }

        if (changed) {
            addToPendingChanges(item, `updated${type}`);
        }
    }

    // Функція для очищення змін
    function clearPendingChanges() {
        pendingChanges = {
            createdCategories: [],
            updatedCategories: [],
            deletedCategories: [],
            createdTypes: [],
            updatedTypes: [],
            deletedTypes: []
        };
    }

    function validatePendingChanges() {
        // Перевіряємо наявність обов'язкових полів
        for (const category of [...pendingChanges.createdCategories, ...pendingChanges.updatedCategories]) {
            if (!category.name || !category.name.trim()) {
                return { valid: false, message: 'Назва категорії не може бути порожньою' };
            }
            if (!category.productCategoryId) {
                return { valid: false, message: 'Категорія товару не вказана' };
            }
        }

        for (const type of [...pendingChanges.createdTypes, ...pendingChanges.updatedTypes]) {
            if (!type.name || !type.name.trim()) {
                return { valid: false, message: 'Назва типу характеристики не може бути порожньою' };
            }
            if (!type.specificationCategoryId) {
                return { valid: false, message: 'Категорія характеристики не вказана' };
            }
        }

        return { valid: true };
    }

    function debugDisplayOrders() {
        console.log('=== ДІАГНОСТИКА DISPLAYORDER ===');

        console.log('PendingChanges.createdTypes:');
        pendingChanges.createdTypes.forEach(type => {
            console.log(`  ${type.name}: ${type.displayOrder} (category: ${type.specificationCategoryId})`);
        });

        console.log('PendingChanges.updatedTypes:');
        pendingChanges.updatedTypes.forEach(type => {
            console.log(`  ${type.name}: ${type.displayOrder} (category: ${type.specificationCategoryId})`);
        });

        console.log('LocalSpecTypesCache:');
        for (const categoryId in localSpecTypesCache) {
            console.log(`  Category ${categoryId}:`);
            localSpecTypesCache[categoryId].forEach(type => {
                console.log(`    ${type.name}: ${type.displayOrder}`);
            });
        }

        console.log('=== КІНЕЦЬ ДІАГНОСТИКИ ===');
    }

    function showNotification(message, type = 'info') {
        // Створюємо елемент повідомлення
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} position-fixed bottom-0 end-0 m-3`;
        notification.style.zIndex = "9999";
        notification.innerHTML = message;

        // Додаємо на сторінку
        document.body.appendChild(notification);

        // Видаляємо через 3 секунди
        setTimeout(() => {
            notification.classList.add('fade');
            setTimeout(() => notification.remove(), 500);
        }, 3000);
    }
});

