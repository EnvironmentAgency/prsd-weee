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
    
    public partial class Business
    {
        public Business()
        {
            this.ProducerSubmissions = new HashSet<ProducerSubmission>();
        }
    
        public System.Guid Id { get; set; }
        public byte[] RowVersion { get; set; }
        public Nullable<System.Guid> CorrespondentForNoticesContactId { get; set; }
        public Nullable<System.Guid> CompanyId { get; set; }
        public Nullable<System.Guid> PartnershipId { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Contact1 Contact1 { get; set; }
        public virtual Partnership Partnership { get; set; }
        public virtual ICollection<ProducerSubmission> ProducerSubmissions { get; set; }
    }
}
