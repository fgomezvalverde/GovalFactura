using com.Goval.FacturaDigital.BusinessProxy.Authentication;
using com.Goval.FacturaDigital.BusinessProxy.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Login
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            
        }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);

            if (!string.IsNullOrEmpty(Entry_Username.Text) && !string.IsNullOrEmpty(Entry_Password.Text))
            {
                ValidateUserClient client = new ValidateUserClient();
                var request = new LoginRequest { UserName = Entry_Username.Text, Password = Entry_Password.Text };

                var vLoginResult = await client.GetDataAsync(request);
                if (vLoginResult.IsSuccessful)
                {
                    DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().SaveString(App.ActualUserDBKey,
                        JsonConvert.SerializeObject(vLoginResult.UserInformation));
                    DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().SaveString(App.ActualUserConfigurationDBKey,
                        JsonConvert.SerializeObject(vLoginResult.UserConfiguration));
                    DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().SaveString(App.ActualSSOTDBKey,
                        JsonConvert.SerializeObject(vLoginResult.SSOT));
                    App.ActualUser = vLoginResult.UserInformation;
                    App.ActualUserConfiguration = vLoginResult.UserConfiguration;
                    App.SSOT = vLoginResult.SSOT;
                    await App.RootPage.ChangePage(new MasterDetail.MasterPageItem { TargetType = MasterDetail.PageType.BillList });
                    await Navigation.PopModalAsync();
                    App.ShowLoading(false);
                    return;
                }
                else
                {
                    await DisplayAlert("", vLoginResult.UserMessage, "Ok");
                }
                

            }
            else
            {
                App.ShowLoading(false);
                await Toasts.ToastRunner.ShowErrorToast("Sistema", "Debe llenar los campos vacíos");
            }



            App.ShowLoading(false);
        }

        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        private async void CreateUser_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new User.CreateNewUser());
        }
    }
}