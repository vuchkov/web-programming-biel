using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GM.Client.Data;
using GM.Models;

namespace GM.Client.ViewModels
{
    public class GroceryViewModel : INotifyPropertyChanged
    {
        private readonly IGroceryDataAccess _dataAccess;
        public readonly Grocery _newGrocery = new Grocery();

        public GroceryViewModel(IGroceryDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public List<string> Errors { get; } = new List<string>();

        public bool ValidationErrors
        {
            get { return Errors.Any(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropChange(string property, bool includeGroceries = false)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if (includeGroceries)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroceryAsync)));
            }
        }

        public async Task<IEnumerable<Grocery>> GroceryAsync()
        {
            return await _dataAccess.GetAsync(showAll, sortByCreatedOn, sortByExpiredOn);
        }

        public string newName = null;
        public string NewName
        {
            get => newName;
            set
            {
                newName = value;
                _newGrocery.Name = value;
                var results = new List<ValidationResult>();
                var validation = new ValidationContext(_newGrocery);
                Errors.Clear();
                if (!Validator.TryValidateObject(_newGrocery, validation, results))
                {
                    foreach (var result in results)
                    {
                        Errors.Add(result.ErrorMessage);
                    }
                    RaisePropChange(nameof(Errors));
                }
                RaisePropChange(nameof(NewName));
            }
        }

        public async Task AddNewAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewName) && !ValidationErrors)
            {
                var newItem = new Grocery { Name = NewName };
                StartAsyncOperation();
                await _dataAccess.AddAsync(newItem);
                EndAsyncOperation();
                newName = string.Empty;
                RaisePropChange(nameof(GroceryAsync));
            }
        }

        public async Task DeleteAsync(Grocery grocery)
        {
            StartAsyncOperation();
            await _dataAccess.DeleteAsync(grocery);
            EndAsyncOperation();
            RaisePropChange(nameof(GroceryAsync));
        }

        public async Task MarkGroceryAsExpiredAsync(Grocery grocery)
        {
            grocery.MarkAsExpire();
            StartAsyncOperation();
            await _dataAccess.UpdateAsync(grocery);
            EndAsyncOperation();
            RaisePropChange(nameof(GroceryAsync));
        }

        #region markers
        private bool showAll;
        public bool ShowAll
        {
            get => showAll;
            set {
                if (value != ShowAll)
                {
                    showAll = value;
                    RaisePropChange(nameof(ShowAll), true);
                }
            }

        }

        private bool sortByExpiredOn;
        public bool SortByExpiredOn
        {
            get => sortByExpiredOn;
            set
            {
                if (value != SortByExpiredOn)
                {
                    sortByExpiredOn = value;
                    RaisePropChange(nameof(SortByExpiredOn), true);
                }
            }

        }

        private bool sortByCreatedOn;
        public bool SortByCreatedOn
        {
            get => sortByCreatedOn;
            set
            {
                if (value != SortByCreatedOn)
                {
                    sortByCreatedOn = value;
                    RaisePropChange(nameof(SortByCreatedOn), true);
                }
            }

        }
        #endregion

        #region async Loader
        public int asyncCount = 0;
        public bool Loading
        {
            get => asyncCount > 0;
        }

        private void StartAsyncOperation()
        {
            var cur = Loading;
            asyncCount++;
            if (cur != Loading)
            {
                RaisePropChange(nameof(Loading));
            }
        }

        private void EndAsyncOperation()
        {
            var cur = Loading;
            asyncCount--;
            if (cur != Loading)
            {
                RaisePropChange(nameof(Loading));
            }
        }
        #endregion
    }
}
