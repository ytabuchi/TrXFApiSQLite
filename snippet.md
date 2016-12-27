### コードスニペット

Webサイトまたはmarkdownプレビューとして表示している場合は、それぞれの枠内をコピー＆ペーストしてください。

## Web Api

### Person.cs

```csharp
[PrimaryKey]
//[AutoIncrement] // 自動採番をオンにします。
[JsonProperty(PropertyName = "id")]
public int Id { get; set; }
[JsonProperty(PropertyName = "name")]
public string Name { get; set; }
[JsonProperty(PropertyName = "birthday")]
public DateTimeOffset Birthday { get; set; }
```

### WebApiClient.cs

```csharp
class AuthResult
{
    [JsonProperty(PropertyName = "access_token")]
    public string AccessToken { get; set; }
}
```

```csharp
public static WebApiClient Instance { get; set; } = new WebApiClient();

private WebApiClient()
{
}
```

```csharp
private Uri baseAddress = Helpers.ApiKeys.baseAddress;
private string Token = "";
private object locker = new object();

// 通常は最初にログイン画面を出してユーザー名、パスワードを入力させます。
private readonly string _name = "admin";
private readonly string _password = "p@ssw0rd";
```

```csharp
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
```

```csharp
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
```

```csharp
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
```

```csharp
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
```

```csharp
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
```

### WebApiClient.cs完成形(現時点)

```csharp
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
```

### SummaryPage.xaml

```xml
<ListView x:Name="peopleList"
          HasUnevenRows="true"
          IsPullToRefreshEnabled="true"
          ItemsSource="{Binding}">
    <ListView.ItemTemplate>
        <DataTemplate>
            <ViewCell>
                <StackLayout Orientation="Horizontal">
                    <Label Style="{DynamicResource LabelStyleId}"
                            Text="{Binding Id}" />
                    <StackLayout Padding="5">
                        <Label Text="{Binding Name}"
                                VerticalTextAlignment="Center" />
                        <Label Text="{Binding Birthday, StringFormat='{0:yyyy/MM/dd HH:mm}'}"
                                TextColor="Gray"
                                VerticalTextAlignment="End" />
                    </StackLayout>
                </StackLayout>
            </ViewCell>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

### SummaryPage.xaml.cs

```csharp
ObservableCollection<Person> listData = new ObservableCollection<Person>();
```

```csharp
this.BindingContext = listData;
```

```csharp
protected override async void OnAppearing()
{
    base.OnAppearing();

    // WebApiから現在の情報を取得します。
    var webPeople = await WebApiClient.Instance.GetPeopleAsync();

    // 全データをListViewに流し込みます。
    listData.Clear();
    foreach (var person in webPeople)
    {
        listData.Add(person);
    }
}
```

### SummaryPage.xaml.cs完成形(現時点)

```csharp
public partial class SummaryPage : ContentPage
{
    ObservableCollection<Person> listData = new ObservableCollection<Person>();

    public SummaryPage()
    {
        InitializeComponent();
        this.BindingContext = listData;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // WebApiから現在の情報を取得します。
        var webPeople = await WebApiClient.Instance.GetPeopleAsync();

        // 全データをListViewに流し込みます。
        listData.Clear();
        foreach (var person in webPeople)
        {
            listData.Add(person);
        }
    }
}
```

### SummaryPage.xaml

```xml
<AbsoluteLayout>
    <ListView x:Name="peopleList"
              AbsoluteLayout.LayoutBounds="0,0,1,1"
              AbsoluteLayout.LayoutFlags="All"
              HasUnevenRows="true"
              IsPullToRefreshEnabled="true"
              ItemsSource="{Binding}">
        <ListView.ItemTemplate>
            <DataTemplate>
                <ViewCell>
                    <StackLayout Orientation="Horizontal">
                        <Label Style="{DynamicResource LabelStyleId}"
                                Text="{Binding Id}" />
                        <StackLayout Padding="5">
                            <Label Text="{Binding Name}"
                                    VerticalTextAlignment="Center" />
                            <Label Text="{Binding Birthday, StringFormat='{0:yyyy/MM/dd HH:mm}'}"
                                    TextColor="Gray"
                                    VerticalTextAlignment="End" />
                        </StackLayout>
                    </StackLayout>
                </ViewCell>
            </DataTemplate>
        </ListView.ItemTemplate>
    </ListView>


