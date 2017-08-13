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
    public partial class ProductList : ContentPage
    {
        public ProductList()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var productList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Product>();
            if (productList != null || productList.Count != 0)
            {
                 ProductListView.ItemsSource = productList;
            }
        }

        private void AddProduct_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(
                new AddProduct()
                );
        }

        private void ProductListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var detailProduct = e.SelectedItem as Model.Product;
            Navigation.PushModalAsync(
               new ProductDetail(detailProduct)
               );
        }
    }
}