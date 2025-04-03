using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherPlugin
{
    //logic names to the choices - status reason
    public static class StatusReasonValue
    {
        public const int NoChange = 266980000;
        public const int Warmer = 266980001;
        public const int Colder = 266980002;
    }

    public static class LogicalNames
    {
        public const string TableName = "cre7e_weatherreport";
        //coulums names
        public const string TemperatureC = "cre7e_temperaturec";
        public const string TemperatureF = "cre7e_temperaturef";
        public const string StatusReason = "cre7e_statusreasonchoice";
        public const string ChangeCounter = "cre7e_changecounter";
        public const string IsActive = "cre7e_isactive";
        public const string Id = "cre7e_weatherreportid";

    }


}
