using com.Goval.FacturaDigital.Amazon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Configuracion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfigurationPage : ContentPage
    {
        Boolean RollBackTogleOn = true;
        public ConfigurationPage()
        {
            InitializeComponent();
            SwitchSendNotificationToClients.IsToggled = Utils.ConfigurationConstants.ConfigurationObject.SendBillToClientEmail;
        }

        private async void ApplyCode(object sender, EventArgs e)
        {
            string code = EntryAdminCode.Text;
            if (!string.IsNullOrEmpty(code))
            {
                if (code.Equals("ivan0505"))
                {
                    App.AdminPrivilegies = true;
                    SwitchSendNotificationToClients.IsEnabled = true;
                    EntryAdminCode.Text = string.Empty;
                    await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Ya tiene privilegios de Admin");
                }
                else
                {
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", "Codigo Invalido");
                }
            }
            else
            {
                await Toasts.ToastRunner.ShowErrorToast("Sistema", "Codigo Vacio");
            }
        }

        private async void EmailsNotificated_Clicked(object sender, EventArgs e)
        {
            if (Utils.ConfigurationConstants.ConfigurationObject != null)
            {
                await Navigation.PushAsync(
                new EmailToSendBillPage(Utils.ConfigurationConstants.ConfigurationObject.EmailsToSendBill)
                );
            }
            else {
                await Toasts.ToastRunner.ShowErrorToast("Sistema", "No se ha podido cargar la configuración");
            }
            
        }

        private async void SwitchSendNotificationToClients_Toggled(object sender, ToggledEventArgs e)
        {
            if (RollBackTogleOn)
            {
                RollBackTogleOn = false;
                return;
            }
            App.ShowLoading(true);
            Utils.ConfigurationConstants.ConfigurationObject.SendBillToClientEmail = e.Value;
            Debug.WriteLine("Test:"+Newtonsoft.Json.JsonConvert.SerializeObject(Utils.ConfigurationConstants.ConfigurationObject));
            try
            {
                if (await DynamoDBManager.GetInstance().SaveAsync<Model.SystemConfiguration>(
                     Utils.ConfigurationConstants.ConfigurationObject
                    ))
                {
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se han guardado los cambios");
                }
                else
                {
                    // Rollback
                    RollBackTogleOn = true;
                    if (e.Value)
                    {
                        SwitchSendNotificationToClients.IsToggled = false;
                        Utils.ConfigurationConstants.ConfigurationObject.SendBillToClientEmail = false;
                    }
                    else {
                        SwitchSendNotificationToClients.IsToggled = true;
                        Utils.ConfigurationConstants.ConfigurationObject.SendBillToClientEmail = true;
                    }
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", "Se ha producido un error al contactar el servicio");
                }

            }
            catch (Exception ex)
            {
                // Rollback
                RollBackTogleOn = true;
                if (e.Value)
                {
                    SwitchSendNotificationToClients.IsToggled = false;
                    Utils.ConfigurationConstants.ConfigurationObject.SendBillToClientEmail = false;
                }
                else
                {
                    SwitchSendNotificationToClients.IsToggled = true;
                    Utils.ConfigurationConstants.ConfigurationObject.SendBillToClientEmail = true;
                }
                App.ShowLoading(false);
                await Toasts.ToastRunner.ShowErrorToast("Sistema", ex.Message);
            }
        }
    }
}