using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PAI_141249.Models
{
    public class UserLogin
    {
        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email jest wymagany")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Haslo jest wymagane")]
        [DataType(DataType.Password)]
        public string Haslo { get; set; }
    }
}