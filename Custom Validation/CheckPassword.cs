using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PAI_141249.Custom_Validation
{
    public class CheckPassword : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var msg = "";

            var special = new Regex("^[a-zA-Z0-9 ]*$");
            var letter = new Regex(".*[a-z].*");
            var Bigletter = new Regex(".*[A-Z].*");
            var number = new Regex(".*[0-9].*");

            if (value != null)
            {
                
                if ((special.IsMatch((string)value)))
                {
                    msg = "Hasło powinno zawierać znak specjalny";
                    return new ValidationResult(ErrorMessage ?? msg);
                }
                else if (!(letter.IsMatch((string)value)))
                {
                    msg = "Hasło powinno zawierać małą literę";
                    return new ValidationResult(ErrorMessage ?? msg);
                }
                else if (!(Bigletter.IsMatch((string)value)))
                {
                    msg = "Hasło powinno zawierać dużą literę";
                    return new ValidationResult(ErrorMessage ?? msg);
                }
                else if (!(number.IsMatch((string)value)))
                {
                    msg = "Hasło powinno zawierać cyfrę";
                    return new ValidationResult(ErrorMessage ?? msg);
                }
            }
            return ValidationResult.Success;
        }
    }
}