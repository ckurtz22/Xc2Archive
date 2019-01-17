using System;
using System.IO;
using Ionic.Zlib;
using LibHac;
using LibHac.IO;

namespace Xc2Archive
{
	internal class ArchiveFile : FileBase
	{
		public ArchiveFileInfo File { get; }
		private IStorage BaseStorage { get; }
		private long Size { get; }

		public ArchiveFile(ArchiveFileInfo file)
		{
			File = file;
			Mode = OpenMode.Read;
			Size = (File.UncompressedSize > 0 ? File.UncompressedSize : File.DataSize);

			if (File.RomfsParent != null)
				BaseStorage = File.RomfsParent.GetBaseStorage().Slice(File.DataOffset + File.RomfsParent.Header.DataOffset, File.DataSize);
			else
				BaseStorage = File.ArchiveParent.Slice(File.DataOffset, File.DataSize);

			if (File.UncompressedSize > 0)
				using (var deflate = new ZlibStream(BaseStorage.AsStream(), CompressionMode.Decompress, true))
				{
					MemoryStream deflateStream = new MemoryStream();
					deflate.CopyStream(deflateStream, File.UncompressedSize);
					BaseStorage = deflateStream.AsStorage();
				}		
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override long GetSize()
		{
			return Size;
		}

		public override int Read(Span<byte> destination, long offset)
		{
			int toRead = ValidateReadParamsAndGetSize(destination, offset);
			BaseStorage.Read(destination.Slice(0, toRead), offset);
			return toRead;
		}

		public override void SetSize(long size)
		{
			throw new NotImplementedException();
		}

		public override void Write(ReadOnlySpan<byte> source, long offset)
		{
			throw new NotImplementedException();
		}
	}
}