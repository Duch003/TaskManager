using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Xml.Serialization;

namespace UI
{
    class Program
    {
        private static SettingsModel _settings;
        private static List<TaskModel> listOfTasks = new List<TaskModel>();
        private static bool _isChanged = false;
        private static int _biggestId = 0;
        private static Timer timer = null;
        private static IOEngine engine = null;
        private static bool result = true;
        private static string message = null;
        static void Main(string[] args)
        {
            #region Zarządzanie programem parametrami
            if (args != null && args.Length > 0)
            {
                ExecuteCommands(args);
                return;
            }
            #endregion

            Initialize();

            ConsoleEx.WriteLine("Inicjalizacja programu przebiegła pomyślnie. Wciśnij ENTER aby kontynuować...");
            Console.ReadLine();

            do
            {
                Console.Clear();
                PrintMenuAndCommands();
                message = Console.ReadLine().ToLower();

                switch (message)
                {
                    case "addtask":
                        AddTask();
                        break;
                    case "showtasks":
                        ShowAllTasks();
                        Console.ReadLine();
                        break;
                    case "removetask":
                        if (AreThereRecordsInTheList())
                        {
                            RemoveTask();
                        }
                        else
                        {
                            ConsoleEx.WriteLine("Brak zadań na liście");
                        }
                        break;
                    case "save":
                        SaveChanges();
                        break;
                    case "edittask":
                        EditTask();
                        break;
                }

                if(_settings.SaveEveryChange && _isChanged)
                {
                    SaveChanges();
                    _isChanged = false;
                }
                
            } while (message != "exit");

            SaveChanges();
        }

        private static void Initialize()
        {
            #region Deserializacja ustawień
            engine = new IOEngine();
            (result, message, _settings) = engine.DeserializeSettings();
            ConsoleEx.WriteLine($"{message}.\nAby przywrócić domyślny plik ustawień uruchom aplikację z poziomu konsoli z parametrem -help\n", result ? _settings.InformationColor : ConsoleColor.Red);

            if (_settings.AutoRemoveOutOfDateTasks)
            {
                timer = new Timer();
                timer.Interval = 600000; //minuta
                timer.AutoReset = true;
                timer.Elapsed += Timer_Elapsed;
                timer.Enabled = true;
            }
            #endregion


            #region Deserializacja obecnych zadań i odczyt najwyższego ID
            (result, message, listOfTasks) = engine.LoadAllTasks(_settings.PathToDBFile);
            ConsoleEx.WriteLine(message + "\n", result ? _settings.InformationColor : _settings.ErrorColor);
            if (listOfTasks != null && listOfTasks.Count > 0) { _biggestId = listOfTasks[listOfTasks.Count - 1].ID + 1; }
            #endregion
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RemoveUnnecessaryTasks();
            if(_settings.SaveEveryChange) { SaveChanges(); }
        }

        private static void ExecuteCommands(string[] commands)
        {
            for (int i = 0; i < commands.Length; i++)
            {
                switch (commands[i].ToLower())
                {
                    case "-restoresettings":
                        engine = new IOEngine();
                        (result, message) = engine.RestoreSettingsFile();
                        ConsoleEx.WriteLine(message, result ? ConsoleColor.Green : ConsoleColor.Red);
                        break;
                    case "-help":
                        PrintAvailableParameters();
                        break;
                    default:
                        ConsoleEx.WriteLine($"Nieznana komenda: {commands[i]}", ConsoleColor.Yellow);
                        break;
                }
            }
            ConsoleEx.WriteLine("Wykonano wszystkie komendy. Przyciśnięcie Enter zamknie teraz aplikację...");
            Console.ReadLine();
        }

        private static bool AreThereRecordsInTheList()
        {
            return listOfTasks.Count > 0;
        }

        private static void ShowAllTasks()
        {
            if(!AreThereRecordsInTheList())
            {
                ConsoleEx.WriteLine("Brak zadań na liście.");
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                for(int i = 0; i < Console.BufferWidth; i++)
                {
                    builder.Append("-");
                }
                foreach (var task in listOfTasks)
                {
                    ConsoleEx.Write("[");
                    ConsoleEx.Write(task.ID.ToString(), _settings.RulesDescriptionColor);
                    ConsoleEx.Write("] ");
                    ConsoleEx.Write($"{task.StartDate.ToShortDateString()} {task.StartDate.ToLongTimeString()} ", _settings.InformationColor);
                    string endDateString = task.EndDate.HasValue ? $" - {task.EndDate.Value.ToShortDateString()} {task.EndDate.Value.ToLongTimeString()}" : "[Całodniowe]";
                    ConsoleEx.WriteLine(endDateString, _settings.InformationColor);
                    ConsoleEx.WriteLine(task.Description);
                    ConsoleEx.WriteLine(builder.ToString(), _settings.RulesDescriptionColor);
                }
            }
            
        }

