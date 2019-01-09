using LibHac;
using LibHac.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xc2Archive
{
	class SwitchFS
	{
		private string FileSystemDir { get; }
		private Keyset Keyset { get; }
		private SwitchFs SwitchFs { get; }

		public SwitchFS(string switchFsDir, IProgressReport logger = null)
		{
			FileSystemDir = switchFsDir;

			Keyset = GetKeyset(logger);
			SwitchFs = new SwitchFs(Keyset, new FileSystem(switchFsDir));
		}

        public List<RomFsFileSystem> GetRomfs(ulong titleId)
        {
            var romfslist = new List<RomFsFileSystem>();
            if (!SwitchFs.Applications.TryGetValue(titleId, out var app))
            {
                Console.WriteLine("{0:X16} not found on filesystem", titleId);
                return null;
            }
            if (app.Main == null)
            {
                var xciFilename = Path.Combine(FileSystemDir, titleId.ToString("X16") + ".xci");
                app.Patch.MainNca.SetBaseNca(GetNcaXci(xciFilename, app.Name));
            }

            var section = app.Patch.MainNca.Sections.FirstOrDefault(x => x?.Type == SectionType.Romfs || x?.Type == SectionType.Bktr);
            romfslist.Add(new RomFsFileSystem(app.Patch.MainNca.OpenSection(section.SectionNum, false, IntegrityCheckLevel.None, true)));
            foreach (var nca in app.AddOnContent.Select(x => x.MainNca).OrderByDescending(x => x.Header.TitleId))
            {
                section = nca.Sections.FirstOrDefault(x => x?.Type == SectionType.Romfs || x?.Type == SectionType.Bktr);
                romfslist.Add(new RomFsFileSystem(nca.OpenSection(section.SectionNum, false, IntegrityCheckLevel.None, true)));
            }
            romfslist.Reverse();
            return romfslist;
        }

        private Nca GetNcaXci(string xciFilename, string appName)
		{
			if (!File.Exists(xciFilename))
				throw new FileNotFoundException("{0} digital not installed and cartridge not dumped to filesystem", appName);
			var xciFs = new FileStream(xciFilename, FileMode.Open, FileAccess.Read);
			var xci = new Xci(Keyset, xciFs.AsStorage());
            var ncaFile = xci.SecurePartition.Files.OrderByDescending(i => i.Size).First();
			var ncaStorage = new FileStorage(xci.SecurePartition.OpenFile(ncaFile, OpenMode.Read));
			return new Nca(Keyset, ncaStorage, true);
		}

		private Keyset GetKeyset(IProgressReport logger = null)
		{
			string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			string homeKeyFile = Path.Combine(home, ".switch", "prod.keys");
			string homeTitleKeyFile = Path.Combine(home, ".switch", "title.keys");
			string homeConsoleKeyFile = Path.Combine(home, ".switch", "console.keys");
			return ExternalKeys.ReadKeyFile(homeKeyFile, homeTitleKeyFile, homeConsoleKeyFile, logger);
		}
	}
}
