using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Views
{
    public class BindableLocationPicker : Picker
    {
        public static readonly BindableProperty SelectedItemProperty =
    BindableProperty.Create<BindablePicker, string>(p => p.SelectedItem, null, BindingMode.TwoWay);


        public static readonly BindableProperty FatherItemCodeProperty =
            BindableProperty.Create<BindablePicker, string>(p => p.FatherItemCode, null, BindingMode.TwoWay, propertyChanged: OnFatherItemCodePropertyChanged);


        public static readonly BindableProperty FatherTypeCodeProperty =
     BindableProperty.Create<BindablePicker, string>(p => p.FatherTypeCode, null, BindingMode.TwoWay);


        public string FatherItemCode
        {
            get { return (string)GetValue(FatherItemCodeProperty); }
            set { SetValue(FatherItemCodeProperty, value); }
        }
        public string FatherTypeCode
        {
            get { return (string)GetValue(FatherTypeCodeProperty); }
            set { SetValue(FatherTypeCodeProperty, value); }
        }

        public string SelectedItem
        {
            get { return (string)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private static void OnFatherItemCodePropertyChanged(BindableObject bindable, string value, string newValue)
        {
            string testing = newValue;
        }
    }
}