        private static void RemoveUnnecessaryTasks()
        {
            var rubbish = new List<TaskModel>();
            foreach(var task in listOfTasks)
            {
                if(task.Daylong && task.StartDate > new DateTime(task.StartDate.Year, task.StartDate.Month, task.StartDate.Day, 23, 59, 59) 
                    || !task.Daylong && task.EndDate.Value < DateTime.Now)
                {
                    rubbish.Add(task);
                }
            }

            foreach(var task in rubbish)
            {
                listOfTasks.Remove(task);
                _isChanged = true;
            }
        }

        private static void RemoveUnnecessaryTasks(List<int> list)
        {
            var rubbish = new List<TaskModel>();
            foreach (var task in listOfTasks)
            {
                if (list.Contains(task.ID))
                {
                    rubbish.Add(task);
                }
            }

            foreach (var task in rubbish)
            {
                listOfTasks.Remove(task);
            }

            _isChanged = true;
        }

        private static void RemoveTask()
        {
            string command = "";
            List<int> list = new List<int>();
            do
            {
                Console.Clear();
                int currentY = Console.CursorTop;
                ConsoleEx.WriteLine("Podaj ID zadania aby oznaczyć je jako do usunięcia. Podanie ID ponownie odznaczy zadanie " +
                    "z listy do usunięcia. Wpisz EXIT aby wyjść: ");
                int currentX = Console.CursorLeft;
                ConsoleEx.Write(list);
                Console.CursorTop = currentY;
                Console.CursorLeft = currentX;
                ShowAllTasks();

                command = Console.ReadLine();

                if(int.TryParse(command, out int id))
                {
                    if (list.Contains(id))
                    {
                        list.Remove(id);
                        ConsoleEx.Write("Zadanie o ID: {id} zostąło odznaczone z listy do usunięcia.");
                    }
                    else
                    {
                        list.Add(id);
                        ConsoleEx.Write("Zadanie o ID: {id} zostało oznaczone jako do usunięcia.");
                    }
                    
                    
                }
            }
            while (command != "exit");

            RemoveUnnecessaryTasks(list);
        }

        private static void EditTask()
        {
            string command = "";
            int id = -1;
            do
            {
                Console.Clear();
                int currentY = Console.CursorTop;
                ConsoleEx.Write("Podaj ID zadania do edycji. Wpisz EXIT aby wyjść: ");
                int currentX = Console.CursorLeft;
                ConsoleEx.WriteLine("\n");
                ShowAllTasks();
                Console.CursorTop = currentY;
                Console.CursorLeft = currentX;

                command = Console.ReadLine().ToLower();

                if (command == "exit") { return; }

                if (int.TryParse(command, out id) && listOfTasks.Find(item => item.ID == id) != null){ break; }
            }
            while (true);

            Console.Clear();

            TaskModel model = listOfTasks.Find(item => item.ID == id);

            ConsoleEx.WriteLine("Aby nie zmieniać danego parametru wciśnij ENTER", _settings.RulesDescriptionColor);


            ConsoleEx.Write("[AKTUALNA] Data startu: ");
            ConsoleEx.WriteLine($"{model.StartDate.ToShortDateString()} {model.StartDate.ToLongTimeString()}",_settings.InformationColor);
            DateTime? newStart = GetValidDate("[NOWA] Data startu:", true);
            if(!newStart.HasValue) { newStart = model.StartDate; }

            ConsoleEx.Write("[AKTUALNA] Data zakończenia: ");
            if (model.Daylong)
            {
                ConsoleEx.WriteLine("[Zadanie całodniowe]", _settings.InformationColor);
            }
            else
            {
                ConsoleEx.WriteLine($"{model.EndDate.Value.ToShortDateString()} {model.EndDate.Value.ToLongTimeString()}", _settings.InformationColor);
            }
            
            DateTime? newEnd = GetValidDate("[NOWA] Data zaończenia:", true);
            bool isDayLong = !newEnd.HasValue;

            ConsoleEx.Write("[AKTUALNA] Zadanie ważne: ");
            ConsoleEx.WriteLine($"{model.Important}");
            bool? newImportance = GetValidBoolean("[NOWA] Zadanie ważne:", true);
            if (!newImportance.HasValue) { newImportance = model.Important; }

            ConsoleEx.Write("[AKTUALNA] Opis: ");
            ConsoleEx.WriteLine($"{model.Description}", _settings.InformationColor);
            ConsoleEx.Write("[NOWA] Opis: ");
            string newDescription = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newDescription)) { newDescription = model.Description; }

