using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TextInputTest
{
    public partial class MainWindow : Window
    {
        private const int MaxCharacters = 32;

        public MainWindow()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(InputBox, OnPaste);
            TextCompositionManager.AddTextInputHandler(InputBox, OnTextInput);
        }

        private void InputBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = WouldExceedLimit(e.Text);
        }

        private void OnTextInput(object sender, TextCompositionEventArgs e)
        {
            if (WouldExceedLimit(e.Text))
            {
                e.Handled = true;
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
            {
                return;
            }

            var pastedText = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;
            if (WouldExceedLimit(pastedText))
            {
                var allowedText = TrimToRemainingSpace(pastedText);
                if (allowedText.Length == 0)
                {
                    e.CancelCommand();
                    return;
                }

                var dataObject = new DataObject();
                dataObject.SetData(DataFormats.UnicodeText, allowedText);
                e.DataObject = dataObject;
            }
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InputBox.Text.Length <= MaxCharacters)
            {
                return;
            }

            var caretIndex = InputBox.CaretIndex;
            InputBox.Text = InputBox.Text[..MaxCharacters];
            InputBox.CaretIndex = Math.Min(caretIndex, MaxCharacters);
        }

        private bool WouldExceedLimit(string incomingText)
        {
            var selectionLength = InputBox.SelectionLength;
            var currentLength = InputBox.Text.Length;
            var proposedLength = currentLength - selectionLength + incomingText.Length;
            return proposedLength > MaxCharacters;
        }

        private string TrimToRemainingSpace(string incomingText)
        {
            var selectionLength = InputBox.SelectionLength;
            var remainingSpace = MaxCharacters - (InputBox.Text.Length - selectionLength);
            if (remainingSpace <= 0)
            {
                return string.Empty;
            }

            return incomingText.Length <= remainingSpace ? incomingText : incomingText[..remainingSpace];
        }
    }
}
