using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace UI
{
    public class IOEngine
    {
        private string _filename;
        private string _path;

        public IOEngine()
        {
            _filename = "TaskManagerSettings.json";
            _path = $@"{Environment.CurrentDirectory}\{_filename}";
        }
        public (bool, string) SaveAllTasks(List<TaskModel> listOfTasks, string path)
        {
            try
            {
                using(var stream = File.Create(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<TaskModel>));
                    serializer.Serialize(stream, listOfTasks);
                }
            }
            catch(Exception e)
            {
                return (false, $"Nie udało się zapisać nowego zadania. Treść błędu:\n{e.Message}");
            }

            return (true, "Pomyślnie dodano nowe zadanie");
        }

        public (bool, string, List<TaskModel>) LoadAllTasks(string path)
        {
            bool outputResult = true;
            string outputMessage = "Poprawnie wczytano wszystkie zadania";
            List<TaskModel> outputList = null;

            if (!File.Exists(path))
            {
                try
                {
                    using(var stream = File.Create(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<TaskModel>));
                        serializer.Serialize(stream, new List<TaskModel>());
                        outputMessage = $"Brak pliku listy zadań {path}. Utworzono nowy.";
                    }
                }
                catch(Exception e)
                {
                    outputMessage = $"Nie znaleziono pliku listy zadań oraz wystąpił błąd podczas próby jego utworzenia:\n{e.Message}" +
                        $"\nPo uruchomieniu programu proszę spróbować zapisać dodane zadania jeszcze raz. Jeżeli problem będzie" +
                        $"się powtarzał skontaktuj się z twórcą aplikacji lub lokalnym administratorem i podaj mu treść błędu.";
                    outputResult = false;
                }
            }
            else
            {
                try
                {
                    using(var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<TaskModel>));
                        outputList = (List<TaskModel>)serializer.Deserialize(stream);
                    }
                }
                catch(Exception e)
                {
                    outputMessage = $"Wystąpił błąd podczas deserializacji listy zadań:\n{e.Message}";
                    outputResult = false;
                }
            }

            return (outputResult, outputMessage, outputList);
        }

        public (bool, string, SettingsModel) DeserializeSettings()
        {
            string outputMessage = $"Poprawnie odczytano i zaaplikowano ustawienia z pliku {_filename}";
            bool outputResult = true;
            SettingsModel outputSettings = new SettingsModel();
            if (!File.Exists(_path))
            {
                try
                {
                    var serializedSettings = JsonConvert.SerializeObject(outputSettings);
                    File.WriteAllText(_path, serializedSettings);
                    
                }
                catch(Exception e)
                {
                    outputMessage = $"Nie odnaleziono pliku z ustawieniami, dlatego podjęto " +
                        $"próbę wygenerowania nowego pliku {_filename}. Niestety wystąpił błąd:\n{e.Message}." +
                        $"\n\nUżyto ustawień domyślnych wbudowanych w program.";
                    outputResult = false;
                }
            }
            else
            {
                try
                {
                    var serializedSettings = File.ReadAllText(_path);
                    outputSettings = JsonConvert.DeserializeObject<SettingsModel>(serializedSettings);
                }
                catch (Exception e)
                {
                    outputMessage = $"Błąd podczas pobierania danych deserializacji danychz pliku {_filename}:" +
                        $"\n{e.Message}.\n\nUżyto ustawień domyślnych wbudowanych w program.";
                    outputResult = false;
                }
            }

            return (outputResult, outputMessage, outputSettings);
        }

        public (bool, string) SerializeNewSettings(SettingsModel newSettings)
        {
            string path = $@"{_path}\{_filename}";
            string outputMessage = $"Poprawnie zapisano nowe ustawienia do pliku {_filename}";
            bool outputResult = true;

            try
            {
                var serializedSettings = JsonConvert.SerializeObject(newSettings);
                File.WriteAllText(path, serializedSettings);
                
            }
            catch (Exception e)
            {
                outputMessage = $"Nie można zapisać nowych ustawień w pliku {_filename}. Niestety wystąpił błąd:\n{e.Message}.";
                outputResult = false;
            }

            return (outputResult, outputMessage);
        }

        public (bool, string) RestoreSettingsFile()
        {
            string outputMessage = $"Pomyślnie przywrócono standardowy plik ustawień {_filename}";
            bool outputResult = true;
            try
            {
                File.Create(_path).Close();

                var serializedSettings = JsonConvert.SerializeObject(new SettingsModel());

                File.WriteAllText(_path, serializedSettings);

            }
            catch(Exception e)
            {
                outputMessage = $"Nie można odtworzyć domyślnego pliku ustawień:\n{e.Message}" +
                        $"\nSpróbuj jeszcze raz. Jeżeli problem będzie się powtarzał skontaktuj się z twórcą aplikacji " +
                        $"lub lokalnym administratorem i podaj mu treść błędu.";
                outputResult = false;
            }

            return (outputResult, outputMessage);
        }
    }
}