</AbsoluteLayout>
```

```xml
<Button x:Name="AddButton"
        Margin="8"
        AbsoluteLayout.LayoutBounds="0,1,0.5,AutoSize"
        AbsoluteLayout.LayoutFlags="PositionProportional,WidthProportional"
        Style="{DynamicResource ButtonStyleTransparent}"
        Text="Add" />
<Button x:Name="ClearButton"
        Margin="8"
        AbsoluteLayout.LayoutBounds="1,1,0.5,AutoSize"
        AbsoluteLayout.LayoutFlags="PositionProportional,WidthProportional"
        Style="{DynamicResource ButtonStyleTransparent}"
        Text="Clear all data" />
```

### SummaryPage.xaml.cs

```csharp
AddButton.Clicked += async (sender, e) =>
{
    await Navigation.PushAsync(new DetailPage(null));
};

ClearButton.Clicked += async (sender, e) =>
{
    var people = await WebApiClient.Instance.GetPeopleAsync();
    foreach (var p in people)
    {
        await WebApiClient.Instance.DeletePersonAsync(p);
    }

    listData.Clear();
};
```

```csharp
peopleList.ItemSelected += async (object sender, SelectedItemChangedEventArgs e) =>
{
    // 選択されたPersonをDetailPageの引数で渡します。
    var person = e.SelectedItem as Person;
    if (person == null)
        return;

    await Navigation.PushAsync(new DetailPage(person));
    peopleList.SelectedItem = null;
};
```

### DetailPage.xaml

```xml
<StackLayout VerticalOptions="FillAndExpand">
    <Grid RowSpacing="8"
          VerticalOptions="FillAndExpand">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="120" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <Label Grid.Row="0"
                Grid.Column="0"
                Text="Id"
                VerticalTextAlignment="Center" />
        <Label x:Name="IdData"
                Grid.Row="0"
                Grid.Column="1"
                Text="{Binding Id}" />
        <Label Grid.Row="1"
                Grid.Column="0"
                Text="Name"
                VerticalTextAlignment="Center" />
        <Entry x:Name="NameData"
                Grid.Row="1"
                Grid.Column="1"
                Text="{Binding Name}" />
        <Label Grid.Row="2"
                Grid.Column="0"
                Text="Birthday"
                VerticalTextAlignment="Center" />
        <DatePicker x:Name="BirthdayData"
                    Grid.Row="2"
                    Grid.Column="1"
                    Date="{Binding Birthday}"
                    Format="yyyy/MM/dd" />
    </Grid>

    <Button x:Name="SaveButton"
            HorizontalOptions="FillAndExpand"
            Style="{DynamicResource ButtonStyleGreen}"
            Text="Save" />

</StackLayout>
```

### DetailPage.xaml.cs

```csharp
// 引数で受け取ったpersonがなければ新規作成します。
if (person != null)
    _person = person;
else
    _person = new Person();

