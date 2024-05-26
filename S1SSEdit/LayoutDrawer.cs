using SonicRetro.SonLVL.API;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace S1SSEdit
{
	static class LayoutDrawer
	{
		public static ColorPalette Palette { get; private set; }
		public static Dictionary<byte, BitmapBits> ObjectBmps { get; private set; } = new Dictionary<byte, BitmapBits>();
		public static Dictionary<byte, BitmapBits> ObjectBmpsNoNum { get; private set; }
		public static BitmapBits StartPosBmp { get; private set; }

		public static void Init()
		{
			InitializePalette();
			InitializeObjectBitmaps();
			InitializeStartPositionBitmap();
		}

		private static void InitializePalette()
		{
			using (Bitmap tmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
				Palette = tmp.Palette;

			SonLVLColor[] pal = SonLVLColor.Load(Properties.Resources.Palette, EngineVersion.S1);
			for (int i = 0; i < pal.Length; i++)
				Palette.Entries[i] = pal[i].RGBColor;

			Palette.Entries[0] = Color.Transparent;
		}

		private static void InitializeObjectBitmaps()
		{
			BitmapBits bmp = new BitmapBits(Properties.Resources.GFX);
			BitmapBits[] sprites = ExtractSprites(bmp, bmp.Width / 24, 24, 24);

			bmp = new BitmapBits(Properties.Resources.Font);
			BitmapBits[] font = ExtractFontBitmaps(bmp);

			ObjectBmps[0] = new BitmapBits(24, 24);
			byte index = 1;

			AddWallBitmaps(sprites, font, ref index);
			AddMiscBitmaps(sprites, ref index);
			AddSpecialBitmaps(sprites, font, ref index);

			ObjectBmpsNoNum = new Dictionary<byte, BitmapBits>(ObjectBmps);
			RemoveNumbersFromWalls();

			StartPosBmp = new BitmapBits(Properties.Resources.StartPos);
		}

		private static BitmapBits[] ExtractSprites(BitmapBits source, int count, int width, int height)
		{
			BitmapBits[] sprites = new BitmapBits[count];
			for (int i = 0; i < count; i++)
				sprites[i] = source.GetSection(i * width, 0, width, height);
			return sprites;
		}

		private static BitmapBits[] ExtractFontBitmaps(BitmapBits source)
		{
			BitmapBits[] font = new BitmapBits[11];
			for (int i = 0; i < 10; i++)
				font[i] = source.GetSection(i * 8, 0, 8, 8);
			font[10] = source.GetSection(10 * 8, 0, 16, 8);
			return font;
		}

		private static void AddWallBitmaps(BitmapBits[] sprites, BitmapBits[] font, ref byte index)
		{
			for (int p = 0; p < 4; p++)
			{
				BitmapBits tmp = new BitmapBits(sprites[0]);
				ObjectBmps[index++] = tmp;
				for (int i = 1; i < 9; i++)
				{
					BitmapBits t2 = new BitmapBits(tmp);
					t2.DrawBitmapComposited(font[i], 16, 16);
					ObjectBmps[index++] = t2;
				}
				sprites[0].IncrementIndexes(16);
			}
		}

		private static void AddMiscBitmaps(BitmapBits[] sprites, ref byte index)
		{
			for (int i = 1; i < 7; i++)
				ObjectBmps[index++] = sprites[i];
		}

		private static void AddSpecialBitmaps(BitmapBits[] sprites, BitmapBits[] font, ref byte index)
		{
			// "R" bitmap
			sprites[7].IncrementIndexes(16);
			ObjectBmps[index++] = sprites[7];

			// Red/White bitmap
			ObjectBmps[index++] = sprites[8];

			// Diamond bitmaps
			for (int p = 0; p < 4; p++)
			{
				BitmapBits tmp = new BitmapBits(sprites[9]);
				ObjectBmps[index++] = tmp;
				sprites[9].IncrementIndexes(16);
			}

			// Unused blocks
			index += 3;

			// "ZONE" blocks
			for (int i = 10; i < 16; i++)
				ObjectBmps[index++] = sprites[i];

			// Ring bitmap
			sprites[16].IncrementIndexes(16);
			ObjectBmps[index++] = sprites[16];

			// Emerald bitmaps
			for (int p = 0; p < 4; p++)
			{
				BitmapBits tmp = new BitmapBits(sprites[19]);
				ObjectBmps[index++] = tmp;
				sprites[19].IncrementIndexes(16);
			}
			ObjectBmps[index++] = sprites[17]; // Red emerald
			ObjectBmps[index++] = sprites[18]; // Gray emerald
			ObjectBmps[index++] = sprites[20]; // Pass-through barrier

			ObjectBmps[0x4A] = new BitmapBits(sprites[20]);
			ObjectBmps[0x4A].DrawBitmapComposited(font[10], 4, 8);
		}

		private static void RemoveNumbersFromWalls()
		{
			for (int p = 0; p < 4; p++)
			{
				BitmapBits tmp = ObjectBmps[(byte)(p * 9 + 1)];
				for (int i = 1; i < 9; i++)
					ObjectBmpsNoNum[(byte)(p * 9 + 1 + i)] = tmp;
			}
		}

		private static void InitializeStartPositionBitmap()
		{
			StartPosBmp = new BitmapBits(Properties.Resources.StartPos);
		}

		public static BitmapBits DrawLayout(byte?[,] layout, bool showNumbers)
		{
			int width = layout.GetLength(0);
			int height = layout.GetLength(1);
			BitmapBits layoutBitmap = new BitmapBits(width * 24, height * 24);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					byte? sp = layout[x, y];
					if (sp.HasValue && sp.Value != 0 && ObjectBmps.ContainsKey(sp.Value))
					{
						layoutBitmap.DrawBitmapComposited(showNumbers ? ObjectBmps[sp.Value] : ObjectBmpsNoNum[sp.Value], x * 24, y * 24);
					}
				}
			}

			return layoutBitmap;
		}

		public static BitmapBits DrawLayout(byte[,] layout, bool showNumbers)
		{
			int width = layout.GetLength(0);
			int height = layout.GetLength(1);
			BitmapBits layoutBitmap = new BitmapBits(width * 24, height * 24);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					byte sp = layout[x, y];
					if (sp != 0 && ObjectBmps.ContainsKey(sp))
					{
						layoutBitmap.DrawBitmapComposited(showNumbers ? ObjectBmps[sp] : ObjectBmpsNoNum[sp], x * 24, y * 24);
					}
				}
			}

			return layoutBitmap;
		}

		public static BitmapBits DrawLayout(LayoutData layout, bool showNumbers)
		{
			BitmapBits layoutBitmap = DrawLayout(layout.Layout, showNumbers);
			if (layout.StartPosition != null)
			{
				layoutBitmap.DrawBitmapComposited(StartPosBmp, layout.StartPosition.X - 736 - (StartPosBmp.Width / 2), layout.StartPosition.Y - 688 - (StartPosBmp.Height / 2));
			}
			return layoutBitmap;
		}
	}
}
