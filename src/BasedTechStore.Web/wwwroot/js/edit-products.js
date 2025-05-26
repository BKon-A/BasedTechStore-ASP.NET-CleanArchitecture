window.isRedirectingUser = false;

function showUnsavedChangesWarning(redirectUrl) {
    if (!window.createInfoModal) {
        console.error('createInfoModal function is not defined. Ensure that the modal library is loaded.');

        window.location.href = redirectUrl;
        return;

        window.createInfoModal({
            id: 'unsavedChangesModal',
            title: 'Незбережені зміни',
            message: 'На сторінці є незбережені зміни. Що ви хочете зробити?',
            type: 'warning',
            showCancelButton: true,
            showConfirmButton: true,
            showSaveButton: true,
            cancelText: 'Скасувати',
            confirmText: 'Вийти без збереження',
            saveText: 'Зберегти та вийти',
            backdrop: true,
            closeButton: false,
            autoClose: true,
            onCancel: function () {
                console.log("User cancelled navigation");
            },
            onConfirm: function () {
                console.log("User chose to leave without saving, redirecting to:", redirectUrl);

                window.hasUnsavedChanges = false;
                window.isRedirectingUser = true;
                window.location.href = redirectUrl;
            },
            onSave: function () {
                console.log("User chose to save and leave, redirecting to:", redirectUrl);

                // Save and then redirect
                window.hasUnsavedChanges = false;
                window.isRedirectingUser = true;

                const saveBtn = document.getElementById('saveProductsBtn');
                if (saveBtn) {
                    saveBtn.click();

                    // Redirect after save
                    setTimeout(() => {
                        window.location.href = redirectUrl;
                    }, 1000);
                } else {
                    console.error("Save button not found");

                    // If save button is not found, just redirect
                    window.location.href = redirectUrl;
                }
            }
        });
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const productModal = new bootstrap.Modal(document.getElementById('productModal'));
    const productModalForm = document.getElementById('productModalForm');

    // Image upload form variables
    const imageInput = document.getElementById('productImageUrl');
    const removeImageBtn = document.getElementById('removeImageBtn');
    const imagePreview = document.getElementById('previewImage');
    const productImageUrlHidden = document.getElementById('productImageUrlHidden');
    const productImageUrlOriginal = document.getElementById('productImageUrlOriginal');

    const addProductBtn = document.getElementById('addProductBtn');
    const saveProductBtn = document.getElementById('saveProductBtn');
    const saveProductsBtn = document.getElementById('saveProductsBtn');
    const deleteProductBtn = document.getElementById('deleteSelectedProductsBtn');
    const tableBody = document.querySelector('#productsTable tbody');
    const selectAllCheckbox = document.getElementById('selectAllProductsCheckbox');

    const categorySelect = document.getElementById('productCategorySelect');
    const subCategorySelect = document.getElementById('productSubCategorySelect');

    // ============== WARNING MODAL ==============
    console.log("ModalHandlers:", window.modalHandlers);
    console.log("WarningModal:", window.modalHandlers ? window.modalHandlers["unsavedChangesModal"] : null);

    // Tracking changes in the form
    const form = document.getElementById('saveProductsForm');
    if (form) {
        form.addEventListener('input', function () {
            window.setUnsavedChanges(true);
        });

        // Відстеження змін через кнопку додавання
        addProductBtn.addEventListener('click', function () {
            // Прапорець буде встановлено при додаванні рядка
        });

        // Скидання прапорця при збереженні
        saveProductsBtn.addEventListener('click', function () {
            // Прапорець буде скинуто після успішного збереження
        });
    }

    // Intercepting clicks on links
    document.addEventListener('click', function (e) {

        const link = e.target.closest('a:not([download]):not([target="_blank"])');
        if (!link) return;

        if (!window.hasUnsavedChanges) return;

        e.preventDefault();
        console.log("Navigation intercepted to:", link.href);

        showUnsavedChangesWarning(link.href);
    });

    // Перехоплення закриття вкладки/браузера
    window.addEventListener('beforeunload', function (e) {
        const isUserRedirecting = window.isRedirectingUser || false;
        if (window.hasUnsavedChanges && !isUserRedirecting) {
            const message = 'На сторінці є незбережені зміни. Ви справді хочете покинути сторінку?';
            e.returnValue = message;
            return message;
        }
    });

    // Handle browser back and forward buttons
    window.addEventListener('popstate', function (e) {
        if (window.hasUnsavedChanges) {
            // Avoid default behavior
            e.preventDefault();

            // Show modal with confirmation
            const targetUrl = document.referrer || '/';
            showUnsavedChangesWarning(targetUrl);

            // History restore
            history.pushState(null, '', window.location.href);
            return false;
        }
    });

    document.getElementById('backToProfileBtn')?.addEventListener('click', function (e) {
        if (window.hasUnsavedChanges) {
            e.preventDefault();

            const profileUrl = '@Url.Action("Index", "Profile")';
            console.log("Profile navigation intercepted, redirecting to:", profileUrl);

            showUnsavedChangesWarning(profileUrl);
        }
    });

    window.setUnsavedChanges = function (hasChanges) {
        window.hasUnsavedChanges = hasChanges;
        console.log("hasUnsavedChanges set to:", hasChanges);
    };

    window.hasUnsavedChanges = false; // Initialize the flag
    window.showUnsavedChangesWarning = showUnsavedChangesWarning;
    // =============== END WARNING MODAL ===============

    categorySelect.addEventListener('change', async function () {
        const selectedCategory = this.value;

        let visibleOptions = 0;

        Array.from(subCategorySelect.options).forEach(option => {
            const optionCategory = option.getAttribute('data-category');

            if (!selectedCategory || !optionCategory) {
                option.classList.remove('d-none');
                if (option.value) visibleOptions++;
            } else if (optionCategory === selectedCategory) {
                option.classList.remove('d-none');
                if (option.value) visibleOptions++;
            } else {
                option.classList.add('d-none');
            }
        });

        // Reset the subcategory selection if the selected category changes
        subCategorySelect.value = '';

        // Load specifications for this product category
        if (this.value) {
            try {
                const response = await fetch(`/AdminPanel/GetSpecificationTypesByCategory?categoryId=${this.value}`);
                if (!response.ok) {
                    throw new Error('Failed to fetch specifications');
                }

                const specTypes = await response.json();

                // Find or create specifications container
                let specsContainer = document.getElementById('productSpecificationsContainer');
                if (!specsContainer) {
                    specsContainer = document.createElement('div');
                    specsContainer.id = 'productSpecificationsContainer';
                    specsContainer.className = 'mt-3';
                    document.querySelector('.modal-body').appendChild(specsContainer);
                }

                // Clear existing fields
                specsContainer.innerHTML = '';

                if (specTypes && specTypes.length > 0) {
                    // Group specifications by category
                    const specsByCategory = {};

                    specTypes.forEach(spec => {
                        const categoryName = spec.specificationCategoryName;
                        if (!specsByCategory[categoryName]) {
                            specsByCategory[categoryName] = [];
                        }
                        specsByCategory[categoryName].push(spec);
                    });

                    // Create fields for each category
                    for (const [categoryName, specs] of Object.entries(specsByCategory)) {
                        // Create category header
                        const categoryTitle = document.createElement('h5');
                        categoryTitle.className = 'mt-3 mb-2';
                        categoryTitle.textContent = categoryName;
                        specsContainer.appendChild(categoryTitle);

                        // Create container for this category's specs
                        const categoryRow = document.createElement('div');
                        categoryRow.className = 'row';
                        specsContainer.appendChild(categoryRow);

                        // Add each specification
                        specs.forEach(spec => {
                            const specCol = document.createElement('div');
                            specCol.className = 'col-md-4 mb-2';
                            specCol.innerHTML = `
                            <label class="form-label">${spec.name}${spec.unit ? ` (${spec.unit})` : ''}</label>
                            <input type="text" 
                                class="form-control form-control-sm product-spec" 
                                data-spec-id="${spec.id}"
                                data-spec-name="${spec.name}"
                                data-spec-unit="${spec.unit || ''}">
                        `;
                            categoryRow.appendChild(specCol);
                        });
                    }
                } else {
                    specsContainer.innerHTML = '<p class="text-muted">Для цієї категорії не знайдено характеристик.</p>';
                }
            } catch (error) {
                console.error('Error loading specifications:', error);
            }
        }
    });

    // Modal open the new product
    addProductBtn.addEventListener('click', () => {
        clearModal();
        productModal.show();
    });

    // Edit existing product
    tableBody.addEventListener('click', function (e) {
        if (e.target.closest('.editProductModalBtn')) {
            const row = e.target.closest("tr");
            fillModalFromRow(row);
            productModal.show();
        }
    });

    // Save from modal to table (not DB yet)
    saveProductBtn.addEventListener('click', function (e) {
        e.preventDefault();

        const productName = document.getElementById('productName').value.trim();

        if (productName === "") {
            alert('Введіть назву продукту');
            return;
        }

        if (!categorySelect.value) {
            alert('Оберіть категорію');
            return;
        }

        if (!subCategorySelect.value) {
            alert('Оберіть підкатегорію');
            return;
        }

        const formData = readModalData();
        parseDecimalInput(formData);

        if (formData.name.trim() === "") {
            alert('Введіть назву продукту');
            return;
        }

        if (formData.id) {
            const existingRow = Array.from(tableBody.rows).find(
                row => row.querySelector('input[name$=".Id"]').value === formData.id
            );

            if (existingRow) {
                const oldImageUrl = existingRow.querySelector('input[name$=".ImageUrl"]').value;

                if (oldImageUrl && oldImageUrl !== formData.imageUrl && !oldImageUrl.includes('default-image.jpg')) {
                    if (oldImageUrl !== productImageUrlOriginal) {
                        if (!window.tempUploadedImages.includes(oldImageUrl)) {
                            window.tempUploadedImages.push(oldImageUrl);
                        }
                    }
                }

                updateRowFromData(existingRow, formData);
                window.setUnsavedChanges(true);
            }
        } else {
            const newRow = buildTableRow(formData);
            tableBody.appendChild(newRow);
            reindexProductRows();
            window.setUnsavedChanges(true);
        }

        // Save specifications for existing products
        const productId = document.getElementById('productId').value;
        if (productId) {
            const specifications = [];
            document.querySelectorAll('.product-spec').forEach(input => {
                if (input.value.trim()) {
                    specifications.push({
                        specificationTypeId: input.dataset.specId,
                        value: input.value.trim()
                    });
                }
            });

            if (specifications.length > 0) {
                fetch('/AdminPanel/SaveProductSpecifications', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({
                        productId: productId,
                        specifications: specifications
                    })
                })
                    .catch(error => console.error('Error saving specifications:', error));
            }
        }

        productModal.hide();
    });

    // SelectAllCheckbox logic
    selectAllCheckbox.addEventListener('change', function () {
        const checked = this.checked;
        document.querySelectorAll('.rowProductsCheckbox').forEach(chk => {
            chk.checked = checked;
        });
        toggleDeleteButton();
    });

    // Individual checkbox logic
    tableBody.addEventListener('click', function (e) {
        if (e.target.classList.contains('rowProductsCheckbox')) {
            toggleDeleteButton();
            updateSelectAllCheckboxState();
        }
    })

    // Delete selected products
    deleteProductBtn.addEventListener('click', function () {
        // Замість звичайного confirm
        window.createInfoModal({
            id: 'deleteProductsModal',
            title: 'Видалення продуктів',
            message: 'Ви дійсно хочете видалити вибрані продукти?',
            type: 'danger',
            showSaveButton: false,
            onConfirm: function () {
                document.querySelectorAll('.rowProductsCheckbox:checked').forEach(cb => cb.closest('tr').remove());
                updateSelectAllCheckboxState();
                toggleDeleteButton();
                window.setUnsavedChanges(true);
            }
        });
    });

    // Save button logic
    saveProductsBtn.addEventListener('click', async function (e) {
        e.preventDefault();

        const form = document.getElementById('saveProductsForm');
        const formData = normalizeDecimalFieldsByClass(form);
        for (const [key, value] of formData.entries()) {
            console.log(key, value);
        }

        console.log('Form data before sending:', formData);

        // Reindex the product rows before sending
        reindexProductRows();

        fetch(form.action, {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest' // Optional, but can be useful for server-side handling'
            }
        })
            .then(response => {
                console.log('Response received:', response);
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.text(); // or response.json() if the server returns JSON
            })
            .then(data => {
                console.log('Data received:', data);
                const productsContainer = document.getElementById('productsContainer');
                if (productsContainer) {
                    productsContainer.innerHTML = data;
                    alert('Products saved successfully!');

                    // Flag reset
                    window.setUnsavedChanges(false);

                    window.tempUploadedImages = [];

                    tableBody.querySelectorAll('tr').forEach(row => {
                        const imageUrl = row.querySelector('input[name$=".ImageUrl"]').value;
                        const productId = row.querySelector('input[name$=".Id"]').value;

                        row.dataset.productImageUrlOriginal = imageUrl;
                    })
                }
            })
            .catch(error => {
                console.error('Error saving products:', error);
                alert('Error saving products: ' + error.message);
            });
    });

    document.getElementById('productModal').addEventListener('hidden.bs.modal', function (e) {
        if (window.tempUploadedImages && window.tempUploadedImages.length > 0) {
            const imageUrl = productImageUrlHidden.value;
            const originalImageUrl = productImageUrlOriginal.value;

            const imagesToDelete = window.tempUploadedImages.filter(url =>
                url !== imageUrl || imageUrl !== originalImageUrl
            );

            if (imagesToDelete.length > 0) {
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

                const formData = FormData();

                imagesToDelete.forEach((url, index) => {
                    formData.append(`imageUrls[${index}]`, url);
                });

                formData.append('__RequestVerificationToken', token);

                fetch('/AdminPanel/DeleteUnusedImages', {
                    method: 'POST',
                    body: formData
                })
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Network response was not ok');
                        }
                        response.text()
                    })
                    .then(result => {
                        console.log('Images deleted:', result);

                        window.tempUploadedImages = [];
                    })
                    .catch(error => {
                        console.error('Error deleting images:', error);
                    });
            }
        }
    });

    // Image upload logic
    imageInput.addEventListener('change', function () {
        const file = this.files[0];
        if (file) {

            if (productImageUrlHidden && productImageUrlHidden.value.includes(file.name)) {
                alert('Це зображення вже завантажено');
                return;
            }

            // Показуємо прев'ю
            const reader = new FileReader();
            reader.onload = function (e) {
                imagePreview.src = e.target.result;
                imagePreview.classList.remove('d-none');
                imagePreview.style.display = 'block';
                removeImageBtn.classList.remove('d-none');
            };
            reader.readAsDataURL(file);

            const formData = new FormData();
            formData.append('image', file);

            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            formData.append('__RequestVerificationToken', token);

            fetch('/AdminPanel/UploadImage', {
                method: 'POST',
                body: formData
            })
                .then(response => response.text())
                .then(text => {
                    if (text === 'error') {
                        alert('Помилка при завантаженні зображення');
                    } else {
                        // if a previous image was uploaded, add it to the tempUploadedImages array
                        if (productImageUrlHidden && productImageUrlHidden !== productImageUrlOriginal &&
                            !window.tempUploadedImages.includes(productImageUrlHidden.value)) {
                            window.tempUploadedImages.push(productImageUrlHidden.value);
                        }

                        productImageUrlHidden.value = text;

                        // add the new image to the tempUploadedImages array
                        if (!window.tempUploadedImages.includes(text)) {
                            window.tempUploadedImages.push(text);
                        }

                        window.setUnsavedChanges(true);
                    }
                })
                .catch(error => {
                    console.error(error);
                    alert('Помилка при відправці запиту');
                });
        } else {
            imagePreview.src = '';
            imagePreview.style.display = 'none';
            removeImageBtn.classList.add('d-none');
        }
    });

    // Delete image logic
    if (removeImageBtn) {
        removeImageBtn.addEventListener('click', function () {
            // Зберігаємо порожній URL
            productImageUrlHidden.value = '';

            // Приховуємо превʼю
            imagePreview.src = '';
            imagePreview.style.display = 'none';
            imagePreview.classList.add('d-none');
            removeImageBtn.classList.add('d-none');

            // Помічаємо, що були зміни
            window.setUnsavedChanges(true);
        });
    }

    // ============== Start ProductDetails =============

    //function initProductButtons() {

    //    if (!document.querySelector('.product-actions')) {
    //        return;
    //    }
    //    console.log("ProductDetails: Initializing product buttons");
    //}

    //function addToCart(productId) {
    //    console.log("ProductDetails: Adding product to cart:", productId);

    //    showNotification('Товар успішно додано до кошика!', 'success');

    //    animateButton(document.querySelector('.add-to-cart-btn'), 'btn-success', 'btn-primary', 1000);
    //}

    //// Додавання товару в обрані
    //function addToWishlist(productId) {
    //    console.log(`Adding product to wishlist: ${productId}`);

    //    // Тут буде запит до API для додавання товару в обрані
    //    // Поки що просто показуємо повідомлення
    //    showNotification('Товар додано в обрані!', 'info');

    //    // Анімація кнопки для візуального фідбеку
    //    animateButton(document.querySelector('.btn-outline-danger'));
    //}

    //// Функція для відображення повідомлення
    //function showNotification(message, type = 'info') {
    //    // Створюємо елемент повідомлення
    //    const notification = document.createElement('div');
    //    notification.className = `alert alert-${type} notification-toast`;
    //    notification.innerHTML = message;

    //    // Додаємо на сторінку
    //    document.body.appendChild(notification);

    //    // Показуємо з анімацією
    //    setTimeout(() => {
    //        notification.classList.add('show');

    //        // Видаляємо через деякий час
    //        setTimeout(() => {
    //            notification.classList.remove('show');
    //            setTimeout(() => {
    //                notification.remove();
    //            }, 300);
    //        }, 2000);
    //    }, 100);
    //}

    //// Анімація кнопки при натисканні
    //function animateButton(button) {
    //    button.classList.add('btn-clicked');
    //    setTimeout(() => {
    //        button.classList.remove('btn-clicked');
    //    }, 300);
    //}
    // ============== END ProductDetails ===============

    // ============== END EVENT LISTENERS ==============

    function clearModal() {

        const inputs = productModalForm.querySelectorAll('input, textarea, select')

        inputs.forEach(input => {
            if (input.type === 'checkbox' || input.type === 'radio') {
                input.checked = false;
            } else if (input.tagName === 'SELECT') {
                input.selectedIndex = 0;
            } else {
                input.value = '';
            }
        });

        // Clear previewImage
        if (imagePreview) {
            imagePreview.src = '';
            imagePreview.style.display = 'none';
            imagePreview.classList.add('d-none');
        }

        productImageUrlHidden.value = '';
        productImageUrlOriginal.value = '';
        removeImageBtn.classList.add('d-none');
        window.tempUploadedImages = [];
    };

    function fillModalFromRow(row) {
        const productId = row.querySelector('input[name$=".Id"]').value;
        const productName = row.querySelector('input[name$=".Name"]').value;
        const categoryName = row.querySelector('input[name$=".CategoryName"]').value;
        const subCategoryId = row.querySelector('input[name$=".SubCategoryId"]').value;
        const description = row.querySelector('input[name$=".Description"]').value;
        const price = row.querySelector('input[name$=".Price"]').value;
        const brand = row.querySelector('input[name$=".Brand"]').value;
        const imageUrl = row.querySelector('input[name$=".ImageUrl"]').value;

        document.getElementById('productId').value = productId;
        document.getElementById('productName').value = productName;
        document.getElementById('productDescription').value = description;
        document.getElementById('productPrice').value = price;
        document.getElementById('productBrand').value = brand;
        productImageUrlHidden.value = imageUrl;
        productImageUrlOriginal.value = imageUrl;

        // Create temp array for uploaded images
        if (!window.tempUploadedImages) {
            window.tempUploadedImages = [];
        }

        // Reset the tempUploadedImages array
        window.tempUploadedImages = [];

        // Fill category and subcategory selects
        let categoryFound = false;;
        for (const option of categorySelect.options) {
            if (option.text === categoryName) {
                categorySelect.value = option.value;
                categoryFound = true;
                break;
            }
        }

        if (!categoryFound) {
            console.warn('Category not found:', categoryName);
        }

        // Trigger change event to show/hide subcategories
        categorySelect.dispatchEvent(new Event('change'));

        subCategorySelect.value = subCategoryId;

        setTimeout(() => {
            if (subCategoryId) {
                subCategorySelect.value = subCategoryId;

                if (subCategorySelect.value !== subCategoryId) {
                    console.warn('Subcategory not found:', subCategoryId);

                    const visibleOptions = Array.from(subCategorySelect.options)
                        .filter(option => !option.classList.contains('d-none') && option.value);

                    if (visibleOptions.length > 0) {
                        subCategorySelect.value = visibleOptions[0].value;
                        console.log('Subcategory set to first visible option:', visibleOptions[0].value);
                    }
                } else {
                    console.log('Subcategory found:', subCategoryId);
                }
            }

            if (imageUrl) {
                imagePreview.src = imageUrl;
                imagePreview.classList.remove('d-none');
                imagePreview.style.display = 'block';
                removeImageBtn.classList.remove('d-none');
            } else {
                imagePreview.src = '';
                imagePreview.style.display = 'none';
                imagePreview.classList.add('d-none');
                removeImageBtn.classList.add('d-none');
                console.log('Image URL is hiden(Url is empty)');
            }
        }, 100);

        // Load specifications for this product if it has an ID
        //const productId = row.querySelector('input[name$=".Id"]').value;
        if (productId) {

            const categoryId = categorySelect.value;

            fetch(`/AdminPanel/GetProductSpecifications?productId=${productId}&categoryId=${categoryId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.text();
                })
                .then(html => {
                    // Знаходимо або створюємо контейнер
                    let specsContainer = document.getElementById('productSpecificationsContainer');
                    if (!specsContainer) {
                        specsContainer = document.createElement('div');
                        specsContainer.id = 'productSpecificationsContainer';
                        specsContainer.className = 'mt-3';
                        document.querySelector('.modal-body').appendChild(specsContainer);
                    }

                    // Вставляємо HTML
                    specsContainer.innerHTML = html;
                })
                .catch(error => console.error('Error loading product specifications:', error));
        }
    };

    function readModalData() {
        let categoryName = '';
        let categoryId = categorySelect.value;
        if (categoryId) {
            const selectedOption = categorySelect.options[categorySelect.selectedIndex];
            if (selectedOption) {
                categoryName = selectedOption.text;
            }
        }

        let subCategoryName = '';
        let subCategoryId = subCategorySelect.value;
        if (subCategoryId) {
            const selectedOption = subCategorySelect.options[subCategorySelect.selectedIndex];
            if (selectedOption) {
                subCategoryName = selectedOption.text;
            }
        }

        // Collect specifications
        const specifications = [];
        document.querySelectorAll('.product-spec').forEach(input => {
            if (input.value.trim()) {
                specifications.push({
                    specificationTypeId: input.dataset.specId,
                    value: input.value.trim()
                });
            }
        });

        const result = {
            id: document.getElementById('productId').value,
            name: document.getElementById('productName').value,
            categoryName: categoryName,
            subCategoryId: subCategorySelect.value,
            subCategoryName: subCategoryName,
            description: document.getElementById('productDescription').value,
            price: document.getElementById('productPrice').value,
            brand: document.getElementById('productBrand').value,
            imageUrl: productImageUrlHidden.value,
            specifications: specifications
        };

        console.log('Modal data:', result);
        return result;
    }

    function updateRowFromData(row, data) {
        row.querySelector('input[name$=".Name"]').value = data.name;
        row.querySelector('input[name$=".CategoryName"]').value = data.categoryName;
        row.querySelector('input[name$=".SubCategoryId"]').value = data.subCategoryId;
        row.querySelector('input[name$=".SubCategoryName"]').value = data.subCategoryName;
        row.querySelector('input[name$=".Description"]').value = data.description;
        row.querySelector('input[name$=".Price"]').value = data.price;
        row.querySelector('input[name$=".Brand"]').value = data.brand;
        row.querySelector('input[name$=".ImageUrl"]').value = data.imageUrl;

        row.querySelector('.categoryText').value = data.categoryName;
        row.querySelector('.subCategoryText').value = data.subCategoryName;
    }

    function buildTableRow(data) {
        const index = tableBody.querySelectorAll('tr').length;
        const row = document.createElement('tr');

        row.innerHTML = `
        <td>
            <input type="checkbox" class="rowProductsCheckbox" />
            <input type="hidden" name="Products[${index}].Id" value="${data.id || crypto.randomUUID()}" />
            <input type="hidden" name="Products[${index}].CategoryName" value="${data.categoryName}" />
            <input type="hidden" name="Products[${index}].SubCategoryId" value="${data.subCategoryId}" />
            <input type="hidden" name="Products[${index}].SubCategoryName" value="${data.subCategoryName}" />
        </td>
        <td><input name="Products[${index}].Name" class="form-control form-control-sm" value="${data.name}" readonly/></td>
        <td><input class="form-control form-control-sm categoryText" value="${data.categoryName}" readonly/></td>
        <td><input class="form-control form-control-sm subCategoryText" value="${data.subCategoryName}" readonly/></td>
        <td><input name="Products[${index}].Description" class="form-control form-control-sm" value="${data.description}" readonly/></td>
        <td><input name="Products[${index}].Price" class="form-control form-control-sm dparse" type="text" inputmode="decimal" value="${data.price}" step="0.01" readonly/></td>
        <td><input name="Products[${index}].Brand" class="form-control form-control-sm" value="${data.brand}" readonly/></td>
        <td><input name="Products[${index}].ImageUrl" class="form-control form-control-sm" value="${data.imageUrl}" readonly/></td>
        <td>
            <button type="button" class="btn btn-sm btn-outline-primary editProductModalBtn" title="Редагувати">
                <i class="bi bi-pencil"></i>
            </button>
        </td>
    `;

        return row;
    }

    function toggleDeleteButton() {
        const anyChecked = document.querySelectorAll(".rowProductsCheckbox:checked").length > 0;
        deleteProductBtn.classList.toggle("d-none", !anyChecked);
    }

    function updateSelectAllCheckboxState() {
        const allCheckboxes = document.querySelectorAll(".rowProductsCheckbox");
        const checked = document.querySelectorAll(".rowProductsCheckbox:checked");
        selectAllCheckbox.checked = allCheckboxes.length === checked.length && allCheckboxes.length > 0;
    }

    function reindexProductRows() {
        const rows = tableBody.querySelectorAll('tr');
        rows.forEach((row, index) => {
            row.querySelectorAll('[name]').forEach(input => {
                const name = input.getAttribute('name');
                const newName = name.replace(/Products\[\d+\]/, `Products[${index}]`);
                input.setAttribute('name', newName);
            });
        });
    }

    function normalizeDecimalFieldsByClass(form) {
        const formData = new FormData(form);
        form.querySelectorAll('.dparse').forEach(input => {
            const key = input.name;
            const value = input.value.trim().replace(',', '.');
            const parsed = parseFloat(value);
            if (!isNaN(parsed)) {
                formData.set(key, parsed.toString());
            }
        });
        return formData;
    }

    function parseDecimalInput(data) {
        const decimalKeys = Object.keys(data).filter(
            key => {
                const input = document.querySelector(`[name="${key}"]`);
                return input && input.classList.contains('dparse');
            }
        );

        for (const key of decimalKeys) {
            const value = data[key];
            if (typeof value === 'string') {
                // Заміна коми на крапку, видалення зайвих пробілів
                const normalized = value.replace(',', '.').trim();
                const parsed = parseFloat(normalized);
                data[key] = isNaN(parsed) ? 0 : parsed;
            }
        }
    }
});

window.onSaveSuccess = function () {
    // Reset unsaved changes flag
    window.hasUnsavedChanges(false);
    console.log("Save successful, unsaved changes flag reset.");
};