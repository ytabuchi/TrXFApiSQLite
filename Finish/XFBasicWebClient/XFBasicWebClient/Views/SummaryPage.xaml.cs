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
        //ObservableCollection<Person> listData = new ObservableCollection<Person>();

        public SummaryPage()
        {
            InitializeComponent();
            var vm = new ViewModels.SummayPageViewModel();
            this.BindingContext = vm;
            //this.BindingContext = listData;

            //addButton.Clicked += async (sender, e) =>
            //{
            //    //await Navigation.PushAsync(new DetailPage(null));
            //    var p = new Person
            //    {
            //        Name = "sample",
            //        Birthday = DateTime.Now
            //    };
            //    await WebApiClient.Instance.PostPersonAsync(p);

            //    var webPeople = await WebApiClient.Instance.GetPeopleAsync();
            //    listData.Clear();
            //    foreach (var person in webPeople)
            //    {
            //        listData.Add(person);
            //    }
            //};

            //clearButton.Clicked += async (sender, e) =>
            //{
            //    await PeopleManager.DeletePeopleAsync();
            //    var people = await WebApiClient.Instance.GetPeopleAsync();
            //    foreach (var person in people)
            //    {
            //        await WebApiClient.Instance.DeletePersonAsync(person);
            //    }

            //    listData.Clear();
            //};

            //peopleList.Refreshing += async (sender, e) =>
            //{
            //    // WebAPIから全データを取得して、SQLiteのデータ数と違ったらSQLiteをアップデートします。
            //    var webPeople = await WebApiClient.Instance.GetPeopleAsync();
            //    if (!await PeopleManager.CheckPersonCountAsync(webPeople))
            //        await PeopleManager.UpdateLocalDataAsync(webPeople);

            //    // SQLiteから全データを持ってきてListに。
            //    listData.Clear();
            //    foreach (var item in await PeopleManager.GetPeopleAsync())
            //    {
            //        listData.Add(item);
            //    }

            //    peopleList.IsRefreshing = false;
            //};

            //peopleList.ItemSelected += async (object sender, SelectedItemChangedEventArgs e) =>
            //{
            //    // 選択されたPersonをDetailPageの引数で渡します。
            //    var person = e.SelectedItem as Person;
            //    if (person == null)
            //        return;
                
            //    await Navigation.PushAsync(new DetailPage(person));
            //    peopleList.SelectedItem = null;
            //};
        }

        //protected override async void OnAppearing()
        //{
        //    base.OnAppearing();

        //    indicator.IsVisible = true;

        //    // WebApiから現在の情報を取得して、登録されているItem数が異なればSQliteをアップデートします。
        //    var webPeople = await WebApiClient.Instance.GetPeopleAsync();
        //    if (!await PeopleManager.CheckPersonCountAsync(webPeople))
        //        await PeopleManager.UpdateLocalDataAsync(webPeople);

        //    // SQLiteから全データを持ってきてListに流し込みます。
        //    listData.Clear();
        //    var people = await PeopleManager.GetPeopleAsync();
        //    foreach (var person in people)
        //    {
        //        listData.Add(person); 
        //    }

        //    indicator.IsVisible = false;
        //}
    }
}
