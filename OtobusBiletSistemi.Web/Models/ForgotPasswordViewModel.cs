using System.ComponentModel.DataAnnotations;

namespace OtobusBiletSistemi.Web.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email Adresiniz")]
        public string Email { get; set; }
    }
} 