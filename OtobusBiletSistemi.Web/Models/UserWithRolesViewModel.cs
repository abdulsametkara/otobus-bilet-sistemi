using OtobusBiletSistemi.Core.Data;

namespace OtobusBiletSistemi.Web.Models
{
    public class UserWithRolesViewModel
    {
        public YolcuUser User { get; set; }
        public bool IsAdmin { get; set; }
    }
} 
 