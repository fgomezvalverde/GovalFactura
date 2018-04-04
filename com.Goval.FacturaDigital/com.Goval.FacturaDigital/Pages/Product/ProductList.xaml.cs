using com.Goval.FacturaDigital.Amazon;
using Plugin.Connectivity;
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
    public partial class ProductList : ContentPage
    {
        List<DataContracts.Model.Product> _ProductList;
        public ProductList()
        {
            InitializeComponent();
            
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();


            ProductListView.ItemsSource = new List<DataContracts.Model.Product>()
            {
                new DataContracts.Model.Product{ Name="TESTPRODUCT1Name",Description="DescriptionTEST",Price=9999,BarCode="pruebaCode"}
            };

            //App.ShowLoading(true);
            if (App.AdminPrivilegies && this.ToolbarItems.Count ==0)
            {
                ToolbarItem item = new ToolbarItem
                {
                    Order = ToolbarItemOrder.Primary,
                    Icon = "ic_action_add.png",
                    Text = "Agregar",
                    Priority = 0,

                };
                item.Clicked += AddProduct_Clicked;
                this.ToolbarItems.Add(item);
            }
            /*if (!string.IsNullOrEmpty(App.SSOT) && App.ActualUser != null)
            {
                var vGetUserProductsClient = new BusinessProxy.Product.GetUserProducts();
                var vProductResponse = await vGetUserProductsClient.GetDataAsync(
                    new BusinessProxy.Models.ProductRequest
                    {
                        SSOT = App.SSOT,
                        UserId = App.ActualUser.UserId
                    });
                _ProductList = vProductResponse.UserProducts;
            }
            
            if (_ProductList != null && _ProductList.Count != 0)
            {
                 ProductListView.ItemsSource = _ProductList;
            }
            App.ShowLoading(false);*/
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private async void AddProduct_Clicked(object sender, EventArgs e)
        {

            if (CrossConnectivity.Current.IsConnected)
            {

                await Navigation.PushAsync(new AddProduct ());
            }
            else
            {
                await Toasts.ToastRunner.ShowErrorToast("Sistema", "No hay internet, vuelva cuando tenga conexión");
            }
        }

        private void ProductListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var detailProduct = e.SelectedItem as DataContracts.Model.Product;
            Navigation.PushAsync(new ProductDetail(detailProduct));
        }
    }
}