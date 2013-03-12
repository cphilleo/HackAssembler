using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HackAssembler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: HackAssembler file");
                return;
            }

            string output = "";
            string file = args[0];

            var parser = new Parser(file);

            parser.Parse();

            while (parser.HasMoreCommands())
            {
                parser.Advance();

                var type = parser.GetCommandType();

                switch (type)
                {
                    case CommandType.A_COMMAND:
                        Console.WriteLine("Type A: Symbol: {0}", parser.GetSymbol());
                        output += string.Format("0{0}\n", StringToBinary(parser.GetSymbol(), 15));
                        break;
                    case CommandType.L_COMMAND:
                        Console.WriteLine("Type L: Symbol: {0}", parser.GetSymbol());
                        break;
                    case CommandType.C_COMMAND:
                        Console.WriteLine("Type C: Dest: {0} Comp: {1} Jump: {2}", parser.GetDest(), parser.GetComp(), parser.GetJump());
                        output += string.Format("111{0}{1}{2}\n", Code.Comp(parser.GetComp()), Code.Dest(parser.GetDest()), Code.Jump(parser.GetJump()));
                        break;
                }
            }

            Console.Write(output);

            var outputPath = file.Replace(".asm", ".hack");
            File.WriteAllText(outputPath, output);
        }

        private static string StringToBinary(string value, int length)
        {
            var i = Convert.ToInt32(value);

            return Convert.ToString(i, 2).PadLeft(length, '0');
        }
    }
}
