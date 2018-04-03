using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.Goval.FacturaDigital.DataContracts.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using com.Goval.FacturaDigital.Amazon;
using Newtonsoft.Json;

namespace com.Goval.FacturaDigital.Pages.Product
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddProduct : ContentPage
    {
        public AddProduct()
        {
            InitializeComponent();
            this.BindingContext = new DataContracts.Model.Product() ;
        }


        private async void AddProduct_Clicked(object sender, EventArgs e)
        {
            /*App.ShowLoading(true);
            var NewProduct = this.BindingContext as DataContracts.Model.Product;
                if (NewProduct!= null && !string.IsNullOrEmpty(NewProduct.Code) && !string.IsNullOrEmpty(NewProduct.Description) &&
                    NewProduct.Price != 0 && UnityPicker.SelectedItem != null && !string.IsNullOrEmpty(Convert.ToString(UnityPicker.SelectedItem)))
                {
                    try
                    {
                    NewProduct.UnityType = Convert.ToString(UnityPicker.SelectedItem);
                    if (await DynamoDBManager.GetInstance().SaveAsync<Model.Product>(
                         NewProduct
                        ))
                    {
                        App.ShowLoading(false);
                        await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se ha Guardado Satifactoriamente");
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
                        App.ShowLoading(false);
                        await Toasts.ToastRunner.ShowErrorToast("Sistema", ex.Message);
                    }
                    
                }
                else
                {
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowInformativeToast("Sistema", "Alguno de los datos falta por rellenar");
                }*/
            
        }
    }
}