using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PAI_141249.Custom_Validation
{
    public class CheckDateRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {   if (value != null)
            {
                DateTime dt = (DateTime)value;
                if (dt >= DateTime.UtcNow)
                {
                    return ValidationResult.Success;
                }
            }
            
            return new ValidationResult(ErrorMessage ?? "Nie można dodać turnieju z przeszłości");
        }

    }
}