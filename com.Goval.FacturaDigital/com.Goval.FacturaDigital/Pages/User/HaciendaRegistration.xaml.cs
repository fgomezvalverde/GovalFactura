using com.Goval.FacturaDigital.BusinessProxy.Authentication;
using com.Goval.FacturaDigital.BusinessProxy.Models;
using com.Goval.FacturaDigital.Utils;
using Newtonsoft.Json;
using Plugin.FilePicker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.User
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HaciendaRegistration : ContentPage
    {
        Byte[] File = null;
        public HaciendaRegistration(DataContracts.Model.User pClient)
        {
            InitializeComponent();
            this.BindingContext = pClient;
        }

        private async void ValidateUser_Clicked(object sender, EventArgs e)
        {
            DataContracts.Model.User pUser = this.BindingContext as DataContracts.Model.User;
            if (pUser != null && !string.IsNullOrEmpty(pUser.HaciendaUsername) &&
                !string.IsNullOrEmpty(pUser.HaciendaPassword) &&
                pUser.HaciendaCryptographicPIN != null && File != null)
            {
                App.ShowLoading(true, "Validando Usuario");
                pUser.HaciendaCryptographicFile = File;
                pUser.HaciendaUserValidation = true;
                UserValidationRequest vRequest = new UserValidationRequest
                {
                    User = pUser,
                    SSOT = App.SSOT
                };
                var vClient = new ValidateUserWithHacienda();
                var vResponse = await vClient.GetDataAsync(vRequest);
                var json = JsonConvert.SerializeObject(vRequest);
                if (vResponse != null)
                {
                    if (vResponse.IsSuccessful)
                    {
                        DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().SaveString(App.ActualUserDBKey,
                        Newtonsoft.Json.JsonConvert.SerializeObject(pUser));
                        App.ActualUser = pUser;
                        App.ShowLoading(false);
                        await Navigation.PopUpAllModals();
                        await Toasts.ToastRunner.ShowSuccessToast("", "Validación Exitosa");
                    }
                    else
                    {
                        App.ShowLoading(false);
                        await DisplayAlert("", vResponse.UserMessage, "Ok");
                    }
                    
                }
                else
                {
                    App.ShowLoading(false);
                    await DisplayAlert("", "Existe un problema al contactar el servidor. Revise su conexión", "Ok");
                }
            }
            else
            {
                await DisplayAlert("", "Falta un dato por llenar", "Ok");
            }
        }

        private async void Dismiss_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopUpAllModals();
        }
        private async void LoadFile_Clicked(object sender, EventArgs e)
        {
            var vFile = await CrossFilePicker.Current.PickFile();

            if (vFile != null && vFile.DataArray != null)
            {
                if (vFile.FileName.Contains(".p12"))
                {
                    File = vFile.DataArray;
                    await DisplayAlert("", vFile.FileName, "Ok");
                }
                else
                {
                    await DisplayAlert("Archivo Incorrecto", "Tiene que ser de extensión .p12", "Ok");
                }
                
            }
        }
    }
}