using Profiles.DAL.Data;
using Profiles.DAL.Entities;
using Profiles.DAL.Interfaces;

namespace Profiles.DAL.Repositories;

public class MedicalStaffRepository(ProfilesDbContext context) : 
    BaseRepository<MedicalStaff>(context), IMedicalStaffRepository
{
}
