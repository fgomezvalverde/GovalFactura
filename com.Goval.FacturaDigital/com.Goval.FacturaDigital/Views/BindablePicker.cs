using com.Goval.FacturaDigital.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Views
{
    public class BindablePicker : Picker
    {
        public BindablePicker()
        {
            if (Device.OS == TargetPlatform.iOS)
            {
                this.Unfocused += OnUnfocused;
            }
            else
            {
                this.SelectedIndexChanged += OnSelectedIndexChanged; ;
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedItem = new List<string>(this.ItemsDictionarySource.Keys)[this.SelectedIndex];
        }

        private void OnUnfocused(object sender, FocusEventArgs e)
        {
            OnSelectedIndexChanged(sender, e);
        }
        #region Fields

        //Bindable property for the items source
        public static readonly BindableProperty ItemsDictionarySourceProperty =
            BindableProperty.Create<BindablePicker, Dictionary<String,string>>(p => p.ItemsDictionarySource, null, propertyChanged: OnItemsDictionarySourcePropertyChanged);

        //Bindable property for the selected item
        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create<BindablePicker, string>(p => p.SelectedItem, null, BindingMode.TwoWay, propertyChanged: OnSelectedItemPropertyChanged);


        public static readonly BindableProperty FatherItemCodeProperty =
            BindableProperty.Create<BindablePicker, string>(p => p.FatherItemCode, null, BindingMode.TwoWay, propertyChanged: OnFatherItemCodePropertyChanged);


        public static readonly BindableProperty FatherTypeCodeProperty =
            BindableProperty.Create<BindablePicker, string>(p => p.FatherTypeCode, null, BindingMode.TwoWay);

        #endregion

        #region Properties

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

        /// <summary>
        /// Gets or sets the items source.
        /// </summary>
        /// <value>
        /// The items source.
        /// </value>
        public Dictionary<string,string> ItemsDictionarySource
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

        #endregion

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
            /*var notifyCollection = newValue as INotifyCollectionChanged;
            if (notifyCollection != null)
            {
                notifyCollection.CollectionChanged += (sender, args) =>
                {
                    if (args.NewItems != null)
                    {
                        foreach (var newItem in args.NewItems)
                        {
                            picker.Items.Add((newItem ?? "").ToString());
                        }
                    }
                    if (args.OldItems != null)
                    {
                        foreach (var oldItem in args.OldItems)
                        {
                            picker.Items.Remove((oldItem ?? "").ToString());
                        }
                    }
                };
            }

            if (newValue == null)
                return;

            picker.Items.Clear();

            foreach (var item in newValue)
                picker.Items.Add((item ?? "").ToString());*/
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
                if (!string.IsNullOrEmpty(picker.SelectedItem) && picker.ItemsSource != null)
                {
                    var vValue = picker.ItemsDictionarySource[picker.SelectedItem];
                    picker.SelectedIndex = IndexOf(picker.ItemsSource, vValue);
                }
            }
                
        }

        

        private static void OnFatherItemCodePropertyChanged(BindableObject bindable, string value, string newValue)
        {
            
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
