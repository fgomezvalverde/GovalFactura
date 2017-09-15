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
using com.Goval.FacturaDigital.Droid.DependencyServices;
using com.Goval.FacturaDigital.Abstraction.DependencyServices;
using Android.Content.Res;
using System.IO;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(FileManagement))]
namespace com.Goval.FacturaDigital.Droid.DependencyServices
{
    public class FileManagement : IFileManagement
    {
        public string OpenPlainTextFile(string pFileName)
        {
            AssetManager assets = Forms.Context.Assets;
            string result = string.Empty;
            using (StreamReader sr = new StreamReader(assets.Open(pFileName)))
            {
                result = sr.ReadToEnd();
            }
            return result;
        }
    }
}