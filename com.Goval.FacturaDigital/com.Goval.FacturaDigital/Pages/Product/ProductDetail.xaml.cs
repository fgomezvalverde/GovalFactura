using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Product
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductDetail : ContentPage
    {
        public ProductDetail(DataContracts.Model.Product pProduct)
        {
            InitializeComponent();
            this.BindingContext = pProduct;
            //UnityPicker.SelectedItem = pProduct.UnityType;
        }

        private async void SaveProduct_Clicked(object sender, EventArgs e)
        {
            try
            {
                var vNewProduct = this.BindingContext as DataContracts.Model.Product;
                //HACER VALIDACION AQUI **********************
                var vClient = new BusinessProxy.Product.SetUserProduct();
                var vSetProductRequest = new BusinessProxy.Models.ProductRequest()
                {
                    SSOT = App.SSOT,
                    UserId = App.ActualUser.UserId,
                    UserProduct = vNewProduct
                };
                var vResponse = await vClient.GetDataAsync(vSetProductRequest);
                if (vResponse != null && vResponse.IsSuccessful)
                {
                    App.ShowLoading(false);
                    await Navigation.PopAsync();
                    await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se ha modificado Satisfactoriamente");
                }
                else
                {
                    App.ShowLoading(false);
                    await DisplayAlert("", vResponse.TechnicalMessage, "Ok");
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", vResponse.UserMessage);
                }
            }
            catch (Exception ex)
            {
                App.ShowLoading(false);
                await DisplayAlert("", ex.ToString(), "Ok");
            }
            /*App.ShowLoading(true);
            var NewProduct = this.BindingContext as DataContracts.Model.Product;
            if (NewProduct != null && !string.IsNullOrEmpty(NewProduct.Code) && !string.IsNullOrEmpty(NewProduct.Description) &&
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
                        await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se han guardado los cambios");
                        this.SendBackButtonPressed();
                    }
                    else {
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

        private async void DeleteProduct_Clicked(object sender, EventArgs e)
        {

            try
            {
                var vProduct = this.BindingContext as DataContracts.Model.Product;
                //HACER VALIDACION AQUI **********************
                var vClient = new BusinessProxy.Product.DeleteUserProduct();
                var vDeteleProductRequest = new BusinessProxy.Models.ProductRequest()
                {
                    SSOT = App.SSOT,
                    UserId = App.ActualUser.UserId,
                    UserProduct = vProduct
                };
                var vResponse = await vClient.GetDataAsync(vDeteleProductRequest);
                if (vResponse != null && vResponse.IsSuccessful)
                {
                    App.ShowLoading(false);
                    await Navigation.PopAsync();
                    await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se ha eliminado Satisfactoriamente");
                    
                }
                else
                {
                    App.ShowLoading(false);
                    await DisplayAlert("", vResponse.TechnicalMessage, "Ok");
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", vResponse.UserMessage);
                }
            }
            catch (Exception ex)
            {
                App.ShowLoading(false);
                await DisplayAlert("", ex.ToString(), "Ok");
            }
            /*App.ShowLoading(true);
            var deleteProduct = this.BindingContext as Model.Product;
            var answer = await DisplayAlert("Sistema", "Estas seguro que deseas eliminar el item", "Si", "No");
            if (deleteProduct != null && answer)
            {
                if (await DynamoDBManager.GetInstance().Delete<Model.Product>(
                     deleteProduct
                    ))
                {
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se ha eliminado el item");
                    this.SendBackButtonPressed();
                }
                else{
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", "Se ha producido un error al contactar el servicio");
                }
                
            }*/
        }
    }
}