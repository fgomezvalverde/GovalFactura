using com.Goval.FacturaDigital.Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Product
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductDetail : ContentPage
    {
        public ProductDetail(Model.Product pProduct)
        {
            InitializeComponent();
            this.BindingContext = pProduct;

        }

        private async void SaveProduct_Clicked(object sender, EventArgs e)
        {
            var NewProduct = this.BindingContext as Model.Product;
            if (NewProduct != null && !string.IsNullOrEmpty(NewProduct.Code) && !string.IsNullOrEmpty(NewProduct.Description) &&
                NewProduct.Price != 0)
            {
                try
                {
                    if (await DynamoDBManager.GetInstance().SaveAsync<Model.Product>(
                     NewProduct
                    ))
                    {
                        await DisplayAlert("Sistema", "Se han guardado los cambios", "ok");
                        this.SendBackButtonPressed();
                    }
                    else {
                        await DisplayAlert("Sistema", "Se ha producido un error al contactar el servicio", "ok");
                    }
                    
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Sistema", ex.Message, "ok");
                }

            }
            else
            {
                await DisplayAlert("Sistema", "Alguno de los datos falta por rellenar", "ok");
            }

        }

        private async void DeleteProduct_Clicked(object sender, EventArgs e)
        {
            var deleteProduct = this.BindingContext as Model.Product;
            var answer = await DisplayAlert("Sistema", "Estas seguro que deseas eliminar el item", "Si", "No");
            if (deleteProduct != null && answer)
            {
                if (await DynamoDBManager.GetInstance().Delete<Model.Product>(
                     deleteProduct
                    ))
                {
                    await DisplayAlert("Sistema", "Se ha eliminado el item", "ok");
                    this.SendBackButtonPressed();
                }
                else{
                    await DisplayAlert("Sistema", "Se ha producido un error al contactar el servicio", "ok");
                }
                
            }
        }
    }
}