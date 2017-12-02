using com.Goval.FacturaDigital.Amazon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Configuracion
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmailToSendBillPage : ContentPage
	{
        public ObservableCollection<string> ListViewItems { get; set; }
        Entry newEmailEntry;
        Button newEmailButton;
        public EmailToSendBillPage (List<String> pEmails)
		{
			InitializeComponent ();
            ListViewItems = new ObservableCollection<string> (pEmails);
            this.BindingContext = ListViewItems;
            if (App.AdminPrivilegies)
            {
                ToolbarItem item = new ToolbarItem
                {
                    Order = ToolbarItemOrder.Primary,
                    Icon = "ic_action_add.png",
                    Text = "Agregar",
                    Priority = 0,

                };
                item.Clicked += AddEmail_Clicked;
                this.ToolbarItems.Add(item);
            }
            
            
        }


        private void AddEmail_Clicked(object sender, EventArgs e)
        {
            if (AddEmailLayout.Children.Count == 0) {
                newEmailEntry = new Entry
                {
                    Placeholder = "Nuevo Correo",
                    BackgroundColor = Color.White,
                    Keyboard = Keyboard.Email,
                    Margin = new Thickness(5)
                };
                newEmailButton = new Button
                {
                    BackgroundColor = Color.FromHex("#90A4AE"),
                    TextColor = Color.White,
                    Margin = new Thickness(5),
                    HorizontalOptions = LayoutOptions.End,
                    Text = "Agregar"
                };
                newEmailButton.Clicked += NewEmailButton_Clicked;
                Device.BeginInvokeOnMainThread(() =>
                {
                    AddEmailLayout.Children.Add(newEmailEntry);
                    AddEmailLayout.Children.Add(newEmailButton);
                });
            }
            
        }

        private async void NewEmailButton_Clicked(object sender, EventArgs e)
        {
            bool isEmail = Regex.IsMatch(newEmailEntry.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            if (newEmailEntry != null && !string.IsNullOrEmpty(newEmailEntry.Text)
                && newEmailButton != null && isEmail)
            {
                newEmailButton.Clicked -= NewEmailButton_Clicked;
                Device.BeginInvokeOnMainThread(async () =>
                {
                    App.ShowLoading(true);
                    var temporalList = ListViewItems.ToList();
                    temporalList.Add(newEmailEntry.Text);
                    await TryToSetConfigurationObj(temporalList);
                    AddEmailLayout.Children.Remove(newEmailEntry);
                    AddEmailLayout.Children.Remove(newEmailButton);
                    App.ShowLoading(false);
                });
            }
            else
            {
                if (!isEmail)
                {
                    await Toasts.ToastRunner.ShowInformativeToast("Sistema", "Formato de Correo inválido");
                }
                else
                {
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", "Existe algun error al procesar.Reinicie el App");
                }
            }
        }

        private async void OnDelete(object sender, EventArgs e)
        {
            if (App.AdminPrivilegies)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    App.ShowLoading(true);
                    var item = ((MenuItem)sender);
                    var temporalList = ListViewItems.ToList();
                    temporalList.Remove(item.CommandParameter as string);
                    await TryToSetConfigurationObj(temporalList);
                    App.ShowLoading(false);
                });
            }
            else
            {
                await Toasts.ToastRunner.ShowInformativeToast("Sistema", "No tiene privilegios para realizar está acción");
            }

        }

        private async Task TryToSetConfigurationObj(List<String> pEmailsToSend)
        {
            string[] securityCopy =new string[Utils.ConfigurationConstants.ConfigurationObject.EmailsToSendBill.Count] ;
            Utils.ConfigurationConstants.ConfigurationObject.EmailsToSendBill.CopyTo(securityCopy);
            try
            {
                Utils.ConfigurationConstants.ConfigurationObject.EmailsToSendBill = pEmailsToSend;
                if (await DynamoDBManager.GetInstance().SaveAsync<Model.SystemConfiguration>(
                     Utils.ConfigurationConstants.ConfigurationObject
                    ))
                {
                    ListViewItems = new ObservableCollection<string>(pEmailsToSend);
                    this.BindingContext = ListViewItems;
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se han guardado los cambios");
                }
                else
                {
                    Utils.ConfigurationConstants.ConfigurationObject.EmailsToSendBill = new List<string>(securityCopy);
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", "Se ha producido un error al contactar el servicio");
                }

            }
            catch (Exception ex)
            {
                Utils.ConfigurationConstants.ConfigurationObject.EmailsToSendBill = new List<string>(securityCopy);
                App.ShowLoading(false);
                await Toasts.ToastRunner.ShowErrorToast("Sistema", ex.Message);
            }
        }
    }
}