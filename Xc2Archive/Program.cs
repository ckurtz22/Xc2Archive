using System;
using CommandLine;

namespace Xc2Archive
{
	class Program
	{
		static void Main(string[] args)
		{
			var options = new Options();
			Parser.Default.ParseArguments<Options>(args).WithParsed(opts => options = opts);

			var FS = new SwitchFS(options.SwitchFs);
            var romfs = FS.GetRomfs(Convert.ToUInt64(options.Title, 16));
            var xc2fs = new ArchiveFileSystem(romfs);

            if(options.Output != null)
			    xc2fs.ExtractFiles(options.Output);
		}
	}
}
	
