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
    
    public partial class Company
    {
        public int CompanyID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Nullable<int> IndustryGroupID { get; set; }
        public Nullable<int> CultureID { get; set; }
    }
}
