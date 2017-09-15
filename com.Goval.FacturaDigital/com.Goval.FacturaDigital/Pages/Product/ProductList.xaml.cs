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
            App.ShowLoading(true);
            base.OnAppearing();

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

            var productList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Product>();
            if (productList != null && productList.Count != 0)
            {
                 ProductListView.ItemsSource = productList;
            }
            App.ShowLoading(false);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
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