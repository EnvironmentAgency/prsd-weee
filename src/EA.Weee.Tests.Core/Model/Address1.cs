//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EA.Weee.Tests.Core.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Address1
    {
        public Address1()
        {
            this.Contact1 = new HashSet<Contact1>();
        }
    
        public System.Guid Id { get; set; }
        public byte[] RowVersion { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string Street { get; set; }
        public string Town { get; set; }
        public string Locality { get; set; }
        public string AdministrativeArea { get; set; }
        public System.Guid CountryId { get; set; }
        public string PostCode { get; set; }
    
        public virtual Country Country { get; set; }
        public virtual ICollection<Contact1> Contact1 { get; set; }
    }
}