// _personをバインディング対象にします。
this.BindingContext = _person;
```

```csharp
// ItemPageの情報を格納してTodoItemManager.SaveItemを実行します。
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
        await WebApiClient.Instance.PostPersonAsync(updatePerson);
    }
    else
    {
        await WebApiClient.Instance.UpdatePersonAsync(updatePerson);
    }

    await Navigation.PopAsync();
};
```

### DetailPage.xaml.cs完成形(現時点)

```csharp
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

        // このページの情報を取得してWeb APIのデータを更新します。
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
                await WebApiClient.Instance.PostPersonAsync(updatePerson);
            }
            else
            {
                await WebApiClient.Instance.UpdatePersonAsync(updatePerson);
            }

            await Navigation.PopAsync();
        };
    }
}
```

## SQLite

### PeopleManager.cs

```csharp
/// <summary>
/// PCLStorageを使用してSQLiteデータベースへのコネクションを取得します。
/// 取得したコネクションは取得した側で正しくクローズ処理すること。
/// </summary>
/// <returns>SQLiteConnection</returns>
private static async Task<SQLiteConnection> CreateConnectionAsync()
{
    const string DatabaseFileName = "people.db3";
    // PCLStorageを使用して、ルートフォルダを取得します。
    IFolder rootFolder = FileSystem.Current.LocalStorage;
    // DBファイルの存在チェックを行います。
    var result = await rootFolder.CheckExistsAsync(DatabaseFileName);
    if (result == ExistenceCheckResult.NotFound)
    {
        // 存在しなかった場合、新たにDBファイルを作成しテーブルも併せて新規作成します。
        IFile file = await rootFolder.CreateFileAsync(DatabaseFileName, CreationCollisionOption.ReplaceExisting);
        var sqliteConnection = new SQLiteConnection(file.Path);
        sqliteConnection.CreateTable<Person>();
        return sqliteConnection;
    }
    else
    {
        // 存在した場合、そのままコネクションを作成します。
        IFile file = await rootFolder.CreateFileAsync(DatabaseFileName, CreationCollisionOption.OpenIfExists);
        return new SQLiteConnection(file.Path);
    }
}
```

```csharp
/// <summary>
/// 引数で渡されたコレクションとSQLiteに保持しているコレクションの数を比較します。
/// </summary>
/// <returns>数が合っているかどうか</returns>
/// <param name="people">Personのコレクション</param>
public static async Task<bool> CheckPersonCountAsync(ObservableCollection<Person> people)
{
    using (var connection = await CreateConnectionAsync())
    {
        var localPeapleCount = connection.Table<Person>().Count();
        connection.Close();
        return localPeapleCount == people.Count;
    }
}
```

```csharp
/// <summary>
/// SQLiteのTableデータを全消しして、引数で渡されるデータを流し込みします。(サボリ)
/// </summary>
/// <returns>The local data async.</returns>
/// <param name="people">Persons.</param>
public static async Task UpdateLocalDataAsync(ObservableCollection<Person> people)
{
    using (var connection = await CreateConnectionAsync())
    {
        // 本来はデータを一つづつチェックして、更新分を書き換えるなどした方が良いでしょう。
        connection.DeleteAll<Person>();
        connection.InsertAll(people);
        connection.Close();
    }
}
```

```csharp
/// <summary>
/// コネクションを張り、全データを取得します。
/// </summary>
/// <returns>Personのコレクションを返します。</returns>
public static async Task<ObservableCollection<Person>> GetPeopleAsync()
{
    using (var connection = await CreateConnectionAsync())
    {
        var peopleCollection = new ObservableCollection<Person>();
        foreach (var person in connection.Table<Person>())
        {
            // SQLiteがDateTimeOffsetを文字列(Ticks)で保持していて、offset値が無視されるので、ToLocalTime()で再度付与します。
            person.Birthday = person.Birthday.ToLocalTime();
            peopleCollection.Add(person);
        }
        connection.Close();
        return peopleCollection;
    }
}
```

```csharp
/// <summary>
/// コネクションを張り、InsertまたはUpdateします。
/// </summary>
/// <returns></returns>
/// <param name="person">Person</param>
public static async Task UpsertPersonAsync(Person person)
{
    using (var connection = await CreateConnectionAsync())
    {
        if (person.Id != 0)
        {
            connection.Update(person);
        }
        else
        {
            connection.Insert(person);
        }
        connection.Close();
    }
}
```

```csharp
/// <summary>
/// コネクションを張り、Personオブジェクトを指定してDeleteします。
/// </summary>
/// <returns></returns>
/// <param name="person">Person</param>
public static async Task DeletePersonAsync(Person person)
{
    using (var connection = await CreateConnectionAsync())
    {
        connection.Delete(person);
        connection.Close();
    }
}

/// <summary>
/// コネクションを張り、すべてのPersonオブジェクトをDeleteします。
/// </summary>
/// <returns>The people async.</returns>
public static async Task DeletePeopleAsync()
{
    using (var connection = await CreateConnectionAsync())
    {
        connection.DeleteAll<Person>();
        connection.Close();
    }
}
```

### PeopleManager.cs完成形

```csharp
public class PeopleManager
{
    public PeopleManager()
    {

    }

