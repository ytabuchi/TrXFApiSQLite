using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace XFBasicWebClient.Models
{
    public class WebApiClient
    {
        /// <summary>
        /// Singlton instance.
        /// </summary>
        public static WebApiClient Instance { get; set; } = new WebApiClient();

        private Uri baseAddress = Helpers.ApiKeys.baseAddress;
        private string Token = "";
        private object locker = new object();

        // 通常は最初にログイン画面を出してユーザー名、パスワードを入力させます。
        private readonly string _name = "admin";
        private readonly string _password = "p@ssw0rd";

        // シングルトンにする場合は、自身にコレクションを保持しても良いでしょう。
        //public ObservableCollection<Person> Persons { get; set; } = new ObservableCollection<Person>();

        /// <summary>
        /// Private Constructor for showing just one instance.
        /// </summary>
        private WebApiClient()
        {
        }

        /// <summary>
        /// WebAPIにアクセスしてTokenを取得します。
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="password">Password.</param>
        private void Initialize(string name, string password)
        {
            // スレッドをロックしてTokenがなければ取得します。
            // 通常はこのタイミングでTokenの有効期限などもチェックします。
            lock (locker)
            {
                if (string.IsNullOrEmpty(Token))
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = baseAddress;

                            // Tokenを入手する処理です。
                            var authContent = new StringContent($"grant_type=password&username={name}&password={password}");
                            authContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                            var authResponse = client.PostAsync("/Token", authContent).Result;
                            authResponse.EnsureSuccessStatusCode();
                            var authResult = authResponse.Content.ReadAsStringAsync().Result;

                            Token = JsonConvert.DeserializeObject<AuthResult>(authResult).AccessToken;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"【InitializeError】{ex.Source},{ex.Message},{ex.InnerException}");
                    }
                }
            }

        }

        /// <summary>
        /// PersonをGETします。Personのコレクションを返します。
        /// </summary>
        /// <returns>ObservableCollection&lt;Persion&gt;</returns>
        public async Task<ObservableCollection<Person>> GetPeopleAsync()
        {
            // 内部的にInitializeを呼んでいます。
            Initialize(_name, _password);

            using (var client = new HttpClient())
            {
                client.BaseAddress = baseAddress;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                try
                {
                    var response = await client.GetAsync("api/People");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ObservableCollection<Person>>(json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"【GetError】{ex.Source},{ex.Message},{ex.InnerException}");

                    return null;
                }
            }
        }

        /// <summary>
        /// PersonをPOSTします。追加した際のIdを返します。
        /// </summary>
        /// <returns>WebAPI側で登録されたId</returns>
        /// <param name="person">Person</param>
        public async Task<int> PostPersonAsync(Person person)
        {
            // 内部的にInitializeを呼んでいます。
            Initialize(_name, _password);

            using (var client = new HttpClient())
            {
                client.BaseAddress = baseAddress;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                try
                {
                    var content = new StringContent(JsonConvert.SerializeObject(person));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.PostAsync("api/People", content);
                    response.EnsureSuccessStatusCode();

                    // ResultにWebAPIで登録したIdを取得できます。
                    var result = await response.Content.ReadAsStringAsync();
                    var id = JsonConvert.DeserializeObject<Person>(result).Id;

                    return id;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"【PostError】{ex.Source},{ex.Message},{ex.InnerException}");
                    throw;
                }
            }
        }

        /// <summary>
        /// 既存のPersonをPUT(Update)します。bool値を返します。
        /// </summary>
        /// <returns>bool</returns>
        /// <param name="person">Person</param>
        public async Task<bool> UpdatePersonAsync(Person person)
        {
            // 内部的にInitializeを呼んでいます。
            Initialize(_name, _password);

            using (var client = new HttpClient())
            {
                client.BaseAddress = baseAddress;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                try
                {
                    var content = new StringContent(JsonConvert.SerializeObject(person));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.PutAsync($"api/People/{person.Id}", content);
                    response.EnsureSuccessStatusCode();

                    return true; 
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"【UpdateError】{ex.Source},{ex.Message},{ex.InnerException}");

                    return false; 
                }
            }
        }

        /// <summary>
        /// 引数で渡されたPersonを削除します。bool値を返します。
        /// </summary>
        /// <returns>bool</returns>
        /// <param name="person">Person</param>
        public async Task<bool> DeletePersonAsync(Person person)
        {
            Initialize(_name, _password);

            using (var client = new HttpClient())
            {
                client.BaseAddress = baseAddress;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                try
                {
                    var response = await client.DeleteAsync($"api/People/{person.Id}");
                    response.EnsureSuccessStatusCode();

                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"【DeleteError】{ex.Source},{ex.Message},{ex.InnerException}");

                    return false;
                }
            }
        }
    }

    class AuthResult
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
    }
}
