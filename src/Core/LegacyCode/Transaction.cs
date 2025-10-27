namespace ClubExample.Core.LegacyCode;



public abstract class Transaction : IDisposable
{
    public abstract void Begin();
    public abstract Task CommitAsync();
    public abstract Task RollbackAsync();

    public abstract void Dispose();
}
