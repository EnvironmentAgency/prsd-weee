﻿namespace EA.Weee.Web.Areas.Scheme.ViewModels
{
    using Core.Shared;
    using Prsd.Core.Validation;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class MemberUploadResultViewModel
    {
        public Guid MemberUploadId { get; set; }

        public Guid PcsId { get; set; }

        public List<ErrorData> ErrorData { get; set; }

        public decimal TotalCharges { get; set; }

        [MustBeTrue(ErrorMessage = "Please confirm that you have read the privacy policy")]
        [DisplayName("Confirm you have read our privacy policy")]
        public bool PrivacyPolicy { get; set; }

        public int? ComplianceYear { get; set; }

        public bool HasAnnualCharge { get; set; }
    }
}