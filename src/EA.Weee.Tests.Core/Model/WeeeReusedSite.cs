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
    
    public partial class WeeeReusedSite
    {
        public System.Guid Id { get; set; }
        public System.Guid WeeeReusedId { get; set; }
        public byte[] RowVersion { get; set; }
        public System.Guid AddressId { get; set; }
    
        public virtual WeeeReused WeeeReused { get; set; }
    }
}
