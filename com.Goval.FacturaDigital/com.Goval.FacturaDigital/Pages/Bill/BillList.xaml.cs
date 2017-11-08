using com.Goval.FacturaDigital.Abstraction.GroupingListView;
using com.Goval.FacturaDigital.Amazon;
using com.Goval.FacturaDigital.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        string MonthlySoldUnitsFormat = "{0} unis.";
        string MonthlyIncomeFormat = "₡{0} k";
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
                var sorted = from bill in billList
                             orderby bill.BillDate.Date descending
                             group bill by bill.BillDate.Date into billGroup
                             select new Grouping<DateTime, Model.Bill>(billGroup.Key, billGroup);

                //create a new collection of groups
                var billsGrouped = new ObservableCollection<Grouping<DateTime, Model.Bill>>(sorted);
                BillListView.ItemsSource = billsGrouped;

                string incomesText, unitsText;
                GetMonthlyIncomesAndSoldUnits(billList, out incomesText, out unitsText);
                monthlyIncomesLabel.Text = incomesText;
                monthlyUnitsLabel.Text = unitsText;
            }
            App.ShowLoading(false);
        }

        private void AddBill_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(
                new BillClientSelection()
                );
        }

        private void billListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var detailbill = e.SelectedItem as Model.Bill;
            Navigation.PushAsync(
               new BillDetail(detailbill)
               );
        }

        private void GetMonthlyIncomesAndSoldUnits(List<Model.Bill> pList,out string pMonthlyIncomes,out string pMonthlyUnits)
        {
            try
            {
                double total = 0;
                int units = 0;
                var ListFilter = pList.Where(x =>
                x.Status.Equals(Enum.GetName(Model.BillStatus.Aprobada.GetType(), Model.BillStatus.Aprobada)) &&
                x.BillDate.Month == DateTime.Now.Month);
                foreach (var bill in ListFilter)
                {

                    int unitsPerProduct = 0;
                    foreach (var product in bill.AssignClient.Products)
                    {
                        if (product.UnityType.Equals("Unitaria"))
                        {
                            unitsPerProduct = product.Amount;
                        }
                        else if (product.UnityType.Equals("Docena"))
                        {
                            unitsPerProduct = product.Amount * 12;
                        }
                    }
                    total += bill.TotalToPay;
                    units += unitsPerProduct;
                }
                // 123,456.23    3,465.11  456.11
                string totalString = Utils.Utils.FormatNumericToString(total);
                var splited = totalString.Split('.');
                if (splited[0].Length > 4)
                {
                    totalString = splited[0].Substring(0, splited[0].Length - 4);
                }
                else
                {
                    totalString = "0";
                }


                //seting results
                pMonthlyIncomes = string.Format(MonthlyIncomeFormat, totalString);
                pMonthlyUnits = string.Format(MonthlySoldUnitsFormat, units);
            }
            catch (Exception ex)
            {
                DisplayAlert("Sistema", ex.Message,"ok");
                pMonthlyIncomes = "0";
                pMonthlyUnits = "0";
            }
            
        }

    }
}