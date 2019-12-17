using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace terminote
{
    internal class Program
    {
        private static void SetSettings(Settings settings)
        {
            bool validInput;
            while (true)
            {
                Console.WriteLine("Current settings:");
                Console.WriteLine(Settings.GetSettingsFileLocation());
                Console.WriteLine("");

                Console.WriteLine("0 Culture: " + settings.Culture);
                Console.WriteLine("1 DateTime format: " + settings.DateTimeFormat);
                Console.WriteLine("2 Encoding: " + settings.Encoding.BodyName);
                Console.WriteLine("3 Line start character: " + settings.LineStartChar);
                Console.WriteLine("4 Number of lines to show: " + settings.NumberOfLinesToShow);

                validInput = false;
                Console.Write("Enter setting number to update [Exit]:");
                string input = Console.ReadLine();
                if (input.Length == 0)
                {
                    break;
                }

                if (int.TryParse(input, out int number))
                {
                    if (number >= 0 && number <= 4)
                    {
                        validInput = true;
                    }
                }

                if (!validInput)
                {
                    Console.WriteLine("Invalid input");
                    continue;
                }

                switch (number)
                {
                    case 0:

                        validInput = false;
                        while (!validInput)
                        {
                            Console.Write("New culture: ");
                            string newCulture = Console.ReadLine();

                            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                            bool cultureExists = false;

                            foreach (CultureInfo c in cultures)
                            {
                                if (c.Name == newCulture)
                                {
                                    cultureExists = true;
                                }
                            }

                            if (cultureExists)
                            {
                                settings.Culture = new CultureInfo(newCulture);
                                settings.Save();
                                Console.WriteLine("Culture updated");
                                validInput = true;
                            }
                            else
                            {
                                Console.WriteLine("Invalid culture name");
                                Console.WriteLine("Valid culture names are:");

                                foreach (CultureInfo c in cultures)
                                {
                                    if (c.Name.Length > 0)
                                    {
                                        Console.Write("["+c.Name+"] ");
                                    }
                                }

                                Console.WriteLine();
                            }
                        }
                        break;
                    case 1:
                        validInput = false;
                        while (!validInput)
                        {
                            Console.Write("New DateTime format: ");
                            string newFormat = Console.ReadLine();

                            try
                            {
                                Console.WriteLine("Using this format, current date and time is:" + DateTime.Now.ToString(newFormat));
                                Console.Write("Is this OK?[Y/n]");
                                bool isOk = Console.ReadKey().KeyChar != 'n';
                                Console.WriteLine();
                                if (isOk)
                                {
                                    settings.DateTimeFormat = newFormat;
                                    settings.Save();
                                    Console.WriteLine("DateTime format updated");
                                    validInput = true;
                                }
                            }
                            catch
                            {
                                Console.WriteLine("Invalid format string");
                            }
                        }
                        break;
                    case 2:
                        validInput = false;
                        while (!validInput)
                        {
                            Console.Write("New encoding: ");
                            string newEncoding = Console.ReadLine();

                            try
                            {
                                settings.Encoding = Encoding.GetEncoding(newEncoding);
                                settings.Save();
                                Console.WriteLine("Encoding updated");
                                validInput = true;
                            }
                            catch
                            {
                                Console.WriteLine("Invalid encoding name");
                                Console.WriteLine("Valid encoding names are:");

                                foreach (EncodingInfo e in Encoding.GetEncodings())
                                {
                                    Console.WriteLine(e.GetEncoding().BodyName);
                                }
                            }
                        }
                        break;
                    case 3:
                        validInput = false;
                        while (!validInput)
                        {
                            Console.Write("New line start character: ");
                            string newChar = Console.ReadLine();
                            if (newChar.Length == 1)
                            {
                                settings.LineStartChar = newChar.ToCharArray()[0];
                                settings.Save();
                                Console.WriteLine("Line start character updated");
                                validInput = true;
                            }
                            else
                            {
                                Console.WriteLine("Please enter a single character");
                            }
                        }
                        break;
                    case 4:
                        validInput = false;
                        while (!validInput)
                        {
                            Console.Write("New number of lines to show: ");
                            string newLines = Console.ReadLine();
                            if (int.TryParse(newLines, out int linesNumber))
                            {
                                if (linesNumber > -1)
                                {
                                    settings.NumberOfLinesToShow = linesNumber;
                                    settings.Save();
                                    Console.WriteLine("Number of lines to show updated");
                                    validInput = true;
                                }
                            }
                        }
                        break;
                }
            }
        }

        private static int GetLineOffset(string text) 
        {
            double length = text.Length;
            return (int)Math.Ceiling(length / Console.WindowWidth);
        }

        private static void TakeNotes(Settings settings)
        {
            const string prompt = "terminote> ";
            while (true)
            {
                Console.Write(prompt);
                string currentInput = Console.ReadLine();
                if (currentInput.ToLower() == ":q" || currentInput.ToLower() == "q:") // to account for typos
                {
                    break; 
                }

                if (currentInput.ToLower() == ":s") 
                {
                    SetSettings(settings);
                    continue;
                }


                string newLine = DateTime.Now.ToString(settings.DateTimeFormat) + settings.LineStartChar + " " + currentInput;
                try
                {                    
                    File.AppendAllText(settings.FileLocation, newLine + Environment.NewLine, settings.Encoding);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: Could not write to file");
                    Console.WriteLine(ex.Message);
                }

                Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - GetLineOffset(prompt + currentInput));                

                Console.WriteLine(newLine);
            }
        }

        private static bool IsValidFilepath(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (File.OpenRead(path))
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    using (File.Create(path))
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        private static void Main(string[] args)
        {
            Console.Title = "terminote";
            Console.WriteLine(Properties.Resources.Copyright);
            if (args.Length == 0 || args[0] != "--settings")
            {
                Console.WriteLine(Properties.Resources.HowToExit);
            }

            Console.WriteLine();

            Settings settings = Settings.Load(out bool fileNotFound, out bool fileNotValid);

            if (fileNotFound)
            {
                Console.WriteLine("No settings file found, using default settings");
            }

            if (fileNotValid)
            {
                Console.WriteLine("Settings file is not valid, using default settings");
            }

            settings.Save();

            if (args.Length == 0)
            {
                Console.WriteLine(Properties.Resources.ShortHelp);
                Console.WriteLine(Properties.Resources.DefaultFile, settings.FileLocation);
                Console.WriteLine();
            }
            else if (args[0] == "--settings")
            {
                SetSettings(settings);
                return;
            }

            if (args.Length == 1)
            {
                if (IsValidFilepath(Settings.GetNotesFileLocation(args[0])))
                {
                    settings.FileLocation = Settings.GetNotesFileLocation(args[0]);
                }
                else
                {
                    Console.WriteLine("Can't access {0} using:", args[0]);
                }
            }

            Console.WriteLine(settings.FileLocation);

            try
            {
                if (File.Exists(settings.FileLocation))
                {
                    string[] lines = File.ReadAllLines(settings.FileLocation);

                    int startLine = lines.Length - settings.NumberOfLinesToShow;
                    if (startLine < 0)
                    {
                        startLine = 0;
                    }

                    for (int i = startLine; i < lines.Length; i++)
                    {
                        Console.WriteLine(lines[i]);
                    }
                }

                TakeNotes(settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Could not open: {0})", settings.FileLocation);
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
