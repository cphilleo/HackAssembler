using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace HackAssembler
{
    public enum CommandType
    {
        A_COMMAND,
        C_COMMAND,
        L_COMMAND
    }

    public class Parser
    {
        private readonly string _file;
        private readonly List<string> _commands = new List<string>();
        private int _currentCommand = -1;
        private Dictionary<string, int> _symbols = new Dictionary<string, int>
                                                   {
                                                       {"SP", 0},
                                                       {"LCL", 1},
                                                       {"ARG", 2},
                                                       {"THIS", 3},
                                                       {"THAT", 4},
                                                       {"R0", 0},
                                                       {"R1", 1},
                                                       {"R2", 2},
                                                       {"R3", 3},
                                                       {"R4", 4},
                                                       {"R5", 5},
                                                       {"R6", 6},
                                                       {"R7", 7},
                                                       {"R8", 8},
                                                       {"R9", 9},
                                                       {"R10", 10},
                                                       {"R11", 11},
                                                       {"R12", 12},
                                                       {"R13", 13},
                                                       {"R14", 14},
                                                       {"R15", 15},
                                                       {"SCREEN", 16384},
                                                       {"KBD", 24576}
                                                   };

        public Parser(string file)
        {
            _file = file;
        }

        public void Parse()
        {
            var lines = File.ReadAllLines(_file);

            //reset commands
            _commands.Clear();
            _currentCommand = -1;

            var linecount = 0;
            var variableCount = 16;

            //Pass 1 create symbols
            foreach (var line in lines)
            {
                //remove whitespace
                var clean = line.Trim().Replace(" ", string.Empty);

                //remove comments
                clean = Regex.Replace(clean, "//.*", string.Empty);

                //skip if it's an empty line
                if (clean.Length == 0)
                {
                    continue;
                }

                //L command
                if (clean.StartsWith("("))
                {
                    var symbol = Regex.Match(clean, @"\((.+)\)").Groups[1].Value;

                    if (!_symbols.ContainsKey(symbol))
                    {
                        _symbols.Add(symbol, linecount);
                    }

                    continue;
                }

                linecount++;
            }

            foreach (var symbol in _symbols)
            {
                Console.WriteLine("Address: {0}\tValue: {1}", symbol.Key, symbol.Value);
            }

            //Pass 2 generate commmands
            foreach (var line in lines)
            {
                //remove whitespace
                var clean = line.Trim().Replace(" ", string.Empty);

                //remove comments
                clean = Regex.Replace(clean, "//.*", string.Empty);

                //skip if it's an empty line
                if (clean.Length == 0)
                {
                    continue;
                }

                //Add to command list
                _commands.Add(clean);

                if (clean.StartsWith("@"))
                {
                    var symbol = Regex.Match(clean, "@(.+)").Groups[1].Value;

                    int address;
                    bool isSymbol = !Int32.TryParse(symbol, out address);

                    if (isSymbol)
                    {
                        if (!_symbols.ContainsKey(symbol))
                        {
                            _symbols.Add(symbol, variableCount);
                            variableCount++;
                        }
                    }
                }
            }
        }

        public bool HasMoreCommands()
        {
            return _currentCommand < _commands.Count - 1;
        }

        public void Advance()
        {
            if (HasMoreCommands())
            {
                _currentCommand++;
            }
        }

        public CommandType GetCommandType()
        {
            var command = _commands[_currentCommand];

            if (command.StartsWith("@"))
            {
                return CommandType.A_COMMAND;
            }

            if (command.StartsWith("("))
            {
                return CommandType.L_COMMAND;
            }

            return CommandType.C_COMMAND;
        }

        public string GetSymbol()
        {
            var type = GetCommandType();
            var command = _commands[_currentCommand];

            if (type == CommandType.A_COMMAND)
            {
                var symbol =  Regex.Match(command, "@(.+)").Groups[1].Value;

                //check symbol table first
                if (_symbols.ContainsKey(symbol))
                {
                    return _symbols[symbol].ToString();
                }

                return symbol;
            }

            if (type == CommandType.L_COMMAND)
            {
                var symbol = Regex.Match(command, @"\((.+)\)").Groups[1].Value;
                return _symbols[symbol].ToString();
            }

            throw new Exception("Must be Address or Label command");
        }

        public string GetDest()
        {
            var type = GetCommandType();
            var command = _commands[_currentCommand];

            if (type != CommandType.C_COMMAND)
            {
                throw new Exception("Must be Command");
            }

            if (!command.Contains("="))
            {
                return string.Empty;
            }

            return Regex.Match(command, "(.+)=").Groups[1].Value;
        }

        public string GetComp()
        {
            var type = GetCommandType();
            var command = _commands[_currentCommand];

            if (type != CommandType.C_COMMAND)
            {
                throw new Exception("Must be Command");
            }

            //Isolate comp command

            //remove dest
            var clean = Regex.Replace(command, "(.+)=", string.Empty);

            //remove jump
            clean = Regex.Replace(clean, ";.+", string.Empty);

            return clean;
        }

        public string GetJump()
        {
            var type = GetCommandType();
            var command = _commands[_currentCommand];

            if (type != CommandType.C_COMMAND)
            {
                throw new Exception("Must be Command");
            }

            if (!command.Contains(";"))
            {
                return string.Empty;
            }

            return Regex.Match(command, ";(.+)").Groups[1].Value;
        }
    }
}