    /// <summary>
    /// PCLStorageを使用してSQLiteデータベースへのコネクションを取得します。
    /// 取得したコネクションは取得した側で正しくクローズ処理すること。
    /// </summary>
    /// <returns>SQLiteConnection</returns>
    private static async Task<SQLiteConnection> CreateConnectionAsync()
    {
        const string DatabaseFileName = "people.db3";
        // ルートフォルダを取得します。
        IFolder rootFolder = FileSystem.Current.LocalStorage;
        // DBファイルの存在チェックを行います。
        var result = await rootFolder.CheckExistsAsync(DatabaseFileName);
        if (result == ExistenceCheckResult.NotFound)
        {
            // 存在しなかった場合、新たにDBファイルを作成しテーブルも併せて新規作成します。
            IFile file = await rootFolder.CreateFileAsync(DatabaseFileName, CreationCollisionOption.ReplaceExisting);
            var sqliteConnection = new SQLiteConnection(file.Path);
            sqliteConnection.CreateTable<Person>();
            return sqliteConnection;
        }
        else
        {
            // 存在した場合、そのままコネクションを作成します。
            IFile file = await rootFolder.CreateFileAsync(DatabaseFileName, CreationCollisionOption.OpenIfExists);
            return new SQLiteConnection(file.Path);
        }
    }

    /// <summary>
    /// 引数で渡されたコレクションとSQLiteに保持しているコレクションの数を比較します。
    /// </summary>
    /// <returns>数が合っているかどうか</returns>
    /// <param name="people">Personのコレクション</param>
    public static async Task<bool> CheckPersonCountAsync(ObservableCollection<Person> people)
    {
        using (var connection = await CreateConnectionAsync())
        {
            var localPeapleCount = connection.Table<Person>().Count();
            connection.Close();
            return localPeapleCount == people.Count;
        }
    }

    /// <summary>
    /// SQLiteのTableデータを全消しして、引数で渡されるデータを流し込みします。(サボリ)
    /// </summary>
    /// <returns>The local data async.</returns>
    /// <param name="people">Persons.</param>
    public static async Task UpdateLocalDataAsync(ObservableCollection<Person> people)
    {
        using (var connection = await CreateConnectionAsync())
        {
            // 本来はデータを一つづつチェックして、更新分を書き換えるなどした方が良いでしょう。
            connection.DeleteAll<Person>();
            connection.InsertAll(people);
            connection.Close();
        }
    }

    /// <summary>
    /// コネクションを張り、全データを取得します。
    /// </summary>
    /// <returns>Personのコレクションを返します。</returns>
    public static async Task<ObservableCollection<Person>> GetPeopleAsync()
    {
        using (var connection = await CreateConnectionAsync())
        {
            var peopleCollection = new ObservableCollection<Person>();
            foreach (var person in connection.Table<Person>())
            {
                // SQLiteがDateTimeOffsetを文字列(Ticks)で保持していて、offset値が無視されるので、ToLocalTime()で再度付与します。
                person.Birthday = person.Birthday.ToLocalTime();
                peopleCollection.Add(person);
            }
            connection.Close();
            return peopleCollection;
        }
    }

    /// <summary>
    /// コネクションを張り、InsertまたはUpdateします。
    /// </summary>
    /// <returns></returns>
    /// <param name="person">Person</param>
    public static async Task UpsertPersonAsync(Person person)
    {
        using (var connection = await CreateConnectionAsync())
        {
            if (person.Id != 0)
            {
                connection.Update(person);
            }
            else
            {
                connection.Insert(person);
            }
            connection.Close();
        }
    }

    /// <summary>
    /// コネクションを張り、Personオブジェクトを指定してDeleteします。
    /// </summary>
    /// <returns></returns>
    /// <param name="person">Person</param>
    public static async Task DeletePersonAsync(Person person)
    {
        using (var connection = await CreateConnectionAsync())
        {
            connection.Delete(person);
            connection.Close();
        }
    }

    /// <summary>
    /// コネクションを張り、すべてのPersonオブジェクトをDeleteします。
    /// </summary>
    /// <returns>The people async.</returns>
    public static async Task DeletePeopleAsync()
    {
        using (var connection = await CreateConnectionAsync())
        {
            connection.DeleteAll<Person>();
            connection.Close();
        }
    }
}
```

### SummaryPage.xaml

```xml
<Frame x:Name="indicator"
        AbsoluteLayout.LayoutBounds="0.5,0.5,AutoSize,AutoSize"
        AbsoluteLayout.LayoutFlags="PositionProportional"
        IsVisible="False">
    <ActivityIndicator IsRunning="True" />
