using LibHac;
using LibHac.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Xc2Archive
{
	public class ArchiveHeader
	{
		private int StringTableOffset { get; set; }
		private int StringTableLength { get; set; }
		private byte[] StringByteTable { get; set; }
		private string[] StringTable { get; set; }

		private int NodeTableOffset { get; set; }
		private int NodeTableLength { get; set; }
		private int NodeCount { get; set; }
		private Node[] Nodes { get; }

		private int FileTableOffset { get; set; }
		private int FileCount { get; set; }
		public ArchiveFileInfo[] Files { get; }
		//public List<ArchiveFileInfo> Files { get; } = new List<ArchiveFileInfo>();

        private char[] Magic { get; set; }
        private int Field4 { get; set; }
        private uint Key { get; set; }
		public long Length { get; private set; }

		private Storage ArchiveParent { get; }

		public ArchiveHeader(Storage header, Storage data)
		{
			using (var reader = new BinaryReader(header.AsStream()))
			{
				ArchiveParent = data;
				ReadHeader(reader);
				Nodes = new Node[NodeCount];
				StringTable = new string[FileCount];
				Files = new ArchiveFileInfo[FileCount];
				ReadNodeTable(reader);
				DecryptStringTable(reader);
				ReadFileInfos(reader);
				//FillMissingInfos(reader);
			}
		}

		private void ReadHeader(BinaryReader reader)
		{
			Magic = reader.ReadChars(4);
			Field4 = reader.ReadInt32();
			NodeCount = reader.ReadInt32();
			StringTableOffset = reader.ReadInt32();
			StringTableLength = reader.ReadInt32();
			NodeTableOffset = reader.ReadInt32();
			NodeTableLength = reader.ReadInt32();
			FileTableOffset = reader.ReadInt32();
			FileCount = reader.ReadInt32();
			Key = reader.ReadUInt32() ^ 0xF3F35353;
			Length = reader.BaseStream.Length;
		}

		private void ReadNodeTable(BinaryReader reader)
		{
			reader.BaseStream.Position = NodeTableOffset;
			for (int i = 0; i < NodeCount; i++)
				Nodes[i] = new Node
				{
					Next = (int)(reader.ReadInt32() ^ Key),
					Prev = (int)(reader.ReadInt32() ^ Key)
				};
		}

		private void DecryptStringTable(BinaryReader reader)
		{
			reader.BaseStream.Position = StringTableOffset;
			StringByteTable = reader.ReadBytes(StringTableLength);
			for (int i = 0; i < StringTableLength; i += 4)
			{
				var value = BitConverter.ToUInt32(StringByteTable, i) ^ Key;
				Array.Copy(BitConverter.GetBytes(value), 0, StringByteTable, i, 4);
			}
		}

		private void ReadFileInfos(BinaryReader reader)
		{
            using (var stringStream = new MemoryStream(StringByteTable))
            using (var stringReader = new BinaryReader(stringStream))
                for (int i = 0; i < NodeCount; i++)
                {
                    Node node = Nodes[i];
                    if (node.Next >= 0 || node.Prev < 0) continue;
                    stringStream.Position = -node.Next;
                    string fileName = GetFilePrefix(i) + stringReader.ReadAsciiZ();
                    int fileIndex = stringReader.ReadInt32();

                    reader.BaseStream.Position = FileTableOffset + 24 * fileIndex;
					Files[fileIndex] = new ArchiveFileInfo(ArchiveParent, reader, fileName);
					//Files.Add(new ArchiveFileInfo(ArchiveParent, reader, fileName));
                }
		}

        private void FillMissingInfos(BinaryReader reader)
        {
            ReadStringTable();
            for(int i = 0; i < FileCount; i++)
            {
                if(Files[i] == null)
                {
                    reader.BaseStream.Position = FileTableOffset + 24 * i;
					Files[i] = new ArchiveFileInfo(ArchiveParent, reader, StringTable[i], true);
				}
            }
        }

        private string GetFilePrefix(int nodeIndex)
		{
			List<char> prefix = new List<char>();
			Node node = Nodes[nodeIndex];
			while (node.Next != 0)
			{
				prefix.Add((char)(nodeIndex ^ Nodes[node.Prev].Next));
				nodeIndex = node.Prev;
				node = Nodes[nodeIndex];
			}
			prefix.Reverse();
			return new string(prefix.ToArray());
		}

		private void ReadStringTable()
		{
            using (var reader = new BinaryReader(new MemoryStream(StringByteTable)))
            {
                reader.BaseStream.Position = 4;
                for (int i = 0; i < FileCount; i++)
                {
                    string name = reader.ReadAsciiZ();
                    StringTable[reader.ReadInt32()] = name;
                }
            }
		}

		private class Node
		{
			public int Next { get; set; }
			public int Prev { get; set; }
		}
	}
}
