#nullable enable
using System;

namespace Hecatomb8
{
    class Test
    {
        public void Method()
        {

        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Test test = null;
            test!.Method();
        }
    }
}
