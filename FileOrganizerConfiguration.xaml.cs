using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace FileOrganizer
{
    /// <summary>
    /// Interaction logic for FileOrganizerConfiguration.xaml
    /// </summary>
    public partial class FileOrganizerConfiguration : Window
    {
        public FileOrganizerConfiguration()
        {
            InitializeComponent();
        }

        public ApplicationSettingsBase Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
                prpGrdSettings.SelectedObject = _settings;
            }
        }

        private ApplicationSettingsBase _settings = null;
    }
}
