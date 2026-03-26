using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;

namespace Profiles.DAL.Repositories;

public class PatientRepository(ProfilesDbContext context) : 
    BaseRepository<Patient>(context), IPatientRepository
{
}
