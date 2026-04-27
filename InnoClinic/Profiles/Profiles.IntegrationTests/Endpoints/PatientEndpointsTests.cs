using Profiles.API.DTOs.Patient;
using Profiles.Domain.Common;
using Profiles.Domain.Enums;
using Profiles.IntegrationTests.Infrastructure;
using Profiles.Tests.Common.Fakes.Entities;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Profiles.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class PatientEndpointsTests(PostgresContainerFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    private async Task<Guid> SeedPatientAsync(int seed = 42)
    {
        var patient = new PatientFaker(seed).Generate();
        await using var ctx = DbFixture.CreateDbContext();
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync(TestContext.Current.CancellationToken);
        return patient.Id;
    }

    private CreatePatientDto GetValidCreateDto() => new(
        FirstName: "John",
        LastName: "Doe",
        MiddleName: null,
        BirthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
        Gender: Gender.Male,
        NationalId: "12345678901",
        ContactPhone: "123456789",
        InsuranceNumber: "INS-12345",
        EmergencyContact: "Jane Doe",
        PrimaryDoctorId: null
    );

    private UpdatePatientDto GetValidUpdateDto() => new(
        FirstName: "Jane",
        LastName: "Smith",
        MiddleName: null,
        BirthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
        Gender: Gender.Female,
        NationalId: "10987654321",
        ContactPhone: "987654321",
        InsuranceNumber: "INS-54321",
        EmergencyContact: "John Smith",
        PrimaryDoctorId: null
    );

    #region CreatePatient

    [Fact]
    public async Task CreatePatient_WithValidPayload_Returns201AndLocationHeader()
    {
        var dto = GetValidCreateDto();

        var response = await Client.PostAsJsonAsync(
            "/api/patients", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        var body = await response.Content.ReadFromJsonAsync<PatientResponseDto>(
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        body.ShouldNotBeNull();
        body.FirstName.ShouldBe(dto.FirstName);
        body.LastName.ShouldBe(dto.LastName);
    }

    [Theory]
    [InlineData("", "Doe", "12345678901")] // Empty FirstName
    [InlineData("John", "", "12345678901")] // Empty LastName
    [InlineData("John", "Doe", "")] // Empty NationalId
    public async Task CreatePatient_WithInvalidPersonField_Returns400(
        string firstName, string lastName, string nationalId)
    {
        var dto = GetValidCreateDto() with
        {
            FirstName = firstName,
            LastName = lastName,
            NationalId = nationalId
        };

        var response = await Client.PostAsJsonAsync(
            "/api/patients", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_WithFutureBirthDate_Returns400()
    {
        var dto = GetValidCreateDto() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
        };

        var response = await Client.PostAsJsonAsync(
            "/api/patients", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_WithEmptyInsuranceNumber_Returns400()
    {
        var dto = GetValidCreateDto() with { InsuranceNumber = "" };

        var response = await Client.PostAsJsonAsync(
            "/api/patients", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetPatient

    [Fact]
    public async Task GetPatientById_WhenExists_Returns200WithBody()
    {
        var patientId = await SeedPatientAsync();

        var response = await Client.GetAsync(
            $"/api/patients/{patientId}", 
            TestContext.Current.CancellationToken);

        var body = await response.Content.ReadFromJsonAsync<PatientResponseDto>(
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body.Id.ShouldBe(patientId);
    }

    [Fact]
    public async Task GetPatientById_WhenNotFound_Returns404()
    {
        var response = await Client.GetAsync(
            $"/api/patients/{Guid.NewGuid()}", 
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllPatients_WithValidPaging_Returns200WithPagedResult()
    {
        await SeedPatientAsync(seed: 100);
        await SeedPatientAsync(seed: 101);

        var response = await Client.GetAsync(
            "/api/patients?PageNumber=1&PageSize=10", 
            TestContext.Current.CancellationToken);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<PatientResponseDto>>(
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        body.Items.ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("PageSize=0")]
    [InlineData("PageSize=101")]
    [InlineData("SortOrder=bad")]
    [InlineData("SortBy=Unknown")]
    public async Task GetAllPatients_WithInvalidQueryParams_Returns400(string queryParam)
    {
        var response = await Client.GetAsync(
            $"/api/patients?{queryParam}", 
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region UpdatePatient

    [Fact]
    public async Task UpdatePatient_WithValidPayload_Returns200WithUpdatedBody()
    {
        var patientId = await SeedPatientAsync();
        var dto = GetValidUpdateDto();

        var response = await Client.PutAsJsonAsync(
            $"/api/patients/{patientId}", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        var body = await response.Content.ReadFromJsonAsync<PatientResponseDto>(
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body.FirstName.ShouldBe(dto.FirstName);
        body.LastName.ShouldBe(dto.LastName);
    }

    [Fact]
    public async Task UpdatePatient_WhenNotFound_Returns404()
    {
        var dto = GetValidUpdateDto();

        var response = await Client.PutAsJsonAsync(
            $"/api/patients/{Guid.NewGuid()}", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("", "10987654321")] // Empty LastName
    [InlineData("Smith", "")] // Empty NationalId
    public async Task UpdatePatient_WithInvalidField_Returns400(string lastName, string nationalId)
    {
        var patientId = await SeedPatientAsync();
        var dto = GetValidUpdateDto() with
        {
            LastName = lastName,
            NationalId = nationalId
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/patients/{patientId}", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion
}
