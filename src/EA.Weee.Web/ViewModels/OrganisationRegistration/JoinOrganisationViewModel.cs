namespace EA.Weee.Web.ViewModels.OrganisationRegistration
{
    using Shared;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class JoinOrganisationViewModel : YesNoChoiceViewModel
    {
        [Required]
        public Guid OrganisationId { get; set; }

        public string OrganisationName { get; set; }

        public bool AnyActiveUsers { get; set; }

        [Required(ErrorMessage = "Confirm whether you want to request access to the organisation")]
        public override string SelectedValue { get; set; }
    }
}