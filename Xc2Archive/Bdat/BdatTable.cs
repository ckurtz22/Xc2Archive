using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Xc2Archive.Bdat
{
	class BdatTable
	{
		public long FileOffset { get; }
		public ushort Flags { get; }
		public ushort NameOffset { get; }
		public ushort ItemSize { get; }
		public ushort HashTableOffset { get; }
		public ushort ItemTableOffset { get; }
		public ushort ItemCount { get; }
		public ushort BaseId { get; }
		public ushort field_14 { get; }
		public ushort Checksum { get; }
		public int StringsOffset { get; }
		public int StringsLength { get; }
		public ushort MemberTableOffset { get; }
		public ushort MemberCount { get; }


		public BdatTable(BinaryReader reader)
		{
			FileOffset = reader.BaseStream.Position;
			if(reader.ReadChars(4).ToString() != "BDAT")
			{
				Console.WriteLine("Tried reading bdat table that wasn't a bdat table");
				return;
			}
			Flags = reader.ReadUInt16();
			NameOffset = reader.ReadUInt16();
			ItemSize = reader.ReadUInt16();
			HashTableOffset = reader.ReadUInt16();
			ItemTableOffset = reader.ReadUInt16();
			ItemCount = reader.ReadUInt16();
			BaseId = reader.ReadUInt16();
			field_14 = reader.ReadUInt16();
			Checksum = reader.ReadUInt16();
			StringsOffset = reader.ReadInt32();
			StringsLength = reader.ReadInt32();
			MemberTableOffset = reader.ReadUInt16();
			MemberCount = reader.ReadUInt16();
			//read filename at NameOffset + FileOffset
			//read bdat members



		}
	}
}
