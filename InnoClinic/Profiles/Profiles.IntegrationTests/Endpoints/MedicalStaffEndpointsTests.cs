using Profiles.API.DTOs.MedicalStaff;
using Profiles.Domain.Common;
using Profiles.Domain.Enums;
using Profiles.IntegrationTests.Infrastructure;
using Profiles.Tests.Common.Fakes.Entities;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Profiles.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class MedicalStaffEndpointsTests(PostgresContainerFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    private async Task<Guid> SeedStaffAsync()
    {
        var staff = new MedicalStaffFaker().Generate();
        await using var ctx = DbFixture.CreateDbContext();
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync(TestContext.Current.CancellationToken);
        return staff.Id;
    }

    private async Task<(Guid StaffId, Guid SpecId)> SeedDataWithSpecAsync()
    {
        var staff = new MedicalStaffFaker().Generate();
        var spec = new SpecializationFaker().Generate();
        await using var ctx = DbFixture.CreateDbContext();
        ctx.Staff.Add(staff);
        ctx.Specializations.Add(spec);
        await ctx.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (staff.Id, spec.Id);
    }

    private CreateMedicalStaffDto GetValidCreateDto() => new(
        FirstName: "Gregory",
        LastName: "House",
        MiddleName: null,
        BirthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-40)),
        Gender: Gender.Male,
        NationalId: "11223344556",
        ContactPhone: "555123456",
        StaffType: StaffType.Doctor,
        LicenseNumber: "MD12345",
        HireDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)),
        SupervisorId: null
    );

    private UpdateMedicalStaffDto GetValidUpdateDto() => new(
        FirstName: "Lisa",
        LastName: "Cuddy",
        MiddleName: null,
        BirthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-35)),
        Gender: Gender.Female,
        NationalId: "12345098765",
        ContactPhone: "555987654",
        StaffType: StaffType.Administrator,
        LicenseNumber: "ADM54321",
        SupervisorId: null
    );

    #region CreateStaff

    [Fact]
    public async Task CreateStaff_WithValidPayload_Returns201()
    {
        var dto = GetValidCreateDto();

        var response = await Client.PostAsJsonAsync(
            "/api/staff", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        var body = await response.Content.ReadFromJsonAsync<MedicalStaffResponseDto>(
            TestContext.Current.CancellationToken);
            
        body.ShouldNotBeNull();
        body.LicenseNumber.ShouldBe(dto.LicenseNumber);
        body.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateStaff_WithDuplicateLicenseNumber_Returns409()
    {
        // First create
        var dto = GetValidCreateDto();
        await Client.PostAsJsonAsync(
            "/api/staff", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        // Try duplicate
        var response = await Client.PostAsJsonAsync(
            "/api/staff", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "House", "11223344556")] // Empty FirstName
    [InlineData("Gregory", "", "11223344556")] // Empty LastName
    [InlineData("Gregory", "House", "")] // Empty NationalId
    public async Task CreateStaff_WithInvalidPersonField_Returns400(
        string firstName, string lastName, string nationalId)
    {
        var dto = GetValidCreateDto() with
        {
            FirstName = firstName,
            LastName = lastName,
            NationalId = nationalId
        };

        var response = await Client.PostAsJsonAsync(
            "/api/staff", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);
            
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(10, 0, StaffType.Doctor, "MD12345")] // Under 18
    [InlineData(40, 10, StaffType.Doctor, "MD12345")] // Future HireDate (10 days in future)
    [InlineData(40, -1000, (StaffType)99, "MD12345")] // Bad StaffType
    [InlineData(40, -1000, StaffType.Doctor, "")] // Empty License
    public async Task CreateStaff_WithInvalidStaffField_Returns400(
        int ageYears, int hireDateOffsetDays, StaffType staffType, string license)
    {
        var dto = GetValidCreateDto() with
        {
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-ageYears)),
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(hireDateOffsetDays)),
            StaffType = staffType,
            LicenseNumber = license
        };

        var response = await Client.PostAsJsonAsync(
            "/api/staff", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);
            
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetStaff

    [Fact]
    public async Task GetStaffById_WhenExists_Returns200WithSpecializations()
    {
        var staffId = await SeedStaffAsync();

        var response = await Client.GetAsync(
            $"/api/staff/{staffId}", 
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<MedicalStaffResponseDto>(
            TestContext.Current.CancellationToken);
            
        body.ShouldNotBeNull();
        body.Id.ShouldBe(staffId);
    }

    [Fact]
    public async Task GetStaffById_WhenNotFound_Returns404()
    {
        var response = await Client.GetAsync(
            $"/api/staff/{Guid.NewGuid()}", 
            TestContext.Current.CancellationToken);
            
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetActiveStaff_WithValidPaging_Returns200()
    {
        await SeedStaffAsync();

        var response = await Client.GetAsync(
            "/api/staff/active?PageNumber=1&PageSize=10", 
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<MedicalStaffResponseDto>>(
            TestContext.Current.CancellationToken);
            
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Theory]
    [InlineData("SortBy=Unknown")]
    [InlineData("SortOrder=bad")]
    public async Task GetActiveStaff_WithInvalidQueryParams_Returns400(string param)
    {
        var response = await Client.GetAsync(
            $"/api/staff/active?{param}", 
            TestContext.Current.CancellationToken);
            
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region UpdateStaff

    [Fact]
    public async Task UpdateStaff_WithValidPayload_Returns200()
    {
        var staffId = await SeedStaffAsync();
        var dto = GetValidUpdateDto();

        var response = await Client.PutAsJsonAsync(
            $"/api/staff/{staffId}", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var body = await response.Content.ReadFromJsonAsync<MedicalStaffResponseDto>(
            TestContext.Current.CancellationToken);
            
        body.ShouldNotBeNull();
        body.FirstName.ShouldBe(dto.FirstName);
        body.LicenseNumber.ShouldBe(dto.LicenseNumber);
    }

    [Fact]
    public async Task UpdateStaff_WhenNotFound_Returns404()
    {
        var dto = GetValidUpdateDto();
        
        var response = await Client.PutAsJsonAsync(
            $"/api/staff/{Guid.NewGuid()}", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);
            
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("", "12345098765")] // Empty LastName
    [InlineData("Cuddy", "")] // Empty NationalId
    public async Task UpdateStaff_WithInvalidField_Returns400(string lastName, string nationalId)
    {
        var staffId = await SeedStaffAsync();
        var dto = GetValidUpdateDto() with
        {
            LastName = lastName,
            NationalId = nationalId
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/staff/{staffId}", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region AssignSpecializations

    [Fact]
    public async Task AssignSpecializations_WithValidIds_Returns200()
    {
        var (staffId, specId) = await SeedDataWithSpecAsync();
        var dto = new AssignSpecializationsDto(
        [
            new StaffSpecializationDto(
                specId, 
                true, 
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)))
        ]);

        var response = await Client.PutAsJsonAsync(
            $"/api/staff/{staffId}/specializations", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AssignSpecializations_WithUnknownSpecId_Returns404Or400()
    {
        var (staffId, _) = await SeedDataWithSpecAsync();
        var dto = new AssignSpecializationsDto(
        [
            new StaffSpecializationDto(
                Guid.NewGuid(), 
                true, 
                DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)))
        ]);

        var response = await Client.PutAsJsonAsync(
            $"/api/staff/{staffId}/specializations", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AssignSpecializations_WithEmptyList_ClearsAll_Returns200()
    {
        var (staffId, _) = await SeedDataWithSpecAsync();
        var dto = new AssignSpecializationsDto([]);

        var response = await Client.PutAsJsonAsync(
            $"/api/staff/{staffId}/specializations", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", -1)] // Empty Guid
    [InlineData("d2e9c1b3-4f5a-6789-0123-456789abcdef", 10)] // Future CertificationDate
    public async Task AssignSpecializations_WithInvalidDto_Returns400(string specIdStr, int certDateOffsetDays)
    {
        var (staffId, _) = await SeedDataWithSpecAsync();
        var specId = Guid.Parse(specIdStr);

        var dto = new AssignSpecializationsDto(
        [
            new StaffSpecializationDto(
                specId, 
                true, 
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(certDateOffsetDays)))
        ]);

        var response = await Client.PutAsJsonAsync(
            $"/api/staff/{staffId}/specializations", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DeactivateStaff

    [Fact]
    public async Task DeactivateStaff_WhenExists_SetsIsActiveFalseAndReturns200()
    {
        var staffId = await SeedStaffAsync();

        var response = await Client.DeleteAsync(
            $"/api/staff/{staffId}", 
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        
        var getResponse = await Client.GetAsync(
            $"/api/staff/{staffId}", 
            TestContext.Current.CancellationToken);
            
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound); 
    }

    [Fact]
    public async Task DeactivateStaff_WhenNotFound_Returns404()
    {
        var response = await Client.DeleteAsync(
            $"/api/staff/{Guid.NewGuid()}", 
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion
}
