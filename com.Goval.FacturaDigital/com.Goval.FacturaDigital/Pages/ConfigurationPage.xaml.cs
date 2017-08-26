using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfigurationPage : ContentPage
    {
        public ConfigurationPage()
        {
            InitializeComponent();
        }

        private async void ApplyCode(object sender, EventArgs e)
        {
            string code = EntryAdminCode.Text;
            if (!string.IsNullOrEmpty(code))
            {
                if (code.Equals("ivan0505"))
                {
                    App.AdminPrivilegies = true;
                    await DisplayAlert("Sistema", "Ya tiene privilegios de Admin", "Ok");
                }
                else
                {
                    await DisplayAlert("Sistema", "Codigo Invalido", "Ok");
                }
            }
            else
            {
                await DisplayAlert("Sistema", "Codigo Vacio", "Ok");
            }
        }
    }
}