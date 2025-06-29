document.addEventListener('DOMContentLoaded', () => {
    window.productFilters = new ProductFilters();
    console.log('Product filters initialized');
});

class ProductFilters {
    constructor(formId = 'productFilterForm', resetBtnId = 'resetFiltersBtn') {
        this.filterForm = document.getElementById(formId);
        this.resetFiltersBtn = document.getElementById(resetBtnId);

        if (this.filterForm) {
            this.bindResetFiltersEvent();
            this.setupOffcanvasBehavior();
            this.initDoublePriceSlider();
            this.initSpecSliders();
            this.setupFilterHeaders();
            this.setupSearchFilters();
            this.setupBulkSelectionButtons();
        }
    }

    bindResetFiltersEvent() {
        if (!this.filterForm || !this.resetFiltersBtn) return;
        this.resetFiltersBtn.addEventListener('click', () => this.resetFilters());
    }

    setupOffcanvasBehavior() {
        const offcanvas = document.getElementById('filterOffcanvas');
        if (offcanvas) {
            offcanvas.addEventListener('click', (e) => {
                if (e.target.closest('form') && !e.target.closest('button')) {
                    e.stopPropagation();
                }
            });
        }
    }

    resetFilters() {
        // Скидаємо текстові та числові поля
        const inputs = this.filterForm.querySelectorAll('input[type="text"], input[type="number"]');
        inputs.forEach(input => {
            input.value = '';
        });

        // Скидаємо подвійний слайдер ціни
        const minPriceRange = document.getElementById('priceMinRange');
        const maxPriceRange = document.getElementById('priceMaxRange');
        const rangeSelection = document.querySelector('.range-selection');
        if (minPriceRange && maxPriceRange && rangeSelection) {
            minPriceRange.value = minPriceRange.min;
            maxPriceRange.value = maxPriceRange.max;
            this.updatePriceRangeSelection(minPriceRange, maxPriceRange, rangeSelection);

            const minPriceInput = document.getElementById('minPrice');
            const maxPriceInput = document.getElementById('maxPrice');
            if (minPriceInput) minPriceInput.value = '';
            if (maxPriceInput) maxPriceInput.value = '';
        }

        // Скидаємо слайдери специфікацій
        const specSliders = this.filterForm.querySelectorAll('.spec-range');
        specSliders.forEach(slider => {
            slider.value = slider.min;
            const specId = slider.closest('.spec-range-container').dataset.specId;
            const minInput = this.filterForm.querySelector(`input[name="spec_${specId}_min"]`);
            const maxInput = this.filterForm.querySelector(`input[name="spec_${specId}_max"]`);
            const valueDisplay = slider.parentNode.querySelector('.current-value');

            if (minInput) minInput.value = slider.min;
            if (maxInput) maxInput.value = slider.max;
            if (valueDisplay) valueDisplay.textContent = slider.min;
        });

        // Скидаємо чекбокси
        const checkboxes = this.filterForm.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(checkbox => checkbox.checked = false);

        this.filterForm.submit();
    }

    initDoublePriceSlider() {
        const minRangeInput = document.getElementById('priceMinRange');
        const maxRangeInput = document.getElementById('priceMaxRange');
        const minPriceInput = document.getElementById('minPrice');
        const maxPriceInput = document.getElementById('maxPrice');
        const rangeSelection = document.querySelector('.range-selection');

        if (!minRangeInput || !maxRangeInput || !minPriceInput || !maxPriceInput || !rangeSelection) return;

        // Встановлюємо початкові значення
        minRangeInput.value = minPriceInput.value || 0;
        maxRangeInput.value = maxPriceInput.value || 100000;

        // Оновлюємо візуальне відображення діапазону
        this.updatePriceRangeSelection(minRangeInput, maxRangeInput, rangeSelection);

        // Додаємо обробники подій для слайдерів
        minRangeInput.addEventListener('input', () => {
            let minValue = parseInt(minRangeInput.value);
            const maxValue = parseInt(maxRangeInput.value);

            if (minValue > maxValue) {
                minValue = maxValue;
                minRangeInput.value = maxValue;
            }

            minPriceInput.value = minValue;
            this.updatePriceRangeSelection(minRangeInput, maxRangeInput, rangeSelection);
        });

        maxRangeInput.addEventListener('input', () => {
            let maxValue = parseInt(maxRangeInput.value);
            const minValue = parseInt(minRangeInput.value);

            if (maxValue < minValue) {
                maxValue = minValue;
                maxRangeInput.value = minValue;
            }

            maxPriceInput.value = maxValue;
            this.updatePriceRangeSelection(minRangeInput, maxRangeInput, rangeSelection);
        });

        // Синхронізуємо інпути з слайдерами
        minPriceInput.addEventListener('change', () => {
            let value = parseInt(minPriceInput.value) || 0;
            const maxValue = parseInt(maxRangeInput.value);

            if (value > maxValue) {
                value = maxValue;
                minPriceInput.value = value;
            }

            minRangeInput.value = value;
            this.updatePriceRangeSelection(minRangeInput, maxRangeInput, rangeSelection);
        });

        maxPriceInput.addEventListener('change', () => {
            let value = parseInt(maxPriceInput.value) || 100000;
            const minValue = parseInt(minRangeInput.value);

            if (value < minValue) {
                value = minValue;
                maxPriceInput.value = value;
            }

            maxRangeInput.value = value;
            this.updatePriceRangeSelection(minRangeInput, maxRangeInput, rangeSelection);
        });
    }

