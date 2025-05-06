class ModalHandler {
    constructor(modalId) {
        this.modalId = modalId;
        this.modal = document.getElementById(modalId);
        this.bootstrapModal = window.bootstrapModals?.[modalId];
        this.hasChanges = false;
        this.redirectUrl = '';

        // Реєстрація у глобальному об'єкті
        if (!window.modalHandlers) {
            window.modalHandlers = {};
        }
        window.modalHandlers[modalId] = this;

        // Налаштування початкових значень
        if (this.modal) {
            this.setupButtonHandlers();
        }
    }

    // 
    setupButtonHandlers() {
        const confirmBtn = this.modal.querySelector('.btn-confirm');
        if (confirmBtn) {
            confirmBtn.addEventListener('click', () => {
                console.log(`Confirm button clicked in ${this.modalId}`);
                this.onConfirm();
            });
        }

        const saveBtn = this.modal.querySelector('.btn-save');
        if (saveBtn) {
            saveBtn.addEventListener('click', () => {
                console.log(`Save button clicked in ${this.modalId}`);
                this.onSave();
            });
        }
    }

    // Показати модальне вікно
    show() {
        if (this.bootstrapModal) {
            this.bootstrapModal.show();
        } else if (this.modal) {
            const bootstrapModal = new bootstrap.Modal(this.modal);
            bootstrapModal.show();
            window.bootstrapModals[this.modalId] = bootstrapModal;
        }
    }

    // Сховати модальне вікно
    hide() {
        if (this.bootstrapModal) {

            this.hasChanges = false;
            this.bootstrapModal.hide();
        }
    }

    // Встановити URL для перенаправлення
    setRedirectUrl(url) {
        this.redirectUrl = url;
        const redirectInput = document.getElementById(`${this.modalId}_redirectUrl`);
        if (redirectInput) {
            redirectInput.value = url;
        }
    }

    // Методи, які можна перевизначити
    onConfirm() {
        console.log(`Modal ${this.modalId} confirmed`);

        this.hasChanges = false;
        if (this.redirectUrl) {
            setTimeout(() => {
                window.location.href = this.redirectUrl;
            }, 50);
        }
    }

    onSave() {
        console.log(`Modal ${this.modalId} saved`);

        this.hasChanges = false;

        const settings = window.modalSettings?.[this.modalId] || {};
        const saveButtonId = settings.saveButtonId || 'saveProductsBtn';
        const redirectDelay = settings.redirectDelay || 1000;

        const saveButton = document.getElementById(saveButtonId);
        if (saveButton) {
            saveButton.click();

            if (this.redirectUrl) {
                setTimeout(() => {
                    window.location.href = this.redirectUrl;
                }, redirectDelay);
            }
        } else {
            console.warn(`Save button with ID ${saveButtonId} not found`);
        }
    }

    onCancel() {
        console.log(`Modal ${this.modalId} cancelled`);
    }
}

// Глобальний хелпер для створення модальних вікон у JavaScript
window.createInfoModal = function (options) {
    const defaultOptions = {
        id: 'dynamicInfoModal',
        title: 'Інформація',
        message: '',
        type: 'info', // info, warning, success, danger
        showCancelButton: true,
        showConfirmButton: true,
        showSaveButton: false,
        cancelText: 'Скасувати',
        confirmText: 'Підтвердити',
        saveText: 'Зберегти',
        size: 'default', // small, default, large, extralarge, fullscreen
        backdrop: false, // чи встановлювати static backdrop
        closeButton: true
    };

    const config = { ...defaultOptions, ...options };

    // Створення HTML модального вікна
    const modalHtml = `
    <div class="modal fade" id="${config.id}" tabindex="-1" aria-hidden="true" ${config.backdrop ? 'data-bs-backdrop="static"' : ''}>
        <div class="modal-dialog modal-dialog-centered ${getSizeClass(config.size)}">
            <div class="modal-content ${getTypeClass(config.type)}">
                <div class="modal-header">
                    ${getTypeIcon(config.type)}
                    <h5 class="modal-title">${config.title}</h5>
                    ${config.closeButton ? '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Закрити"></button>' : ''}
                </div>
                <div class="modal-body">
                    <p>${config.message}</p>
                </div>
                <div class="modal-footer">
                    ${config.showCancelButton ? `<button type="button" class="btn btn-secondary btn-cancel" data-bs-dismiss="modal">${config.cancelText}</button>` : ''}
                    ${config.showConfirmButton ? `<button type="button" class="btn ${getButtonClass(config.type)} btn-confirm">${config.confirmText}</button>` : ''}
                    ${config.showSaveButton ? `<button type="button" class="btn btn-primary btn-save">${config.saveText}</button>` : ''}
                </div>
            </div>
        </div>
    </div>
    `;

    // Inner HTML to avoid appending the modal directly
    const modalContainer = document.createElement('div');
    modalContainer.innerHTML = modalHtml;
    document.body.appendChild(modalContainer.firstElementChild);

    // Create a new handler for event listeners
    const modalHandler = new ModalHandler(config.id);
    const modal = document.getElementById(config.id);

    modal.querySelector('.btn-confirm')?.addEventListener('click', function () {
        modalHandler.hasChanges = false;
        window.isRedirectingUser = true;

        if (config.onConfirm) {
            config.onConfirm();
        } else {
            modalHandler.onConfirm();
        }
        if (config.autoClose !== false) {
            modalHandler.hide();
        }
    });

    modal.querySelector('.btn-save')?.addEventListener('click', function () {
        modalHandler.hasChanges = false;
        window.isRedirectingUser = true;

        if (config.onSave) {
            config.onSave();
        } else {
            modalHandler.onSave();
        }
        if (config.autoClose !== false) {
            modalHandler.hide();
        }
    });

    // Create and show the modal
    const bootstrapModal = new bootstrap.Modal(modal);
    window.bootstrapModals[config.id] = bootstrapModal;
    bootstrapModal.show();

    // Automatically hide the modal after a delay if specified
    if (config.destroyOnClose !== false) {
        modal.addEventListener('hidden.bs.modal', function () {
            modal.remove();
            delete window.bootstrapModals[config.id];
            delete window.modalHandlers[config.id];
        });
    }

    return modalHandler;
};

