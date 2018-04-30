
using com.Goval.FacturaDigital.Pages.User;
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
                await ChangePage(item);
                
            }
        }

        public async Task ChangePage(MasterPageItem pPageRequest)
        {
            if (pPageRequest.TargetType.Equals(PageType.Logout))
            {
                DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().RemoveString(App.ActualUserDBKey);
                DependencyService.Get<Abstraction.DependencyServices.ISharedPreferences>().RemoveString(App.ActualUserConfigurationDBKey);
                
                await Navigation.PushModalAsync(new Login.LoginPage());
                masterPage.ListView.SelectedItem = null;
                this.IsPresented = false;
            }
            else
            {
                Page newPage = null;

                switch (pPageRequest.TargetType)
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
                    case PageType.ValidateHaciendaUser:
                        await Navigation.PushModalAsync(new HaciendaRegistration(App.ActualUser));
                        masterPage.ListView.SelectedItem = null;
                        this.IsPresented = false;
                        return;
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