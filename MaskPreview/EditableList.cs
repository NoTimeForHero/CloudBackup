using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MaskPreview
{
    internal class EditableList : ListBox
    {
        protected bool isEditing;
        protected Action cancelEditing;

        public event EventHandler<ItemChangeEvent> ItemChanged;
        //public string[] DisplayItems => Items.OfType<TextBlock>().Select(x => x.Text).ToArray();
        // public string[] DisplayItems => Items.OfType<string>().ToArray();
        public string[] DisplayItems
        {
            get => Items.OfType<string>().ToArray();
            set
            {
                Items.Clear();
                foreach (var item in value) Items.Add(item);
            }
        }

        // TODO: Биндинг, когда обновился весь список ViewModel
        public void DumpItemBinding(ObservableCollection<string> target, Action<string[]> onChange)
        {
            target.CollectionChanged += (o, ev) => DisplayItems = target.ToArray();
            ItemChanged += (o, ev) => onChange(DisplayItems);
        }

        public class ItemChangeEvent : EventArgs
        {
            public int Index { get; set; }
            public object OldValue { get; set; }
            public object NewValue { get; set; }
            public bool Removed { get; set; }
        }

        public EditableList()
        {
            SelectionMode = SelectionMode.Single;
            InitializeContextMenu();
            MouseDoubleClick += (o,ev) => EditItem();
            KeyUp += EditableList_KeyUp;
            // for (int i = 0; i < 10; i++) Items.Add($"Line {i}");
        }

        private void EditableList_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    RemoveItem();
                    return;
                case Key.Insert:
                    AddItem();
                    return;
                case Key.Enter:
                    if (isEditing) return;
                    e.Handled = true;
                    EditItem();
                    return;
            }
        }

        // https://stackoverflow.com/a/10463162
        private void FocusOn(int index)
        {
            if (index < 0 || index >= Items.Count) return;
            SelectedIndex = index;
            if (SelectedItem is FrameworkElement fx) fx.Focus();
            var Item = (FrameworkElement)ItemContainerGenerator.ContainerFromItem(SelectedItem);
            Item.Focus();
        }

        private void EditItem()
        {
            if (SelectedIndex == -1) return;
            //if (!(SelectedItem is TextBlock source)) return;
            //var sourceText = source.Text;
            if (!(SelectedItem is string sourceText)) return;
            var targetIndex = SelectedIndex; // Копируем локальную переменную
            var editBox = new TextBox { Text = sourceText };
            editBox.KeyUp += (o, ev) =>
            {
                string newText;
                int? moveTo = null;
                switch (ev.Key)
                {
                    case Key.Up:
                        newText = sourceText;
                        moveTo = targetIndex - 1;
                        break;
                    case Key.Down:
                        newText = sourceText;
                        moveTo = targetIndex + 1;
                        break;
                    case Key.Escape:
                        newText = sourceText;
                        break;
                    case Key.Enter:
                        ev.Handled = true;
                        newText = editBox.Text;
                        break;
                    default:
                        return;
                }
                if (string.IsNullOrEmpty(newText))
                {
                    Items.RemoveAt(targetIndex);
                    FocusOn(targetIndex-1);
                    if (!string.IsNullOrEmpty(sourceText)) // Когда удалили только что созданный элемент
                    {
                        ItemChanged?.Invoke(this, new ItemChangeEvent { Index = targetIndex, OldValue = sourceText, Removed = true });
                    }
                }
                else
                {
                    Items[targetIndex] = newText;
                    // Items[targetIndex] = new TextBlock { Text = newText };
                    FocusOn(moveTo ?? targetIndex);
                    if (sourceText != newText)
                    {
                        ItemChanged?.Invoke(this,
                            new ItemChangeEvent { Index = targetIndex, OldValue = sourceText, NewValue = newText });
                    }
                }
                cancelEditing = null;
                isEditing = false;
            };
            cancelEditing?.Invoke();
            cancelEditing = () =>
            {
                Items[targetIndex] = sourceText;
                // Items[targetIndex] = new TextBlock { Text = sourceText };
                isEditing = false;
            };
            var grid = new Grid();
            grid.Children.Add(editBox);
            grid.Loaded += (o, ev) =>
            {
                var parent = VisualTreeHelper.GetParent(grid) as ContentPresenter;
                if (parent == null) return;
                parent.HorizontalAlignment = HorizontalAlignment.Stretch;
                editBox.CaretIndex = editBox.Text.Length;
                editBox.Focus();
            };
            Items[targetIndex] = grid;
            isEditing = true;
        }

        private void AddItem()
        {
            Items.Add("");
            // Items.Add(new TextBlock());
            SelectedIndex = Items.Count - 1;
            ScrollIntoView(SelectedItem);
            EditItem();
        }

        private void RemoveItem()
        {
            if (SelectedIndex == -1) return;
            var value = SelectedItem;
            var index = SelectedIndex;
            Items.RemoveAt(index);
            FocusOn(index - 1);
            ItemChanged?.Invoke(this, new ItemChangeEvent { Index = index, OldValue = value, Removed = true });
        }

        private void InitializeContextMenu()
        {
            var menu = new ContextMenu();
            var menuAdd = new MenuItem { Header = "Добавить" };
            var menuEdit = new MenuItem { Header = "Редакторировать" };
            var menuRemove = new MenuItem { Header = "Удалить" };
            menu.Items.Add(menuAdd);
            menu.Items.Add(menuEdit);
            menu.Items.Add(menuRemove);
            ContextMenu = menu;

            menuEdit.Click += (o, ev) => EditItem();
            menuAdd.Click += (o, ev) => AddItem();
            menuRemove.Click += (o, ev) => RemoveItem();
        }
    }
}
