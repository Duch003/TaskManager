using System;
using System.Collections.Generic;
using System.Text;

namespace UI
{
    public class SettingsModel
    {
        public ConsoleColor InformationColor { get; set; } = ConsoleColor.Green;
        public ConsoleColor BasicColor { get; set; } = ConsoleColor.Gray;
        public ConsoleColor RulesDescriptionColor { get; set; } = ConsoleColor.Blue;
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
        public int Padding { get; set; } = 20;
        public bool SaveEveryChange { get; set; } = false;
        public string PathToDBFile { get; set; } = "listOfTasks.xml";
        public bool AutoRemoveOutOfDateTasks { get; set; } = false;
    }
}
