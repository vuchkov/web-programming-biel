using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GM.Models;

namespace GM.Client.Data
{
    public class MockGrocerySimpleData : IGroceryDataAccess
    {
        private readonly List<Grocery> _database = new List<Grocery>(new Grocery[]
{
            new Grocery
            {
                Id = 1,
                IsExpire = true,
                Name = "Bread",
                ExpireOn = DateTime.UtcNow.AddDays(-1),
                CreatedOn = DateTime.UtcNow.AddDays(-2)
            },
            new Grocery
            {
                Id = 2,
                IsExpire = false,
                Name = "Milk"
            }
        });

        public Task<bool> DeleteAsync(Grocery item)
        {
            var delete = _database.Where(Grocery => Grocery.Id == item.Id).FirstOrDefault();
            if (delete != null)
            {
                _database.Remove(delete);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<IEnumerable<Grocery>> GetAsync(bool showAll, bool sortByCreatedOn, bool sortByExpiredOn)
        {
            IQueryable<Grocery> query = _database.AsQueryable();
            if (sortByCreatedOn == true)
            {
                sortByExpiredOn = false;
            }
            if (!showAll)
            {
                query = query.Where(Grocery => !Grocery.IsExpire);
            }
            if (showAll && sortByExpiredOn)
            {
                query = query.OrderBy(Grocery => -(Grocery.ExpireOn.HasValue ?
                    Grocery.ExpireOn.Value.Ticks : -(long.MaxValue - Grocery.CreatedOn.Ticks)));
            }
            else if (sortByCreatedOn)
            {
                query = query.OrderBy(Grocery => -Grocery.CreatedOn.Ticks);
            }
            else
            {
                query = query.OrderBy(Grocery => Grocery.Name);
            }
            return Task.FromResult(query.AsEnumerable());
        }

        public Task<Grocery> GetAsync(int id)
        {
            return Task.FromResult(_database.Where(item => item.Id == id).FirstOrDefault());
        }

        public Task<Grocery> AddAsync(Grocery item)
        {
            var results = new List<ValidationResult>();
            var validation = new ValidationContext(item);
            if (Validator.TryValidateObject(item, validation, results))
            {
                item.Id = _database.Max(Grocery => Grocery.Id) + 1;
                _database.Add(item);
                return Task.FromResult(item);
            }
            else
            {
                throw new ValidationException();
            }
        }

        public Task<Grocery> UpdateAsync(Grocery item)
        {
            var results = new List<ValidationResult>();
            var validation = new ValidationContext(item);
            if (Validator.TryValidateObject(item, validation, results))
            {
                var dbItem = _database.Where(Grocery => Grocery.Id == item.Id).First();
                if (!dbItem.IsExpire && item.IsExpire)
                {
                    dbItem.MarkAsExpire();
                }
                dbItem.Name = item.Name;
                return Task.FromResult(dbItem);
            }
            else
            {
                throw new ValidationException();
            }
        }
    }
}