    updatePriceRangeSelection(minRangeInput, maxRangeInput, rangeSelection) {
        const min = parseInt(minRangeInput.value);
        const max = parseInt(maxRangeInput.value);
        const minPos = (min / parseInt(minRangeInput.max)) * 100;
        const maxPos = 100 - (max / parseInt(maxRangeInput.max)) * 100;

        rangeSelection.style.left = minPos + '%';
        rangeSelection.style.right = maxPos + '%';
    }

    initSpecSliders() {
        const specContainers = this.filterForm.querySelectorAll('.spec-range-container');
        specContainers.forEach(container => {
            const specId = container.dataset.specId;
            const slider = container.querySelector('.spec-range');
            const minInput = this.filterForm.querySelector(`input[name="spec_${specId}_min"]`);
            const maxInput = this.filterForm.querySelector(`input[name="spec_${specId}_max"]`);
            const currentValue = container.querySelector('.current-value');

            if (!slider || !minInput || !maxInput || !currentValue) return;

            const step = parseFloat(slider.getAttribute('data-step') || 1);

            // Встановлюємо початкове значення
            slider.value = minInput.value || slider.min || 0;
            currentValue.textContent = this.formatNumberByStep(slider.value, step);

            // Додаємо обробник події для слайдера
            slider.addEventListener('input', () => {
                const formattedMinValue = this.formatNumberByStep(slider.value, step);
                currentValue.textContent = formattedMinValue;
                minInput.value = formattedMinValue;
            });

            // Synchronize inputs with slider
            minInput.addEventListener('change', () => {
                let value = minInput.value || slider.min || 0;
                value = value.toString().replace(',', '.');
                slider.value = value;
                currentValue.textContent = this.formatNumberByStep(value, step);
            });
        });
    }

    setupFilterHeaders() {
        const headers = this.filterForm.querySelectorAll('.filter-header');
        headers.forEach(header => {
            header.addEventListener('click', () => {
                header.classList.toggle('collapsed');
                const icon = header.querySelector('i');
                if (icon) {
                    icon.classList.toggle('bi-chevron-up');
                    icon.classList.toggle('bi-chevron-down');
                }
            });
        });
    }

    setupSearchFilters() {
        const searchInputs = this.filterForm.querySelectorAll('.filter-search');
        searchInputs.forEach(input => {
            input.addEventListener('input', (e) => {
                const searchTerm = e.target.value.toLowerCase();
                const container = input.closest('.filter-content');
                const items = container.querySelectorAll('.list-group-item');

                items.forEach(item => {
                    const text = item.textContent.toLowerCase();
                    const display = text.includes(searchTerm) ? '' : 'none';
                    item.style.display = display;
                });
            });
        });
    }

    setupBulkSelectionButtons() {
        // Select all functionality
        const selectAllButtons = this.filterForm.querySelectorAll('[data-action="select-all"]');
        selectAllButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                const container = button.closest('.filter-content');
                const checkboxes = container.querySelectorAll('input[type="checkbox"]');
                checkboxes.forEach(checkbox => checkbox.checked = true);
            });
        });

        // Clear all functionality
        const clearAllButtons = this.filterForm.querySelectorAll('[data-action="clear-all"]');
        clearAllButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                const container = button.closest('.filter-content');
                const checkboxes = container.querySelectorAll('input[type="checkbox"]');
                checkboxes.forEach(checkbox => checkbox.checked = false);
            });
        });
    }

    formatNumberByStep(value, step) {
        const numValue = parseFloat(value);
        if (isNaN(numValue)) return value;

        if (step < 1) {
            const decimals = step.toString().split('.')[1]?.length || 0;
            return numValue.toFixed(decimals);
        }
        return Math.round(numValue).toString();
    }
}