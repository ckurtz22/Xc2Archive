using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LibHac;
using LibHac.IO;

namespace Xc2Archive
{
    public class ArchiveFileSystem : IFileSystem
	{

        public List<ArchiveFileInfo> Files { get; private set; } = new List<ArchiveFileInfo>();
        public Dictionary<string, ArchiveFileInfo> FileDict { get; }
		private Directory Dir { get; }

        public ArchiveFileSystem(List<RomFsFileSystem> romfsList)
        {
            foreach(var romfs in romfsList)
                ReadRomfs(romfs);

            FileDict = Files.Distinct().ToDictionary(x => x.FullPath, x => x);
            Dir = new Directory();
            foreach (var file in Files)
                Dir.AddFile(file);
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateFile(string path, long size)
        {
            throw new NotImplementedException();
        }

        public void DeleteDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public IDirectory OpenDirectory(string path, OpenDirectoryMode mode)
        {
            throw new NotImplementedException();
        }

		public IFile OpenFile(ArchiveFileInfo file) => new ArchiveFile(file);
		public IFile OpenFile(string path) => OpenFile(path, OpenMode.Read);
        public IFile OpenFile(string path, OpenMode mode)
		{
			path = PathTools.Normalize(path);

			if (!FileDict.TryGetValue(path, out ArchiveFileInfo file))
			{
				throw new FileNotFoundException();
			}

			if (mode != OpenMode.Read)
			{
				throw new ArgumentOutOfRangeException(nameof(mode), "RomFs files must be opened read-only.");
			}

			return OpenFile(file);
		}


        public void RenameDirectory(string srcPath, string dstPath)
        {
            throw new NotImplementedException();
        }

        public void RenameFile(string srcPath, string dstPath)
        {
            throw new NotImplementedException();
        }
		
        private void ReadRomfs(RomFsFileSystem romfs)
        {
			Storage header = null;
			Storage data = null;
			foreach (var file in romfs.Files) {
				if (file.FullPath.Contains(".arh")) header = new FileStorage(romfs.OpenFile(file));
				if (file.FullPath.Contains(".ard")) data = new FileStorage(romfs.OpenFile(file));
                else Files.Add(new ArchiveFileInfo(romfs, file));
            }

			if (header != null)
				Files.AddRange((new ArchiveHeader(header, data)).Files.Where(x => x != null));
        }

		public void ExtractFiles(string outDir, string pattern = "(.*)")
		{
			//Regex regex = new Regex(pattern);
			//var files = Files.Where(x => regex.IsMatch(x.FullPath));
			var files = Files;

			var report = new ProgressBar();
			report.SetTotal(files.Count());
			foreach (var file in files)
			{
				string filename = Path.Combine(outDir, file.FullPath.TrimStart('/'));
				string dir = Path.GetDirectoryName(filename) ?? throw new InvalidOperationException();
				System.IO.Directory.CreateDirectory(dir);


				report.LogMessage(filename);
				using (var outStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
				using (var inStream = OpenFile(file).AsStream())
				{
					inStream.CopyTo(outStream);
				}
				report.ReportAdd(1);
			}
		}

		private class Directory
		{
			public List<Directory> Dirs { get; } = new List<Directory>();
			public List<ArchiveFileInfo> Files { get; } = new List<ArchiveFileInfo>();
			public string Name;
			public override string ToString() => Name;
			public Directory(string name = "") => Name = name;

			public void AddFile(ArchiveFileInfo file, int level = 1)
			{
				var parts = file.FullPath.Split('/');

				if(parts.Count() == 1)
				{
					Console.WriteLine(file.FullPath);
					return;
				}
				if (level + 1 == parts.Count())
					Files.Add(file);
				else
				{
					if (!Dirs.Exists(x => x.Name == parts[level]))
						Dirs.Add(new Directory(parts[level]));
					Dirs.FirstOrDefault(x=>x.Name == parts[level]).AddFile(file, level + 1);
				}
			}
		}
    }
}
