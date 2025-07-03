// ===== MAIN APPLICATION JAVASCRIPT =====

// DOM Ready
document.addEventListener('DOMContentLoaded', function() {
    
    // Initialize all components
    initializeAnimations();
    initializeFormValidation();
    initializeDatePickers();
    initializeNavbar();
    initializeTicketSearch();
    initializeCitySelection();
    
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
    // Bu fonksiyon şu anda sadece select dropdown'lar için basit işlevsellik sağlar
    
    // Swap origin and destination (eğer varsa)
    const swapButton = document.querySelector('.swap-cities');
    if (swapButton) {
        swapButton.addEventListener('click', function() {
            const neredenSelect = document.querySelector('select[name="Nereden"]');
            const nereyeSelect = document.querySelector('select[name="Nereye"]');
            
            if (neredenSelect && nereyeSelect) {
                const temp = neredenSelect.value;
                neredenSelect.value = nereyeSelect.value;
                nereyeSelect.value = temp;
                
                // Add animation
                this.style.transform = 'rotate(180deg)';
                setTimeout(() => {
                    this.style.transform = 'rotate(0deg)';
                }, 300);
                
                // Trigger change event
                neredenSelect.dispatchEvent(new Event('change'));
            }
        });
    }
}

// ===== ŞEHİR SEÇİMİ VE TARİH FİLTRELEME =====
function initializeCitySelection() {
    const neredenSelect = document.querySelector('select[name="Nereden"]');
    const nereyeSelect = document.querySelector('select[name="Nereye"]');
    const tarihInput = document.querySelector('input[name="Tarih"]');
    
    if (!neredenSelect || !nereyeSelect || !tarihInput) return;
    
    let mevcutTarihler = [];
    
    // Şehir seçimi değiştiğinde
    function handleCityChange() {
        const nereden = neredenSelect.value;
        const nereye = nereyeSelect.value;
        
        // Aynı şehir seçimini engelle
        if (nereden && nereye && nereden === nereye) {
            showNotification('Kalkış ve varış şehri aynı olamaz!', 'warning', 3000);
            resetDatePicker();
            return;
        }
        
        // Her iki şehir de seçildiyse sefer tarihlerini getir
        if (nereden && nereye && nereden !== nereye) {
            getMevcutSeferTarihleri(nereden, nereye);
        } else {
            // Seçim eksikse tarih alanını varsayılana döndür
            resetDatePicker();
        }
    }
    
    // Event listener'ları ekle
    neredenSelect.addEventListener('change', handleCityChange);
    nereyeSelect.addEventListener('change', handleCityChange);
    
    // API çağrısı fonksiyonu
    async function getMevcutSeferTarihleri(nereden, nereye) {
        try {
            // Loading göster
            showDatePickerLoading(true);
            
            const response = await fetch(`/Home/GetMevcutSeferTarihleri?nereden=${encodeURIComponent(nereden)}&nereye=${encodeURIComponent(nereye)}`);
            const data = await response.json();
            
            if (data.success) {
                mevcutTarihler = data.dates || [];
                updateDatePicker(mevcutTarihler);
                showNotification(data.message, 'success', 3000);
            } else {
                mevcutTarihler = [];
                resetDatePicker();
                showNotification(data.message, 'warning', 5000);
            }
        } catch (error) {
            console.error('Sefer tarihleri getirilirken hata:', error);
            mevcutTarihler = [];
            resetDatePicker();
            showNotification('Sefer tarihleri yüklenirken bir hata oluştu.', 'error');
        } finally {
            showDatePickerLoading(false);
        }
    }
    
    // Tarih picker'ı güncelle
    function updateDatePicker(dates) {
        // Tarihi sıfırla
        tarihInput.value = '';
        
        // Özel tarih validasyonu ekle
        tarihInput.removeEventListener('input', validateSelectedDate);
        tarihInput.addEventListener('input', validateSelectedDate);
        
        // Min tarih olarak bugünü ayarla
        const today = new Date().toISOString().split('T')[0];
        tarihInput.min = today;
        
        // CSS sınıflarını güncelle
        tarihInput.classList.remove('has-trips', 'no-trips');
        if (dates.length > 0) {
            tarihInput.classList.add('has-trips');
        } else {
            tarihInput.classList.add('no-trips');
        }
        
        // Info mesajını ve sefer tarihlerini güncelle
        updateDateInfo(dates);
        
        // Eğer mevcut tarihler varsa, ilk mevcut tarihi varsayılan olarak seç
        if (dates.length > 0) {
            const firstAvailableDate = dates[0];
            if (firstAvailableDate >= today) {
                tarihInput.value = firstAvailableDate;
            }
        }
        
        // CSS ile disable edilmiş görünüm için data attribute ekle
        tarihInput.setAttribute('data-available-dates', JSON.stringify(dates));
    }
    
    // Tarih bilgi kutusunu güncelle
    function updateDateInfo(dates) {
        const dateInfo = document.getElementById('date-info');
        if (!dateInfo) return;
        
        // Mevcut içeriği temizle
        dateInfo.innerHTML = '';
        
        if (dates.length > 0) {
            // Ana bilgi mesajı
            const infoBox = document.createElement('div');
            infoBox.className = 'date-info-box success';
            infoBox.innerHTML = `
                <i class="fas fa-calendar-check me-2"></i>
                <strong>${dates.length} farklı tarihte sefer mevcut</strong>
            `;
            dateInfo.appendChild(infoBox);
            
            // Sefer tarihlerini chip olarak göster
            const datesContainer = document.createElement('div');
            datesContainer.className = 'available-dates';
            
            dates.forEach(date => {
                const dateChip = document.createElement('span');
                dateChip.className = 'date-chip';
                
                // Tarih formatını güzelleştir
                const formattedDate = formatTurkishDate(date);
                dateChip.textContent = formattedDate;
                dateChip.title = `${date} tarihinde sefer mevcut - tıklayın`;
                
                // Chip'e tıklandığında tarihi seç
                dateChip.addEventListener('click', function() {
                    tarihInput.value = date;
                    
                    // Diğer chip'leri normal yap, bu chip'i seçili yap
                    datesContainer.querySelectorAll('.date-chip').forEach(chip => {
                        chip.classList.remove('selected');
                    });
                    this.classList.add('selected');
                    
                    // Change event'ini trigger et
                    tarihInput.dispatchEvent(new Event('change'));
                });
                
                // İlk tarih varsayılan seçili
                if (date === dates[0]) {
                    dateChip.classList.add('selected');
                }
                
                datesContainer.appendChild(dateChip);
            });
            
            dateInfo.appendChild(datesContainer);
            
        } else {
            // Sefer yok mesajı
            const infoBox = document.createElement('div');
            infoBox.className = 'date-info-box warning';
            infoBox.innerHTML = `
                <i class="fas fa-calendar-times me-2"></i>
                <strong>Bu güzergahta sefer bulunamadı</strong><br>
                <small>Lütfen farklı bir güzergah seçin</small>
            `;
            dateInfo.appendChild(infoBox);
        }
    }
    
    // Türkçe tarih formatla
    function formatTurkishDate(dateString) {
        const date = new Date(dateString);
        const options = { 
            day: 'numeric', 
            month: 'short',
            weekday: 'short'
        };
        return date.toLocaleDateString('tr-TR', options);
    }
    
    // Seçilen tarihi validate et
    function validateSelectedDate(event) {
        const selectedDate = event.target.value;
        
        if (selectedDate && mevcutTarihler.length > 0) {
            if (!mevcutTarihler.includes(selectedDate)) {
                showFieldError(tarihInput, 'Bu tarihte sefer bulunmamaktadır. Lütfen mevcut tarihlerden birini seçin.');
                return false;
            } else {
                clearFieldError(tarihInput);
                return true;
            }
        }
        return true;
    }
    
    // Tarih picker'ı sıfırla
    function resetDatePicker() {
        const today = new Date().toISOString().split('T')[0];
        tarihInput.min = today;
        tarihInput.value = today;
        tarihInput.removeAttribute('data-available-dates');
        tarihInput.removeEventListener('input', validateSelectedDate);
        mevcutTarihler = [];
        clearFieldError(tarihInput);
        
        // CSS sınıflarını temizle
        tarihInput.classList.remove('has-trips', 'no-trips');
        
        // Info mesajını sıfırla
        const dateInfo = document.getElementById('date-info');
        if (dateInfo) {
            dateInfo.innerHTML = '';
            const infoBox = document.createElement('div');
            infoBox.className = 'date-info-box info';
            infoBox.innerHTML = `
                <i class="fas fa-info-circle me-2"></i>
                <strong>Önce şehirleri seçin</strong><br>
                <small>Sefer olan günler otomatik olarak gösterilecek</small>
            `;
            dateInfo.appendChild(infoBox);
        }
    }
    
    // Loading göster/gizle
    function showDatePickerLoading(show) {
        const formGroup = tarihInput.closest('.form-group');
        let loadingElement = formGroup.querySelector('.date-loading');
        
        if (show) {
            if (!loadingElement) {
                loadingElement = document.createElement('div');
                loadingElement.className = 'date-loading text-muted small mt-1';
                loadingElement.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Müsait tarihler yükleniyor...';
                formGroup.appendChild(loadingElement);
            }
            tarihInput.disabled = true;
        } else {
            if (loadingElement) {
                loadingElement.remove();
            }
            tarihInput.disabled = false;
        }
    }
}

// ===== NOTIFICATIONS =====
function showNotification(message, type = 'info', duration = 4000) {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerHTML = `
        ${message}
        <button class="close-btn" type="button" aria-label="Close">&times;</button>
    `;
    
    // Add to page
    document.body.appendChild(notification);
    
    // Auto remove
    setTimeout(() => {
        removeNotification(notification);
    }, duration);
    
    // Close button
    notification.querySelector('.close-btn').addEventListener('click', () => {
        removeNotification(notification);
    });
}

function removeNotification(notification) {
    notification.style.animation = 'slideOutRight 0.3s ease-out forwards';
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