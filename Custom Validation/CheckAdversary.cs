using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Web;
using PAI_141249.Models;
using System.Web.Mvc;

namespace PAI_141249.Custom_Validation
{
    public class CheckAdversary : ValidationAttribute
    {
        WynikiDatabaseEntities db = new WynikiDatabaseEntities();
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            

            if (value != null)
            {
                //var w = db.WynikiTurniejows.Where(a => a.IdTurnieju == id_turnieju).Where(a => a.IdUser == id_uczestnika).OrderByDescending(a => a.Runda).FirstOrDefault();

                //var przec = db.WynikiTurniejows.Where(a => a.IdTurnieju == id_turnieju && a.Para == w.Para && a.IdUser != id_uczestnika && a.Runda == w.Runda).OrderByDescending(a => a.Runda).FirstOrDefault();

                if (true)
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