# Gmail Email Kurulumu

## Gmail Hesabı Kurulumu

### 1. Gmail Hesabı Hazırlama
1. Gmail hesabınızda **2-Factor Authentication (2FA)** açın
2. **App Password** oluşturun:
   - Google Account Settings → Security → 2-Step Verification → App passwords
   - "Select app" → Mail
   - "Select device" → Custom name (örn: "Otobus Sistemi")
   - Oluşturulan 16 karakter app password'ü kopyalayın

### 2. Konfigürasyon
`appsettings.json` dosyasında EmailSettings bölümünü güncelleyin:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "sizin-email@gmail.com",
    "SmtpPassword": "16-karakter-app-password",
    "FromEmail": "sizin-email@gmail.com",
    "FromName": "Otobüs Bilet Sistemi"
  }
}
```

### 3. Test Etme
1. Sistemi çalıştırın
2. Şifre sıfırlama özelliğini test edin
3. Email gelip gelmediğini kontrol edin

### 4. Güvenlik Notları
⚠️ **Önemli:**
- App password'ü asla kod deposuna yüklemeyin
- Normal Gmail şifrenizi değil, App Password'ü kullanın
- 2FA mutlaka açık olmalı

### 5. Sorun Giderme

**"Authentication Required" Hatası:**
- 2FA açık olduğundan emin olun
- App password kullanıyor olduğunuzdan emin olun
- Username ve password doğru olduğundan emin olun

**Email Gelmiyor:**
- Spam klasörünü kontrol edin
- SMTP ayarlarını tekrar kontrol edin
- Port 587 kullandığınızdan emin olun 