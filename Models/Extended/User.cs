using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PAI_141249.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        public string PotwierdzHaslo { get; set; }
    }

    public class UserMetadata
    {
        [Display(Name = "Imię")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Pole Imię nie może być puste")]
        public string Imie { get; set; }

        [Display(Name = "Nazwisko")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Pole Nazwisko nie może być puste")]
        public string Nazwisko { get; set; }

        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Pole Email nie może być puste")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Pole Hasło nie może być puste")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Hasło ma mniej niż 8 znaków")]
        public string Haslo { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Pole Hasło nie może być puste")]
        [DataType(DataType.Password)]
        [Compare("Haslo", ErrorMessage = "Hasło i powtórzone hasło nie są takie same")]
        public string PotwierdzHaslo { get; set; }

    }
}