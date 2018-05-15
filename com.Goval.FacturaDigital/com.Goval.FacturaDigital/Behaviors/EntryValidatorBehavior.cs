using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace com.Goval.FacturaDigital.Behaviors
{
    public class EntryValidatorBehavior : Behavior<Entry>
    {
        public int MaxLength { get; set; } = 150;

        public string ValidChars { get; set; } = string.Empty;

        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += OnEntryTextChanged;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged -= OnEntryTextChanged;
        }

        void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;

            // if Entry text is longer then valid length
            if (entry != null && !string.IsNullOrEmpty(entry.Text))
            {
                if (!string.IsNullOrEmpty(ValidChars))
                {
                    if (entry.Text.Length > this.MaxLength | !ValidChars.Contains(entry.Text.Substring(entry.Text.Length - 1, 1)))
                    {
                        entry.TextChanged -= OnEntryTextChanged;
                        entry.Text = e.OldTextValue;
                        entry.TextChanged += OnEntryTextChanged;
                    }
                }
                else
                {
                    if (entry.Text.Length > this.MaxLength)
                    {
                        entry.TextChanged -= OnEntryTextChanged;
                        entry.Text = e.OldTextValue;
                        entry.TextChanged += OnEntryTextChanged;
                    }
                }
            }
        }
    }
}
