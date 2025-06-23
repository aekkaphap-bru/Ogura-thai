// Ensure DocumentFormManager is globally accessible if needed, or initialized directly.
// If using as a module, export it.
// e.g., export class DocumentFormManager { ... }

class DocumentFormManager {
    // Constructor to initialize properties.
    constructor() {
        this.documentCount = 0; // Will be initialized to 0-based index of existing documents
        this.maxDocuments = 10; // Max number of document sections allowed
        this.debounceTimer = null;
        this.isSubmitting = false;

        // Example data for available areas. In a real app, this would come from your MVC Model.
        // For demonstration, let's assume it's pre-populated or fetched via AJAX.
        this.availableAreas = [
            { Id: 1, SectionCode: "A1", SectionName: "Admin" },
            { Id: 2, SectionCode: "P2", SectionName: "Production" },
            { Id: 3, SectionCode: "Q3", SectionName: "Quality Control" },
            { Id: 4, SectionCode: "R4", SectionName: "R&D" },
            { Id: 5, SectionCode: "S5", SectionName: "Sales" },
            { Id: 6, SectionCode: "HR", SectionName: "Human Resources" }
        ];

        // Initialize elements and bind events
        this.init();
    }

    init() {
        this.cacheElements();
        // Dynamically generate area checkboxes if they are not rendered by Razor.
        // If Razor renders them, you can remove this line.
        // This is important: If your Razor view renders these, this method will overwrite them.
        // It's better to render them with Razor if data comes from the model.
        // For this example, I'll comment it out, assuming Razor renders them.
        // this.generateAreaCheckboxes(); 

        this.bindEvents();
        this.initializeDocumentCards(); // Initialize existing document cards
        this.updateProgress();
        this.initializeTooltips();
        this.loadDraft(); // Load draft on initialization
    }

    // Cache frequently used DOM elements
    cacheElements() {
        this.elements = {
            form: document.getElementById('documentForm'),
            progressBar: document.getElementById('progressBar'),
            areaCheckboxesContainer: document.getElementById('areaCheckboxe'), // Correct ID based on HTML
            documentDetailsList: document.getElementById('document-details-list'),
            addDocumentBtn: document.getElementById('addDocumentBtn'),
            saveDraftBtn: document.getElementById('saveDraftBtn'),
            printBtn: document.getElementById('printBtn'),
            // Add notification container if you have one
            notificationContainer: document.getElementById('notification-container')
        };
    }

