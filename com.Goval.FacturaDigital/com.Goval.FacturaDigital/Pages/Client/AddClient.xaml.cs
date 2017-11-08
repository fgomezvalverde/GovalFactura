using com.Goval.FacturaDigital.Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddClient : ContentPage
    {
        public AddClient()
        {
            InitializeComponent();
            
        }


        protected async override void OnAppearing()
        {
            App.ShowLoading(true);
            var productList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Product>();
            if (productList != null && productList.Count != 0)
            {
                this.BindingContext = new Model.Client() {Products= productList};
            }
            else
            {
                this.BindingContext = new Model.Client() { };
            }
            base.OnAppearing();
            App.ShowLoading(false);
        }

        private async void AddClient_Clicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            var NewClient = this.BindingContext as Model.Client;
            if (NewClient != null && !string.IsNullOrEmpty(NewClient.ClientId) && !string.IsNullOrEmpty(NewClient.Name))
            {
                try
                {
                    NewClient.Products = RemoveUnUsedProduct(NewClient.Products);
                    if (await DynamoDBManager.GetInstance().SaveAsync<Model.Client>(
                     NewClient
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
                    await DisplayAlert("Sistema", ex.Message, "ok");
                }

            }
            else
            {
                App.ShowLoading(false);
                await DisplayAlert("Sistema", "Alguno de los datos falta por rellenar", "ok");
            }

        }

        private void ProductListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            //ProductListView.SelectedItem = null;
        }


        private List<Model.Product> RemoveUnUsedProduct(List<Model.Product> pProductList)
        {
            List<Model.Product> resultList = new List<Model.Product>();
            if (pProductList != null)
            {
                foreach (var product in pProductList)
                {
                    if (product.IsUsed)
                    {
                        resultList.Add(product);
                    }
                }
            }

            return resultList;
        }
    }
}