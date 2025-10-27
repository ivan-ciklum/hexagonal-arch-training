using ClubExample.Core.Domain;

namespace ClubExample.Core.LegacyCode;

public class MemberDal
{
    public Task InsertAsync(Member member)
        => throw new NotImplementedException();
    public Task<Member?> GetByName(string memberName, Guid clubId)
        => throw new NotImplementedException();
}