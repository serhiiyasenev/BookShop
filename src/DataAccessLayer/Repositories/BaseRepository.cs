namespace DataAccessLayer.Repositories
{
    public abstract class BaseDbRepository
    {
        protected readonly EfCoreContext _dbContext;
        protected BaseDbRepository(EfCoreContext efContext)
        {
            _dbContext = efContext;
        }
    }
}
