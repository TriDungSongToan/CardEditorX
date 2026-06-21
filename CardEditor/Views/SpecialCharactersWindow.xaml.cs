using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Diagnostics;
using System.Collections.Generic;
using CardEditor.Models;
using CardEditor.ViewModels;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for SpecialCharactersWindow.xaml
    /// </summary>
    public partial class SpecialCharactersWindow : Window
    {
        private readonly FrameworkElement _targetControl;
        public SpecialCharactersWindow(FrameworkElement targetControl)
        {
            InitializeComponent();
            DataContext = UIConfigViewModel.Instance;
            _targetControl = targetControl;
            if (_targetControl == null)
            {
                Debug.WriteLine("SpecialCharactersWindow: targetControl is null");
                Close();
                return;
            }
            LoadCategories();
            UpdateCharacterList();
            Debug.WriteLine($"SpecialCharactersWindow opened for control: {_targetControl.GetType().Name} (Name: {(_targetControl as Control)?.Name})");
        }

        private void LoadCategories()
        {
            var categories = CardDataViewModel.Instance.SpecialCharacters
                .Select(c => c.Category)
                .Distinct()
                .OrderBy(c => c)
                .Prepend("All")
                .ToList();
            cmbCategory.ItemsSource = categories;
            cmbCategory.SelectedIndex = 0;
        }

        private void UpdateCharacterList()
        {
            var characters = CardDataViewModel.Instance.SpecialCharacters.AsEnumerable();
            if (cmbCategory.SelectedItem is string category && category != "All")
            {
                characters = characters.Where(c => c.Category == category);
            }

            string searchText = txtSearch.Text?.ToLower() ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                characters = characters.Where(c =>
                    c.Character.ToLower().Contains(searchText) ||
                    c.Description.ToLower().Contains(searchText));
            }

            var characterList = characters.ToList();
            lstCharacters.ItemsSource = characterList;
            btnInsert.IsEnabled = characterList.Count > 0;
            Debug.WriteLine($"UpdateCharacterList: {characterList.Count} items");
            foreach (var item in characterList)
            {
                Debug.WriteLine($"Character: {item.Character}, Category: {item.Category}, Description: {item.Description}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCharacterList();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCharacterList();
        }

        private void BtnInsert_Click(object sender, RoutedEventArgs e)
        {
            if (lstCharacters.SelectedItem is CharacterItem selected)
            {
                InsertCharacter(selected.Character);
                Close();
            }
        }

        private void InsertCharacter(string character)
        {
            Debug.WriteLine($"Inserting '{character}' into {_targetControl.GetType().Name} (Name: {(_targetControl as Control)?.Name})");

            switch (_targetControl)
            {
                case RichTextBox richTextBox:
                    richTextBox.Document.InsertText(character, richTextBox.CaretPosition);
                    break;
                case TextBox textBox:
                    int caretIndex = textBox.CaretIndex;
                    textBox.Text = textBox.Text.Insert(caretIndex, character);
                    textBox.CaretIndex = caretIndex + character.Length;
                    break;
                case ICSharpCode.AvalonEdit.TextEditor textEditor:
                    textEditor.Document.Insert(textEditor.CaretOffset, character);
                    break;
                default:
                    Debug.WriteLine($"Unsupported control type: {_targetControl?.GetType().Name}");
                    break;
            }
        }

        private void lstCharacters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCharacters.SelectedItem is CharacterItem selected)
            {
                txtresult.Text = selected.Character.ToString();
            }
        }
    }

    public static class RichTextBoxExtensions
    {
        public static void InsertText(this FlowDocument document, string text, TextPointer position)
        {
            position.InsertTextInRun(text);
        }
    }

}
