﻿namespace EA.Weee.Domain.AatfReturn
{
    using EA.Prsd.Core;
    using EA.Prsd.Core.Domain;
    using System;
    using System.Collections.Generic;

    public class WeeeSentOn : Entity
    {
        public virtual AatfAddress OperatorAddress { get; private set; }

        public virtual AatfAddress SiteAddress { get; private set; }

        public virtual Aatf Aatf { get; private set; }

        public virtual Return @Return { get; private set; }

        public virtual Guid ReturnId { get; private set; }

        public virtual Guid AatfId { get; private set; }

        public virtual Guid SiteAddressId { get; private set; }

        public virtual void UpdateWithOperatorAddress(AatfAddress @operator)
        {
            OperatorAddress = @operator;
        }

        public WeeeSentOn()
        {
        }

        public WeeeSentOn(Guid siteAddress, Guid aatf, Guid @return)
        {
            this.SiteAddressId = siteAddress;
            this.AatfId = aatf;
            this.ReturnId = @return;
        }

        public WeeeSentOn(AatfAddress operatorAddress, AatfAddress siteAddress, Aatf aatf, Return @return)
        {
            Guard.ArgumentNotNull(() => siteAddress, siteAddress);
            Guard.ArgumentNotNull(() => operatorAddress, operatorAddress);
            Guard.ArgumentNotNull(() => aatf, aatf);
            Guard.ArgumentNotNull(() => @return, @return);

            this.SiteAddress = siteAddress;
            this.OperatorAddress = operatorAddress;
            this.Aatf = aatf;
            this.Return = @return;
        }

        public WeeeSentOn(AatfAddress siteAddress, Aatf aatf, Return @return)
        {
            Guard.ArgumentNotNull(() => siteAddress, siteAddress);
            Guard.ArgumentNotNull(() => aatf, aatf);
            Guard.ArgumentNotNull(() => @return, @return);

            this.SiteAddress = siteAddress;
            this.Aatf = aatf;
            this.Return = @return;
        }
    }
}
