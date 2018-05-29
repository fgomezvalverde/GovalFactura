using com.Goval.FacturaDigital.Abstraction.DependencyServices;
using com.Goval.FacturaDigital.Amazon;
using com.Goval.FacturaDigital.Test;
using com.Goval.FacturaDigital.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
        DataContracts.Model.Bill ActualBill;
        public AddBill(DataContracts.Model.Bill pClient)
        {
            InitializeComponent();
            this.BindingContext = pClient;
            if (pClient.SoldProductsJSON != null && pClient.SoldProductsJSON.ClientProducts != null && pClient.SoldProductsJSON.ClientProducts.Any())
            {
                ProductListView.HeightRequest = pClient.SoldProductsJSON.ClientProducts.Count * 60;
            }
            
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
            ActualBill = this.BindingContext as DataContracts.Model.Bill;
            ActualBill.DiscountAmount = 0;
            ActualBill.TaxesToPay = 0;
            ActualBill.TotalAfterDiscount = 0;
            ActualBill.TotalToPay = 0;
            ActualBill.SubTotalProducts = 0;

            foreach (var parsedProduct in ActualBill.SoldProductsJSON.ClientProducts)
            {
                if (parsedProduct == null || parsedProduct.ProductQuantity == 0)
                {

                }
                else
                {
                    parsedProduct.TotalCost = parsedProduct.ProductQuantity * parsedProduct.Price;
                    ActualBill.SubTotalProducts += parsedProduct.TotalCost;
                }
                
            }
            //We need to apply a discount
            if (ActualBill.SoldProductsJSON.DefaultDiscountPercentage != 0)
            {
                ActualBill.DiscountAmount = (ActualBill.SubTotalProducts / 100) * ActualBill.SoldProductsJSON.DefaultDiscountPercentage;
            }

            ActualBill.TotalAfterDiscount = ActualBill.SubTotalProducts - ActualBill.DiscountAmount;

            if (ActualBill.SoldProductsJSON.DefaultTaxesPercentage != 0)
            {
                ActualBill.TaxesToPay = (ActualBill.TotalAfterDiscount / 100) * ActualBill.SoldProductsJSON.DefaultTaxesPercentage;
            }
            ActualBill.TotalToPay = ActualBill.TaxesToPay + ActualBill.TotalAfterDiscount;

            //Setting Amounts

            TotalProducts.Text = "₡" + Utils.Utils.FormatNumericToString(ActualBill.SubTotalProducts);


            DescountAmountLabel.Text = string.Format("-Descuento({0}%)", ActualBill.SoldProductsJSON.DefaultDiscountPercentage);
            DescountAmount.Text = "₡" + Utils.Utils.FormatNumericToString(ActualBill.DiscountAmount);

            TotalAfterDescount.Text = "₡" + Utils.Utils.FormatNumericToString(ActualBill.TotalAfterDiscount);


            TaxesAmountLabel.Text = string.Format("+Impuestos({0}%)", ActualBill.SoldProductsJSON.DefaultTaxesPercentage);
            TaxesAmount.Text = "₡"+Utils.Utils.FormatNumericToString(ActualBill.TaxesToPay);

            Total.Text = "₡" + Utils.Utils.FormatNumericToString(ActualBill.TotalToPay) + " cols";


            if (ActualBill.SubTotalProducts != 0)
            {
                Button_CreateBill.IsVisible = true;
            }
            else
            {
                Button_CreateBill.IsVisible = false;
            }
        }

        private async void Generate_Bill(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            ActualBill = this.BindingContext as DataContracts.Model.Bill;
            ActualBill.EmissionDate = null;
            ActualBill.LastSendDate = null;
            ActualBill.ClientId = ActualBill.SoldProductsJSON.ClientId;
            try
            {
                // Remove Unused Products
                ActualBill.SoldProductsJSON.RemoveProductsWithoutQuantity();

                var vCreateUserBills = new BusinessProxy.Bill.CreateBill();
                var vBillRequest = new BusinessProxy.Models.BillRequest
                {
                    SSOT = App.SSOT,
                    User = App.ActualUser,
                    ClientBill = ActualBill,
                };
                var vCreateBillsResponse = await vCreateUserBills.GetDataAsync(vBillRequest);

                // For testing
                var jsonTEST = Newtonsoft.Json.JsonConvert.SerializeObject(vBillRequest);
                if (vCreateBillsResponse != null)
                {
                    if (vCreateBillsResponse.IsSuccessful)
                    {
                        App.ShowLoading(false);
                        if (vCreateBillsResponse.PdfInvoice != null)
                        {
                            DependencyService.Get<IReportingService>().SaveAndOpenFile("Factura"+ vCreateBillsResponse.BillNumber + ".pdf",
                            vCreateBillsResponse.PdfInvoice);
                        }
                        
                        await Navigation.PopToRootAsync();
                        await Toasts.ToastRunner.ShowSuccessToast("Sistema", 
                            string.IsNullOrEmpty(vCreateBillsResponse.UserMessage)?"Se procesará la solicitud de la factura N°"+ vCreateBillsResponse .BillNumber: vCreateBillsResponse.UserMessage
                            );
                        
                    }
                    else
                    {
                        App.ShowLoading(false);
                        await Toasts.ToastRunner.ShowErrorToast("Sistema", vCreateBillsResponse.UserMessage);
                        await DisplayAlert("", vCreateBillsResponse.TechnicalMessage, "Ok");
                    }
                }
                else
                {
                    App.ShowLoading(false);
                    await DisplayAlert("", "Respuesta Null", "Ok");
                }

            }
            catch (Exception ex)
            {
                await Toasts.ToastRunner.ShowErrorToast("Sistema", ex.Message);
            }
        }

        

        private void DiscountOrTaxEntry_TextChanged(object sender, EventArgs e)
        {
            ExecuteBalance();
        }
    }
}