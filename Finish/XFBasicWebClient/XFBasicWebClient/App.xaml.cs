using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XFBasicWebClient.Views;

using Xamarin.Forms;

namespace XFBasicWebClient
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // App.xamlの設定で、AppThemeが読み込まれます。
            MainPage = new NavigationPage(new SummaryPage());
        }

        protected override void OnStart()
        {

        }

        protected override async void OnSleep()
        {
            // アプリ終了時にローカルSQLiteとWebのデータを確認してPerson.Nameが違うアイテムだけをポストします。
            var localPeople = await Models.PeopleManager.GetPeopleAsync();
            var webPeopoe = await Models.WebApiClient.Instance.GetPeopleAsync();

            if (webPeopoe.Count == 0)
            {
                // Web API側にデータがない場合は、すべてアップロードします。
                foreach (var l in localPeople)
                {
                    await Models.WebApiClient.Instance.PostPersonAsync(l);
                }
            }
            else
            {
                // Web API側に含まれないものだけを抽出しアップロードします。
                var hash = new HashSet<string>(webPeopoe.Select(n => n.Name));
                var diff = localPeople.Where(n => hash.Contains(n.Name) == false).ToArray();

                foreach (var item in diff)
                {
                    var data = localPeople.FirstOrDefault(x => x.Name == item.Name);
                    await Models.WebApiClient.Instance.PostPersonAsync(data);
                }
            }
        }

        protected override void OnResume()
        {

        }
    }
}
