using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using GM.Models;

namespace GM.Client.Data
{
    public class GrocerySimpleData : IGroceryDataAccess
    {
        private readonly IODataClient _client;
        public GrocerySimpleData(HttpClient client)
        {
            client.BaseAddress = new Uri("http://localhost:49248/odata");
            var setting = new ODataClientSettings(client);
            _client = new ODataClient(setting);
        }
        public async Task<Grocery> AddAsync(Grocery item)
        {
            var results = new List<ValidationResult>();
            var validation = new ValidationContext(item);
            if (Validator.TryValidateObject(item, validation, results))
            {
                return await _client.For<Grocery>().Set(item).InsertEntryAsync();
            }
            else 
            {
                throw new ValidationException();
            }
        }

        public async Task<bool> DeleteAsync(Grocery item)
        {
            await _client.For<Grocery>().Key(item.Id).DeleteEntryAsync();
            return true;
        }

        public async Task<IEnumerable<Grocery>> GetAsync(bool showAll, bool sortByCreatedOn, bool sortByCompletedOn)
        {
            var helper = _client.For<Grocery>();
            if (!showAll)
            {
                helper.Filter(w => !w.IsExpire);
            }
            else if (showAll && sortByCompletedOn)
            {
                helper.OrderByDescending(w => w.ExpireOn)
                    .ThenByDescending(w => w.CreatedOn);
            }
            else if (sortByCreatedOn)
            {
                helper.OrderByDescending(w => w.CreatedOn);
            }
            else
            {
                helper.OrderBy(w => w.Name);
            }
            
            return await helper.FindEntriesAsync();
        }

        public async Task<Grocery> UpdateAsync(Grocery item)
        {
            var results = new List<ValidationResult>();
            var validation = new ValidationContext(item);
            if (Validator.TryValidateObject(item, validation, results))
            {
                await _client.For<Grocery>().Key(item.Id).Set(item).UpdateEntryAsync();
                return item;
            }
            else 
            {
                throw new ValidationException();
            }
        }
    }
}
