﻿namespace EA.Weee.RequestHandlers.Mappings
{
    using System;
    using Core.AatfReturn;
    using Core.DataReturns;
    using Core.Scheme;
    using Core.Shared;
    using Domain.AatfReturn;
    using Prsd.Core.Mapper;
    using ObligationType = Core.Shared.ObligationType;
    using SchemeStatus = Core.Shared.SchemeStatus;

    //public class NonObligatedWeeeMap : IMap<NonObligatedCategoryValue, NonObligatedWeee>
    //{
    //    private readonly IMapper mapper;

    //    public NonObligatedWeeeMap(IMapper mapper)
    //    {
    //        this.mapper = mapper;
    //    }

    //    public NonObligatedWeee Map(NonObligatedCategoryValue source)
    //    {
    //        return new NonObligatedWeee
    //        {
    //            Tonnage = source.NonObligated,
    //            Dcf = source.Dcf,
    //            CategoryId = (int)source.Category
    //        };
    //    }

    //    /*
    //    public NonObligatedCategoryValue Map(NonObligatedWeee source)
    //    {
    //        return new NonObligatedCategoryValue
    //        {
    //            NonObligated = source.Tonnage,
    //            Dcf = source.Dcf,
    //            Category = (WeeeCategory)source.CategoryId
    //        };
    //    }*/
    //}
}