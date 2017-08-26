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
    public partial class BillList : ContentPage
    {
        public BillList()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var billList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Bill>();
            if (billList != null && billList.Count != 0)
            {
                BillListView.ItemsSource = billList;
            }
        }

        private void AddBill_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(
                new BillClientSelection()
                );
        }

        private void billListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var detailbill = e.SelectedItem as Model.Bill;
            Navigation.PushModalAsync(
               new BillDetail(detailbill)
               );
        }
    }
}