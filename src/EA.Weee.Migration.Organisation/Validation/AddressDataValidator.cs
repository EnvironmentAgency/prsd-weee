﻿namespace EA.Weee.Migration.Organisation.Validation
{
    using FluentValidation;

    public class AddressDataValidator : AbstractValidator<AddressData>
    {
        public AddressDataValidator()
        {
            RuleSet("Address", () =>
            {
                RuleFor(a => a.Address1).NotEmpty().MaximumLength(60);
                RuleFor(a => a.Address2).MaximumLength(60);
                RuleFor(a => a.TownOrCity).NotEmpty().MaximumLength(35);
                RuleFor(a => a.CountyOrRegion).MaximumLength(35);
                RuleFor(a => a.Postcode).MaximumLength(10);
                RuleFor(a => a.Telephone).NotEmpty().MaximumLength(20);
                RuleFor(a => a.Email).NotEmpty().MaximumLength(256);
            });
        }
    }
}