</Frame>
```

### SummaryPage.xaml完成形

```xml
<AbsoluteLayout>
    <ListView x:Name="peopleList"
              AbsoluteLayout.LayoutBounds="0,0,1,1"
              AbsoluteLayout.LayoutFlags="All"
              HasUnevenRows="true"
              IsPullToRefreshEnabled="true"
              ItemsSource="{Binding}">
        <ListView.ItemTemplate>
            <DataTemplate>
                <ViewCell>
                    <StackLayout Orientation="Horizontal">
                        <Label Style="{DynamicResource LabelStyleId}"
                                Text="{Binding Id}" />
                        <StackLayout Padding="5">
                            <Label Text="{Binding Name}"
                                    VerticalTextAlignment="Center" />
                            <Label Text="{Binding Birthday, StringFormat='{0:yyyy/MM/dd HH:mm}'}"
                                    TextColor="Gray"
                                    VerticalTextAlignment="End" />
                        </StackLayout>
                    </StackLayout>
                </ViewCell>
            </DataTemplate>
        </ListView.ItemTemplate>
    </ListView>

    <Button x:Name="AddButton"
            Margin="8"
            AbsoluteLayout.LayoutBounds="0,1,0.5,AutoSize"
            AbsoluteLayout.LayoutFlags="PositionProportional,WidthProportional"
            Style="{DynamicResource ButtonStyleTransparent}"
            Text="Add" />
    <Button x:Name="ClearButton"
            Margin="8"
            AbsoluteLayout.LayoutBounds="1,1,0.5,AutoSize"
            AbsoluteLayout.LayoutFlags="PositionProportional,WidthProportional"
            Style="{DynamicResource ButtonStyleTransparent}"
            Text="Clear all data" />

    <Frame x:Name="indicator"
            AbsoluteLayout.LayoutBounds="0.5,0.5,AutoSize,AutoSize"
            AbsoluteLayout.LayoutFlags="PositionProportional"
            IsVisible="False">
        <ActivityIndicator IsRunning="True" />
    </Frame>
</AbsoluteLayout>
```

### SummaryPage.xaml.cs

```csharp
ClearButton.Clicked += async (sender, e) =>
{
    await PeopleManager.DeletePeopleAsync();
    var people = await WebApiClient.Instance.GetPeopleAsync();
    foreach (var p in people)
    {
        await WebApiClient.Instance.DeletePersonAsync(p);
    }

    listData.Clear();
};
```

```csharp
peopleList.Refreshing += async (sender, e) =>
{
    // WebAPIから全データを取得して、SQLiteのデータ数と違ったらSQLiteをアップデートします。
    var webPeople = await WebApiClient.Instance.GetPeopleAsync();
    if (!await PeopleManager.CheckPersonCountAsync(webPeople))
        await PeopleManager.UpdateLocalDataAsync(webPeople);

    // SQLiteから全データを持ってきてListに。
    listData.Clear();
    foreach (var item in await PeopleManager.GetPeopleAsync())
    {
        listData.Add(item);
    }

    peopleList.IsRefreshing = false;
};

peopleList.ItemSelected += async (object sender, SelectedItemChangedEventArgs e) =>
{
    // 選択されたPersonをDetailPageの引数で渡します。
    var person = e.SelectedItem as Person;
    if (person == null)
        return;

    await Navigation.PushAsync(new DetailPage(person));
    peopleList.SelectedItem = null;
};
```

```csharp
indicator.IsVisible = true;

// WebApiから現在の情報を取得して、登録されているItem数が異なればSQliteをアップデートします。
var webPeople = await WebApiClient.Instance.GetPeopleAsync();
if (!await PeopleManager.CheckPersonCountAsync(webPeople))
    await PeopleManager.UpdateLocalDataAsync(webPeople);

// SQLiteから全データを持ってきてListに流し込みます。
listData.Clear();
var people = await PeopleManager.GetPeopleAsync();
foreach (var person in people)
{
    listData.Add(person);
}

indicator.IsVisible = false;
```

### SummaryPage.xaml.cs完成形

```csharp
public partial class SummaryPage : ContentPage
{
    ObservableCollection<Person> listData = new ObservableCollection<Person>();

