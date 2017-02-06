using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using XFBasicWebClient.Models;

namespace XFBasicWebClient.Views
{
    public partial class SummaryPage : ContentPage
    {
        ObservableCollection<Person> listData = new ObservableCollection<Person>();

        public SummaryPage()
        {
            InitializeComponent();
            this.BindingContext = listData;

            addButton.Clicked += async (sender, e) =>
            {
                var p = new Person
                {
                    Name = "sample",
                    Birthday = DateTime.Now
                };
                await WebApiClient.Instance.PostPersonAsync(p);

                var webPeople = await WebApiClient.Instance.GetPeopleAsync();
                listData.Clear();
                foreach (var person in webPeople)
                {
                    listData.Add(person);
                }
            };

            clearButton.Clicked += async (sender, e) =>
            {
                var people = await WebApiClient.Instance.GetPeopleAsync();
                foreach (var person in people)
                {
                    await WebApiClient.Instance.DeletePersonAsync(person);
                }

                listData.Clear();
            };

            peopleList.Refreshing += async (sender, e) =>
            {
                var webPeople = await WebApiClient.Instance.GetPeopleAsync();

                listData.Clear();
                foreach (var person in await PeopleManager.GetPeopleAsync())
                {
                    listData.Add(person);
                }

                peopleList.IsRefreshing = false;
            };

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var webPeople = await WebApiClient.Instance.GetPeopleAsync();

            listData.Clear();
            foreach (var person in webPeople)
            {
                listData.Add(person); 
            }
        }
    }
}
