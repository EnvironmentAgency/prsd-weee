﻿namespace EA.Weee.Web.Areas.Admin.ViewModels.CopyAatf
{
    using EA.Weee.Core.AatfReturn;
    using System.ComponentModel.DataAnnotations;

    public class CopyAeViewModel : CopyFacilityViewModelBase
    {
        public CopyAeViewModel()
        {
            FacilityType = FacilityType.Ae;
        }

        [RegularExpression(@"WEE/([A-Z]{2}[0-9]{4}[A-Z]{2})/(EXP|AE)", ErrorMessage = "Approval number is not in correct format")]
        public override string ApprovalNumber { get; set; }

        private string aatfName;
        [Required(ErrorMessage = "Enter name of AE")]
        [Display(Name = "Name of AE")]
        public override string Name
        {
            get => aatfName;

            set
            {
                aatfName = value;

                if (SiteAddressData != null)
                {
                    SiteAddressData.Name = value;
                }
            }
        }
    }
}