using Plugin.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Toasts
{
    public class ToastRunner
    {
        const int DelayToastSeconds = 3;

        public async static Task ShowWarningToast(String pTitle,String pMessage)
        {
            var notificator = DependencyService.Get<IToastNotificator>();

           

            var result = await notificator.Notify(ToastNotificationType.Warning,pTitle,pMessage,
                TimeSpan.FromSeconds(DelayToastSeconds));
        }


        public async static Task ShowErrorToast(String pTitle, String pMessage)
        {
            var notificator = DependencyService.Get<IToastNotificator>();



            var result = await notificator.Notify(ToastNotificationType.Error, pTitle, pMessage,
                TimeSpan.FromSeconds(DelayToastSeconds));
        }

        public async static Task ShowInformativeToast(String pTitle, String pMessage)
        {
            var notificator = DependencyService.Get<IToastNotificator>();



            var result = await notificator.Notify(ToastNotificationType.Info, pTitle, pMessage,
                TimeSpan.FromSeconds(DelayToastSeconds));
        }


        public async static Task ShowSuccessToast(String pTitle, String pMessage)
        {
            var notificator = DependencyService.Get<IToastNotificator>();



            var result = await notificator.Notify(ToastNotificationType.Success, pTitle, pMessage,
                TimeSpan.FromSeconds(DelayToastSeconds));
        }
    }
}
