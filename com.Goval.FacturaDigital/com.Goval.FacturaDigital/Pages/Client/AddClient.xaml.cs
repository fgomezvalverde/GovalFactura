using com.Goval.FacturaDigital.Amazon;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [AddINotifyPropertyChangedInterface]
    public partial class AddClient : ContentPage
    {
        List<DataContracts.Model.Product> ActualProducts = null;
        public AddClient()
        {
            InitializeComponent();
        }


        protected async override void OnAppearing()
        {
            base.OnAppearing();
            App.ShowLoading(true);
            if (ActualProducts == null)
            {
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
                        ActualProducts = vProductResponse.UserProducts;
                        this.BindingContext = new DataContracts.Model.Client() { ClientProducts = ActualProducts };
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
            
            App.ShowLoading(false);
        }

        private async void AddClient_Clicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            var NewClient = this.BindingContext as DataContracts.Model.Client;
            var test = JsonConvert.SerializeObject(NewClient);
            if (NewClient != null && !string.IsNullOrEmpty(NewClient.Name))
            {
                try
                {
                    NewClient.RemoveOnUsedProducts();
                    var vAddClient = new BusinessProxy.Client.AddUserClient();
                    var vAddResponse = await vAddClient.GetDataAsync(
                        new BusinessProxy.Models.ClientRequest
                        {
                            SSOT = App.SSOT,
                            UserId = App.ActualUser.UserId,
                            UserClient = NewClient
                        });
                    if (vAddResponse != null)
                    {
                        if (vAddResponse.IsSuccessful)
                        {
                            App.ShowLoading(false);
                            await Navigation.PopAsync();
                            await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se ha Agregado el item");
                        }
                        else
                        {
                            App.ShowLoading(false);
                            await Toasts.ToastRunner.ShowSuccessToast("Sistema", vAddResponse.UserMessage);
                            await DisplayAlert("", vAddResponse.TechnicalMessage, "Ok");
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
                    await Toasts.ToastRunner.ShowErrorToast("Sistema", ex.Message);
                }

            }
            else
            {
                App.ShowLoading(false);
                await Toasts.ToastRunner.ShowInformativeToast("Sistema", "Alguno de los datos falta por rellenar");
            }

        }



        public void ChangeProductsAssociated_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(
               new ClientProductSelection(ActualProducts,true)
               );
        }

        
    }
}