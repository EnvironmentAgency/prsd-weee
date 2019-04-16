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
    
    public partial class Organisation
    {
        public Organisation()
        {
            this.MemberUploads = new HashSet<MemberUpload>();
            this.OrganisationUsers = new HashSet<OrganisationUser>();
            this.Schemes = new HashSet<Scheme>();
            this.Operators = new HashSet<Operator>();
        }
    
        public System.Guid Id { get; set; }
        public byte[] RowVersion { get; set; }
        public string Name { get; set; }
        public int OrganisationType { get; set; }
        public int OrganisationStatus { get; set; }
        public string TradingName { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public Nullable<System.Guid> BusinessAddressId { get; set; }
        public Nullable<System.Guid> NotificationAddressId { get; set; }
    
        public virtual Address Address { get; set; }
        public virtual Address Address1 { get; set; }
        public virtual ICollection<MemberUpload> MemberUploads { get; set; }
        public virtual ICollection<OrganisationUser> OrganisationUsers { get; set; }
        public virtual ICollection<Scheme> Schemes { get; set; }
        public virtual ICollection<Operator> Operators { get; set; }
    }
}
