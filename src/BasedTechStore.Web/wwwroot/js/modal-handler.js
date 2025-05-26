class ModalHandler {
    constructor(modalId) {
        this.modalId = modalId;
        this.modal = document.getElementById(modalId);
        this.bootstrapModal = null;
        this.hasChanges = false;
        this.redirectUrl = '';

        window.modalSettings = window.modalSettings || {};

        // Init global obj
        if (!window.bootstrapModals) {
            window.bootstrapModals = {};
        }

        if (!window.modalHandlers) {
            window.modalHandlers = {};
        }

        // Register in global
        window.modalHandlers[modalId] = this;

        // Налаштування початкових значень
        if (this.modal) {
            this.initializeBootstrapModal();
            this.setupButtonHandlers();
        }
    }

    // Init bootstrap modal
    initializeBootstrapModal() {
        if (window.bootstrapModals[this.modalId]) {
            this.bootstrapModal = window.bootstrapModals[this.modalId];
        } else {
            // Create a new bootstrap modal instance if it doesn't exist
            this.bootstrapModal = new bootstrap.Modal(this.modal);
            window.bootstrapModals[this.modalId] = this.bootstrapModal;
        }
    }

    // Handlers setting
    setupButtonHandlers() {
        const confirmBtn = this.modal.querySelector('.btn-confirm');
        if (confirmBtn) {
            confirmBtn.removeEventListener('click', this.confirmHandler);
            this.confirmHandler = (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log(`Confirm button clicked in ${this.modalId}`);
                this.onConfirm();
            };
            confirmBtn.addEventListener('click', this.confirmHandler);
        }

        const saveBtn = this.modal.querySelector('.btn-save');
        if (saveBtn) {
            saveBtn.removeEventListener('click', this.saveHandler);
            this.saveHandler = (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log(`Save button clicked in ${this.modalId}`);
                this.onSave();
            };
            saveBtn.addEventListener('click', this.saveHandler);
        }

        const cancelBtn = this.modal.querySelector('.btn-cancel');
        if (cancelBtn) {
            cancelBtn.removeEventListener('click', this.cancelHandler);
            this.cancelHandler = (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log(`Cancel button clicked in ${this.modalId}`);
                this.onCancel();
            };
            cancelBtn.addEventListener('click', this.cancelHandler);
        }
    }

    show() {
        if (this.bootstrapModal) {
            this.bootstrapModal.show();
        } else {
            console.error(`Bootstrap modal for ${this.modalId} is not initialized`);
        }
    }

    hide() {
        console.log(`Attempting to hide modal ${this.modalId}`);
        if (this.bootstrapModal) {
            this.hasChanges = false;
            try {
                this.bootstrapModal.hide();
                console.log(`Modal ${this.modalId} hidden successfully`);
            } catch (error) {
                console.error(`Error hiding modal ${this.modalId}:`, error);
            }
        } else {
            console.error(`Bootstrap modal not found for ${this.modalId}`);
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

        this.hide();

        if (this.redirectUrl) {
            setTimeout(() => {
                window.location.href = this.redirectUrl;
            }, 300);
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
            this.hide();

            setTimeout(() => {
                saveButton.click();

                if (this.redirectUrl) {
                    setTimeout(() => {
                        window.location.href = this.redirectUrl;
                    }, redirectDelay);
                }
            }, 300) 
        } else {
            console.warn(`Save button with ID ${saveButtonId} not found`);
            this.hide();
        }
    }

    onCancel() {
        console.log(`Modal ${this.modalId} cancelled`);
        this.hasChanges = false;
        this.hide();
    }

    destroy() {
        if (this.boostrapModal) {
            this.bootstrapModal.dispose();
        }
        delete window.bootstrapModals[this.modalId];
        delete window.modalHandlers[this.modalId];
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

    const existingModal = document.getElementById(config.id);
    if (existingModal) {
        if (window.modalHandlers[config.id]) {
            window.modalHandlers[config.id].destroy();
        }
        existingModal.remove();
    }

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

    if (config.onConfirm) {
        modalHandler.onConfirm = function () {
            console.log(`Modal ${this.modalId} confirmed (custom handler)`);
            this.hasChanges = false;
            window.isRedirectingUser = true;

            config.onConfirm();

            if (config.autoClose !== false) {
                this.hide();
            }
        };
    }

    if (config.onSave) {
        modalHandler.onSave = function () {
            console.log(`Modal ${this.modalId} saved (custom handler)`);
            this.hasChanges = false;
            window.isRedirectingUser = true;

            config.onSave();

            if (config.autoClose !== false) {
                this.hide();
            }
        };
    }

    if (config.onCancel) {
        modalHandler.onCancel = function () {
            console.log(`Modal ${this.modalId} cancelled (custom handler)`);
            this.hasChanges = false;

            config.onCancel();

            if (config.autoClose !== false) {
                this.hide();
            }
        };
    }

    modalHandler.show();

    // Automatically hide the modal after a delay if specified
    if (config.destroyOnClose !== false) {
        modal.addEventListener('hidden.bs.modal', function () {
            modalHandler.destroy();
            modal.remove();
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

function getTypeIcon(type) {
    switch (type) {
        case 'warning': return '<i class="bi bi-exclamation-triangle-fill text-warning me-2"></i>';
        case 'danger': return '<i class="bi bi-exclamation-circle-fill text-danger me-2"></i>';
        case 'success': return '<i class="bi bi-check-circle-fill text-success me-2"></i>';
        default: return '<i class="bi bi-info-circle-fill text-info me-2"></i>';
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
    console.log('Initializing modal handlers on DOMContentLoaded...');

    if (!window.bootstrapModals) {
        window.bootstrapModals = {};
    }
    if (!window.modalHandlers) {
        window.modalHandlers = {};
    }

    // Ініціалізація всіх модальних вікон, які вже є на сторінці
    document.querySelectorAll('.modal').forEach(modal => {
        const modalId = modal.id;
        if (modalId && !window.modalHandlers[modalId]) {
            console.log(`Initializing modal handler for ${modalId}`);

            // Create a new ModalHandler instance
            const modalHandler = new ModalHandler(modalId);

            // Initialize listeners in modal
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

    console.log('Modal handlers initialized:', Object.keys(window.modalHandlers));
});
//======================= DOMContentLoaded =======================