            model.Description = newDescription;
            model.StartDate = newStart.Value;
            model.EndDate = newEnd;
            model.Daylong = isDayLong;
            model.Important = newImportance.Value;
            Console.ReadLine();
        }

        private static void PrintAvailableParameters()
        {
            ConsoleEx.WriteLine("Lista dostępnych parametrów:\n");
            ConsoleEx.Write("-restoresettings", ConsoleColor.Green);
            ConsoleEx.WriteLine(" - przywraca plik ustawień ze standardowymi ustawieniami");
        }

        private static void AddTask()
        {
            ConsoleEx.WriteLine("Aby dodać nowe zadanie trzeba podać wymagane informacje i zatwierdzić ENTEREM", _settings.RulesDescriptionColor);
            ConsoleEx.WriteLine("Podaj daty w formacie [DD/MM/YYYY]; opcjonalnie dodaj po dacie czas: [HH:MM:SS].", _settings.BasicColor);

            DateTime startDate = DateTime.Now;
            DateTime? endDate = startDate;

            startDate = GetValidDate("Początek [brak daty - teraz]: ", false).Value;

            string message = "Koniec [brak daty - całodniowe] ";

            endDate = GetValidDate(message, true);

            Console.Write("Opis: ".PadRight(35), _settings.InformationColor);
            string description = Console.ReadLine();

            bool important = GetValidBoolean("Ważne zadanie [true/false]: ", false).Value;

            if(endDate == null || endDate.Value > startDate)
            {
                listOfTasks.Add(new TaskModel(_biggestId++, description, startDate, important, endDate));
                _isChanged = true;
                return;
            }

            ConsoleEx.WriteLine("Niepowodzenie! Data startowa większa niż data zakończenia.", _settings.ErrorColor);
            Console.ReadLine();
        }

        private static void SaveChanges()
        {
            IOEngine engine = new IOEngine();

            (bool result, string message) = engine.SaveAllTasks(listOfTasks, _settings.PathToDBFile);

            ConsoleEx.WriteLine(message, result ? _settings.InformationColor : _settings.ErrorColor);
            Console.ReadLine();
        }

        private static void PrintMenuAndCommands()
        {
            ConsoleEx.WriteLine("      MAIN MENU", _settings.InformationColor);
            ConsoleEx.WriteLine("Spis dostępnych komend:", _settings.BasicColor);
            ConsoleEx.Write("AddTask", _settings.InformationColor);
            ConsoleEx.WriteLine(" - dodaj nowe zadanie", _settings.BasicColor);
            ConsoleEx.Write("RemoveTask", _settings.InformationColor);
            ConsoleEx.WriteLine(" - usuń zadanie", _settings.BasicColor);
            ConsoleEx.Write("ShowTasks", _settings.InformationColor);
            ConsoleEx.WriteLine(" - wypisz wszystkie zadania z listy", _settings.BasicColor);
            ConsoleEx.Write("EditTask", _settings.InformationColor);
            ConsoleEx.WriteLine(" - przechodzi do menu edycji zapisanych zadań", _settings.BasicColor);
            ConsoleEx.Write("Save", _settings.InformationColor);
            ConsoleEx.WriteLine(" - wzapisuje wszystkie zmiany do bazy", _settings.BasicColor);

            ConsoleEx.Write("\nWpisz komendę: ", _settings.InformationColor);
        }

        private static DateTime? GetValidDate(string message, bool canBeNull)
        {
            DateTime output;
            DateTime now = DateTime.Now;
            bool result = false;
            string command = "";
            do
            {
                Console.Write(message.PadRight(35), _settings.InformationColor);
                command = Console.ReadLine();

                if (canBeNull && string.IsNullOrWhiteSpace(command)) { return null; }

                if (!canBeNull && string.IsNullOrWhiteSpace(command)) { return DateTime.Now; }

                result = DateTime.TryParse(command, out output);

                if (!result) { message = "Niepoprawne dane. Spróbuj jeszcze raz:"; }

            } while (!result);

            return output;
            
        }

        private static bool? GetValidBoolean(string message, bool canBeNull)
        {
            bool output;
            string command = "";
            do
            {
                Console.Write(message.PadRight(35), _settings.InformationColor);
                command = Console.ReadLine();

                if (canBeNull && string.IsNullOrWhiteSpace(command)) { return null; }


            } while (!bool.TryParse(command, out output));

            return output;
        }

    }
}
