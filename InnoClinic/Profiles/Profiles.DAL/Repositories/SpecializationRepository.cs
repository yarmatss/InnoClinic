using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;

namespace Profiles.DAL.Repositories;

public class SpecializationRepository(ProfilesDbContext context) : 
    BaseRepository<Specialization>(context), ISpecializationRepository
{
}
