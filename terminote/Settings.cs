using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace terminote
{
    public class Settings
    {
        public string FileLocation { get; set; }
        [JsonIgnore]
        public Encoding Encoding { get; set; }
        public string EncodingName
        {
            get => Encoding.BodyName;
            set => Encoding = Encoding.GetEncoding(value);
        }
        public string DateTimeFormat { get; set; }
        public CultureInfo Culture { get; set; }

        public int NumberOfLinesToShow { get; set; }
        public char LineStartChar { get; set; }

        private const string SettingsFilename = "terminote.config";
        private const string DefaultFilename = "terminote.log";

        public Settings(string fileLocation, string encodingName, string dateTimeFormat, CultureInfo culture, int numberOfLinesToShow, char lineStartChar)
        {
            FileLocation = fileLocation ?? throw new ArgumentNullException(nameof(fileLocation));
            EncodingName = encodingName ?? throw new ArgumentNullException(nameof(encodingName));
            DateTimeFormat = dateTimeFormat ?? throw new ArgumentNullException(nameof(dateTimeFormat));
            Culture = culture ?? throw new ArgumentNullException(nameof(culture));
            NumberOfLinesToShow = numberOfLinesToShow;
            LineStartChar = lineStartChar;
        }

        public Settings()
        {
            FileLocation = GetNotesFileLocation();
            Encoding = Encoding.UTF8;
            DateTimeFormat = "g";
            NumberOfLinesToShow = 25;
            Culture = CultureInfo.CurrentCulture;
            LineStartChar = '>';
        }

        public static string GetSettingsFileLocation()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SettingsFilename);
        }

        public static string GetNotesFileLocation(string filename = DefaultFilename)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this);
            string location = GetSettingsFileLocation();
            File.WriteAllText(location, json);
        }

        public static Settings Load(out bool fileNotFound, out bool fileNotValid)
        {
            string location = GetSettingsFileLocation();
            if (File.Exists(location))
            {
                fileNotFound = false;
                try
                {
                    string json = File.ReadAllText(location);
                    fileNotValid = false;
                    Settings settings = JsonConvert.DeserializeObject<Settings>(json);
                    return settings ?? new Settings();
                }
                catch
                {
                    fileNotValid = true;
                    return new Settings();
                }
            }

            fileNotFound = true;
            fileNotValid = false;
            return new Settings();
        }
    }
}
