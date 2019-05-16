using CommandLine;

namespace KnxCli
{

    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        // [Option('h', "help", Required = false, HelpText = "Print command line help.")]
        // public bool Help { get; set; }

        [Value(0, MetaName = "actor", HelpText = "Actor name.")]
        public string Actor { get; set; }

        [Value(1, MetaName = "action", HelpText = "Actor action.")]
        public string Action { get; set; }

    }
}