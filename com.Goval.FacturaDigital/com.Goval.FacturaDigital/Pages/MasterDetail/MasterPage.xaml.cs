using com.Goval.FacturaDigital.Pages.Bill;
using com.Goval.FacturaDigital.Pages.Client;
using com.Goval.FacturaDigital.Pages.Configuracion;
using com.Goval.FacturaDigital.Pages.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Pages.MasterDetail
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterPage : ContentPage
    {
        public ListView ListView { get { return listView; } }

        public MasterPage()
        {
            InitializeComponent();

            var masterPageItems = new List<MasterPageItem>();
            /*masterPageItems.Add(new MasterPageItem
            {
                Title = "Facturas",
                IconSource = "contacts.png",
                TargetType = PageType.BillList
            });
            masterPageItems.Add(new MasterPageItem
            {
                Title = "Clientes",
                IconSource = "todo.png",
                TargetType = PageType.ClientList
            });
            masterPageItems.Add(new MasterPageItem
            {
                Title = "Productos",
                IconSource = "reminders.png",
                TargetType = PageType.ProductList
            });
            masterPageItems.Add(new MasterPageItem
            {
                Title = "Configuración",
                IconSource = "reminders.png",
                TargetType = PageType.Configuration
            });*/

            masterPageItems.Add(new MasterPageItem
            {
                Title = "Logout",
                IconSource = "reminders.png",
                TargetType = PageType.Logout
            });

            listView.ItemsSource = masterPageItems;
        }
    }
}