    public SummaryPage()
    {
        InitializeComponent();
        this.BindingContext = listData;

        AddButton.Clicked += async (sender, e) =>
        {
            await Navigation.PushAsync(new DetailPage(null));
        };

        ClearButton.Clicked += async (sender, e) =>
        {
            await PeopleManager.DeletePeopleAsync();
            var people = await WebApiClient.Instance.GetPeopleAsync();
            foreach (var p in people)
            {
                await WebApiClient.Instance.DeletePersonAsync(p);
            }

            listData.Clear();
        };

        peopleList.Refreshing += async (sender, e) =>
        {
            // WebAPIから全データを取得して、SQLiteのデータ数と違ったらSQLiteをアップデートします。
            var webPeople = await WebApiClient.Instance.GetPeopleAsync();
            if (!await PeopleManager.CheckPersonCountAsync(webPeople))
                await PeopleManager.UpdateLocalDataAsync(webPeople);

            // SQLiteから全データを持ってきてListに。
            listData.Clear();
            foreach (var item in await PeopleManager.GetPeopleAsync())
            {
                listData.Add(item);
            }

            peopleList.IsRefreshing = false;
        };

        peopleList.ItemSelected += async (object sender, SelectedItemChangedEventArgs e) =>
        {
            // 選択されたPersonをDetailPageの引数で渡します。
            var person = e.SelectedItem as Person;
            if (person == null)
                return;

            await Navigation.PushAsync(new DetailPage(person));
            peopleList.SelectedItem = null;
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        indicator.IsVisible = true;

        // WebApiから現在の情報を取得して、登録されているItem数が異なればSQliteをアップデートします。
        var webPeople = await WebApiClient.Instance.GetPeopleAsync();
        if (!await PeopleManager.CheckPersonCountAsync(webPeople))
            await PeopleManager.UpdateLocalDataAsync(webPeople);

        // SQLiteから全データを持ってきてListに流し込みます。
        listData.Clear();
        var people = await PeopleManager.GetPeopleAsync();
        foreach (var person in people)
        {
            listData.Add(person);
        }

        indicator.IsVisible = false;
    }
}
```

### DetailPage.xaml

```xml
<StackLayout Padding="4"
              Orientation="Horizontal"
              Spacing="8">
    <Button x:Name="SaveButton"
            HorizontalOptions="FillAndExpand"
            Style="{DynamicResource ButtonStyleGreen}"
            Text="Save" />
    <Button x:Name="DeleteButton"
            HorizontalOptions="FillAndExpand"
            Style="{DynamicResource ButtonStyleRed}"
            Text="Delete" />
</StackLayout>
```

### DetailPage.xaml完成形

```xml
<StackLayout VerticalOptions="FillAndExpand">
    <Grid RowSpacing="8"
          VerticalOptions="FillAndExpand">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="120" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <Label Grid.Row="0"
                Grid.Column="0"
                Text="Id"
                VerticalTextAlignment="Center" />
        <Label x:Name="IdData"
                Grid.Row="0"
                Grid.Column="1"
                Text="{Binding Id}" />
        <Label Grid.Row="1"
                Grid.Column="0"
                Text="Name"
                VerticalTextAlignment="Center" />
        <Entry x:Name="NameData"
                Grid.Row="1"
                Grid.Column="1"
                Text="{Binding Name}" />
        <Label Grid.Row="2"
                Grid.Column="0"
                Text="Birthday"
                VerticalTextAlignment="Center" />
        <DatePicker x:Name="BirthdayData"
                    Grid.Row="2"
                    Grid.Column="1"
                    Date="{Binding Birthday}"
                    Format="yyyy/MM/dd" />
    </Grid>

    <StackLayout Padding="4"
                  Orientation="Horizontal"
                  Spacing="8">
        <Button x:Name="SaveButton"
                HorizontalOptions="FillAndExpand"
                Style="{DynamicResource ButtonStyleGreen}"
                Text="Save" />
        <Button x:Name="DeleteButton"
                HorizontalOptions="FillAndExpand"
                Style="{DynamicResource ButtonStyleRed}"
                Text="Delete" />
    </StackLayout>

</StackLayout>
```

### DetailPage.xaml.cs

```csharp
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
```

### DetailPage.xaml.cs完成形

```csharp
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
```

### App.xaml.cs

```csharp
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
```

### App.xaml.cs完成形

```csharp
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
```

```csharp

```
