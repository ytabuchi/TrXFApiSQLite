using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using XFBasicWebClient.Models;

namespace XFBasicWebClient.ViewModels
{
    public class SummaryPageViewModel : ViewModelBase
    {
        public ObservableCollection<Person> PeopleData { get; set; } = new ObservableCollection<Person>();

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                AddCommand.ChangeCanExecute();
                ClearCommand.ChangeCanExecute();
                RefreshCommand.ChangeCanExecute();
            }
        }

        public Command AddCommand { get; private set; }
        public Command ClearCommand { get; private set; }
        public Command RefreshCommand { get; private set; }

        public SummaryPageViewModel()
        {
            this.AddCommand = new Command(
                async () => await AddPerson(),
                () => !IsBusy);

            this.ClearCommand = new Command(
                async () => await ClearPeople(),
                () => !IsBusy);

            this.RefreshCommand = new Command(
                async () => await Refresh(),
                () => !IsBusy);
        }

        private async Task AddPerson()
        {
            IsBusy = true;

            var person = new Person
            {
                Name = "sample",
                Birthday = DateTime.Now
            };
            await WebApiClient.Instance.PostPersonAsync(person);

            var people = await WebApiClient.Instance.GetPeopleAsync();
            PeopleData.Clear();
            foreach (var p in people)
            {
                PeopleData.Add(p);
            }

            IsBusy = false;
        }

        private async Task ClearPeople()
        {
            IsBusy = true;

            var people = await WebApiClient.Instance.GetPeopleAsync();
            foreach (var p in people)
            {
                await WebApiClient.Instance.DeletePersonAsync(p);
            }
            PeopleData.Clear();

            IsBusy = false;
        }

        private async Task Refresh()
        {
            IsBusy = true;

            var people = await WebApiClient.Instance.GetPeopleAsync();
            PeopleData.Clear();
            foreach (var p in people)
            {
                PeopleData.Add(p);
            }

            IsBusy = false;
        }
    }
}
