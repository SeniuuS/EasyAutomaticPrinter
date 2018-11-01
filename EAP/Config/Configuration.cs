using EAP.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EAP.Config
{
    public class Configuration
    {
        private string _path = @AppDomain.CurrentDomain.BaseDirectory + @"/settings.ini";
        private IniFile _iniFile;

        public static string[] DocumentExtension
        {
            get
            {
                return new string[] { "docx", "doc", "pdf", "xls", "xlsx", "txt" };
            }
        }

        public static string[] ImageExtension
        {
            get
            {
                return new string[] { "png", "jpg", "bmp", "gif" };
            }
        }

        public bool AutoRetry { get; set; } = false;
        public bool ImageMarginBool { get; set; } = true;
        public bool ShowToolbar { get; set; } = false;

        public Configuration()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            _iniFile = new IniFile(_path);
            if (File.Exists(_path))
            {
                AutoRetry = BooleanHelper.ConvertToBool(_iniFile.Read("Retry", "Automatic"));
                ShowToolbar = BooleanHelper.ConvertToBool(_iniFile.Read("ToolBar", "Visibility"));
            }
        }

        public void SaveSettings()
        {
            _iniFile.Write("Retry", "Automatic", BooleanHelper.ConvertToString(AutoRetry));
            _iniFile.Write("ToolBar", "Visibility", BooleanHelper.ConvertToString(ShowToolbar));
        }
    }
}
