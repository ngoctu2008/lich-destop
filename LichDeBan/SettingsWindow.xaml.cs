using System.Windows;
using LichDeBan.Services;

namespace LichDeBan
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            ColorTextBox.Text = SettingsManager.CurrentSettings.BackgroundColor;
            OpacitySlider.Value = SettingsManager.CurrentSettings.Opacity;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.CurrentSettings.BackgroundColor = ColorTextBox.Text;
            SettingsManager.CurrentSettings.Opacity = OpacitySlider.Value;
            SettingsManager.SaveSettings();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
