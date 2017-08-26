using com.Goval.FacturaDigital.Amazon;
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
    public partial class AddBill : ContentPage
    {
        Model.Bill ActualBill;
        public AddBill(Model.Bill pClient)
        {
            InitializeComponent();
            this.BindingContext = pClient;
        }

        private void ProductListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var listView = sender as ListView;
            listView.SelectedItem = null;
        }

        private void EntryAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            ExecuteBalance();
        }


        private void ExecuteBalance()
        {
            ActualBill = this.BindingContext as Model.Bill;
            ActualBill.discountAmount = 0;
            ActualBill.taxesToPay = 0;
            ActualBill.totalAfterDiscount = 0;
            ActualBill.TotalToPay = 0;
            ActualBill.subTotalProducts = 0;

            foreach (var parsedProduct in ActualBill.AssignClient.Products)
            {
                if (parsedProduct == null || parsedProduct.Amount == 0)
                {

                }
                else
                {
                    parsedProduct.TotalCost = (double)parsedProduct.Amount * parsedProduct.Price;
                    ActualBill.subTotalProducts += parsedProduct.TotalCost;
                }
                
            }

            if (ActualBill.subTotalProducts != 0)
            {
                //We need to apply a discount
                if (ActualBill.AssignClient.DiscountPercentage != 0)
                {
                    ActualBill.discountAmount = (ActualBill.subTotalProducts / 100) * ActualBill.AssignClient.DiscountPercentage;
                }

                ActualBill.totalAfterDiscount = ActualBill.subTotalProducts - ActualBill.discountAmount;

                if (ActualBill.AssignClient.TaxesPercentage != 0)
                {
                    ActualBill.taxesToPay = (ActualBill.totalAfterDiscount / 100) * ActualBill.AssignClient.TaxesPercentage;
                }

                ActualBill.TotalToPay = ActualBill.taxesToPay + ActualBill.totalAfterDiscount;

                TotalProducts.Text = "Subtotal: " + ActualBill.subTotalProducts;
                DescountAmount.Text = string.Format("Descuento({0}%): -{1}", ActualBill.AssignClient.DiscountPercentage, ActualBill.discountAmount);
                TotalAfterDescount.Text = "SubTotal: " + ActualBill.totalAfterDiscount;
                TaxesAmount.Text = string.Format("+Impuestos({0}%): +{1}", ActualBill.AssignClient.TaxesPercentage, ActualBill.taxesToPay);
                Total.Text = "TOTAL: " + ActualBill.TotalToPay + " col";

                Button_CreateBill.IsVisible = true;

                TotalProducts.IsVisible = true;
                DescountAmount.IsVisible = true;
                TotalAfterDescount.IsVisible = true;
                TaxesAmount.IsVisible = true;
                Total.IsVisible = true;
            }
            else
            {
                TotalProducts.IsVisible = false;
                DescountAmount.IsVisible = false;
                TotalAfterDescount.IsVisible = false;
                TaxesAmount.IsVisible = false;
                Total.IsVisible = false;
                Button_CreateBill.IsVisible = false;
            }
        }

        private async void Generate_Bill(object sender, EventArgs e)
        {
            ActualBill.BillDate = DateTime.Now;
            try
            {
                if (await DynamoDBManager.GetInstance().SaveAsync<Model.Bill>(
                 ActualBill
                ))
                {
                    await DisplayAlert("Sistema", "Se ha Guardado Satifactoriamente", "ok");
                    Dictionary<string, string> values = Utils.BillToDictionary(ActualBill);
                    DependencyService.Get<IReportingService>().RunReport(values);
                    this.SendBackButtonPressed();
                    this.SendBackButtonPressed();
                }
                else
                {
                    await DisplayAlert("Sistema", "Se ha producido un error al contactar el servicio", "ok");
                }
                
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sistema", ex.Message, "ok");
                return;
            }
        }

        private void Save_Changes(object sender, EventArgs e)
        {

        }
    }
}