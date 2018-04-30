using com.Goval.FacturaDigital.BusinessProxy.Models;
using com.Goval.FacturaDigital.BusinessProxy.Authentication;
using com.Goval.FacturaDigital.DataContracts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;

namespace com.Goval.FacturaDigital.Pages.User
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CreateNewUser : ContentPage
	{
		public CreateNewUser ()
		{
			InitializeComponent ();
            SignupRequest vSignUpRequest = new SignupRequest();
            this.BindingContext = vSignUpRequest;
        }

        private async void CreateUser_Clicked(object sender, EventArgs e)
        {
            try
            {
                RegisterUser vRegisterUserClient = new RegisterUser();
                var vRequest = this.BindingContext as SignupRequest;
                vRequest.User.UserName = vRequest.User.Email;
                var json = JsonConvert.SerializeObject(vRequest);
                App.ShowLoading(true, "Creando Usuario");
                var vResponse = await vRegisterUserClient.GetDataAsync(vRequest);
                if (vResponse != null)
                {
                    if (vResponse.IsSuccessful)
                    {
                        DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().SaveString(App.ActualUserDBKey,
                        JsonConvert.SerializeObject(vResponse.UserInformation));
                        DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().SaveString(App.ActualUserConfigurationDBKey,
                            JsonConvert.SerializeObject(vResponse.UserConfiguration));
                        DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().SaveString(App.ActualSSOTDBKey,
                            JsonConvert.SerializeObject(vResponse.SSOT));
                        App.ActualUser = vResponse.UserInformation;
                        App.ActualUserConfiguration = vResponse.UserConfiguration;
                        App.SSOT = vResponse.SSOT;

                        await Navigation.PushModalAsync(new User.HaciendaRegistration(vResponse.UserInformation));
                        App.ShowLoading(false);
                    }
                    else
                    {
                        App.ShowLoading(false);
                        await DisplayAlert("", vResponse.TechnicalMessage, "Ok");
                        await Toasts.ToastRunner.ShowErrorToast("Sistema", vResponse.UserMessage);
                        
                    }
                }
                else
                {
                    App.ShowLoading(false);
                    await DisplayAlert("", "No hubo respuesta del Servidor", "Ok");
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", "No hubo respuesta del Servidor");
                }
            }
            catch (Exception ex)
            {
                App.ShowLoading(false);
                await DisplayAlert("", ex.ToString(), "Ok");
            }
            


        }

        Dictionary<string, string> prueba = new Dictionary<string, string>() { { "clientName", "Fabian Gomez" } };
    }
}