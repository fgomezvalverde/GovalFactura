using com.Goval.FacturaDigital.Amazon;
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
        List<Model.Product> ActualProducts = null;
        public AddClient()
        {
            InitializeComponent();
        }


        protected async override void OnAppearing()
        {
            App.ShowLoading(true);
            if (ActualProducts == null)
            {
                ActualProducts = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Product>();
                if (ActualProducts != null && ActualProducts.Count != 0)
                {
                    this.BindingContext = new Model.Client() { Products = ActualProducts };
                }
                else
                {
                    this.BindingContext = new Model.Client() { };
                }
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
            }

        }



        public void ChangeProductsAssociated_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(
               new ClientProductSelection(ActualProducts,true)
               );
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