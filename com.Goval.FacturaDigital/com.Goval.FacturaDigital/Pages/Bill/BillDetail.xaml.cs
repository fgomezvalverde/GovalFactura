using com.Goval.FacturaDigital.DependencyServices;
using com.Goval.FacturaDigital.Test;
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
    public partial class BillDetail : ContentPage
    {
        Model.Bill ActualBill;


        public BillDetail(Model.Bill pCurrentBill)
        {
            InitializeComponent();
            ActualBill = pCurrentBill;
            this.BindingContext = ActualBill;
        }

        private void Button_SeeBill_Clicked(object sender, EventArgs e)
        {
            Dictionary<string, string> values = Utils.BillToDictionary(ActualBill);
            DependencyService.Get<IReportingService>().RunReport(values);
        }

        private void ProductListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var listView = sender as ListView;
            listView.SelectedItem = null;
        }
    }
}