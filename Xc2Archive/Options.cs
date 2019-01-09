using CommandLine;

namespace Xc2Archive
{
	class Options
	{
        [Option('o', "output", Required = false,
            HelpText = "Output directory")]
        public string Output { get; set; }

        [Option('f', "filesystem", Required = true, 
			HelpText = "Location of the Switch Filesystem to use")]
		public string SwitchFs { get; set; }

        [Option('t', "title", Required = true,
            HelpText = "Title ID of game to extract")]
        public string Title { get; set; }


    }
}
