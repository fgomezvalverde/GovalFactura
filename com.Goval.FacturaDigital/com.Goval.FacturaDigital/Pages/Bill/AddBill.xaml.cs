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

                TotalProducts.Text = "Subtotal: " + Utils.Utils.FormatNumericToString(ActualBill.subTotalProducts);
                TotalAfterDescount.Text = "SubTotal: " + Utils.Utils.FormatNumericToString(ActualBill.totalAfterDiscount);
                TaxesAmount.Text = string.Format("+Impuestos({0}%): +{1}", ActualBill.AssignClient.TaxesPercentage, Utils.Utils.FormatNumericToString(ActualBill.taxesToPay));
                Total.Text = "TOTAL: " + Utils.Utils.FormatNumericToString(ActualBill.TotalToPay) + " col";

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
            App.ShowLoading(true);
            ActualBill.BillDate = DateTime.Now;
            try
            {
                if (await DynamoDBManager.GetInstance().SaveAsync<Model.Bill>(
                 ActualBill
                ))
                {
                    
                    Dictionary<string, string> values = Utils.BillSecurity.BillToDictionary(ActualBill);
                    var streamResult = await DependencyService.Get<IReportingService>().CreateAndRunReport(values, ActualBill.Id+"");

                    if (streamResult != null)
                    {
                        SendMailReport(streamResult);
                        streamResult.Dispose();
                    }
                    App.ShowLoading(false);
                    await DisplayAlert("Sistema", "Se ha Guardado Satifactoriamente", "ok");
                    this.SendBackButtonPressed();
                    this.SendBackButtonPressed();

                    
                }
                else
                {
                    App.ShowLoading(false);
                    await DisplayAlert("Sistema", "Se ha producido un error al contactar el servicio", "ok");
                    
                }
                
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sistema", ex.Message, "ok");
            }
        }

        private void SendMailReport(Stream pReportData)
        {

            string mailBody = DependencyService.Get<IFileManagement>().OpenPlainTextFile("MailTemplate.html");
            string subject = string.Format(ConfigurationConstants.MailBillSubject, ActualBill.Id + "");
            mailBody = mailBody.Replace("$NUMERO_FACTURA$", ActualBill.Id+"");
            mailBody = mailBody.Replace("$FECHA$", ActualBill.BillDate.ToString("g"));

            DependencyService.Get<IMailService>().SendMail(subject, mailBody,
                            ConfigurationConstants.EmailsToSendBill,
                            new List<Abstraction.Mail.AttachmentMail>() {
                                new Abstraction.Mail.AttachmentMail
                                {
                                    FileData = pReportData,
                                    FileName = string.Format(ConfigurationConstants.BillNameFile,ActualBill.Id)
                                }
                            },
                            true);
        }

        private void Save_Changes(object sender, EventArgs e)
        {

        }
    }
}