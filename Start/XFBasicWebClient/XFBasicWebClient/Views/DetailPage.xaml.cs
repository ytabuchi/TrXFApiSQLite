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
        }
    }
}
