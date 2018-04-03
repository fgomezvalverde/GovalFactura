using Acr.UserDialogs;
using com.Goval.FacturaDigital.Abstraction.DependencyServices;
using com.Goval.FacturaDigital.Amazon;
using com.Goval.FacturaDigital.DataContracts.Model;
using com.Goval.FacturaDigital.Test;
using com.Goval.FacturaDigital.Utils;
using Newtonsoft.Json;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace com.Goval.FacturaDigital
{
    public partial class App : Application
    {
        
        public static Page RootPage;
        public static Boolean AdminPrivilegies = true;
        public static int StarterBillNumber = 2500;


        public static User ActualUser = null;
        public static AppConfiguration ActualUserConfiguration = null;
        public static string SSOT = string.Empty;

        public static string ActualUserDBKey = "ActualUserDBKey";
        public static string ActualSSOTDBKey = "ActualSSOTDBKey";
        public static string ActualUserConfigurationDBKey = "ActualUserConfigurationDBKey";
        

        public App()
        {
            InitializeComponent();
            SetInitialUser();
/*#if DEBUG
            AdminPrivilegies = true;
#endif*/
            CrossConnectivity.Current.ConnectivityChanged += ConnectivityChanged;


            RootPage = new Pages.MasterDetail.RootPage();
            MainPage = RootPage;
            
        }

        private async void ConnectivityChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                await Toasts.ToastRunner.ShowSuccessToast("Sistema", "Se ha recuperado la conexión");
            }
            else
            {
                await Toasts.ToastRunner.ShowWarningToast("Sistema", "Se ha perdidio la conexión, es posible que el app no funcione bien");
            }
        }

        protected async override void OnStart()
        {
            // Handle when your app starts
            

            //Get the ConfigurationFile
            /*var configObj = await DynamoDBManager.GetInstance().GetItemsAsync<Model.SystemConfiguration>();
            if (configObj != null && configObj.Count != 0)
            {
                Utils.ConfigurationConstants.ConfigurationObject = configObj.FirstOrDefault();
                Utils.ConfigurationConstants.PDFGeneratorKey = configObj.FirstOrDefault().PdfGeneratorKey;
                Utils.ConfigurationConstants.StarterBillNumber = configObj.FirstOrDefault().StartBillNumber;
            }
            else
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Sistema", "No se ha podido cargar el archivo de configuración, vuelva a iniciar el APP", "Ok");
                throw new Exception();
            }*/
            //await BillSecurity.SaveBaseBill();

            /*if (App.ActualUser == null)
            {
                await MainPage.Navigation.PushModalAsync(
               new Pages.Login.LoginPage()
               );
            }*/
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public static void ShowLoading(Boolean pIsRunning, string pMessage = "Cargando")
        {
            if (pIsRunning)
            {
                UserDialogs.Instance.ShowLoading(pMessage);
            }
            else
            {
                UserDialogs.Instance.Loading().Hide();
            }
        }

        private void SetInitialUser()
        {
            try
            {
                if (DependencyService.Get<ISharedPreferences>().Contains(App.ActualUserDBKey) && DependencyService.Get<ISharedPreferences>().Contains(App.ActualUserConfigurationDBKey) &&
                    DependencyService.Get<ISharedPreferences>().Contains(App.ActualSSOTDBKey))
                {
                    string jsonUser = DependencyService.Get<ISharedPreferences>().GetString(App.ActualUserDBKey);
                    string jsonUserConfiguration = DependencyService.Get<ISharedPreferences>().GetString(App.ActualUserConfigurationDBKey);
                    App.SSOT = DependencyService.Get<ISharedPreferences>().GetString(App.ActualSSOTDBKey);
                    App.ActualUser = JsonConvert.DeserializeObject<User>(jsonUser);
                    App.ActualUserConfiguration = JsonConvert.DeserializeObject<AppConfiguration>(jsonUserConfiguration);
                    
                }
            }
            catch (Exception)
            {
                Toasts.ToastRunner.ShowErrorToast("Sistema", "Error al cargar usuario Inicial");

            }
        }
    }


    
}
