using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Views
{
    public class BindableLocationPicker : Picker
    {
        public BindableLocationPicker()
        {
            if (Device.OS == TargetPlatform.iOS)
            {
                this.Unfocused += OnUnfocused;
            }
            else
            {
                this.SelectedIndexChanged += OnSelectedIndexChanged; 
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ItemsDictionarySource != null && this.ItemsDictionarySource.Keys.Any())
            {
                SelectedItem = new List<string>(this.ItemsDictionarySource.Keys)[this.SelectedIndex];
            }
        }

        private void OnUnfocused(object sender, FocusEventArgs e)
        {
            OnSelectedIndexChanged(sender, e);
        }

        //Bindable property for the items source
        public static readonly BindableProperty ItemsDictionarySourceProperty =
            BindableProperty.Create<BindablePicker, Dictionary<String, string>>(p => p.ItemsDictionarySource, null, propertyChanged: OnItemsDictionarySourcePropertyChanged);


        public static readonly BindableProperty SelectedItemProperty =
    BindableProperty.Create<BindablePicker, string>(p => p.SelectedItem, null, BindingMode.TwoWay);



        public static readonly BindableProperty FatherPickerNameProperty =
     BindableProperty.Create<BindablePicker, string>(p => p.FatherPickerName, null, BindingMode.TwoWay, propertyChanged: OnFatherPickerNamePropertyChanged);


        public string FatherPickerName
        {
            get { return (string)GetValue(FatherPickerNameProperty); }
            set { SetValue(FatherPickerNameProperty, value); }
        }

        /// <summary>
        /// Gets or sets the items source.
        /// </summary>
        /// <value>
        /// The items source.
        /// </value>
        public Dictionary<string, string> ItemsDictionarySource
        {
            get { return (Dictionary<string, string>)GetValue(ItemsDictionarySourceProperty); }
            set { SetValue(ItemsDictionarySourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>
        /// The selected item.
        /// </value>
        public string SelectedItem
        {
            get { return (string)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }



        #region Methods

        /// <summary>
        /// Called when [items source property changed].
        /// </summary>
        /// <param name="bindable">The bindable.</param>
        /// <param name="value">The value.</param>
        /// <param name="newValue">The new value.</param>
        private static void OnItemsDictionarySourcePropertyChanged(BindableObject bindable, Dictionary<string, string> value, Dictionary<string, string> newValue)
        {
            var picker = (BindablePicker)bindable;
            if (newValue == null)
                return;
            picker.Items.Clear();
            picker.ItemsSource = new List<string>(newValue.Values);
        }

        /// <summary>
        /// Called when [selected item property changed].
        /// </summary>
        /// <param name="bindable">The bindable.</param>
        /// <param name="value">The value.</param>
        /// <param name="newValue">The new value.</param>
        private static void OnSelectedItemPropertyChanged(BindableObject bindable, string value, string newValue)
        {
            var picker = (BindablePicker)bindable;
            if (picker.SelectedIndex == -1)
            {
                if (!string.IsNullOrEmpty(picker.SelectedItem) && picker.ItemsSource != null && picker.ItemsDictionarySource != null)
                {
                    var vValue = picker.ItemsDictionarySource[picker.SelectedItem];
                    picker.SelectedIndex = IndexOf(picker.ItemsSource, vValue);
                }
            }

        }

        private static void OnFatherPickerNamePropertyChanged(BindableObject bindable, string value, string newValue)
        {
            string vFatherName = newValue;

            var vFatherPicker = Xamarin.Forms.Application.Current.MainPage.FindByName<BindableLocationPicker>(vFatherName);

        }



        /// <summary>
        /// Returns the index of the specified string in the collection.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="obj">The string.</param>
        /// <returns>If found returns index otherwise -1</returns>
        private static int IndexOf(IEnumerable self, string obj)
        {
            int index = -1;

            var enumerator = self.GetEnumerator();
            enumerator.Reset();
            int i = 0;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Equals(obj))
                {
                    index = i;
                    break;
                }

                i++;
            }

            return index;
        }


        
        #endregion


    }
}