    // Initialize tooltips for all elements with data-bs-toggle="tooltip"
    initializeTooltips() {
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }
    }

    // --- Document Details Management ---

    // Initialize existing document cards on page load
    initializeDocumentCards() {
        const existingCards = this.elements.documentDetailsList.querySelectorAll('.document-card');
        this.documentCount = existingCards.length > 0 ? existingCards.length - 1 : -1; // Set documentCount to be the 0-based index of the last existing card. Next new card will be ++.
        this.renumberDocumentCards(); // Ensure initial numbering and button states are correct
        this.updateAddButtonState();
        existingCards.forEach(card => this.bindDocumentEvents(card)); // Bind events to existing cards
    }

    // Add new document detail card
    addDocumentDetail() {
        // Increment documentCount before creating, so it acts as the 0-based index for the new card
        this.documentCount++;

        if (this.documentCount >= this.maxDocuments) {
            this.showNotification('เพิ่มเอกสารได้สูงสุด ' + this.maxDocuments + ' รายการ', 'warning');
            this.documentCount--; // Revert count if max reached
            return;
        }

        const card = document.createElement('div');
        card.className = 'document-card card border-light mb-4 shadow-sm'; // Use updated design classes
        // No data-index needed here, renumberDocumentCards will set it
        card.innerHTML = this.createDocumentCardHTML(this.documentCount); // Pass the 0-based index

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
        this.renumberDocumentCards(); // Renumber all cards to ensure correct ASP.NET MVC binding
        this.updateProgress();
        this.updateAddButtonState();
        this.initializeTooltips(); // Initialize tooltips for newly added elements
    }

    // Create HTML for a single document card (using 0-based index for names/ids)
    createDocumentCardHTML(currentIndex) {
        const displayIndex = currentIndex + 1; // For display to user (1-based)

        return `
            <div class="card-header bg-light py-3 d-flex justify-content-between align-items-center">
                <h6 class="fw-semibold mb-0 text-primary">เอกสารลำดับที่ ${displayIndex}</h6>
                <button type="button" class="btn btn-outline-danger btn-sm remove-document" 
                    ${currentIndex === 0 ? 'style="display: none;"' : ''} >
                    <i class="fas fa-trash-alt me-1"></i> ลบ
                </button>
            </div>
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-md-6">
                        <div class="form-floating">
                            <input type="text" class="form-control" id="DocumentDetails_${currentIndex}__WS_number"
                                   name="DocumentDetails[${currentIndex}].WS_number" required placeholder="หมายเลขเอกสาร"
                                   data-bs-toggle="tooltip" title="กรอกหมายเลขเอกสาร">
                            <label for="DocumentDetails_${currentIndex}__WS_number">หมายเลขเอกสาร *</label>
                            <div class="invalid-feedback">กรุณากรอกหมายเลขเอกสาร</div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="form-floating">
                            <input type="text" class="form-control" id="DocumentDetails_${currentIndex}__WS_name"
                                   name="DocumentDetails[${currentIndex}].WS_name" required placeholder="ชื่อเรื่อง"
                                   data-bs-toggle="tooltip" title="กรอกชื่อเรื่องของเอกสาร">
                            <label for="DocumentDetails_${currentIndex}__WS_name">ชื่อเรื่อง *</label>
                            <div class="invalid-feedback">กรุณากรอกชื่อเรื่อง</div>
                        </div>
                    </div>

                    <div class="col-md-4">
                        <div class="form-floating">
                            <input type="text" id="DocumentDetails_${currentIndex}__Revision" name="DocumentDetails[${currentIndex}].Revision"
                                   class="form-control" required placeholder="ลำดับการแก้ไข"
                                   data-bs-toggle="tooltip" title="กรอกลำดับการแก้ไข เช่น Rev.01">
                            <label for="DocumentDetails_${currentIndex}__Revision">ลำดับการแก้ไข *</label>
                            <div class="invalid-feedback">กรุณากรอกลำดับการแก้ไข</div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="form-floating">
                            <input type="number" id="DocumentDetails_${currentIndex}__Num_pages" name="DocumentDetails[${currentIndex}].Num_pages"
                                   class="form-control" min="1" max="9999" required placeholder="จำนวนหน้า"
                                   data-bs-toggle="tooltip" title="กรอกจำนวนหน้าหรือชุดของเอกสาร">
                            <label for="DocumentDetails_${currentIndex}__Num_pages">จำนวนหน้า/ชุด *</label>
                            <div class="invalid-feedback">กรุณากรอกจำนวนหน้า (1-9999)</div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="form-floating">
                            <input type="number" id="DocumentDetails_${currentIndex}__Num_copies" name="DocumentDetails[${currentIndex}].Num_copies"
                                   class="form-control" min="1" max="999" required placeholder="จำนวนสำเนา"
                                   data-bs-toggle="tooltip" title="กรอกจำนวนสำเนาที่ต้องการ">
                            <label for="DocumentDetails_${currentIndex}__Num_copies">จำนวนสำเนา *</label>
                            <div class="invalid-feedback">กรุณากรอกจำนวนสำเนา (1-999)</div>
                        </div>
                    </div>

                    <div class="col-12">
                        <div class="form-floating">
                            <textarea id="DocumentDetails_${currentIndex}__Change_detail" name="DocumentDetails[${currentIndex}].Change_detail"
                                      class="form-control" style="height: 100px"
                                      placeholder="รายละเอียดการเปลี่ยนแปลง"
                                      data-bs-toggle="tooltip" title="อธิบายรายละเอียดการเปลี่ยนแปลง"></textarea>
                            <label for="DocumentDetails_${currentIndex}__Change_detail">
                                <i class="fas fa-clipboard-list me-1"></i>รายละเอียดการเปลี่ยนแปลง
                            </label>
                        </div>
                    </div>

                    <div class="col-md-6">
                        <label class="form-label mb-2 text-muted small">
                            <i class="fas fa-file-pdf me-1 text-danger"></i> อัปโหลดไฟล์ PDF
                        </label>
                        <input type="file" class="form-control" id="DocumentDetails_${currentIndex}__File_pdf" name="DocumentDetails[${currentIndex}].File_pdf"
                               accept=".pdf" required data-max-size="10485760">
                        <div class="form-text">รองรับไฟล์ .pdf ขนาดไม่เกิน 10MB</div>
                        <div class="invalid-feedback"></div>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label mb-2 text-muted small">
                            <i class="fas fa-file-excel me-1 text-success"></i> อัปโหลดไฟล์ Excel
                        </label>
                        <input type="file" class="form-control" id="DocumentDetails_${currentIndex}__File_excel" name="DocumentDetails[${currentIndex}].File_excel"
                               accept=".xls,.xlsx" required data-max-size="10485760">
                        <div class="form-text">รองรับไฟล์ .xls, .xlsx ขนาดไม่เกิน 10MB</div>
                        <div class="invalid-feedback"></div>
                    </div>
                </div>
            </div>
        `;
    }

    // Bind events to a specific document card (for remove button)
    bindDocumentEvents(card) {
        const removeBtn = card.querySelector('.remove-document');
        removeBtn?.addEventListener('click', () => this.removeDocumentDetail(card));

        // Re-initialize tooltips for new elements within this card
        this.initializeTooltips();
        // Add event listeners for validation on input fields within the new card
        card.querySelectorAll('input, select, textarea').forEach(input => {
            input.addEventListener('input', () => this.debounceUpdateProgress());
            input.addEventListener('change', () => this.debounceUpdateProgress());
            if (input.type === 'file') {
                input.addEventListener('change', () => this.validateFileUpload(input));
            }
        });
    }

    // Remove document detail card
    removeDocumentDetail(card) {
        const totalCards = this.elements.documentDetailsList.querySelectorAll('.document-card').length;

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
            this.documentCount--; // Decrement the count after removal
            this.renumberDocumentCards(); // Renumber remaining cards
            this.updateProgress();
            this.updateAddButtonState();
        }, 300);
    }

    // Renumber document cards and update their attributes
    renumberDocumentCards() {
        const cards = this.elements.documentDetailsList.querySelectorAll('.document-card');
        this.documentCount = cards.length - 1; // Ensure documentCount is the 0-based index of the last card

        cards.forEach((card, index) => {
            const displayIndex = index + 1; // For display (1-based)
            card.setAttribute('data-index', displayIndex); // Update data-index for display purposes

            // Update header text
            const header = card.querySelector('.card-header span.fw-semibold');
            if (header) {
                header.textContent = `เอกสารลำดับที่ ${displayIndex}`;
            }

            // Update field IDs and names for ASP.NET MVC model binding
            const fields = [
                'WS_number', 'WS_name', 'Revision',
                'Num_pages', 'Num_copies', 'Change_detail',
                'File_pdf', 'File_excel' // Include file fields for re-indexing
            ];

            fields.forEach(fieldName => {
                const input = card.querySelector(`[id^="DocumentDetails_"][id$="__${fieldName}"]`); // Select by prefix and suffix for robustness
                if (input) {
                    const newId = `DocumentDetails_${index}__${fieldName}`;
                    const newName = `DocumentDetails[${index}].${fieldName}`;

                    input.id = newId;
                    input.name = newName;

                    const label = card.querySelector(`label[for^="DocumentDetails_"][for$="__${fieldName}"]`);
                    if (label) {
                        label.setAttribute('for', newId);
                    }
                }
            });

            // Show/hide remove buttons based on total count
            const removeBtn = card.querySelector('.remove-document');
            if (removeBtn) {
                // Only show remove button if there's more than one document card
                removeBtn.style.display = cards.length > 1 ? 'block' : 'none';
            }
        });
    }

    // --- Event Binding ---

    bindEvents() {
        // Add document button
        this.elements.addDocumentBtn?.addEventListener('click', () => this.addDocumentDetail());

        // Form submission
        this.elements.form?.addEventListener('submit', (e) => this.handleSubmit(e));

        // Save draft button (if present in HTML)
        this.elements.saveDraftBtn?.addEventListener('click', () => this.saveDraft());

        // Print button (if present in HTML)
        this.elements.printBtn?.addEventListener('click', () => this.handlePrint());

        // Progress update with debouncing for general input/change
        this.elements.form?.addEventListener('input', () => this.debounceUpdateProgress());
        this.elements.form?.addEventListener('change', () => this.debounceUpdateProgress());

        // Specific event listeners for custom UI elements (radio/checkbox cards)
        // Request Type
        this.elements.form?.querySelectorAll('input[name="Request_type"]').forEach(input => {
            input.addEventListener('change', (e) => {
                this.elements.form.querySelectorAll('.form-check-card').forEach(card => card.classList.remove('checked'));
                e.target.closest('.form-check-card')?.classList.add('checked');
                this.updateProgress();
            });
        });

        // Area Checkboxes
        this.elements.areaCheckboxesContainer?.querySelectorAll('input[name^="AvailableAreas"][type="checkbox"]').forEach(input => {
            input.addEventListener('change', (e) => {
                e.target.closest('.form-check-card')?.classList.toggle('checked', e.target.checked);
                this.updateProgress();
            });
        });

        // Document Type
        this.elements.form?.querySelectorAll('input[name="Document_type"]').forEach(input => {
            input.addEventListener('change', (e) => {
                this.elements.form.querySelectorAll('.form-check-card').forEach(card => card.classList.remove('checked'));
                e.target.closest('.form-check-card')?.classList.add('checked');
                this.updateProgress();
            });
        });

        // File upload validation (delegate to the form for dynamic elements)
        this.elements.form?.addEventListener('change', (e) => {
            if (e.target.type === 'file' && e.target.closest('.document-card')) {
                this.validateFileUpload(e.target);
            }
        });

        // Auto-save draft every 30 seconds
        setInterval(() => this.autoSaveDraft(), 30000);
    }

    // Debounced progress update for better performance
    debounceUpdateProgress() {
        clearTimeout(this.debounceTimer);
        this.debounceTimer = setTimeout(() => this.updateProgress(), 250); // Increased debounce time slightly
    }

    // --- Progress Bar & Validation ---

    // Improved progress calculation
    updateProgress() {
        if (!this.elements.form || !this.elements.progressBar) return;

        const requiredInputs = this.elements.form.querySelectorAll('input[required], select[required], textarea[required]');
        let totalFields = 0;
        let filledFields = 0;

        // Track radio groups to count them only once
        const radioGroups = new Set();
        // Track checkbox groups (like areas) to count them only once as "at least one"
        const checkboxGroups = new Set();

        requiredInputs.forEach(input => {
            if (input.type === 'radio') {
                radioGroups.add(input.name);
            } else if (input.type === 'checkbox') {
                checkboxGroups.add(input.name); // Add checkbox group name (e.g., AvailableAreas[i].IsSelected)
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

        // Handle checkbox groups (e.g., Areas - count as filled if at least one is checked)
        // Note: For ASP.NET MVC, AvailableAreas[i].IsSelected generates multiple checkboxes
        // We need to count it as one fulfilled requirement if any 'IsSelected' is checked.
        if (this.elements.areaCheckboxesContainer && this.elements.areaCheckboxesContainer.querySelectorAll('input[name^="AvailableAreas"][type="checkbox"]').length > 0) {
            totalFields++;
            if (this.elements.areaCheckboxesContainer.querySelector('input[name^="AvailableAreas"][type="checkbox"]:checked')) {
                filledFields++;
            }
        }

        // Handle required file inputs (only new ones are required, existing ones depend on model)
        const fileInputs = this.elements.form.querySelectorAll('input[type="file"][required]');
        fileInputs.forEach(input => {
            totalFields++;
            if (input.files.length > 0) {
                filledFields++;
            }
        });


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

    // Validate overall form state
    validateForm() {
        if (!this.elements.form) return false;

        let allValid = true;

        // Use Bootstrap's built-in validation for native HTML5 fields
        this.elements.form.querySelectorAll('input[required], select[required], textarea[required], input[type="file"][required]').forEach(input => {
            if (!input.checkValidity()) {
                input.classList.add('is-invalid');
                allValid = false;
            } else {
                input.classList.remove('is-invalid');
            }
        });

        // Custom validations (these would ideally be handled by Bootstrap's validation or specific messages)
        if (!this.validateDocumentType()) allValid = false;
        if (!this.validateAreaSelection()) allValid = false;
        if (!this.validateFileUploads()) allValid = false; // Checks file sizes

        // Add 'was-validated' class to show Bootstrap validation feedback
        if (!allValid) {
            this.elements.form.classList.add('was-validated');
        } else {
            this.elements.form.classList.remove('was-validated');
        }

        return allValid;
    }

    // Validate document type selection (for custom error message display)
    validateDocumentType() {
        const documentTypeInputs = this.elements.form.querySelectorAll('input[name="Document_type"]');
        const isSelected = Array.from(documentTypeInputs).some(input => input.checked);
        const errorElement = this.elements.form.querySelector('.form-section:has(input[name="Document_type"]) .text-danger'); // Find validation message for this section

        if (!isSelected) {
            if (errorElement) {
                errorElement.style.display = 'block';
                errorElement.textContent = 'กรุณาเลือกประเภทเอกสารที่ร้องขอ';
            }
            return false;
        } else {
            if (errorElement) {
                errorElement.style.display = 'none';
            }
            return true;
        }
    }

    // Validate area selection (for custom error message display)
    validateAreaSelection() {
        const areaInputs = this.elements.areaCheckboxesContainer?.querySelectorAll('input[name^="AvailableAreas"][type="checkbox"]');
        if (!areaInputs || areaInputs.length === 0) return true; // No areas to select

        const isSelected = Array.from(areaInputs).some(input => input.checked);

        const errorElement = this.elements.form.querySelector('.form-section:has(#areaCheckboxe) + .text-danger'); // Assuming a validation message placeholder near the area section if needed

        if (!isSelected) {
            this.showNotification('กรุณาเลือกพื้นที่การใช้งานอย่างน้อย 1 รายการ', 'warning');
            if (errorElement) {
                errorElement.style.display = 'block';
                errorElement.textContent = 'กรุณาเลือกพื้นที่การใช้งานอย่างน้อย 1 รายการ';
            }
            return false;
        } else {
            if (errorElement) {
                errorElement.style.display = 'none';
            }
        }
        return true;
    }

    // Validate all file uploads (for size)
    validateFileUploads() {
        const fileInputs = this.elements.form.querySelectorAll('input[type="file"]');
        let allFilesValid = true;

        for (let input of fileInputs) {
            if (input.files.length > 0) {
                const file = input.files[0];
                const maxSize = parseInt(input.dataset.maxSize) || 10485760; // 10MB default

                if (file.size > maxSize) {
                    const maxSizeMB = Math.round(maxSize / 1048576);
                    this.showNotification(`ไฟล์ "${file.name}" มีขนาดใหญ่เกิน ${maxSizeMB}MB`, 'error');
                    this.markInvalid(input, `ไฟล์มีขนาดใหญ่เกิน ${maxSizeMB}MB`);
                    allFilesValid = false;
                } else {
                    this.markValid(input);
                }
            } else if (input.required) {
                // If the file input is required and no file is selected
                this.markInvalid(input, 'กรุณาอัปโหลดไฟล์');
                allFilesValid = false;
            }
        }
        return allFilesValid;
    }

    // Validate individual file upload for size and required state
    validateFileUpload(input) {
        const feedback = input.parentElement.querySelector('.invalid-feedback');
        input.setCustomValidity(''); // Clear previous validation message

        if (input.files.length === 0) {
            if (input.required) {
                this.markInvalid(input, 'กรุณาอัปโหลดไฟล์');
            } else {
                this.markValid(input); // If not required, it's valid if no file
            }
            return;
        }

        const file = input.files[0];
        const maxSize = parseInt(input.dataset.maxSize) || 10485760; // Default 10MB

        if (file.size > maxSize) {
            const maxSizeMB = Math.round(maxSize / 1048576);
            this.markInvalid(input, `ไฟล์มีขนาดใหญ่เกิน ${maxSizeMB}MB`);
        } else {
            this.markValid(input);
        }
    }

    markInvalid(input, message) {
        input.classList.add('is-invalid');
        input.classList.remove('is-valid');
        const feedback = input.parentElement.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.textContent = message;
            feedback.style.display = 'block';
        }
        input.setCustomValidity(message); // Set custom validity for HTML5 validation
    }

    markValid(input) {
        input.classList.remove('is-invalid');
        input.classList.add('is-valid');
        const feedback = input.parentElement.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.textContent = ''; // Clear message
            feedback.style.display = 'none';
        }
        input.setCustomValidity(''); // Clear custom validity
    }


    // --- Form Submission & Draft Management ---

    async handleSubmit(e) {
        e.preventDefault();

        if (this.isSubmitting) return;

        // Perform validation
        if (!this.validateForm()) {
            this.showNotification('กรุณากรอกข้อมูลให้ครบถ้วนและถูกต้อง', 'error');
            // Scroll to the first invalid element
            const firstInvalid = this.elements.form.querySelector('.is-invalid, .text-danger[style*="display: block"]');
            if (firstInvalid) {
                firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            return;
        }

        this.isSubmitting = true;
        const submitBtn = this.elements.form.querySelector('button[type="submit"]');
        const originalContent = submitBtn.innerHTML;

        try {
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>กำลังส่งคำร้อง...';
            submitBtn.disabled = true;

            // Use actual form submission if this is an MVC post
            // The following is a simulation for client-side only
            // If you are actually posting the form to an MVC controller,
            // you'd typically remove this simulateForm and let the browser handle the POST.
            // Or, if using AJAX, you'd send formData via fetch/axios.
            await this.simulateFormSubmission(); // Or your actual AJAX call

            this.showNotification('ส่งคำร้องเรียบร้อยแล้ว', 'success');
            this.clearDraftData();
            // Optionally, redirect or clear form here
            // this.elements.form.reset(); // Resets form fields
            // window.location.reload(); // Reloads page

        } catch (error) {
            console.error('Submit error:', error);
            this.showNotification('เกิดข้อผิดพลาดในการส่งคำร้อง กรุณาลองใหม่อีกครั้ง', 'error');
        } finally {
            submitBtn.innerHTML = originalContent;
            submitBtn.disabled = false;
            this.isSubmitting = false;
            // Removed was-validated class here, as it might interfere with subsequent changes
            // this.elements.form.classList.remove('was-validated'); 
        }
    }

    // Simulate form submission (replace with actual AJAX/fetch if not a full page post)
    async simulateFormSubmission() {
        return new Promise((resolve) => {
            setTimeout(() => {
                // Here you would send the form data, e.g., using FormData and fetch
                // const formData = new FormData(this.elements.form);
                // fetch('/Document/Create', {
                //     method: 'POST',
                //     body: formData
                // })
                // .then(response => response.json())
                // .then(data => {
                //     if (data.success) resolve(data);
                //     else reject(new Error(data.message || 'Server error'));
                // })
                // .catch(error => reject(error));
                console.log("Simulating form submission...");
                resolve({ success: true, message: 'Form submitted successfully!' });
            }, 2000); // Simulate 2-second network delay
        });
    }

    // Save draft data to localStorage
    saveDraft() {
        if (!this.elements.form) return;

        const formData = new FormData(this.elements.form);
        const draftData = {};

        // Convert FormData to a plain object, handling array inputs
        for (let [key, value] of formData.entries()) {
            // Special handling for file inputs: store only the file name
            // For a real draft, you'd need server-side handling or indexedDB for actual file data.
            if (value instanceof File) {
                if (value.name) { // Only store if a file was selected
                    draftData[key] = value.name;
                }
            } else {
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
        }

        try {
            localStorage.setItem('documentFormDraft', JSON.stringify(draftData));
            this.showNotification('บันทึกแบบร่างเรียบร้อยแล้ว', 'success');
        } catch (e) {
            console.error('Failed to save draft to localStorage:', e);
            this.showNotification('ไม่สามารถบันทึกแบบร่างได้: พื้นที่จัดเก็บเต็ม', 'error');
        }
    }

    // Check if form has any data (to decide if auto-save should run)
    hasFormData() {
        const inputs = this.elements.form.querySelectorAll('input, textarea, select');
        return Array.from(inputs).some(input => {
            if (input.type === 'checkbox' || input.type === 'radio') {
                return input.checked;
            }
            if (input.type === 'file') { // Check if a file is selected
                return input.files.length > 0;
            }
            return input.value.trim() !== '';
        });
    }

    // Clear draft data from localStorage
    clearDraftData() {
        localStorage.removeItem('documentFormDraft');
        // this.showNotification('ล้างข้อมูลแบบร่างแล้ว', 'info'); // Optional: notify when cleared
    }

    // Load draft data from localStorage
    loadDraft() {
        const draftData = localStorage.getItem('documentFormDraft');
        if (!draftData) return;

        try {
            const data = JSON.parse(draftData);
            // Iterate over the form elements and try to populate them
            for (const key in data) {
                if (data.hasOwnProperty(key)) {
                    const value = data[key];
                    const elements = this.elements.form.querySelectorAll(`[name="${key}"]`);

                    elements.forEach(element => {
                        if (element.type === 'radio' || element.type === 'checkbox') {
                            element.checked = (Array.isArray(value) ? value.includes(element.value) : element.value === value);
                            // Update custom UI for checked elements
                            element.closest('.form-check-card')?.classList.toggle('checked', element.checked);
                        } else if (element.type === 'file') {
                            // Cannot programmatically set file input value for security reasons.
                            // You might display the previously saved file name as a hint to the user.
                            const existingFileText = element.closest('.col-md-6').querySelector('.form-text.mt-1');
                            if (existingFileText) {
                                existingFileText.textContent = `(ไฟล์เดิม: ${value || 'ไม่มี'})`;
                            } else if (value) {
                                // Add a new div if it doesn't exist
                                const div = document.createElement('div');
                                div.className = 'form-text mt-1';
                                div.textContent = `(ไฟล์เดิม: ${value})`;
                                element.closest('.col-md-6').appendChild(div);
                            }
                        } else {
                            element.value = value;
                        }
                    });
                }
            }
            this.showNotification('โหลดแบบร่างที่บันทึกไว้เรียบร้อยแล้ว', 'info');
            this.updateProgress(); // Update progress after loading draft
        } catch (error) {
            console.error('Error loading draft:', error);
            this.showNotification('ไม่สามารถโหลดแบบร่างได้: ข้อมูลเสียหาย', 'error');
            this.clearDraftData(); // Clear corrupted draft
        }
    }

    // --- Print Functionality ---

    handlePrint() {
        // Validate form before printing
        if (!this.validateForm()) {
            this.showNotification('กรุณากรอกข้อมูลให้ครบถ้วนก่อนพิมพ์', 'warning');
            return;
        }

        // Add a print-specific class to body for custom print styles
        document.body.classList.add('print-mode');

        // Hide non-printable elements using CSS class
        const nonPrintableElements = document.querySelectorAll('.btn, .progress, .invalid-feedback, .form-text, .section-title i, .form-check-card .me-2 i');
        nonPrintableElements.forEach(el => el.classList.add('d-print-none')); // Use Bootstrap's d-print-none utility class

        // Temporarily hide specific elements if they cause layout issues in print
        const elementsToHide = document.querySelectorAll('.card-header .remove-document, #addDocumentBtn, footer');
        elementsToHide.forEach(el => el.classList.add('d-print-none'));

        window.print();

        // Restore elements after print dialog is closed
        // Using setTimeout with a small delay for better reliability after print dialog closes
        setTimeout(() => {
            document.body.classList.remove('print-mode');
            nonPrintableElements.forEach(el => el.classList.remove('d-print-none'));
            elementsToHide.forEach(el => el.classList.remove('d-print-none'));
        }, 500); // Give browser some time to handle print dialog
    }


    // --- UI Notifications ---

    // Show dynamic notification (e.g., Bootstrap Toast)
    showNotification(message, type = 'info') {
        // Assuming you have a Bootstrap Toast setup or a custom notification area
        // For simplicity, let's use a basic alert or console log if no Toast is set up.
        // A full Toast implementation would require a container and proper Bootstrap JS.
        console.log(`Notification (${type}): ${message}`);

        if (this.elements.notificationContainer) {
            // Example for a simple notification div
            const alertDiv = document.createElement('div');
            alertDiv.className = `alert alert-${type} alert-dismissible fade show mt-3`;
            alertDiv.role = 'alert';
            alertDiv.innerHTML = `
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            `;
            this.elements.notificationContainer.appendChild(alertDiv);

            // Auto-hide after some time
            setTimeout(() => {
                const bsAlert = new bootstrap.Alert(alertDiv);
                bsAlert.close();
            }, 5000); // Notification disappears after 5 seconds
        } else {
            // Fallback if no specific container
            // alert(message); 
        }
    }

    // Update add button text and disabled state
    updateAddButtonState() {
        if (this.elements.addDocumentBtn) {
            const currentDocumentCards = this.elements.documentDetailsList.querySelectorAll('.document-card').length;
            const isMaxReached = currentDocumentCards >= this.maxDocuments;
            this.elements.addDocumentBtn.disabled = isMaxReached;

            if (isMaxReached) {
                this.elements.addDocumentBtn.innerHTML = `
                    <i class="fas fa-ban me-1"></i> เพิ่มได้สูงสุด ${this.maxDocuments} รายการ
                `;
                this.elements.addDocumentBtn.classList.remove('btn-success');
                this.elements.addDocumentBtn.classList.add('btn-secondary');
            } else {
                this.elements.addDocumentBtn.innerHTML = `
                    <i class="fas fa-plus me-1"></i> เพิ่มเอกสาร
                `;
                this.elements.addDocumentBtn.classList.remove('btn-secondary');
                this.elements.addDocumentBtn.classList.add('btn-success');
            }
        }
    }
}

// Global initialization of the DocumentFormManager
// Make sure this runs after DOM is fully loaded.
document.addEventListener('DOMContentLoaded', () => {
    // Check if the form exists before trying to initialize
    if (document.getElementById('documentForm')) {
        new DocumentFormManager();
    }
});

// Optionally, if you want to expose it globally or for other modules
window.DocumentFormManager = DocumentFormManager;