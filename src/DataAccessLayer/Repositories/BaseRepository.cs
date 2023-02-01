using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public abstract class BaseDbRepository
    {
        protected readonly EfCoreContext dbContext;
        protected BaseDbRepository(EfCoreContext efContext)
        {
            dbContext = efContext;
        }
    }
}
