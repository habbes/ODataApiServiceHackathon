using System;

namespace Hackathon2020.Poc01.Lib
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DynamicControllerAttribute : Attribute
    {
        public string Route { get; set; }
        public DynamicControllerAttribute(string route)
        {
            Route = route;
        }
    }
}
