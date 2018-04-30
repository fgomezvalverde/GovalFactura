using com.Goval.FacturaDigital.Amazon;
using com.Goval.FacturaDigital.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Bill
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BillClientSelection : ContentPage
    {
        public BillClientSelection()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            App.ShowLoading(true);
            if (!string.IsNullOrEmpty(App.SSOT) && App.ActualUser != null)
            {
                var vGetUserClient = new BusinessProxy.Client.GetUserClients();
                var vClientsResponse = await vGetUserClient.GetDataAsync(
                    new BusinessProxy.Models.ClientRequest
                    {
                        SSOT = App.SSOT,
                        UserId = App.ActualUser.UserId
                    });
                if (vClientsResponse != null)
                {
                    if (vClientsResponse.IsSuccessful)
                    {
                        ClientListView.ItemsSource = vClientsResponse.UserClients;
                    }
                    else
                    {
                        ClientListView.ItemsSource = null;
                        await Toasts.ToastRunner.ShowErrorToast("Sistema", vClientsResponse.UserMessage);
                        await DisplayAlert("", vClientsResponse.TechnicalMessage, "Ok");
                    }
                }
                else
                {
                    ClientListView.ItemsSource = null;
                    await DisplayAlert("", "Respuesta Null", "Ok");
                }

            }

            else
            {
                ClientListView.ItemsSource = null;
                //await DisplayAlert("", "SSOT null o User null", "Ok");
            }
            App.ShowLoading(false);
        }

        private async void ClientListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var detailClient = e.SelectedItem as DataContracts.Model.Client;
            App.ShowLoading(true);
            await Navigation.PushAsync(new AddBill(new DataContracts.Model.Bill
               {
                   SoldProductsJSON = detailClient
               })
            );
            

            App.ShowLoading(false);
        }
    }
}