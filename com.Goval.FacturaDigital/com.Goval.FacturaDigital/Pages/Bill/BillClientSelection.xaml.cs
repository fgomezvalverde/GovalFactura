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
            var clientList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Client>();
            if (clientList != null && clientList.Count != 0)
            {
                ClientListView.ItemsSource = clientList;
            }
        }

        private async void ClientListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var detailClient = e.SelectedItem as Model.Client;
            int nextBillNumber = 0;
            nextBillNumber = await BillSecurity.GetNextBillNumber();
            if (nextBillNumber != 0)
            {
               await Navigation.PushModalAsync(
               new AddBill(new Model.Bill
               {
                   Id = nextBillNumber,
                   AssignClient = detailClient,
                   BillDate = DateTime.Now
               })
               );
            }
            else
            {
                await DisplayAlert("Sistema", "Hubo un problema al conseguir el Numero de Factura", "Ok");
            }
            
        }
    }
}