using System;
using System.IO;
using System.Linq;
using CommandLine;
using Xc2Archive.Bdat;

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

			var common = xc2fs.Files.FirstOrDefault(x => x.FullPath.Contains("common.bdat"));
			var a = new BdatFile(xc2fs.OpenFile(common), Path.GetFileName(common.FullPath));

		}
	}
}
	
