//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAnalytic.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Country
    {
        public int CountryID { get; set; }
        public int CultureID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string ISO3166A2 { get; set; }
        public string ISO3166A3 { get; set; }
        public Nullable<short> ISO3166NUM { get; set; }
    }
}
