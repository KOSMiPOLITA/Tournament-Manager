using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using PAI_141249.Models;

namespace PAI_141249.Custom_Validation
{
    public class CheckRankUnique : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                SignUpDatabaseEntities db = new SignUpDatabaseEntities();
                if (db.SingUps.Where(a => a.Ranking == (int)value).FirstOrDefault() == null)
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage ?? "Osoba z takim rankingiem znajduje się już w bazie danych");
        }
    }
}