using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Utils
{
    public static class ExtensionMethods
    {
        public static async Task PopUpAllModals(this INavigation pNavigation)
        {
            int numModals = Application.Current.MainPage.Navigation.ModalStack.Count;
            for (int currModal = 0; currModal < numModals; currModal++)
            {
                await Application.Current.MainPage.Navigation.PopModalAsync(false);
            }
        }
    }
}
