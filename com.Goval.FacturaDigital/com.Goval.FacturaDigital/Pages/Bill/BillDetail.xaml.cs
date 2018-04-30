using com.Goval.FacturaDigital.Abstraction.DependencyServices;
using com.Goval.FacturaDigital.Amazon;
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
        DataContracts.Model.Bill ActualBill;


        public BillDetail(DataContracts.Model.Bill pCurrentBill)
        {
            InitializeComponent();
            ActualBill = pCurrentBill;
           /* StatusPicker.ItemsSource = Enum.GetNames(typeof(Model.BillStatus));
            StatusPicker.SelectedItem = pCurrentBill.Status;
            StatusPicker.SelectedIndexChanged += SetStatusClicked;*/
            this.BindingContext = ActualBill;
            ProductListView.HeightRequest = pCurrentBill.SoldProductsJSON.ClientProducts.Count * 60;
        }

        /*private async void Button_SeeBill_Clicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            Dictionary<string, string> values = await Utils.BillSecurity.BillToDictionary(ActualBill);
            if (Model.BillStatus.Done.ToString().Equals(ActualBill.Status))
            {
                await DependencyService.Get<IReportingService>().CreateAndRunReport(values, ActualBill.Id + "", Utils.ConfigurationConstants.BillExcelCopyFormat);
            }
            else if (Model.BillStatus.Error.ToString().Equals(ActualBill.Status))
            {
                await DependencyService.Get<IReportingService>().CreateAndRunReport(values, ActualBill.Id + "", Utils.ConfigurationConstants.BillExcelCanceledFormat);
            }
            App.ShowLoading(false);
        }*/

        private void ProductListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var listView = sender as ListView;
            listView.SelectedItem = null;
        }

        private async void SetStatusClicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            /*if (StatusPicker != null && StatusPicker.SelectedItem != null && StatusPicker.SelectedItem is string)
            {
                try
                {
                    ActualBill.Status = StatusPicker.SelectedItem as string;
                    //ActualBill.UpdatedBy = App.ActualUser.FullName;
                    if (await DynamoDBManager.GetInstance().SaveAsync<Model.Bill>(
                     ActualBill
                    ))
                    {
                        App.ShowLoading(false);
                        await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se han guardado los cambios");
                        this.SendBackButtonPressed();
                    }
                    else
                    {
                        App.ShowLoading(false);
                        await Toasts.ToastRunner.ShowErrorToast("Sistema", "Se ha producido un error al contactar al servicio");
                    }

                }
                catch (Exception ex)
                {
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", ex.Message);
                }

            }
            else
            {
                App.ShowLoading(false);
                await Toasts.ToastRunner.ShowInformativeToast("Sistema", "El status esta vacío");
            }*/
        }


    }
}