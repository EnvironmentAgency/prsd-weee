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
    
    public partial class ReportOnQuestion
    {
        public ReportOnQuestion()
        {
            this.ReturnReportOns = new HashSet<ReturnReportOn>();
        }
    
        public int Id { get; set; }
        public string Question { get; set; }
        public string Description { get; set; }
        public Nullable<int> ParentId { get; set; }
    
        public virtual ICollection<ReturnReportOn> ReturnReportOns { get; set; }
    }
}
