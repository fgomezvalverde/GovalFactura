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
    public partial class ClientDetail : ContentPage
    {
        Model.Client ActualClient;
        public ClientDetail(Model.Client pClient)
        {
            InitializeComponent();
            ActualClient = pClient;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await SetProducts();
            this.BindingContext = ActualClient;
        }

        private async void SaveClient_Clicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            var NewClient = this.BindingContext as Model.Client;
            if (NewClient != null && !string.IsNullOrEmpty(NewClient.ClientId) && !string.IsNullOrEmpty(NewClient.Name))
            {
                try
                {
                    NewClient.RemoveOnUsedProducts();
                    if (await DynamoDBManager.GetInstance().SaveAsync<Model.Client>(
                     NewClient
                    ))
                    {
                        App.ShowLoading(false);
                        await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se han guardado los cambios");
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
            }

        }

        private async void DeleteClient_Clicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            var deleteClient = this.BindingContext as Model.Client;
            var answer = await DisplayAlert("Sistema", "Estas seguro que deseas eliminar el item", "Si", "No");
            if (deleteClient != null && answer)
            {
                if (await DynamoDBManager.GetInstance().Delete<Model.Client>(
                     deleteClient
                    ))
                {
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se ha eliminado el item");
                    this.SendBackButtonPressed();
                }
                else
                {
                    App.ShowLoading(false);
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", "Se ha producido un error al contactar el servicio");
                }
                
            }
        }

        public void ChangeProductsAssociated_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(
               new ClientProductSelection(ActualClient.Products,false)
               );
        }


        private async Task SetProducts()
        {
            App.ShowLoading(true);
            var productList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Product>();
            if (productList != null || productList.Count != 0)
            {
                foreach (var product in ActualClient.Products)
                {
                    var findedProduct = productList.First(d => d.Code == product.Code);
                    if (findedProduct != null)
                    {
                        findedProduct.IsUsed = product.IsUsed;
                    }
                    else
                    {
                        App.ShowLoading(false);
                        await Toasts.ToastRunner.ShowErrorToast("Sistema", "El item " + product.Description + " ha sido elimado, y ya era referencia a este cliente");
                    }
                }
                ActualClient.Products = productList;
            }
            else
            {
                App.ShowLoading(false);
                await Toasts.ToastRunner.ShowErrorToast("Sistema", "No se han podido traer los productos del server");
            }
            App.ShowLoading(false);
        }

    }
}