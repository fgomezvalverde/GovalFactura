using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Acr.UserDialogs;
using Plugin.Toasts;
using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Droid
{
    [Activity(Label = "Factura Goval Beta", Icon = "@drawable/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            UserDialogs.Init(this);
            DependencyService.Register<ToastNotificatorImplementation>();
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);
            ToastNotificatorImplementation.Init(this);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}

