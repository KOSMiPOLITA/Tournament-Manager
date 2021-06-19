using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PAI_141249.Custom_Validation
{
    public class CheckMapy : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var dotdot = new Regex("^.+\\..+$");
                string slowo = value.ToString();
                Debug.Write(slowo);
                if (dotdot.IsMatch(slowo))
                {
                    Debug.Write("Zły separator");
                    return new ValidationResult(ErrorMessage ?? "Separatorem dla współrzędnych jest przecinek");
                }
                Debug.Write("Dobry separator");

                double val = Convert.ToDouble(value);
                if (val >= -180 && val <= 180)
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage ?? "Współrzędne powinny być w zakresie <-180, 180>");
        }
    }
}