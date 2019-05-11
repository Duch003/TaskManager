using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace UI
{
    [XmlRoot("TaskModelList")]
    public class TaskModel
    {
        [XmlElement("ID")]
        public int ID { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }
        [XmlElement("StartDate")]
        public DateTime StartDate { get; set; }
        [XmlElement("EndDate")]
        public DateTime? EndDate { get; set; }
        [XmlElement("DayLong")]
        public bool Daylong { get; set; }
        [XmlElement("Important")]
        public bool Important { get; set; }

        public TaskModel(int id, 
            string description, 
            DateTime startDate, 
            bool important, 
            DateTime? endDate)
        {
            ID = id;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            Daylong = endDate.HasValue ? false : true;
            Important = important;
        }

        public TaskModel() { }
    }
}
