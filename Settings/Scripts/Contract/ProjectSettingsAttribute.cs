using System;

namespace Blackhole.Settings.Contract
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ProjectSettingsAttribute : Attribute
    {
        public string Section { get; }
        public int Order { get; set; }
        public ProjectSettingsAttribute(string section, int order)
        {
            Section = section;
            Order = order;
        }
    }
}