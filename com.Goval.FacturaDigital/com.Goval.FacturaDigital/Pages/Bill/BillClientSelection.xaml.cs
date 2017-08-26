using com.Goval.FacturaDigital.Amazon;
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

        private void ClientListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var detailClient = e.SelectedItem as Model.Client;
            Navigation.PushModalAsync (
               new AddBill(new Model.Bill {
                   Id=new Random().Next(),
                   AssignClient = detailClient,
                   BillDate = DateTime.Now
               })
               );
        }
    }
}