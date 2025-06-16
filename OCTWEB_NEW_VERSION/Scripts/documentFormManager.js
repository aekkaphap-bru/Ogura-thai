class DocumentFormManager {
    constructor() {
        this.documentCount = 1;
        this.maxDocuments = 10; // จำกัดจำนวนเอกสารสูงสุด
        this.areas = [
            'IS', 'IT', 'QA', 'PP', 'GS', 'FC', 'AB2', 'SE',
            'HR', 'PE', 'QC', 'L2', 'BR', 'CA', 'AS', 'FR',
            'ACC', 'EN', 'PD1', 'L3', 'AB1', 'AF', 'AC', 'ACC',
            'PU', 'PC', 'L1', 'RA', 'AT', 'GN', 'PD2'
        ];

        // Cache DOM elements
        this.elements = {};
        this.debounceTimer = null;
        this.isSubmitting = false;

        this.init();
    }

    init() {
        this.cacheElements();
        this.generateAreaCheckboxes();
        this.bindEvents();
        this.updateProgress();
        this.initializeTooltips();
    }

    // Cache frequently used DOM elements
    cacheElements() {
        this.elements = {
            form: document.getElementById('documentForm'),
            progressBar: document.getElementById('progressBar'),
            areaCheckboxes: document.getElementById('areaCheckboxes'),
            documentDetailsList: document.getElementById('document-details-list'),
            addDocumentBtn: document.getElementById('addDocumentBtn'),
            saveDraftBtn: document.getElementById('saveDraftBtn'),
            printBtn: document.getElementById('printBtn')
        };
    }

    // Generate area checkboxes with better performance
    generateAreaCheckboxes() {
        if (!this.elements.areaCheckboxes) return;

        const fragment = document.createDocumentFragment();

        this.areas.forEach(area => {
            const wrapper = document.createElement('div');
            wrapper.className = "col-md-3 col-sm-4 col-6";
            wrapper.innerHTML = `
                <div class="form-check form-check-card">
                    <input class="form-check-input" type="checkbox" 
                           id="area-${area}" name="areas[]" value="${area}"
                           data-bs-toggle="tooltip" title="เลือกพื้นที่ ${area}">
                    <label class="form-check-label" for="area-${area}">${area}</label>
                </div>
            `;
            fragment.appendChild(wrapper);
        });

        this.elements.areaCheckboxes.innerHTML = '';
        this.elements.areaCheckboxes.appendChild(fragment);
    }

    // Initialize tooltips
    initializeTooltips() {
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }
    }

    // Bind events with better performance
    bindEvents() {
        // Add document button
        this.elements.addDocumentBtn?.addEventListener('click', () => this.addDocumentDetail());

        // Form submission
        this.elements.form?.addEventListener('submit', (e) => this.handleSubmit(e));

        // Save draft button
        this.elements.saveDraftBtn?.addEventListener('click', () => this.saveDraft());

        // Print button
        this.elements.printBtn?.addEventListener('click', () => this.handlePrint());

        // Progress update with debouncing
        this.elements.form?.addEventListener('input', () => this.debounceUpdateProgress());
        this.elements.form?.addEventListener('change', () => this.debounceUpdateProgress());

        // Document type validation
        this.elements.form?.addEventListener('change', (e) => {
            if (e.target.name === 'document_type') {
                this.validateDocumentType();
            }
        });

        // File upload validation
        this.elements.form?.addEventListener('change', (e) => {
            if (e.target.type === 'file') {
                this.validateFileUpload(e.target);
            }
        });

        // Auto-save draft every 30 seconds
        setInterval(() => this.autoSaveDraft(), 30000);
    }

    // Debounced progress update for better performance
    debounceUpdateProgress() {
        clearTimeout(this.debounceTimer);
        this.debounceTimer = setTimeout(() => this.updateProgress(), 100);
    }

    // Add document detail with improved validation
    addDocumentDetail() {
        if (this.documentCount >= this.maxDocuments) {
            this.showNotification('เพิ่มเอกสารได้สูงสุด ' + this.maxDocuments + ' รายการ', 'warning');
            return;
        }

        this.documentCount++;
        const card = this.createDocumentCard(this.documentCount);

        // Add with animation
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        this.elements.documentDetailsList.appendChild(card);

        // Animate in
        requestAnimationFrame(() => {
            card.style.transition = 'all 0.3s ease-out';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        });

        this.bindDocumentEvents(card);
        this.updateProgress();
        this.updateAddButtonState();
    }

    // Create document card with better structure
    createDocumentCard(index) {
        const card = document.createElement('div');
        card.className = 'document-card card border-light mb-3';
        card.setAttribute('data-index', index);
        card.innerHTML = this.createDocumentCardHTML(index);
        return card;
    }

    // Improved HTML generation
    createDocumentCardHTML(index) {
        return `
            <div class="card-header bg-light py-2">
                <div class="d-flex justify-content-between align-items-center">
                    <span class="fw-semibold">เอกสารลำดับที่ ${index}</span>
                    <button type="button" class="btn btn-outline-danger btn-sm remove-document" 
                            ${index === 1 ? 'style="display: none;"' : ''}>
                        <i class="fas fa-trash-alt"></i>
                    </button>
                </div>
            </div>
            <div class="card-body">
                <div class="row g-3">
                    <!-- Basic Info Row -->
                    <div class="col-md-6">
                        <div class="form-floating">
                            <input type="text" class="form-control" id="doc_number_${index}" 
                                   name="doc_number[]" required placeholder="หมายเลขเอกสาร"
                                   data-bs-toggle="tooltip" title="กรอกหมายเลขเอกสาร">
                            <label for="doc_number_${index}">หมายเลขเอกสาร *</label>
                            <div class="invalid-feedback">กรุณากรอกหมายเลขเอกสาร</div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="form-floating">
                            <input type="text" class="form-control" id="doc_title_${index}" 
                                   name="doc_title[]" required placeholder="ชื่อเรื่อง"
                                   data-bs-toggle="tooltip" title="กรอกชื่อเรื่องของเอกสาร">
                            <label for="doc_title_${index}">ชื่อเรื่อง *</label>
                            <div class="invalid-feedback">กรุณากรอกชื่อเรื่อง</div>
                        </div>
                    </div>

                    <!-- Details Row -->
                    <div class="col-md-4">
                        <div class="form-floating">
                            <input type="text" id="doc_revision_${index}" name="doc_revision[]" 
                                   class="form-control" required placeholder="ลำดับการแก้ไข"
                                   data-bs-toggle="tooltip" title="กรอกลำดับการแก้ไข เช่น Rev.01">
                            <label for="doc_revision_${index}">ลำดับการแก้ไข *</label>
                            <div class="invalid-feedback">กรุณากรอกลำดับการแก้ไข</div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="form-floating">
                            <input type="number" id="doc_pages_${index}" name="doc_pages[]" 
                                   class="form-control" min="1" max="9999" required placeholder="จำนวนหน้า"
                                   data-bs-toggle="tooltip" title="กรอกจำนวนหน้าหรือชุดของเอกสาร">
                            <label for="doc_pages_${index}">จำนวนหน้า/ชุด *</label>
                            <div class="invalid-feedback">กรุณากรอกจำนวนหน้า (1-9999)</div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="form-floating">
                            <input type="number" id="doc_copies_${index}" name="doc_copies[]" 
                                   class="form-control" min="1" max="999" required placeholder="จำนวนสำเนา"
                                   data-bs-toggle="tooltip" title="กรอกจำนวนสำเนาที่ต้องการ">
                            <label for="doc_copies_${index}">จำนวนสำเนา *</label>
                            <div class="invalid-feedback">กรุณากรอกจำนวนสำเนา (1-999)</div>
                        </div>
                    </div>

                    <!-- Change Details -->
                    <div class="col-12">
                        <div class="form-floating">
                            <textarea id="change_detail_${index}" name="change_detail[]" 
                                     class="form-control" style="height: 100px" 
                                     placeholder="รายละเอียดการเปลี่ยนแปลง"
                                     data-bs-toggle="tooltip" title="อธิบายรายละเอียดการเปลี่ยนแปลง"></textarea>
                            <label for="change_detail_${index}">
                                <i class="fas fa-clipboard-list me-1"></i>รายละเอียดการเปลี่ยนแปลง
                            </label>
                        </div>
                    </div>

                    <!-- File Uploads -->
                    <div class="col-md-6">
                        <label class="form-label">
                            <i class="fas fa-file-pdf me-1 text-danger"></i>ไฟล์ PDF
                        </label>
                        <input type="file" class="form-control" name="attachment_pdf[]" 
                               accept=".pdf" data-max-size="10485760">
                        <div class="form-text">รองรับไฟล์ .pdf ขนาดไม่เกิน 10MB</div>
                        <div class="invalid-feedback"></div>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">
                            <i class="fas fa-file-excel me-1 text-success"></i>ไฟล์ Excel
                        </label>
                        <input type="file" class="form-control" name="attachment_excel[]" 
                               accept=".xls,.xlsx" data-max-size="10485760">
                        <div class="form-text">รองรับไฟล์ .xls, .xlsx ขนาดไม่เกิน 10MB</div>
                        <div class="invalid-feedback"></div>
                    </div>
                </div>
            </div>
        `;
    }

    // Bind events to document card
    bindDocumentEvents(card) {
        const removeBtn = card.querySelector('.remove-document');
        removeBtn?.addEventListener('click', () => this.removeDocumentDetail(card));

        // Initialize tooltips for new elements
        this.initializeTooltips();
    }

    // Remove document detail with animation
    removeDocumentDetail(card) {
        const totalCards = document.querySelectorAll('.document-card').length;

        if (totalCards <= 1) {
            this.showNotification('ต้องมีรายการเอกสารอย่างน้อย 1 รายการ', 'warning');
            return;
        }

        // Animate out
        card.style.transition = 'all 0.3s ease-out';
        card.style.opacity = '0';
        card.style.transform = 'translateX(-100%)';

        setTimeout(() => {
            card.remove();
            this.renumberDocumentCards();
            this.updateProgress();
            this.updateAddButtonState();
        }, 300);
    }

    // Improved progress calculation
    updateProgress() {
        if (!this.elements.form || !this.elements.progressBar) return;

        const requiredInputs = this.elements.form.querySelectorAll('input[required], select[required], textarea[required]');
        const radioGroups = new Set();
        let totalFields = 0;
        let filledFields = 0;

        requiredInputs.forEach(input => {
            if (input.type === 'radio') {
                radioGroups.add(input.name);
            } else if (input.type === 'checkbox') {
                // Skip individual checkboxes, handle groups separately
                return;
            } else {
                totalFields++;
                if (input.value.trim() !== '') {
                    filledFields++;
                }
            }
        });

        // Handle radio button groups
        radioGroups.forEach(groupName => {
            totalFields++;
            if (this.elements.form.querySelector(`input[name="${groupName}"]:checked`)) {
                filledFields++;
            }
        });

        // Handle area checkboxes (at least one should be selected)
        const areaCheckboxes = this.elements.form.querySelectorAll('input[name="areas[]"]');
        if (areaCheckboxes.length > 0) {
            totalFields++;
            if (this.elements.form.querySelector('input[name="areas[]"]:checked')) {
                filledFields++;
            }
        }

        const percent = totalFields > 0 ? Math.round((filledFields / totalFields) * 100) : 0;
        this.elements.progressBar.style.width = `${percent}%`;
        this.elements.progressBar.setAttribute('aria-valuenow', percent);

        // Update progress bar color based on completion
        this.elements.progressBar.className = 'progress-bar progress-bar-striped progress-bar-animated';
        if (percent < 30) {
            this.elements.progressBar.classList.add('bg-danger');
        } else if (percent < 70) {
            this.elements.progressBar.classList.add('bg-warning');
        } else {
            this.elements.progressBar.classList.add('bg-success');
        }
    }

    // Validate form with better error handling
    validateForm() {
        if (!this.elements.form) return false;

        const isValid = this.elements.form.checkValidity();

        // Custom validations
        if (!this.validateDocumentType()) return false;
        if (!this.validateAreaSelection()) return false;
        if (!this.validateFileUploads()) return false;

        return isValid;
    }

    // Validate document type selection
    validateDocumentType() {
        const documentTypeInputs = this.elements.form.querySelectorAll('input[name="document_type"]');
        const isSelected = Array.from(documentTypeInputs).some(input => input.checked);
        const errorElement = document.getElementById('documentTypeError');

        if (!isSelected) {
            errorElement.style.display = 'block';
            errorElement.textContent = 'กรุณาเลือกประเภทเอกสาร';
            return false;
        } else {
            errorElement.style.display = 'none';
            return true;
        }
    }

    // Validate area selection
    validateAreaSelection() {
        const areaInputs = this.elements.form.querySelectorAll('input[name="areas[]"]');
        const isSelected = Array.from(areaInputs).some(input => input.checked);

        if (!isSelected) {
            this.showNotification('กรุณาเลือกพื้นที่การใช้งานอย่างน้อย 1 รายการ', 'warning');
            return false;
        }
        return true;
    }

    // Validate file uploads
    validateFileUploads() {
        const fileInputs = this.elements.form.querySelectorAll('input[type="file"]');

        for (let input of fileInputs) {
            if (input.files.length > 0) {
                const file = input.files[0];
                const maxSize = parseInt(input.dataset.maxSize) || 10485760; // 10MB default

                if (file.size > maxSize) {
                    const maxSizeMB = Math.round(maxSize / 1048576);
                    this.showNotification(`ไฟล์ ${file.name} มีขนาดใหญ่เกิน ${maxSizeMB}MB`, 'error');
                    return false;
                }
            }
        }
        return true;
    }

    // Validate individual file upload
    validateFileUpload(input) {
        if (input.files.length === 0) return;

        const file = input.files[0];
        const maxSize = parseInt(input.dataset.maxSize) || 10485760;
        const feedback = input.parentElement.querySelector('.invalid-feedback');

        if (file.size > maxSize) {
            const maxSizeMB = Math.round(maxSize / 1048576);
            input.setCustomValidity(`ไฟล์มีขนาดใหญ่เกิน ${maxSizeMB}MB`);
            feedback.textContent = `ไฟล์มีขนาดใหญ่เกิน ${maxSizeMB}MB`;
            input.classList.add('is-invalid');
        } else {
            input.setCustomValidity('');
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
        }
    }

    // Handle form submission
    async handleSubmit(e) {
        e.preventDefault();

        if (this.isSubmitting) return;

        if (!this.validateForm()) {
            this.elements.form.classList.add('was-validated');
            this.showNotification('กรุณากรอกข้อมูลให้ครบถ้วนและถูกต้อง', 'error');
            return;
        }

        this.isSubmitting = true;
        const submitBtn = this.elements.form.querySelector('button[type="submit"]');
        const originalContent = submitBtn.innerHTML;

        try {
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>กำลังส่งคำร้อง...';
            submitBtn.disabled = true;

            // Simulate API call
            await this.submitForm();

            this.showNotification('ส่งคำร้องเรียบร้อยแล้ว', 'success');
            this.clearDraftData();

        } catch (error) {
            console.error('Submit error:', error);
            this.showNotification('เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง', 'error');
        } finally {
            submitBtn.innerHTML = originalContent;
            submitBtn.disabled = false;
            this.isSubmitting = false;
            this.elements.form.classList.remove('was-validated');
        }
    }

    // Simulate form submission
    async submitForm() {
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve({ success: true });
            }, 2000);
        });
    }

    // Save draft functionality
    saveDraft() {
        const formData = new FormData(this.elements.form);
        const draftData = {};

        for (let [key, value] of formData.entries()) {
            if (draftData[key]) {
                if (Array.isArray(draftData[key])) {
                    draftData[key].push(value);
                } else {
                    draftData[key] = [draftData[key], value];
                }
            } else {
                draftData[key] = value;
            }
        }

        localStorage.setItem('documentFormDraft', JSON.stringify(draftData));
        this.showNotification('บันทึกแบบร่างเรียบร้อยแล้ว', 'success');
    }

    // Auto save draft
    autoSaveDraft() {
        if (this.hasFormData()) {
            this.saveDraft();
        }
    }

    // Check if form has data
    hasFormData() {
        const inputs = this.elements.form.querySelectorAll('input, textarea, select');
        return Array.from(inputs).some(input => {
            if (input.type === 'checkbox' || input.type === 'radio') {
                return input.checked;
            }
            return input.value.trim() !== '';
        });
    }

    // Clear draft data
    clearDraftData() {
        localStorage.removeItem('documentFormDraft');
    }

    // Load draft data
    loadDraft() {
        const draftData = localStorage.getItem('documentFormDraft');
        if (draftData) {
            try {
                const data = JSON.parse(draftData);
                // Implement loading logic here
                this.showNotification('โหลดแบบร่างเรียบร้อยแล้ว', 'info');
            } catch (error) {
                console.error('Error loading draft:', error);
            }
        }
    }

    // Handle print
    handlePrint() {
        if (!this.validateForm()) {
            this.showNotification('กรุณากรอกข้อมูลให้ครบถ้วนก่อนพิมพ์', 'warning');
            return;
        }

        // Hide non-printable elements
        const nonPrintable = document.querySelectorAll('.btn, .progress, .invalid-feedback');
        nonPrintable.forEach(el => el.style.display = 'none');

        window.print();

        // Restore elements
        setTimeout(() => {
            nonPrintable.forEach(el => el.style.display = '');
        }, 1000);
    }

    // Renumber document cards
    renumberDocumentCards() {
        const cards = document.querySelectorAll('.document-card');
        cards.forEach((card, index) => {
            const newIndex = index + 1;
            card.setAttribute('data-index', newIndex);

            // Update header
            const header = card.querySelector('.card-header span.fw-semibold');
            if (header) {
                header.textContent = `เอกสารลำดับที่ ${newIndex}`;
            }

            // Update field IDs and labels
            const fields = [
                'doc_number', 'doc_title', 'doc_revision',
                'doc_pages', 'doc_copies', 'change_detail'
            ];

            fields.forEach(fieldName => {
                const input = card.querySelector(`[id^="${fieldName}_"]`);
                if (input) {
                    const newId = `${fieldName}_${newIndex}`;
                    input.id = newId;

                    const label = card.querySelector(`label[for="${input.id}"], label[for*="${fieldName}_"]`);
                    if (label) {
                        label.setAttribute('for', newId);
                    }
                }
            });

            // Show/hide remove buttons
            const removeBtn = card.querySelector('.remove-document');
            if (removeBtn) {
                removeBtn.style.display = newIndex === 1 ? 'none' : 'block';
            }
        });

        this.documentCount = cards.length;
    }

    // Update add button state
    updateAddButtonState() {
        if (this.elements.addDocumentBtn) {
            const isMaxReached = this.documentCount >= this.maxDocuments;
            this.elements.addDocumentBtn.disabled = isMaxReached;

            if (isMaxReached) {
                this.elements.addDocumentBtn.innerHTML = `
                    <i class="fas fa-ban me-1"></i>สูงสุด ${this.maxDocuments} รายการ
                `;
            } else {
                this.elements.addDocumentBtn.innerHTML = `
                    <i class="fas fa-plus me-1"></i>เพิ่มเอกสาร
                `;
            }
        }
    }

    // Show notification
    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';

        const icons = {
            success: 'check-circle',
            error: 'exclamation-triangle',
            warning: 'exclamation-circle',
            info: 'info-circle'
        };

        notification.innerHTML = `
            <i class="fas fa-${icons[type] || 'info-circle'} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(notification);

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 5000);
    }
}

// Static method to initialize
DocumentFormManager.init = function () {
    return new DocumentFormManager();
};

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    DocumentFormManager.init();
});

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DocumentFormManager;
}