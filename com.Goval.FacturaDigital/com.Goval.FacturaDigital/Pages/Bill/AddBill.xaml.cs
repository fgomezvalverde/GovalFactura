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
            ProductListView.HeightRequest = pClient.AssignClient.Products.Count * 60;
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

            //Setting Amounts

            TotalProducts.Text = "₡" + Utils.Utils.FormatNumericToString(ActualBill.subTotalProducts);


            DescountAmountLabel.Text = string.Format("-Descuento({0}%)", ActualBill.AssignClient.DiscountPercentage);
            DescountAmount.Text = "₡" + Utils.Utils.FormatNumericToString(ActualBill.discountAmount);

            TotalAfterDescount.Text = "₡" + Utils.Utils.FormatNumericToString(ActualBill.totalAfterDiscount);


            TaxesAmountLabel.Text = string.Format("+Impuestos({0}%)", ActualBill.AssignClient.TaxesPercentage);
            TaxesAmount.Text = "₡"+Utils.Utils.FormatNumericToString(ActualBill.taxesToPay);

            Total.Text = "₡" + Utils.Utils.FormatNumericToString(ActualBill.TotalToPay) + " cols";


            if (ActualBill.subTotalProducts != 0)
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
            ActualBill.BillDate = DateTime.Now;
            try
            {
                if (await DynamoDBManager.GetInstance().SaveAsync<Model.Bill>(
                 ActualBill
                ))
                {
                    
                    Dictionary<string, string> values = await Utils.BillSecurity.BillToDictionary(ActualBill);
                    var streamResult = await DependencyService.Get<IReportingService>().CreateAndRunReport(values, ActualBill.Id+"", Utils.ConfigurationConstants.BillExcelOriginalFormat);

                    if (streamResult != null)
                    {
                        SendMailReport(streamResult);
                        streamResult.Dispose();
                    }
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se ha Guardado Satifactoriamente");
                    this.SendBackButtonPressed();
                    this.SendBackButtonPressed();

                    
                }
                else
                {
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", "Se ha producido un error al contactar el servicio");                    
                }
                
            }
            catch (Exception ex)
            {
                await Toasts.ToastRunner.ShowErrorToast("Sistema", ex.Message);
            }
        }

        private void SendMailReport(Stream pReportData)
        {

            string mailBody = DependencyService.Get<IFileManagement>().OpenPlainTextFile("MailTemplate.html");
            string subject = string.Format(ConfigurationConstants.MailBillSubject, ActualBill.Id + "");
            mailBody = mailBody.Replace("$NUMERO_FACTURA$", ActualBill.Id+"");
            mailBody = mailBody.Replace("$FECHA$", ActualBill.BillDate.ToString(ConfigurationConstants.DateTimeFormat,ConfigurationConstants.Culture));
            mailBody = mailBody.Replace("$MONTO_TOTAL$", Utils.Utils.FormatNumericToString(ActualBill.TotalToPay) + "");
            var emailsToSend = new List<String>(ConfigurationConstants.ConfigurationObject.EmailsToSendBill);

            // Add client email to send email
            if (ConfigurationConstants.ConfigurationObject.SendBillToClientEmail && !string.IsNullOrEmpty(ActualBill.AssignClient.Email))
            {
                emailsToSend.Add(ActualBill.AssignClient.Email);
            }


            DependencyService.Get<IMailService>().SendMail(subject, mailBody,
                            emailsToSend,
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