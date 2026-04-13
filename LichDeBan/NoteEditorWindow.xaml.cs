using System;
using System.Windows;

namespace LichDeBan
{
    public partial class NoteEditorWindow : Window
    {
        public string NoteContent { get; private set; } = string.Empty;
        public bool IsSaved { get; private set; } = false;

        public NoteEditorWindow(DateTime date, string currentNote)
        {
            InitializeComponent();
            DateText.Text = $"Ghi chú cho ngày: {date:dd/MM/yyyy}";
            NoteTextBox.Text = currentNote;
            NoteTextBox.Focus();
            NoteTextBox.CaretIndex = NoteTextBox.Text.Length;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            NoteContent = NoteTextBox.Text;
            IsSaved = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}