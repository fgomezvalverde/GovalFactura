using com.Goval.FacturaDigital.Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClientList : ContentPage
    {
        public ClientList()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            /*
             App.ShowLoading(true);

            if (App.AdminPrivilegies && this.ToolbarItems.Count == 0)
            {
                ToolbarItem item = new ToolbarItem
                {
                    Order = ToolbarItemOrder.Primary,
                    Icon = "ic_action_add.png",
                    Text = "Agregar",
                    Priority = 0,

                };
                item.Clicked += AddClient_Clicked;
                this.ToolbarItems.Add(item);
            }

            var clientList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Client>();
            if (clientList != null && clientList.Count != 0)
            {
                ClientListView.ItemsSource = clientList;
            }
            App.ShowLoading(false);*/


            App.ShowLoading(true);
            if (App.AdminPrivilegies && this.ToolbarItems.Count == 0)
            {
                ToolbarItem item = new ToolbarItem
                {
                    Order = ToolbarItemOrder.Primary,
                    Icon = "ic_action_add.png",
                    Text = "Agregar",
                    Priority = 0,

                };
                item.Clicked += AddClient_Clicked;
                this.ToolbarItems.Add(item);
            }
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
                        await DisplayAlert("", vClientsResponse.TechnicalMessage,"Ok");
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
                await DisplayAlert("", "SSOT null o User null", "Ok");
            }
            App.ShowLoading(false);
        }

        private void AddClient_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(
                new AddClient()
                );
        }

        private void ClientListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var detailClient = e.SelectedItem as DataContracts.Model.Client;
            Navigation.PushAsync(
               new ClientDetail(detailClient)
               );
        }

    }
}