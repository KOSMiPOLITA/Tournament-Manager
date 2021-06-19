using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PAI_141249.Custom_Validation
{
    public class CheckModulo2 : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                double liczba = Convert.ToDouble(value);
            
                if ((Math.Log(liczba, 2) - (int)Math.Log(liczba, 2)) == 0.0)
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage ?? "Liczba uczestników / rozstawionych powinna być potęgą dwójki");
        }
    }
}