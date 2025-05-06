//const categoriesContainer = document.getElementById('categoriesContainer');
const selectAllCheckbox = document.getElementById('selectAllCategoriesCheckbox');

const saveCategoryButton = document.getElementById('saveCategoriesButton');
    
const deleteCategoryBtn = document.getElementById('deleteSelectedCategoriesBtn');
const confirmCategoryDeleteBtn = document.getElementById('confirmCategoriesDeleteBtn');

function handleEditCategoryClick(e) {
    const clickedBtn = e.target.closest('.editCategoryBtn');
    if (!clickedBtn) return;

    const clickedRow = clickedBtn.closest('tr');
    if (!clickedRow) return;

    console.log("Click event detected on:", e.target);

    //const isCurrentlyEditing = Array.from(clickedRow.querySelectorAll('input')).some(input => !input.readOnly);
    const isCurrentlyEditing = clickedRow.dataset.editing === 'true';
    //clickedRow.dataset.editing = (!isCurrentlyEditing).toString();

    document.querySelectorAll('#categoriesTable tbody tr').forEach(row => {
        row.dataset.editing = 'false'; // Reset all rows to not editing
        row.querySelectorAll('input').forEach(input => input.setAttribute('readonly', true));
        row.querySelectorAll('.removeSubCategoryBtn').forEach(btn => btn.classList.add('d-none'));
        row.querySelectorAll('.addSubCategoryBtn').forEach(btn => btn.classList.add('d-none'));
    });

    if (!isCurrentlyEditing) {
        clickedRow.dataset.editing = 'true';
        clickedRow.querySelectorAll('input').forEach(input => input.removeAttribute('readonly'));
        clickedRow.querySelectorAll('.removeSubCategoryBtn').forEach(btn => btn.classList.remove('d-none'));
        clickedRow.querySelectorAll('.addSubCategoryBtn').forEach(btn => btn.classList.remove('d-none'));
    }
}


function bindCheckboxEvents() {
    document.querySelectorAll('.rowCategoriesCheckbox').forEach(cb => {
        cb.removeEventListener('change', updateSelectAllCheckboxState);
        cb.addEventListener('change', updateSelectAllCheckboxState);
    });
}

function updateSelectAllCheckboxState() {
    const itemCheckboxes = document.querySelectorAll('.rowCategoriesCheckbox');
    const anyChecked = Array.from(itemCheckboxes).some(cb => cb.checked);
    selectAllCheckbox.checked = anyChecked;
    toggleDeleteButton();
}

function toggleDeleteButton() {
    const anyChecked = document.querySelectorAll('.rowCategoriesCheckbox:checked').length > 0;
    deleteCategoryBtn.classList.toggle('d-none', !anyChecked);
}

function registerAllCategoryEvents() {
    //const container = document.getElementById('categoriesContainer');
    bindCheckboxEvents();
    updateSelectAllCheckboxState();
    toggleDeleteButton();

    //if (container) {
    //    container.removeEventListener('click', handleEditCategoryClick);
    //    container.addEventListener('click', handleEditCategoryClick);
    //}
}

