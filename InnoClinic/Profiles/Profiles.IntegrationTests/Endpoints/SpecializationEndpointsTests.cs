using Profiles.API.DTOs.Specialization;
using Profiles.Domain.Common;
using Profiles.IntegrationTests.Infrastructure;
using Profiles.Tests.Common.Fakes.Entities;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Profiles.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class SpecializationEndpointsTests(PostgresContainerFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    private async Task<Guid> SeedSpecializationAsync()
    {
        var spec = new SpecializationFaker().Generate();
        await using var ctx = DbFixture.CreateDbContext();
        ctx.Specializations.Add(spec);
        await ctx.SaveChangesAsync(TestContext.Current.CancellationToken);
        return spec.Id;
    }

    private CreateSpecializationDto GetValidCreateDto() => new(
        Name: "Cardiology",
        Code: "CARD-001"
    );

    private UpdateSpecializationDto GetValidUpdateDto() => new(
        Name: "Neurology",
        Code: "NEURO-01"
    );

    #region CreateSpecialization

    [Fact]
    public async Task CreateSpecialization_WithValidPayload_Returns201AndLocationHeader()
    {
        var dto = GetValidCreateDto();

        var response = await Client.PostAsJsonAsync(
            "/api/specializations", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    public static TheoryData<string> InvalidNameData =>
    [
        "", // Empty Name
        new string('x', 101) // Very long Name, assuming 101+ characters is invalid
    ];

    [Theory]
    [MemberData(nameof(InvalidNameData))]
    public async Task CreateSpecialization_WithInvalidName_Returns400(string name)
    {
        var dto = GetValidCreateDto() with { Name = name };

        var response = await Client.PostAsJsonAsync(
            "/api/specializations", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetSpecialization

    [Fact]
    public async Task GetAllSpecializations_WithValidPaging_Returns200()
    {
        await SeedSpecializationAsync();

        var response = await Client.GetAsync(
            "/api/specializations?PageNumber=1&PageSize=10", 
            TestContext.Current.CancellationToken);

        var body = await response.Content.ReadFromJsonAsync<PagedResponse<SpecializationResponseDto>>(
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
    }

    #endregion

    #region UpdateSpecialization

    [Fact]
    public async Task UpdateSpecialization_WithValidPayload_Returns200()
    {
        var specId = await SeedSpecializationAsync();
        var dto = GetValidUpdateDto();

        var response = await Client.PutAsJsonAsync(
            $"/api/specializations/{specId}", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateSpecialization_WhenNotFound_Returns404()
    {
        var dto = GetValidUpdateDto();

        var response = await Client.PutAsJsonAsync(
            $"/api/specializations/{Guid.NewGuid()}", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSpecialization_WithInvalidName_Returns400()
    {
        var specId = await SeedSpecializationAsync();
        var dto = GetValidUpdateDto() with { Name = "" };

        var response = await Client.PutAsJsonAsync(
            $"/api/specializations/{specId}", 
            dto, 
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion
}
