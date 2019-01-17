using LibHac.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xc2Archive.Bdat;

namespace Xc2Archive.Bdat
{
	class BdatFile
	{
		public List<BdatTable> Tables = new List<BdatTable>();
		public BdatFile(IFile bdat)
		{
			using (var reader = new BinaryReader(bdat.AsStream()))
			{
				var tableCount = reader.ReadInt32();
				var fileLength = reader.ReadInt32();
				for (int i = 0; i < tableCount; i++)
				{
					reader.BaseStream.Position = 8 + (4 * i);
					reader.BaseStream.Position = reader.ReadInt32();
					Tables.Add(new BdatTable(reader));
				}

			}
		}
	}
}
