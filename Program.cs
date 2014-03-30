using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FunctionInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Interpreter interpreter = new Interpreter();

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                int value = interpreter.Interpret(input);

                Console.WriteLine(value);
            }
        }
    }
}
