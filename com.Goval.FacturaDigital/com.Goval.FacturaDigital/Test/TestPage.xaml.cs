using com.Goval.FacturaDigital.Abstraction.DependencyServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace com.Goval.FacturaDigital.Test
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        public TestPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            DependencyService.Get<IMailService>().SendMail("Prueba", "Esto es una prueba",
                new List<string>() { "fabiangomezvalverde@gmail.com" });
        }
    }
}