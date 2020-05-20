using System;

namespace Methane.Toolkit.Desktop
{
    class Program
    {
        static void Main(string[] args)
        {

            Methane.Toolkit.Program program = new Toolkit.Program();
            program.Main(args, new UniUI.CommandLineUI());
        }
    }
}
