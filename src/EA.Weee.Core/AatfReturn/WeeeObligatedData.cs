﻿namespace EA.Weee.Core.AatfReturn
{
    using System;

    [Serializable]
    public sealed class WeeeObligatedData
    {
        public Guid Id { get; set; }

        public Scheme Scheme { get; set; }

        public AatfData Aatf { get; set; }

        public Guid ReturnId { get; set; }

        public Guid WeeeSentOnId { get; set; }

        public int CategoryId { get; set; }

        public decimal? B2B { get; set; }

        public decimal? B2C { get; set; }

        public decimal? Total
        {
            get
            {
                if (B2C.HasValue && B2B.HasValue)
                {
                    return B2B.Value + B2C.Value;
                }
                else if (B2C.HasValue)
                {
                    return B2C;
                }
                else if (B2B.HasValue)
                {
                    return B2B.Value;
                }

                return null;
            }
        }

        public WeeeObligatedData(Guid id, Scheme scheme, AatfData aatf, int categoryId, decimal? b2b, decimal? b2c)
        {
            this.Id = id;
            this.Scheme = scheme;
            this.Aatf = aatf;
            this.CategoryId = categoryId;
            this.B2B = b2b;
            this.B2C = b2c;
        }

        public WeeeObligatedData(Guid id, AatfData aatf, int categoryId, decimal? b2b, decimal? b2c)
        {
            this.Id = id;
            this.CategoryId = categoryId;
            this.B2B = b2b;
            this.B2C = b2c;
            this.Aatf = aatf;
        }

        public WeeeObligatedData()
        {
        }
    }
}
