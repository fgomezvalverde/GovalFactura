using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Client
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClientDetail : ContentPage
    {
        DataContracts.Model.Client ActualClient;
        Boolean FirstLoading = true;
        public ClientDetail(DataContracts.Model.Client pClient)
        {
            InitializeComponent();
            ActualClient = pClient;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (FirstLoading)
            {
                await SetProducts();
                FirstLoading = false;
            }
            
            this.BindingContext = ActualClient;
        }

        private async void SaveClient_Clicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            var vSetClientObj = this.BindingContext as DataContracts.Model.Client;
            try
            {
                vSetClientObj.RemoveOnUsedProducts();
                var vSetClient = new BusinessProxy.Client.SetUserClient();
                var vSetedResponse = await vSetClient.GetDataAsync(
                    new BusinessProxy.Models.ClientRequest
                    {
                        SSOT = App.SSOT,
                        UserId = App.ActualUser.UserId,
                        UserClient = vSetClientObj
                    });
                if (vSetedResponse != null)
                {
                    if (vSetedResponse.IsSuccessful)
                    {
                        App.ShowLoading(false);
                        await Navigation.PopAsync();
                        await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se ha modificado el item");
                    }
                    else
                    {
                        App.ShowLoading(false);
                        await Toasts.ToastRunner.ShowSuccessToast("Sistema", vSetedResponse.UserMessage);
                        await DisplayAlert("", vSetedResponse.TechnicalMessage, "Ok");
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
                App.ShowLoading(false);
                await DisplayAlert("", ex.Message, "Ok");
            }
        }

        private async void DeleteClient_Clicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            var vDeleteClientObj = this.BindingContext as DataContracts.Model.Client;
            var answer = await DisplayAlert("Sistema", "Estas seguro que deseas eliminar el item", "Si", "No");
            if (vDeleteClientObj != null && answer)
            {
                var vDeleteClient = new BusinessProxy.Client.DeleteUserClient();
                var vDeletedResponse = await vDeleteClient.GetDataAsync(
                    new BusinessProxy.Models.ClientRequest
                    {
                        SSOT = App.SSOT,
                        UserId = App.ActualUser.UserId,
                        UserClient = vDeleteClientObj
                    });
                if (vDeletedResponse != null)
                {
                    if (vDeletedResponse.IsSuccessful)
                    {
                        App.ShowLoading(false);
                        await Navigation.PopAsync();
                        await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se ha eliminado el item");
                    }
                    else
                    {
                        App.ShowLoading(false);
                        await Toasts.ToastRunner.ShowSuccessToast("Sistema", vDeletedResponse.UserMessage);
                        await DisplayAlert("", vDeletedResponse.TechnicalMessage, "Ok");
                    }
                }
                else
                {
                    App.ShowLoading(false);
                    await DisplayAlert("", "Respuesta Null", "Ok");
                }

            }
        }

        public async void ChangeProductsAssociated_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(
               new ClientProductSelection(ActualClient.ClientProducts,true)
               );
        }


        private async Task SetProducts()
        {
            App.ShowLoading(true);
            var vGetUserProductsClient = new BusinessProxy.Product.GetUserProducts();
            var vProductResponse = await vGetUserProductsClient.GetDataAsync(
                new BusinessProxy.Models.ProductRequest
                {
                    SSOT = App.SSOT,
                    UserId = App.ActualUser.UserId
                });

            if (vProductResponse != null)
            {
                if (vProductResponse.IsSuccessful)
                {
                    foreach (var vProduct in ActualClient.ClientProducts)
                    {
                        var vFindedProduct = vProductResponse.UserProducts.First(d => d.ProductId.Equals( vProduct.ProductId));
                        if (vFindedProduct != null)
                        {
                            vFindedProduct.IsUsed = true;
                        }
                    }
                    ActualClient.ClientProducts = vProductResponse.UserProducts;
                    App.ShowLoading(false);
                }
                else
                {
                    App.ShowLoading(false);
                    await DisplayAlert("", vProductResponse.TechnicalMessage, "Ok");
                }
            }
            else
            {
                App.ShowLoading(false);
                await DisplayAlert("", "Respuesta Null de los Productos", "Ok");
            }
            
        }

    }
}