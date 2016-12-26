using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using XFBasicWebClient.Models;

namespace XFBasicWebClient.Views
{
    public partial class DetailPage : ContentPage
    {
        Person _person;

        public DetailPage(Person person)
        {
            InitializeComponent();

            // 引数で受け取ったpersonがなければ新規作成します。
            if (person != null)
                _person = person;
            else
                _person = new Person();

            // _personをバインディング対象にします。
            this.BindingContext = _person;

            // このページの情報を取得してローカル、Web APIのデータを更新します。
            SaveButton.Clicked += async (sender, e) =>
            {
                var updatePerson = new Person
                {
                    Id = int.Parse(IdData.Text),
                    Name = NameData.Text,
                    Birthday = BirthdayData.Date
                };

                if (updatePerson.Id == 0)
                {
                    var id = await WebApiClient.Instance.PostPersonAsync(updatePerson);
                    updatePerson.Id = id;
                    await PeopleManager.UpsertPersonAsync(updatePerson);
                }
                else
                {
                    var webUpdate = WebApiClient.Instance.UpdatePersonAsync(updatePerson);
                    var localUpdate = PeopleManager.UpsertPersonAsync(updatePerson);
                    await Task.WhenAll(webUpdate, localUpdate);
                }

                await Navigation.PopAsync();
            };

            // Idを指定してデータを削除します。
            DeleteButton.Clicked += async (sender, e) =>
            {
                await WebApiClient.Instance.DeletePersonAsync(_person);
                await Navigation.PopAsync();
            };
        }
    }
}
