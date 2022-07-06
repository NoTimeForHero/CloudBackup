using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MaskPreview
{
    // TODO: Наследоваться от UserControl вместо ComboBox
    internal class EntryCombo : ComboBox
    {
        private readonly ComboBoxItem itemNew = new ComboBoxItem { Content = "Новая конфигурация" };
        private readonly ComboBoxItem itemSelect = new ComboBoxItem { Content = "Выбрать config.yml" };
        private List<string> _items = new List<string>();

        public IEnumerable<string> TypedItems
        {
            get => _items;
            set
            {
                _items = value.ToList();
                BuildList();
            }
        }

        public event Action onFileSelect;
        public event Action<string> onSelect;

        public EntryCombo()
        { ;
            BuildList();
            SelectedIndex = 0;
            SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedValue == null) return;
            if (ReferenceEquals(SelectedValue, itemSelect))
            {
                onFileSelect?.Invoke();
                SelectedItem = itemNew;
            }
            else if (ReferenceEquals(SelectedValue, itemNew)) onSelect?.Invoke(null);
            else if (SelectedValue is string strValue) onSelect?.Invoke(strValue);
            else throw new ApplicationException($"Unknown SelectedValue: {SelectedValue}");
        }

        private void BuildList()
        {
            Items.Clear();
            Items.Add(itemNew);
            if (_items.Count > 0)
            {
                Items.Add(new Separator());
                foreach (var item in _items) Items.Add(item);
                Items.Add(new Separator());
            }
            Items.Add(itemSelect);
            SelectedItem = itemNew;
        }
    }
}
