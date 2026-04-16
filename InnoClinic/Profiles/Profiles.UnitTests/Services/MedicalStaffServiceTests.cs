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

public class MedicalStaffServiceTests
{
    private readonly IMedicalStaffRepository _staffRepo = Substitute.For<IMedicalStaffRepository>();
    private readonly ISpecializationRepository _specRepo = Substitute.For<ISpecializationRepository>();
    private readonly MedicalStaffService _sut;

    public MedicalStaffServiceTests()
    {
        _sut = new MedicalStaffService(_staffRepo, _specRepo);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WhenLicenseNumberDuplicate_ReturnsConflictError()
    {
        // Arrange
        var model = new MedicalStaffModelFaker().Generate();
        var duplicate = new MedicalStaffFaker().Generate();

        _staffRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<MedicalStaff, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<MedicalStaff> { duplicate }.AsReadOnly());

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.DuplicateLicenseNumber);
    }

    [Fact]
    public async Task CreateAsync_WhenNationalIdDuplicate_ReturnsConflictError()
    {
        // Arrange
        var model = new MedicalStaffModelFaker().Generate();
        var duplicate = new MedicalStaffFaker().Generate();

        _staffRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<MedicalStaff, bool>>>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<MedicalStaff>>(Array.Empty<MedicalStaff>(), [duplicate]);

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.DuplicateNationalId);
    }

    [Fact]
    public async Task CreateAsync_WhenSupervisorNotFound_ReturnsError()
    {
        // Arrange
        var model = new MedicalStaffModelFaker().Generate();
        model.SupervisorId = Guid.NewGuid();

        _staffRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<MedicalStaff, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MedicalStaff>());

        _staffRepo
            .GetByIdAsync(model.SupervisorId.Value, Arg.Any<CancellationToken>())
            .Returns((MedicalStaff?)null);

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.SupervisorNotFound);
    }

    [Fact]
    public async Task CreateAsync_WhenSupervisorIsInactive_ReturnsError()
    {
        // Arrange
        var model = new MedicalStaffModelFaker().Generate();
        model.SupervisorId = Guid.NewGuid();

        var inactiveSupervisor = new MedicalStaffFaker().Generate();
        inactiveSupervisor.IsActive = false;

        _staffRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<MedicalStaff, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MedicalStaff>());

        _staffRepo
            .GetByIdAsync(model.SupervisorId.Value, Arg.Any<CancellationToken>())
            .Returns(inactiveSupervisor);

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.SupervisorNotFound);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var model = new MedicalStaffModelFaker().Generate();

        _staffRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<MedicalStaff, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MedicalStaff>());

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.LicenseNumber.ShouldBe(model.LicenseNumber);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var id = Guid.NewGuid();
        _staffRepo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((MedicalStaff?)null);

        // Act
        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsMappedModel()
    {
        // Arrange
        var entity = new MedicalStaffFaker().Generate();
        _staffRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        // Act
        var result = await _sut.GetByIdAsync(entity.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(entity.Id);
        result.Value.LicenseNumber.ShouldBe(entity.LicenseNumber);
    }

    #endregion

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPagedResponse()
    {
        // Arrange
        var entities = new MedicalStaffFaker().Generate(3);
        var totalCount = 10;
        var query = new MedicalStaffQueryParameters { PageNumber = 2, PageSize = 3 };

        _staffRepo
            .GetPagedAsync(query, Arg.Any<CancellationToken>())
            .Returns((entities.AsReadOnly() as IReadOnlyList<MedicalStaff>, totalCount));

        // Act
        var result = await _sut.GetPagedAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(totalCount);
        result.Value.PageNumber.ShouldBe(2);
        result.Value.PageSize.ShouldBe(3);
        result.Value.Items.Count.ShouldBe(3);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var model = new MedicalStaffModelFaker().Generate();

        _staffRepo.GetByIdAsync(id, Arg.Any<CancellationToken>(), trackChanges: true).Returns((MedicalStaff?)null);

        // Act
        var result = await _sut.UpdateAsync(id, model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenLicenseDuplicate_ReturnsConflictError()
    {
        // Arrange
        var entity = new MedicalStaffFaker().Generate();
        var model = new MedicalStaffModelFaker().Generate();
        var duplicate = new MedicalStaffFaker(seed: 99).Generate();

        _staffRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);

        _staffRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<MedicalStaff, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<MedicalStaff> { duplicate }.AsReadOnly());

        // Act
        var result = await _sut.UpdateAsync(entity.Id, model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.DuplicateLicenseNumber);
    }

    [Fact]
    public async Task UpdateAsync_WhenNationalIdDuplicate_ReturnsConflictError()
    {
        // Arrange
        var entity = new MedicalStaffFaker().Generate();
        var model = new MedicalStaffModelFaker().Generate();
        var duplicate = new MedicalStaffFaker(seed: 99).Generate();

        _staffRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);

        _staffRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<MedicalStaff, bool>>>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<MedicalStaff>>(Array.Empty<MedicalStaff>(), [duplicate]);

        // Act
        var result = await _sut.UpdateAsync(entity.Id, model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.DuplicateNationalId);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedModel()
    {
        // Arrange
        var entity = new MedicalStaffFaker().Generate();
        var model = new MedicalStaffModelFaker().Generate();

        _staffRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);
        _staffRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<MedicalStaff, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MedicalStaff>());

        // Act
        var result = await _sut.UpdateAsync(entity.Id, model, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(entity.Id);
    }

    #endregion

    #region DeactivateAsync

    [Fact]
    public async Task DeactivateAsync_WhenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var id = Guid.NewGuid();
        _staffRepo.GetByIdAsync(id, Arg.Any<CancellationToken>(), trackChanges: true).Returns((MedicalStaff?)null);

        // Act
        var result = await _sut.DeactivateAsync(id, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.NotFound);
    }

    [Fact]
    public async Task DeactivateAsync_WhenFound_SetsIsActiveFalseAndSaves()
    {
        // Arrange
        var entity = new MedicalStaffFaker().Generate();
        entity.IsActive = true;

        _staffRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);

        // Act
        var result = await _sut.DeactivateAsync(entity.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        entity.IsActive.ShouldBeFalse();
        _staffRepo.Received(1).MarkUpdate(entity);
        await _staffRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region AssignSpecializationsAsync

    [Fact]
    public async Task AssignSpecializationsAsync_WhenStaffNotFound_ReturnsError()
    {
        // Arrange
        var staffId = Guid.NewGuid();
        _staffRepo.GetByIdAsync(staffId, Arg.Any<CancellationToken>(), trackChanges: true).Returns((MedicalStaff?)null);

        // Act
        var result = await _sut.AssignSpecializationsAsync(staffId, [], CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.NotFound);
    }

    [Fact]
    public async Task AssignSpecializationsAsync_WhenInvalidSpecializationId_ReturnsError()
    {
        // Arrange
        var entity = new MedicalStaffFaker().Generate();
        var assignments = new StaffSpecializationModelFaker().Generate(2);
        assignments.ForEach(a => a.StaffId = entity.Id);

        _staffRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);

        // Only 1 specialization found for 2 requested ids — count mismatch
        var oneSpec = new SpecializationFaker().Generate(1);
        _specRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Specialization, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(oneSpec.AsReadOnly());

        // Act
        var result = await _sut.AssignSpecializationsAsync(entity.Id, assignments, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(MedicalStaffErrors.InvalidSpecialization);
    }

    [Fact]
    public async Task AssignSpecializationsAsync_WithValidAssignments_ClearsOldAndAddsNew()
    {
        // Arrange
        var entity = new MedicalStaffFaker().Generate();
        var assignments = new StaffSpecializationModelFaker().Generate(2);
        assignments.ForEach(a => a.StaffId = entity.Id);

        _staffRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);

        var specs = new SpecializationFaker().Generate(2);
        specs[0].Id = assignments[0].SpecializationId;
        specs[1].Id = assignments[1].SpecializationId;

        _specRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Specialization, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(specs.AsReadOnly());
        // Act
        var result = await _sut.AssignSpecializationsAsync(entity.Id, assignments, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        entity.StaffSpecializations.Count.ShouldBe(2);
        await _staffRepo.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignSpecializationsAsync_WithEmptyList_ClearsSpecializations()
    {
        // Arrange
        var entity = new MedicalStaffFaker().Generate();
        entity.StaffSpecializations.Add(new StaffSpecialization { SpecializationId = Guid.NewGuid() });

        _staffRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);

        // Act
        var result = await _sut.AssignSpecializationsAsync(entity.Id, [], CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        entity.StaffSpecializations.Count.ShouldBe(0);
    }

    #endregion
}
