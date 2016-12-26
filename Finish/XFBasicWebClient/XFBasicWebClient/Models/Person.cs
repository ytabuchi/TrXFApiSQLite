using System;
using Newtonsoft.Json;
using SQLite;

namespace XFBasicWebClient.Models
{
    public class Person
    {
        [PrimaryKey]
        //[AutoIncrement] // 自動採番をオンにします。
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "birthday")]
        public DateTimeOffset Birthday { get; set; }
    }
}
