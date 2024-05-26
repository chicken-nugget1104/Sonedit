using SonicRetro.SonLVL.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S1SSEdit
{
	public class LayoutData
	{
		public byte[,] Layout { get; private set; }
		public Position StartPosition { get; private set; }

		// Default constructor initializing a 0x40 x 0x40 layout
		public LayoutData()
		{
			Layout = new byte[0x40, 0x40];
		}

		// Constructor to initialize layout from a byte array
		public LayoutData(byte[] layoutData)
			: this()
		{
			if (layoutData == null)
				throw new ArgumentNullException(nameof(layoutData), "Layout data cannot be null.");

			if (layoutData.Length != 0x40 * 0x40)
				throw new ArgumentException("Invalid layout data length.", nameof(layoutData));

			int index = 0;
			for (int y = 0; y < 0x40; y++)
				for (int x = 0; x < 0x40; x++)
					Layout[x, y] = layoutData[index++];
		}

		// Constructor to initialize layout and start position from byte arrays
		public LayoutData(byte[] layoutData, byte[] startPosData)
			: this(layoutData)
		{
			if (startPosData == null)
				throw new ArgumentNullException(nameof(startPosData), "Start position data cannot be null.");

			StartPosition = new Position(startPosData);
		}

		// Method to get the layout data as a byte array
		public byte[] GetBytes()
		{
			byte[] result = new byte[0x40 * 0x40];
			int index = 0;
			for (int y = 0; y < 0x40; y++)
				for (int x = 0; x < 0x40; x++)
					result[index++] = Layout[x, y];
			return result;
		}

		// Method to clone the LayoutData object
		public LayoutData Clone()
		{
			LayoutData clone = new LayoutData();
			clone.Layout = (byte[,])Layout.Clone();
			if (StartPosition != null)
				clone.StartPosition = new Position(StartPosition.X, StartPosition.Y);
			return clone;
		}
	}
}