//============== DOMContentLoaded ==============
document.addEventListener('DOMContentLoaded', () => {
    ////registerAllCategoryEvents();
    //document.addEventListener('click', handleEditCategoryClick);

    const categoriesTable = document.getElementById('categoriesTable');
    if (categoriesTable) {
        categoriesTable.addEventListener('click', handleEditCategoryClick);
    }

    const confirmDeleteModal = new bootstrap.Modal(document.getElementById('confirmCategoriesDeleteModal'));

    document.querySelectorAll('.rowCategoriesCheckbox').forEach(cb => {
        cb.addEventListener('change', updateSelectAllCheckboxState);
    });

    selectAllCheckbox.addEventListener('change', function () {
        const isChecked = selectAllCheckbox.checked;
        document.querySelectorAll('.rowCategoriesCheckbox').forEach(cb => {
            cb.checked = isChecked;
        });
        toggleDeleteButton();
    });

    deleteCategoryBtn.addEventListener('click', () => {
        confirmDeleteModal.show();
    });

    confirmCategoryDeleteBtn.addEventListener('click', () => {
        document.querySelectorAll('.rowCategoriesCheckbox:checked').forEach(cb => {
            cb.closest('tr').remove();
        });
        toggleDeleteButton();
        confirmDeleteModal.hide();
    });

    document.getElementById('addCategoryBtn').addEventListener('click', () => {
        const tbody = document.querySelector('#categoriesTable tbody');
        const index = tbody.querySelectorAll('tr').length;

        const row = document.createElement('tr');
        row.innerHTML = `
            <td><input type="checkbox" class="rowCategoriesCheckbox" /></td>
            <td>
                <input type="hidden" name="Categories[${index}].Id" value="00000000-0000-0000-0000-000000000000" />
                <input class="form-control form-control-sm category-name" name="Categories[${index}].Name" readonly />
            </td>
            <td>
                <div class="d-flex align-items-center mb-1 subcategory-row">
                    <input type="hidden" name="Categories[${index}].SubCategories[0].Id" value="00000000-0000-0000-0000-000000000000" />
                    <input class="form-control form-control-sm me-2 subcategory-name" name="Categories[${index}].SubCategories[0].Name" readonly />
                    <button type="button" class="btn btn-sm btn-outline-danger removeSubCategoryBtn d-none" title="Видалити підкатегорію">
                        <i class="bi bi-x"></i>
                    </button>
                </div>
                <button type="button" class="btn btn-outline-success btn-sm addSubCategoryBtn mt-1 d-none" data-category-index="${index}" title="Додати підкатегорію">
                    <i class="bi bi-plus"></i>
                </button>
            </td>
            <td>
                <button type="button" class="btn btn-sm btn-outline-primary editCategoryBtn" title="Редагувати">
                    <i class="bi bi-pencil"></i>
                </button>
            </td>`;
        tbody.appendChild(row);

        bindCheckboxEvents();
        updateSelectAllCheckboxState();
        toggleDeleteButton();
    });

    document.addEventListener('click', function (e) {
        if (e.target.closest('.removeSubCategoryBtn')) {
            e.preventDefault();
            e.target.closest('.subcategory-row').remove();
        }

        if (e.target.closest('.addSubCategoryBtn')) {
            const button = e.target.closest('.addSubCategoryBtn');
            const categoryIndex = button.dataset.categoryIndex;
            const subCategoryCount = button.parentElement.querySelectorAll('.subcategory-row').length;

            const newRow = document.createElement('div');
            newRow.classList.add('d-flex', 'align-items-center', 'mb-1', 'subcategory-row');
            newRow.innerHTML = `
                <input type="hidden" name="Categories[${categoryIndex}].SubCategories[${subCategoryCount}].Id" value="00000000-0000-0000-0000-000000000000" />
                <input class="form-control form-control-sm me-2 subcategory-name" name="Categories[${categoryIndex}].SubCategories[${subCategoryCount}].Name" />
                <button type="button" class="btn btn-sm btn-outline-danger removeSubCategoryBtn" title="Видалити підкатегорію">
                    <i class="bi bi-x"></i>
                </button>
            `;
            button.before(newRow);
        }
    });

    if (saveCategoryButton) {
        saveCategoryButton.addEventListener('click', async function (e) {
            e.preventDefault();

            const form = document.getElementById('saveCategoriesForm');
            const formData = new FormData(form);
            // const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;
            // formData.append('__RequestVerificationToken', csrfToken);

            fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest' // Optional, but can be useful for server-side handling'
                }
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Помилка збереження категорій.');
                    }
                    return response.text(); // HTML response
                })
                .then(html => {
                    const container = document.getElementById('categoriesContainer');
                    if (container) {
                        container.innerHTML = html;
                        registerAllCategoryEvents();
                    }
                })
                .catch(error => {
                    console.error('Помилка:', error);
                    alert('Сталася помилка під час збереження категорій.');
                });
        });
    }
});
//============== End DOMContentLoaded ==============