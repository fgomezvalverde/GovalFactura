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
        List<Model.Product> _ProductList;
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

            _ProductList = await DynamoDBManager.GetInstance().GetItemsAsync<Model.Product>();
            if (_ProductList != null && _ProductList.Count != 0)
            {
                 ProductListView.ItemsSource = _ProductList;
            }
            App.ShowLoading(false);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private void AddProduct_Clicked(object sender, EventArgs e)
        {
            int newId = 1;

            if (CrossConnectivity.Current.IsConnected)
            {
                if (_ProductList != null && _ProductList.Count != 0)
                    newId = _ProductList.Max(t => t.Id) + 1;
                Navigation.PushAsync(
                    new AddProduct (new Model.Product { Id= newId})
                    );
            }
            else
            {
                DisplayAlert("Sistema", "No hay internet", "Ok");
            }
        }

        private void ProductListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var detailProduct = e.SelectedItem as Model.Product;
            Navigation.PushAsync(
               new ProductDetail(detailProduct)
               );
        }
    }
}