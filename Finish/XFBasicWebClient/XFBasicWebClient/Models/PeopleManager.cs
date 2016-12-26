using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PCLStorage;
using SQLite;

namespace XFBasicWebClient.Models
{
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
}
