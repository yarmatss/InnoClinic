using NSubstitute;
using Profiles.BLL.Errors;
using Profiles.BLL.Services;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;
using Profiles.Domain.Models;
using Profiles.Tests.Common.Fakes.Entities;
using Profiles.UnitTests.Fakes.Models;
using Shouldly;
using System.Linq.Expressions;

namespace Profiles.UnitTests.Services;

public class PatientServiceTests
{
    private readonly IPatientRepository _patientRepo = Substitute.For<IPatientRepository>();
    private readonly IMedicalStaffRepository _staffRepo = Substitute.For<IMedicalStaffRepository>();
    private readonly PatientService _sut;

    public PatientServiceTests()
    {
        _sut = new PatientService(_patientRepo, _staffRepo);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WhenInsuranceNumberDuplicate_ReturnsConflictError()
    {
        // Arrange
        var model = new PatientModelFaker().Generate();
        var duplicate = new PatientFaker().Generate();

        _patientRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Patient, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Patient> { duplicate }.AsReadOnly());

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PatientErrors.DuplicateInsuranceNumber);
    }

    [Fact]
    public async Task CreateAsync_WhenNationalIdDuplicate_ReturnsConflictError()
    {
        // Arrange
        var model = new PatientModelFaker().Generate();
        var duplicate = new PatientFaker().Generate();

        _patientRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Patient, bool>>>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<Patient>>(Array.Empty<Patient>(), [duplicate]);

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PatientErrors.DuplicateNationalId);
    }

    [Fact]
    public async Task CreateAsync_WhenPrimaryDoctorNotFound_ReturnsError()
    {
        // Arrange
        var model = new PatientModelFaker().Generate();
        model.PrimaryDoctorId = Guid.NewGuid();

        _patientRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Patient, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Patient>());

        _staffRepo.GetByIdAsync(model.PrimaryDoctorId.Value, Arg.Any<CancellationToken>()).Returns((MedicalStaff?)null);

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PatientErrors.PrimaryDoctorNotFound);
    }

    [Fact]
    public async Task CreateAsync_WhenPrimaryDoctorIsInactive_ReturnsError()
    {
        // Arrange
        var model = new PatientModelFaker().Generate();
        model.PrimaryDoctorId = Guid.NewGuid();

        var inactiveDoctor = new MedicalStaffFaker().Generate();
        inactiveDoctor.IsActive = false;

        _patientRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Patient, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Patient>());

        _staffRepo.GetByIdAsync(model.PrimaryDoctorId.Value, Arg.Any<CancellationToken>()).Returns(inactiveDoctor);

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PatientErrors.PrimaryDoctorNotFound);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var model = new PatientModelFaker().Generate();

        _patientRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Patient, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Patient>());

        // Act
        var result = await _sut.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.InsuranceNumber.ShouldBe(model.InsuranceNumber);
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsCorrectPagedResponse()
    {
        // Arrange
        var entities = new PatientFaker().Generate(5);
        var totalCount = 20;
        var query = new PatientQueryParameters { PageNumber = 1, PageSize = 5 };

        _patientRepo
            .GetPagedAsync(query, Arg.Any<CancellationToken>())
            .Returns((entities.AsReadOnly() as IReadOnlyList<Patient>, totalCount));

        // Act
        var result = await _sut.GetAllAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(totalCount);
        result.Value.PageNumber.ShouldBe(1);
        result.Value.PageSize.ShouldBe(5);
        result.Value.Items.Count.ShouldBe(5);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var id = Guid.NewGuid();
        _patientRepo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Patient?)null);

        // Act
        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PatientErrors.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsMappedModel()
    {
        // Arrange
        var entity = new PatientFaker().Generate();
        _patientRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        // Act
        var result = await _sut.GetByIdAsync(entity.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(entity.Id);
        result.Value.InsuranceNumber.ShouldBe(entity.InsuranceNumber);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var model = new PatientModelFaker().Generate();

        _patientRepo.GetByIdAsync(id, Arg.Any<CancellationToken>(), trackChanges: true).Returns((Patient?)null);

        // Act
        var result = await _sut.UpdateAsync(id, model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PatientErrors.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenInsuranceNumberDuplicate_ReturnsConflictError()
    {
        // Arrange
        var entity = new PatientFaker().Generate();
        var model = new PatientModelFaker().Generate();
        var duplicate = new PatientFaker(seed: 99).Generate();

        _patientRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);
        _patientRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Patient, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Patient> { duplicate }.AsReadOnly());

        // Act
        var result = await _sut.UpdateAsync(entity.Id, model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PatientErrors.DuplicateInsuranceNumber);
    }

    [Fact]
    public async Task UpdateAsync_WhenNationalIdDuplicate_ReturnsConflictError()
    {
        // Arrange
        var entity = new PatientFaker().Generate();
        var model = new PatientModelFaker().Generate();
        var duplicate = new PatientFaker(seed: 99).Generate();

        _patientRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);
        _patientRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Patient, bool>>>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<Patient>>(Array.Empty<Patient>(), [duplicate]);
            
        // Act
        var result = await _sut.UpdateAsync(entity.Id, model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PatientErrors.DuplicateNationalId);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedModel()
    {
        // Arrange
        var entity = new PatientFaker().Generate();
        var model = new PatientModelFaker().Generate();

        _patientRepo.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>(), trackChanges: true).Returns(entity);
        _patientRepo
            .GetByConditionAsync(Arg.Any<Expression<Func<Patient, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Patient>());

        // Act
        var result = await _sut.UpdateAsync(entity.Id, model, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(entity.Id);
    }

    #endregion
}
