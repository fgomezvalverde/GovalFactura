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
    public partial class BillList : ContentPage
    {
        public BillList()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            App.ShowLoading(true);
            var billList = await BillSecurity.GetBillList();
            if (billList != null && billList.Count != 0)
            {
                BillListView.ItemsSource = billList.OrderByDescending(x => x.Id);
            }
            App.ShowLoading(false);
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