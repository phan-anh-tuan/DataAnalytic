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
    
    public partial class Security
    {
        public int SecurityID { get; set; }
        public string ISINCode { get; set; }
        public string Code { get; set; }
        public Nullable<int> CompanyID { get; set; }
        public Nullable<int> SecurityTypeID { get; set; }
        public Nullable<System.DateTime> ListingDate { get; set; }
    }
}