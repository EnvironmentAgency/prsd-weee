﻿namespace EA.Weee.Domain
{
    using System;
    public class UKCompetentAuthority
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public string Abbreviation { get; private set; }
 
        public virtual Country Country { get; protected set; }

        protected UKCompetentAuthority()
        {
        }
    }
}