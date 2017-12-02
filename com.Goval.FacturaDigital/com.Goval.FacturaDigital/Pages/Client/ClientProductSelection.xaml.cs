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
    public partial class ClientProductSelection : ContentPage
    {
        public static Boolean IsSwitchAvailable = true;
        public ClientProductSelection(List<Model.Product> pProducts,Boolean pNewClient)
        {
            IsSwitchAvailable = true;
            BindingContext = pProducts;
            InitializeComponent();
            if (!pNewClient && !App.AdminPrivilegies)
            {
                IsSwitchAvailable = false;
            }
        }
        private void ProductListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ProductListView.SelectedItem = null;
        }
    }
}