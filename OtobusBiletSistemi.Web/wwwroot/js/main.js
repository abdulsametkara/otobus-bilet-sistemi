// ===== MAIN APPLICATION JAVASCRIPT =====

// DOM Ready
document.addEventListener('DOMContentLoaded', function() {
    
    // Initialize all components
    initializeAnimations();
    initializeFormValidation();
    initializeDatePickers();
    initializeNavbar();
    initializeTicketSearch();
    
});

// ===== ANIMATIONS =====
function initializeAnimations() {
    
    // Fade in elements on scroll
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in-up');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);
    
    // Observe elements
    document.querySelectorAll('.feature-card, .route-card, .ticket-card').forEach(el => {
        observer.observe(el);
    });
    
    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
}

// ===== NAVBAR FUNCTIONALITY =====
function initializeNavbar() {
    
    // Navbar scroll effect
    window.addEventListener('scroll', function() {
        const navbar = document.querySelector('.navbar');
        if (window.scrollY > 50) {
            navbar.classList.add('navbar-scrolled');
        } else {
            navbar.classList.remove('navbar-scrolled');
        }
    });
    
    // Mobile menu toggle
    const navbarToggler = document.querySelector('.navbar-toggler');
    const navbarCollapse = document.querySelector('.navbar-collapse');
    
    if (navbarToggler) {
        navbarToggler.addEventListener('click', function() {
            navbarCollapse.classList.toggle('show');
        });
    }
}

// ===== FORM VALIDATION =====
function initializeFormValidation() {
    
    // Custom validation for all forms
    const forms = document.querySelectorAll('.needs-validation');
    
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
                
                // Show first invalid field
                const firstInvalid = form.querySelector(':invalid');
                if (firstInvalid) {
                    firstInvalid.focus();
                    showNotification('Lütfen tüm alanları doğru şekilde doldurun.', 'error');
                }
            }
            form.classList.add('was-validated');
        });
    });
    
    // Real-time validation
    const inputs = document.querySelectorAll('input[required], select[required]');
    inputs.forEach(input => {
        input.addEventListener('blur', validateField);
        input.addEventListener('input', clearFieldError);
    });
    
    // TC No validation
    const tcInputs = document.querySelectorAll('input[name="TCNo"]');
    tcInputs.forEach(input => {
        input.addEventListener('input', function() {
            validateTCNo(this);
        });
    });
    
    // Email validation
    const emailInputs = document.querySelectorAll('input[type="email"]');
    emailInputs.forEach(input => {
        input.addEventListener('input', function() {
            validateEmail(this);
        });
    });
    
    // Password confirmation
    const confirmPasswordInputs = document.querySelectorAll('input[name="ConfirmPassword"]');
    confirmPasswordInputs.forEach(input => {
        input.addEventListener('input', function() {
            validatePasswordConfirmation(this);
        });
    });
}

// ===== FIELD VALIDATION FUNCTIONS =====
function validateField(event) {
    const field = event.target;
    const value = field.value.trim();
    
    if (field.hasAttribute('required') && !value) {
        showFieldError(field, 'Bu alan zorunludur.');
        return false;
    }
    
    clearFieldError(field);
    return true;
}

function clearFieldError(field) {
    if (typeof field === 'object' && field.target) {
        field = field.target;
    }
    
    const errorElement = field.parentNode.querySelector('.field-error');
    if (errorElement) {
        errorElement.remove();
    }
    field.classList.remove('is-invalid');
    field.classList.add('is-valid');
}

function showFieldError(field, message) {
    clearFieldError(field);
    
    field.classList.add('is-invalid');
    field.classList.remove('is-valid');
    
    const errorElement = document.createElement('div');
    errorElement.className = 'field-error text-danger small mt-1';
    errorElement.textContent = message;
    
    field.parentNode.appendChild(errorElement);
}

function validateTCNo(field) {
    const tcNo = field.value.replace(/\D/g, '');
    
    if (tcNo.length !== 11) {
        showFieldError(field, 'TC Kimlik No 11 haneli olmalıdır.');
        return false;
    }
    
    // TC Kimlik No algorithm validation
    if (!isValidTCNo(tcNo)) {
        showFieldError(field, 'Geçersiz TC Kimlik No.');
        return false;
    }
    
    clearFieldError(field);
    return true;
}

