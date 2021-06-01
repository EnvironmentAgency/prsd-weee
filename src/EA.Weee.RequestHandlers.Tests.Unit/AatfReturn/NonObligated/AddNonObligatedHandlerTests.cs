namespace EA.Weee.RequestHandlers.Tests.Unit.AatfReturn.NonObligated
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.AatfReturn;
    using EA.Weee.Domain.DataReturns;
    using EA.Weee.Domain.Organisation;
    using EA.Weee.RequestHandlers.AatfReturn;
    using EA.Weee.RequestHandlers.Security;
    using EA.Weee.Requests.AatfReturn.NonObligated;
    using FakeItEasy;
    using FakeItEasy.Configuration;
    using FluentAssertions;
    using RequestHandlers.AatfReturn.NonObligated;
    using Xunit;

    public class AddNonObligatedHandlerTests
    {
        private readonly int numberOfCategories = 14;
        private readonly Guid organisationId = Guid.NewGuid();
        private readonly Guid returnId = Guid.NewGuid();
        private readonly Return @return = new Return(new Organisation(), new Quarter(2019, QuarterType.Q4), "someone", FacilityType.Aatf);

        private AddNonObligatedHandler handler;
        private IEnumerable<NonObligatedWeee> submittedCollection;
        private INonObligatedDataAccess nonObligatedDataAccess;
        private IReturnDataAccess returnDataAccess;
        private IReturnValueArgumentValidationConfiguration<System.Threading.Tasks.Task<List<NonObligatedWeee>>> fetchNonObligated;
        private IReturnValueArgumentValidationConfiguration<System.Threading.Tasks.Task> sumbitCall;
        private IReturnValueConfiguration<System.Threading.Tasks.Task<Return>> getReturnById;
        private IWeeeAuthorization authorization;

        public AddNonObligatedHandlerTests()
        {
        }

        [Fact]
        public async void HandleAsync_AddNonObligate_FullSet()
        {
            // Arrange
            Arrange(new List<NonObligatedWeee>());
            var message = CreateMessage(CreateMessageNonObligatedFullSet());

            // Act
            bool result = await this.handler.HandleAsync(message);

            // Assert
            AssertMandatory();
            sumbitCall.MustHaveHappened();
            submittedCollection.Count().Should().Be(message.CategoryValues.Count());
        }

        [Fact]
        public async void HandleAsync_AddNonObligate_PartialSet_PreExistingNonConflicting()
        {
            // Arrange
            Arrange(CreateRepoNonObligatedHalfSet(false));
            var message = CreateMessage(CreateMessageNonObligatedHalfSet());

            // Act
            bool result = await this.handler.HandleAsync(message);

            // Assert
            AssertMandatory();
            sumbitCall.MustHaveHappened();
            this.submittedCollection.Count().Should().Be(message.CategoryValues.Count());
        }

        [Fact]
        public async void HandleAsync_AddNonObligate_PartialSet_PreExistingConflicting()
        {
            // Arrange
            Arrange(CreateRepoNonObligatedFullSet());
            var message = CreateMessage(CreateMessageNonObligatedHalfSet());

            // Act
            bool result = await this.handler.HandleAsync(message);

            // Assert
            AssertMandatory();
            sumbitCall.MustNotHaveHappened();
            submittedCollection.Should().BeNull();
        }

        [Fact]
        public async void HandleAsync_AddNonObligate_FullSet_PreExistingConflicting()
        {
            // Arrange
            Arrange(CreateRepoNonObligatedFullSet());
            var message = CreateMessage(CreateMessageNonObligatedFullSet());

            // Act
            bool result = await this.handler.HandleAsync(message);

            // Assert
            AssertMandatory();
            sumbitCall.MustNotHaveHappened();
            submittedCollection.Should().BeNull();
        }

        [Fact]
        public async void HandleAsync_AddNonObligate_FullSet_Duplicating()
        {
            // Arrange
            Arrange(new List<NonObligatedWeee>());
            var message = CreateMessage(CreateMessageNonObligatedFullSet().Concat(CreateMessageNonObligatedFullSet()).ToList());

            // Act
            bool result = await this.handler.HandleAsync(message);

            // Assert
            AssertMandatory();
            sumbitCall.MustHaveHappened();
            submittedCollection.Count().Should().Be(message.CategoryValues.Count() / 2);
        }

        [Fact]
        public async void HandleAsync_AddNonObligate_FullSet_DuplicatingAndConflicting()
        {
            // Arrange
            Arrange(CreateRepoNonObligatedFullSet());
            var message = CreateMessage(CreateMessageNonObligatedFullSet().Concat(CreateMessageNonObligatedFullSet()).ToList());

            // Act
            bool result = await this.handler.HandleAsync(message);

            // Assert
            AssertMandatory();
            sumbitCall.MustNotHaveHappened();
            submittedCollection.Should().BeNull();
        }

        [Fact]
        public async void HandleAsync_AddNonObligate_FullSet_PartialConflicting()
        {
            // Arrange
            var existingRecords = CreateRepoNonObligatedHalfSet(false);
            Arrange(existingRecords);
            var message = CreateMessage(CreateMessageNonObligatedFullSet());

            // Act
            bool result = await this.handler.HandleAsync(message);

            // Assert
            AssertMandatory();
            sumbitCall.MustHaveHappened();
            submittedCollection.Count().Should().Be(message.CategoryValues.Count() - existingRecords.Count());
        }

        private AddNonObligated CreateMessage(IList<NonObligatedValue> values)
        {
            return new AddNonObligated()
            {
                Dcf = false,
                OrganisationId = this.organisationId,
                ReturnId = this.returnId,
                CategoryValues = values
            };
        }

        private void Arrange(List<NonObligatedWeee> existing)
        {
            this.authorization = A.Fake<IWeeeAuthorization>();
            this.nonObligatedDataAccess = A.Fake<INonObligatedDataAccess>();
            this.returnDataAccess = A.Fake<IReturnDataAccess>();

            this.getReturnById = A.CallTo(() => this.returnDataAccess.GetById(this.returnId));
            this.getReturnById.Returns(@return);

            this.fetchNonObligated = A.CallTo(() => this.nonObligatedDataAccess.FetchNonObligatedWeeeForReturn(this.returnId));
            this.fetchNonObligated.Returns(existing);

            this.sumbitCall = A.CallTo(() => this.nonObligatedDataAccess.Submit(A<IEnumerable<NonObligatedWeee>>.Ignored));
            this.sumbitCall.Invokes(a => this.submittedCollection = a.GetArgument<IEnumerable<NonObligatedWeee>>("nonObligated"));

            this.handler = new AddNonObligatedHandler(this.authorization, this.nonObligatedDataAccess, this.returnDataAccess);
        }

        private void AssertMandatory()
        {
            getReturnById.MustHaveHappened();
            fetchNonObligated.MustHaveHappened();
        }

        private List<NonObligatedValue> CreateMessageNonObligatedFullSet()
        {
            return CreateMessageNonObligatedHalfSet().Concat(CreateMessageNonObligatedHalfSet(false)).ToList();
        }

        private List<NonObligatedValue> CreateMessageNonObligatedHalfSet(bool isFirstHalf = true)
        {
            int halfCategoryCount = numberOfCategories / 2;
            int startCountAt = 1;
            if (isFirstHalf)
            {
                startCountAt += halfCategoryCount;
            }
            return Enumerable.Range(startCountAt, halfCategoryCount).Select(n => new NonObligatedValue(n, CreateTonnageForCategory(n), false, Guid.NewGuid())).ToList();
        }

        private List<NonObligatedWeee> CreateRepoNonObligatedFullSet()
        {
            return CreateRepoNonObligatedHalfSet().Concat(CreateRepoNonObligatedHalfSet(false)).ToList();
        }

        private List<NonObligatedWeee> CreateRepoNonObligatedHalfSet(bool isFirstHalf = true)
        {
            int halfCategoryCount = numberOfCategories / 2;
            int startCountAt = 1;
            if (isFirstHalf)
            {
                startCountAt += halfCategoryCount;
            }
            return Enumerable.Range(startCountAt, halfCategoryCount).Select(n => new NonObligatedWeee(this.@return, n, false, CreateTonnageForCategory(n))).ToList();
        }

        private int CreateTonnageForCategory(int category)
        {
            return category * 25;
        }
    }
}
