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
        string MonthlyIncomeFormat = "₡{0}";
        public BillList()
        {
            InitializeComponent();
            BillListView.IsPullToRefreshEnabled = true;
            BillListView.RefreshCommand = new Command (async ()=> {

                await RefreshBillList();
                BillListView.IsRefreshing = false;
            });
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await RefreshBillList();
        }


        private async Task RefreshBillList()
        {
            App.ShowLoading(true);
            if (!string.IsNullOrEmpty(App.SSOT) && App.ActualUser != null)
            {
                var vGetUserBills = new BusinessProxy.Bill.GetUserBills();
                var vBillsResponse = await vGetUserBills.GetDataAsync(
                    new BusinessProxy.Models.BillRequest
                    {
                        SSOT = App.SSOT,
                        User = App.ActualUser
                    });
                if (vBillsResponse != null)
                {
                    if (vBillsResponse.IsSuccessful)
                    {
                        if (vBillsResponse != null && vBillsResponse.UserBills.Any())
                        {
                            var sorted = from bill in vBillsResponse.UserBills
                                         orderby bill.EmissionDate descending
                                         group bill by bill.EmissionDate into billGroup
                                         select new Grouping<DateTime?, DataContracts.Model.Bill>(billGroup.Key, billGroup);

                            //create a new collection of groups
                            var billsGrouped = new ObservableCollection<Grouping<DateTime?, DataContracts.Model.Bill>>(sorted);
                            BillListView.ItemsSource = billsGrouped;

                            string incomesText, unitsText;
                            GetMonthlyIncomesAndSoldUnits(vBillsResponse.UserBills, out incomesText, out unitsText);
                            monthlyIncomesLabel.Text = incomesText;
                            //monthlyUnitsLabel.Text = unitsText;
                        }

                    }
                    else
                    {
                        BillListView.ItemsSource = null;
                        await Toasts.ToastRunner.ShowErrorToast("Sistema", vBillsResponse.UserMessage);
                        await DisplayAlert("", vBillsResponse.TechnicalMessage, "Ok");
                    }
                }
                else
                {
                    BillListView.ItemsSource = null;
                    await DisplayAlert("", "Respuesta Null", "Ok");
                }

            }

            else
            {
                BillListView.ItemsSource = null;
                //await DisplayAlert("", "SSOT null o User null", "Ok");
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
            var detailbill = e.SelectedItem as DataContracts.Model.Bill;
            Navigation.PushAsync(
               new BillDetail(detailbill)
               );
        }

        private void GetMonthlyIncomesAndSoldUnits(List<DataContracts.Model.Bill> pList,out string pMonthlyIncomes,out string pMonthlyUnits)
        {
            try
            {
                double total = 0;
                int units = 0;
                var ListFilter = pList.Where(x =>
                x.Status.Equals(Enum.GetName(Model.BillStatus.Done.GetType(), Model.BillStatus.Done)) &&
                x.EmissionDate.Value.Month == DateTime.Now.Month);
                foreach (var bill in ListFilter)
                {

                    int unitsPerProduct = 0;
                    /*foreach (var product in bill.SoldProductsJSON.ClientProducts)
                    {
                        if (product.UnityType.Equals("Unitaria"))
                        {
                            unitsPerProduct = product.Amount;
                        }
                        else if (product.UnityType.Equals("Docena"))
                        {
                            unitsPerProduct = product.Amount * 12;
                        }
                    }*/
                    total += (double)bill.TotalToPay ;
                    units += unitsPerProduct;
                }
                // 123,456.23    3,465.11  456.11
                string totalString = Utils.Utils.FormatNumericToString(total);
                var splited = totalString.Split('.');
                if (splited[0].Length > 4)
                {
                    //totalString = splited[0].Substring(0, splited[0].Length - 4); // Se puso todo el largo
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
                Toasts.ToastRunner.ShowErrorToast("Sistema", ex.Message);
                pMonthlyIncomes = "0";
                pMonthlyUnits = "0";
            }
            
        }

    }
}