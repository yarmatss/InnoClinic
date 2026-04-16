using NSubstitute;
using Profiles.BLL.Errors;
using Profiles.BLL.Services;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Models;
using Profiles.UnitTests.Fakes.Entities;
using Profiles.UnitTests.Fakes.Models;
using Shouldly;
using System.Linq.Expressions;

namespace Profiles.UnitTests.Services;

public class SpecializationServiceTests
{
    private readonly ISpecializationRepository _specRepo = Substitute.For<ISpecializationRepository>();
    private readonly SpecializationService _sut;

    public SpecializationServiceTests()
    {
        _sut = new SpecializationService(_specRepo);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WhenNameDuplicate_ReturnsConflictError()
    {
        // Arrange
        var model = new SpecializationModelFaker().Generate();
        var duplicate = new SpecializationFaker().Generate();

        _specRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Specialization, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Specialization> { duplicate }.AsReadOnly());

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SpecializationErrors.DuplicateName);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var model = new SpecializationModelFaker().Generate();

        _specRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Specialization, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Specialization>());

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(model.Name);
    }

    #endregion

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPagedResponse()
    {
        // Arrange
        var entities = new SpecializationFaker().Generate(4);
        var totalCount = 15;
        var query = new SpecializationQueryParameters { PageNumber = 1, PageSize = 4 };

        _specRepo
            .GetPagedAsync(query, Arg.Any<CancellationToken>())
            .Returns((entities.AsReadOnly() as IReadOnlyList<Specialization>, totalCount));

        // Act
        var result = await _sut.GetPagedAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(totalCount);
        result.Value.PageNumber.ShouldBe(1);
        result.Value.PageSize.ShouldBe(4);
        result.Value.Items.Count.ShouldBe(4);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var model = new SpecializationModelFaker().Generate();

        _specRepo.GetByIdAsync(id, Arg.Any<CancellationToken>(), trackChanges: true).Returns((Specialization?)null);

        // Act
        var result = await _sut.UpdateAsync(id, model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SpecializationErrors.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameDuplicate_ReturnsConflictError()
    {
        // Arrange
        var entity = new SpecializationFaker().Generate();
        var model = new SpecializationModelFaker().Generate();
        var duplicate = new SpecializationFaker(seed: 99).Generate();

        _specRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);
        _specRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Specialization, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Specialization> { duplicate }.AsReadOnly());

        // Act
        var result = await _sut.UpdateAsync(entity.Id, model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SpecializationErrors.DuplicateName);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedModel()
    {
        // Arrange
        var entity = new SpecializationFaker().Generate();
        var model = new SpecializationModelFaker().Generate();

        _specRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);
        _specRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Specialization, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Specialization>());

        // Act
        var result = await _sut.UpdateAsync(entity.Id, model, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(entity.Id);
    }

    #endregion
}