function isValidTCNo(tcNo) {
    if (tcNo.length !== 11) return false;
    if (tcNo[0] === '0') return false;
    
    const digits = tcNo.split('').map(Number);
    
    // Check algorithm
    let sum1 = 0, sum2 = 0;
    for (let i = 0; i < 9; i += 2) sum1 += digits[i];
    for (let i = 1; i < 8; i += 2) sum2 += digits[i];
    
    if ((sum1 * 7 - sum2) % 10 !== digits[9]) return false;
    if ((sum1 + sum2 + digits[9]) % 10 !== digits[10]) return false;
    
    return true;
}

function validateEmail(field) {
    const email = field.value.trim();
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    
    if (email && !emailRegex.test(email)) {
        showFieldError(field, 'Geçerli bir e-posta adresi girin.');
        return false;
    }
    
    clearFieldError(field);
    return true;
}

function validatePasswordConfirmation(field) {
    const password = document.querySelector('input[name="Password"]');
    const confirmPassword = field;
    
    if (password && confirmPassword.value !== password.value) {
        showFieldError(field, 'Şifreler eşleşmiyor.');
        return false;
    }
    
    clearFieldError(field);
    return true;
}

// ===== DATE PICKERS =====
function initializeDatePickers() {
    const dateInputs = document.querySelectorAll('input[type="date"]');
    
    dateInputs.forEach(input => {
        // Set minimum date to today
        const today = new Date().toISOString().split('T')[0];
        input.setAttribute('min', today);
        
        // Set maximum date to 1 year from now
        const maxDate = new Date();
        maxDate.setFullYear(maxDate.getFullYear() + 1);
        input.setAttribute('max', maxDate.toISOString().split('T')[0]);
    });
}

// ===== TICKET SEARCH =====
function initializeTicketSearch() {
    
    // Auto-complete for city inputs
    const cityInputs = document.querySelectorAll('.city-input');
    const turkishCities = [
        'İstanbul', 'Ankara', 'İzmir', 'Bursa', 'Antalya', 'Adana', 'Konya', 'Gaziantep',
        'Mersin', 'Diyarbakır', 'Kayseri', 'Eskişehir', 'Urfa', 'Malatya', 'Erzurum',
        'Van', 'Batman', 'Elazığ', 'Tokat', 'Isparta', 'Sivas', 'Kahramanmaraş',
        'Trabzon', 'Denizli', 'Ordu', 'Balıkesir', 'Kırıkkale', 'Afyon', 'Yozgat'
    ];
    
    cityInputs.forEach(input => {
        setupAutoComplete(input, turkishCities);
    });
    
    // Swap origin and destination
    const swapButton = document.querySelector('.swap-cities');
    if (swapButton) {
        swapButton.addEventListener('click', function() {
            const originInput = document.querySelector('input[name="Origin"]');
            const destinationInput = document.querySelector('input[name="Destination"]');
            
            if (originInput && destinationInput) {
                const temp = originInput.value;
                originInput.value = destinationInput.value;
                destinationInput.value = temp;
                
                // Add animation
                this.style.transform = 'rotate(180deg)';
                setTimeout(() => {
                    this.style.transform = 'rotate(0deg)';
                }, 300);
            }
        });
    }
}

// ===== NOTIFICATIONS =====
function showNotification(message, type = 'info', duration = 4000) {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <span class="notification-message">${message}</span>
            <button class="notification-close">&times;</button>
        </div>
    `;
    
    // Add to page
    let container = document.querySelector('.notification-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'notification-container';
        document.body.appendChild(container);
    }
    
    container.appendChild(notification);
    
    // Show animation
    setTimeout(() => notification.classList.add('show'), 100);
    
    // Auto remove
    setTimeout(() => {
        removeNotification(notification);
    }, duration);
    
    // Close button
    notification.querySelector('.notification-close').addEventListener('click', () => {
        removeNotification(notification);
    });
}

function removeNotification(notification) {
    notification.classList.remove('show');
    setTimeout(() => {
        if (notification.parentNode) {
            notification.parentNode.removeChild(notification);
        }
    }, 300);
}

// ===== UTILITY FUNCTIONS =====

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(amount);
}

// Format date
function formatDate(date) {
    return new Intl.DateTimeFormat('tr-TR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(new Date(date));
}

// ===== EXPORT FOR GLOBAL USE =====
window.BusTicketApp = {
    showNotification,
    formatCurrency,
    formatDate
}; 