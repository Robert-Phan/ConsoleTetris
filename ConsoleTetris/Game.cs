using System;
using System.Linq;

namespace ConsoleTetris;

internal static class Game
{
	internal static int Width { get; set; } = 10;
	internal static int Height { get; set; } = 20;
	internal static int FallTime { get; set; } = 800;
	internal static bool EnableDropProjection { get; set; } = true;

	internal static (int x, int y) SideDisplayOffSet { get; } = (1, 0);

	internal static DroppedBlocks DroppedBlocks { get; set; } = new();
	internal static bool IsDrawing { get; set; } = false;

	internal static int Score { get; set; }

	internal static void SetScore(int value)
	{
		IsDrawing = true;

		Score += (int)(
			Math.Round(Math.Pow(value, 2.5))
			* 1000);

		var scoreStringSegments = Score.ToString().Segments(8).ToList();

		Console.ResetColor();

		for (int i = 0; i < scoreStringSegments.Count; i++)
		{
			var segment = scoreStringSegments[i];
			Utilities.PlaceCursor(Width + 1, i + 4);
			Console.Write(segment);
		}

		IsDrawing = false;
	}

	#region Draw Game UI
	static void DrawBoundaryBoxes(
		int width, int height,
		int offsetX = 0, int offsetY = 0,
		string horizontal = "─", string vertical = "│",
		string ul_corner = "┌", string ur_corner = "┐",
		string ll_corner = "└", string lr_corner = "┘"
		)
	{
		Console.SetCursorPosition(offsetX, offsetY);
		Console.Write(ul_corner);
		Console.SetCursorPosition(width * 2 + offsetX + 1, offsetY);
		Console.Write(ur_corner);
		Console.SetCursorPosition(offsetX, height + offsetY + 1);
		Console.Write(ll_corner);
		Console.SetCursorPosition(width * 2 + offsetX + 1, height + offsetY + 1);
		Console.Write(lr_corner);

		Console.SetCursorPosition(offsetX + 1, offsetY);
		Console.Write(horizontal.Repeat(width * 2));
		Console.SetCursorPosition(offsetX + 1, height + offsetY + 1);
		Console.Write(horizontal.Repeat(width * 2));

		for (int i = offsetY; i < height + offsetY; i++)
		{
			Console.SetCursorPosition(offsetX, i + 1);
			Console.Write(vertical);
			Console.SetCursorPosition(width * 2 + offsetX + 1, i + 1);
			Console.Write(vertical);
		}
	}

	static void DrawGameBoundaries()
	{
		Console.ForegroundColor = ConsoleColor.Gray;
		DrawBoundaryBoxes(Width, Height);
		DrawBoundaryBoxes(4, 2, (Width + 1) * 2);
		DrawBoundaryBoxes(4, 2, (Width + 1) * 2, 4);

		Console.ForegroundColor = ConsoleColor.DarkRed;
		Console.SetCursorPosition(Width * 2 + 1, 1);
		Console.Write("!");
		Console.SetCursorPosition(Width * 2 + 1, 2);
		Console.Write("!");

		Console.ForegroundColor = ConsoleColor.DarkCyan;
		Console.SetCursorPosition(Width * 2 + 3, 0);
		Console.Write("NEXT");
		Console.SetCursorPosition(Width * 2 + 3, 4);
		Console.Write("SCORE");
	}

	static void DrawBackgroundBlocks()
	{
		Console.ForegroundColor = ConsoleColor.DarkGray;
		for (int i = 0; i < Height; i++)
		{
			Utilities.PlaceCursor(0, i);
			Console.Write("[]".Repeat(Width));
		}

		Utilities.PlaceCursor(Width + 1, 4);
		Console.Write("[][][][]");
		Utilities.PlaceCursor(Width + 1, 5);
		Console.Write("[][][][]");
	}

	static void ResizeWindow()
	{
		if (OperatingSystem.IsWindows())
		{
			Console.WindowWidth = Width * 2 + 13;
			Console.WindowHeight = Height + 2;
		}
	}
	#endregion

	internal static int Start()
	{
		Height += 2;

		ResizeWindow();
		Console.Clear();
		DrawGameBoundaries();
		DrawBackgroundBlocks();

		var nextPiece = new TetrisPiece();
		while (true)
		{
			var piece = nextPiece;
			nextPiece = new();
			nextPiece.DisplayToSide();

			if (!piece.StartControl()) return Score;

			DroppedBlocks.CheckForLine();
			if (DroppedBlocks.CheckFailure()) return Score;
		}
	}

	static internal void ResetGame()
	{
		Console.Clear();
		Console.ResetColor();
		Score = 0;
		IsDrawing = false;
		DroppedBlocks.Clear();
	}
}
