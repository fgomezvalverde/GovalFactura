using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.Goval.FacturaDigital.Model;
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
            this.BindingContext =new Model.Product();
        }


        private async void AddProduct_Clicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            var NewProduct = this.BindingContext as Model.Product;
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
                        await DisplayAlert("Sistema", "Se ha Guardado Satifactoriamente", "ok");
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
                    App.ShowLoading(false);
                    await DisplayAlert("Sistema",ex.Message , "ok");
                    }
                    
                }
                else
                {
                App.ShowLoading(false);
                await DisplayAlert("Sistema", "Alguno de los datos falta por rellenar", "ok");
                }
            
        }
    }
}