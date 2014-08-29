using System;
using System.Reflection;

namespace A
{
    public class Startpoint
    {
        public static void Main()
        {
            Console.WriteLine("Assembly " + 
                Assembly.GetExecutingAssembly().FullName + 
                " correctly loaded and started");
            new C.CommonClass().Run();
        }
    }
}
