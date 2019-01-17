using LibHac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Xc2Archive.Bdat
{
	class BdatTable
	{
		public string FileName { get; }
		public long FileOffset { get; }

		public ushort Flags { get; }
		public ushort NameOffset { get; }
		public ushort ItemSize { get; }
		public ushort HashTableOffset { get; }
		public ushort HashTableLength { get; }
		public ushort ItemTableOffset { get; }
		public ushort ItemCount { get; }
		public ushort BaseId { get; }
		public ushort field_14 { get; }
		public ushort Checksum { get; }
		public int StringsOffset { get; }
		public int StringsLength { get; }
		public ushort MemberTableOffset { get; }
		public ushort MemberCount { get; }
		public string Name { get; }


		public BdatTable(BinaryReader reader, string filename = "")
		{
			FileName = filename;
			FileOffset = reader.BaseStream.Position;
			if(reader.ReadAscii(4) != "BDAT")
			{
				Console.WriteLine("Tried reading bdat table that wasn't a bdat table");
				return;
			}
			Flags = reader.ReadUInt16(); // Decrypt
			bool encrypted = Convert.ToBoolean(Flags & 2);
			NameOffset = reader.ReadUInt16();
			ItemSize = reader.ReadUInt16();
			HashTableOffset = reader.ReadUInt16();
			HashTableLength = reader.ReadUInt16();
			ItemTableOffset = reader.ReadUInt16();
			ItemCount = reader.ReadUInt16();
			BaseId = reader.ReadUInt16();
			field_14 = reader.ReadUInt16();
			Checksum = reader.ReadUInt16();
			StringsOffset = reader.ReadInt32();
			StringsLength = reader.ReadInt32();
			MemberTableOffset = reader.ReadUInt16();
			MemberCount = reader.ReadUInt16();
			
			reader.BaseStream.Position = FileOffset + NameOffset;
			Name = (encrypted ? DecryptName(reader) : reader.ReadAsciiZ());

			for(int i = 0; i < MemberCount; i++)
			{
				reader.BaseStream.Position = FileOffset + MemberTableOffset + (i * 6);


			}

			//read bdat members



		}

		private string DecryptName(BinaryReader reader)
		{
			byte[] name = new byte[HashTableOffset - NameOffset];
			ushort chksum = (ushort)(((~Checksum & 0xFF) << 8) | ((~Checksum & 0xFF00) >> 8));
			ushort val = 0;
			int i = 0;

			do
			{
				chksum = (ushort)((((chksum & 0xFF) + (val & 0xFF)) & 0xFF) | (((chksum & 0xFF00) + (val & 0xFF00)) & 0xFF00));
				val = reader.ReadUInt16();
				Array.Copy(BitConverter.GetBytes((ushort)(chksum ^ val)), 0, name, (i++) * 2, 2);
			} while (((val ^ chksum) & 0xFF00) != 0);

			return Encoding.UTF8.GetString(name);
		}
	}
}
