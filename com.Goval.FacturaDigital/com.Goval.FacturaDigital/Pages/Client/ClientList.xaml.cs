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
            /*var clientList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Client>();
            if (clientList != null || clientList.Count != 0)
            {
                //ProductListView.ItemsSource = clientList;
            }*/
        }

        private void AddClient_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(
                new AddClient()
                );
        }

        private void ClientListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var detailClient = e.SelectedItem as Model.Client;
            Navigation.PushModalAsync(
               new ClientDetail(detailClient)
               );
        }

    }
}