using LibHac;
using LibHac.IO;
using System;
using System.IO;

namespace Xc2Archive
{
    public class ArchiveFileInfo
    {
        public string FullPath { get; }
		public long DataSize { get; }
		public long DataOffset { get; }
		public long UncompressedSize { get; }
		public Storage ArchiveParent { get; }
		public RomFsFileSystem RomfsParent { get; }

        public ArchiveFileInfo(RomFsFileSystem parent, RomfsFile file)
        {
			RomfsParent = parent; 
            FullPath = file.FullPath;
			DataSize = file.DataLength;
			DataOffset = file.DataOffset;
		}

        public ArchiveFileInfo(Storage parent, BinaryReader reader, string filename, bool missingNodes = false)
        {
			FullPath = filename;
			ArchiveParent = parent;
			DataOffset = reader.ReadInt64();
			DataSize = reader.ReadInt32();
			UncompressedSize = reader.ReadInt32();
			var type = reader.ReadInt32();
			var id = reader.ReadInt32();

			if (UncompressedSize > 0)
				DataOffset += 0x30;
		}

		public override string ToString() => FullPath;
		public override bool Equals(object obj) => obj is ArchiveFileInfo info && FullPath == info.FullPath;
		public override int GetHashCode() => HashCode.Combine(FullPath);
	}
}
