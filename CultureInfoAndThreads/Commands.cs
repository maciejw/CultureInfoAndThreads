using System;
using System.Collections.Generic;
using System.Linq;

namespace CultureInfoAndThreads
{

    public class Commands : Dictionary<string, Action>
    {

        public Commands() : base(StringComparer.CurrentCultureIgnoreCase)
        {
            this.Add(nameof(Help), Help);
        }

        private void Help()
        {
            var lines = new List<string> { "Available commands:" };
            lines.AddRange(this.Keys.Select(i => $"\t{i}"));
            lines.ForEach(Console.WriteLine);
        }


        public void RunCommand(string commandName)
        {

            commandName = commandName ?? nameof(Help);
            Action command;
            if (this.TryGetValue(commandName, out command))
            {
                command();
            }
            else
            {
                Console.WriteLine($"Invalid command: {commandName}");
                Help();
            }
        }
    }
}
