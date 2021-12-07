using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GM.Models;

namespace GM.Client.Data
{
    public interface IGroceryDataAccess
    {
        Task<Grocery> AddAsync(Grocery item);
        Task<bool> DeleteAsync(Grocery item);
        Task<IEnumerable<Grocery>> GetAsync(bool showAll, bool sortByCreatedOn, bool sortByExpiredOn);
        Task<Grocery> UpdateAsync(Grocery item);
    }
}
