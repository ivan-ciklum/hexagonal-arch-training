using ClubExample.Core.Domain;

namespace ClubExample.Core.LegacyCode;

public class ClubDal
{
    public Task<Club?> GetByIdAsync(Guid clubId) 
        => throw new NotImplementedException();
}