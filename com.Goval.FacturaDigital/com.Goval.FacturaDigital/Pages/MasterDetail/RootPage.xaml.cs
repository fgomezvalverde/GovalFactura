
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
    public partial class RootPage : MasterDetailPage
    {
        public RootPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("#455A64");
            masterPage.ListView.ItemSelected += OnItemSelected;

            if (Device.RuntimePlatform == Device.Windows)
            {
                MasterBehavior = MasterBehavior.Popover;
            }

            Detail = new NavigationPage(new Bill.BillList());
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            if (item != null)
            {
                if (item.TargetType.Equals(PageType.Logout))
                {
                    DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().RemoveString(App.UserDefaultKey);
                    await Navigation.PushModalAsync(new Login.LoginPage());
                }
                else
                {
                    Page newPage = null;

                    switch (item.TargetType)
                    {
                        case PageType.BillList:
                            newPage = new Bill.BillList();
                            break;
                        case PageType.ClientList:
                            newPage = new Client.ClientList();
                            break;
                        case PageType.ProductList:
                            newPage = new Product.ProductList();
                            break;


                        case PageType.Configuration:
                            newPage = new Configuracion.ConfigurationPage();
                            break;
                        default:
                            newPage = new Bill.BillList();
                            break;
                    }

                    Detail = new NavigationPage(newPage);
                    masterPage.ListView.SelectedItem = null;
                    IsPresented = false;
                }

                
            }
        }
    }
}