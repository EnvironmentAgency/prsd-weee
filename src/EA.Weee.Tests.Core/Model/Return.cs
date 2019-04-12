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
    
    public partial class Return
    {
        public Return()
        {
            this.NonObligatedWeees = new HashSet<NonObligatedWeee>();
            this.WeeeReceiveds = new HashSet<WeeeReceived>();
            this.WeeeReuseds = new HashSet<WeeeReused>();
            this.WeeeSentOns = new HashSet<WeeeSentOn>();
        }
    
        public System.Guid Id { get; set; }
        public System.Guid OperatorId { get; set; }
        public int ComplianceYear { get; set; }
        public int Quarter { get; set; }
        public byte[] RowVersion { get; set; }
        public int ReturnStatus { get; set; }
    
        public virtual ICollection<NonObligatedWeee> NonObligatedWeees { get; set; }
        public virtual ICollection<WeeeReceived> WeeeReceiveds { get; set; }
        public virtual ICollection<WeeeReused> WeeeReuseds { get; set; }
        public virtual ICollection<WeeeSentOn> WeeeSentOns { get; set; }
        public virtual Operator Operator { get; set; }
    }
}
