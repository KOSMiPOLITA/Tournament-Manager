//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PAI_141249.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class WynikiTurniejow
    {
        public int IdTurnieju { get; set; }
        public int IdUser { get; set; }
        public Nullable<int> IdPrzeciwnik { get; set; }
        public int Para { get; set; }
        public Nullable<int> Wygrany { get; set; }
        public int Runda { get; set; }
        public string Nazwa { get; set; }
    }
}
