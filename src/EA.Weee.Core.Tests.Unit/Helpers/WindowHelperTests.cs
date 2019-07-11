﻿namespace EA.Weee.Core.Tests.Unit.Helpers
{
    using EA.Prsd.Core;
    using EA.Weee.Core.Helpers;
    using System;
    using Xunit;

    public class WindowHelperTests
    {
        [Fact]
        public void CurrentDateIsOutsideOfClosedWindow_ReturnsTrue()
        {
            SystemTime.Freeze(new DateTime(2019, 01, 01));

            bool result = WindowHelper.IsThereAnOpenWindow();

            SystemTime.Unfreeze();

            Assert.True(result);
        }

        [Fact]
        public void CurrentDateIsInsideOfClosedWindow_ReturnsFalse()
        {
            SystemTime.Freeze(new DateTime(2019, 3, 30));

            bool result = WindowHelper.IsThereAnOpenWindow();

            SystemTime.Unfreeze();

            Assert.False(result);
        }

        [Fact]
        public void CurrentDateIsStartOfClosedWindow_ReturnsFalse()
        {
            SystemTime.Freeze(new DateTime(2019, 3, 17));

            bool result = WindowHelper.IsThereAnOpenWindow();

            SystemTime.Unfreeze();

            Assert.False(result);
        }

        [Fact]
        public void CurrentDateIsEndOfClosedWindow_ReturnsTrue()
        {
            SystemTime.Freeze(new DateTime(2019, 4, 1));

            bool result = WindowHelper.IsThereAnOpenWindow();

            SystemTime.Unfreeze();

            Assert.True(result);
        }
    }
}