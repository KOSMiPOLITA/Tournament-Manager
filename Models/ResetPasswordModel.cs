using PAI_141249.Custom_Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PAI_141249.Models
{
    public class ResetPasswordModel
    {
        [Display(Name = "Hasło")]
        [Required(AllowEmptyStrings = false, ErrorMessage ="Nowe hasło nie może być puste")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Hasło ma mniej niż 8 znaków")]
        [CheckPassword]
        public string NewPassword { get; set; }

        [Display(Name = "Powtórzone hasło")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Nowe hasło i powtórzone nowe hasło się nie zgadzają")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string ResetCode { get; set; }
    }
}