// Utility functions to get classes based on type and size
function getSizeClass(size) {
    switch (size) {
        case 'small': return 'modal-sm';
        case 'large': return 'modal-lg';
        case 'extralarge': return 'modal-xl';
        case 'fullscreen': return 'modal-fullscreen';
        default: return '';
    }
}

function getTypeClass(type) {
    switch (type) {
        case 'warning': return 'modal-warning';
        case 'danger': return 'modal-danger';
        case 'success': return 'modal-success';
        default: return 'modal-info';
    }
}

function getTypeIcon(type) {
    switch (type) {
        case 'warning': return '<i class="bi bi-exclamation-triangle-fill text-warning me-2"></i>';
        case 'danger': return '<i class="bi bi-exclamation-circle-fill text-danger me-2"></i>';
        case 'success': return '<i class="bi bi-check-circle-fill text-success me-2"></i>';
        default: return '<i class="bi bi-info-circle-fill text-info me-2"></i>';
    }
}

function getButtonClass(type) {
    switch (type) {
        case 'warning': return 'btn-warning';
        case 'danger': return 'btn-danger';
        case 'success': return 'btn-success';
        default: return 'btn-primary';
    }
}

//======================= DOMContentLoaded =======================
document.addEventListener('DOMContentLoaded', function () {
    // Ініціалізація всіх модальних вікон, які вже є на сторінці
    document.querySelectorAll('.modal').forEach(modal => {
        const modalId = modal.id;
        if (modalId && !window.modalHandlers?.[modalId]) {
            // Створюємо обробник для існуючого модального вікна
            const modalHandler = new ModalHandler(modalId);

            // Ініціалізуємо слухачі подій для кнопок
            const confirmBtn = modal.querySelector('.btn-confirm');
            if (confirmBtn) {
                confirmBtn.addEventListener('click', function () {
                    modalHandler.onConfirm();
                });
            }

            const saveBtn = modal.querySelector('.btn-save');
            if (saveBtn) {
                saveBtn.addEventListener('click', function () {
                    modalHandler.onSave();
                });
            }

            // Налаштування відстеження змін у формі
            const trackChanges = modal.getAttribute('data-track-changes') === 'true';
            if (trackChanges) {
                const trackingFormId = document.getElementById(`${modalId}_trackingFormId`)?.value;
                if (trackingFormId) {
                    const form = document.getElementById(trackingFormId);
                    if (form) {
                        form.addEventListener('input', function () {
                            modalHandler.hasChanges = true;
                        });
                        form.addEventListener('change', function () {
                            modalHandler.hasChanges = true;
                        });
                    }
                }
            }
        }
    });

    // Ініціалізація вже створених глобальних обробників модальних вікон
    if (window.bootstrapModals && window.modalHandlers) {
        for (const modalId in window.bootstrapModals) {
            if (!window.modalHandlers[modalId]) {
                new ModalHandler(modalId);
            }
        }
    }
});
//======================= DOMContentLoaded =======================

// Оголошення глобальних об'єктів, якщо вони ще не існують
if (!window.bootstrapModals) {
    window.bootstrapModals = {};
}

if (!window.modalHandlers) {
    window.modalHandlers = {};
}