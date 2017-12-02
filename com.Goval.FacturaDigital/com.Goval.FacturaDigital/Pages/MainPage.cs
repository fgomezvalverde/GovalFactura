using com.Goval.FacturaDigital.Pages.Bill;
using com.Goval.FacturaDigital.Pages.Client;
using com.Goval.FacturaDigital.Pages.Configuracion;
using com.Goval.FacturaDigital.Pages.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Pages
{
    public class MainPage : TabbedPage
    {
        public MainPage()
        {
            var billPage = new BillList();
            billPage.Title = "Facturas";

            var clientPage = new ClientList();
            clientPage.Title = "Clientes";

            var productPage = new ProductList();
            productPage.Title = "Productos";

            var configurationPage = new ConfigurationPage();
            configurationPage.Title = "Configuración";

            Children.Add(
                new NavigationPage(billPage) { Title= "Facturas" });
            Children.Add(
                new NavigationPage(clientPage) { Title = "Clientes" });
            Children.Add(
                new NavigationPage(productPage) { Title = "Productos" });

            Children.Add(
                new NavigationPage(configurationPage) { Title = "Config" });
        }
    }
}