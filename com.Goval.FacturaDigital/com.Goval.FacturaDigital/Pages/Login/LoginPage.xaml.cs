using com.Goval.FacturaDigital.Amazon;
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
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);

            if (!string.IsNullOrEmpty(Entry_Username.Text) && !string.IsNullOrEmpty(Entry_Password.Text))
            {
                var userList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.User>();

                if (userList != null && userList.Count > 0)
                {
                    Boolean noValidUser = false;

                    foreach (var user in userList)
                    {
                        if (Entry_Username.Text.Equals(user.UserName) &&
                            Entry_Password.Text.Equals(user.UserPassword))
                        {
                            noValidUser = true;
                            App.AdminPrivilegies = user.HasAdminPrivilegies;
                            string jsonUser = JsonConvert.SerializeObject(user);
                            DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().SaveString(App.UserDefaultKey, jsonUser);
                            App.ActualUser = user;
                            await Navigation.PopModalAsync();
                            App.ShowLoading(false);
                            return;
                        }
                    }

                    if (!noValidUser)
                    {
                        await Toasts.ToastRunner.ShowErrorToast("Sistema", "Usuario o/y Contraseña Inválida");
                    }

                }
                else
                {
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", "Hubo un problema al contactar al servidor");
                }

            }
            else
            {
                await Toasts.ToastRunner.ShowErrorToast("Sistema", "Debe llenar los campos vacíos");
            }



            App.ShowLoading(false);
        }

        protected override bool OnBackButtonPressed()
        {
            return false;
        }
    }
}