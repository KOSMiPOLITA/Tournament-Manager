using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PAI_141249.Models
{
    public class ResetPasswordModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage ="Noew hasło nie może być puste")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Nowe hasło i powtórzone nowe hasło się nie zgadzają")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string ResetCode { get; set; }
    }
}