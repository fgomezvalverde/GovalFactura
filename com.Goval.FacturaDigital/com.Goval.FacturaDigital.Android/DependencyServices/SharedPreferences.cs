using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using com.Goval.FacturaDigital.Abstraction.DependencyServices;
using Android.Preferences;

[assembly: Xamarin.Forms.Dependency(typeof(com.Goval.FacturaDigital.Droid.DependencyServices.SharedPreferences))]
namespace com.Goval.FacturaDigital.Droid.DependencyServices
{
    public class SharedPreferences : com.Goval.FacturaDigital.Abstraction.DependencyServices.ISharedPreferences
    {
         bool Abstraction.DependencyServices.ISharedPreferences.Contains(string pKey)
        {
            Android.Content.ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Xamarin.Forms.Forms.Context);
            return prefs.Contains(pKey); 
        }
         string Abstraction.DependencyServices.ISharedPreferences.GetString(string pKey)
        {
            Android.Content.ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Xamarin.Forms.Forms.Context);
            return prefs.GetString(pKey, string.Empty); 

        }

         void Abstraction.DependencyServices.ISharedPreferences.RemoveString(string pkey)
        {
            Android.Content.ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Xamarin.Forms.Forms.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.Remove(pkey);
            editor.Apply();
        }

         void Abstraction.DependencyServices.ISharedPreferences.SaveString(string pKey, string pValue)
        {
            Android.Content.ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Xamarin.Forms.Forms.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString(pKey, pValue);
            editor.Apply();
        }
    }
}