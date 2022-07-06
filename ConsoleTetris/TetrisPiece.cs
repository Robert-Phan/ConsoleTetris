using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace ConsoleTetris;

using PieceBlocks = List<(int x, int y)>;

internal class TetrisPiece
{
	ConsoleColor Color { get; set; }
	int RandomOffset { get; set; }

	PieceBlocks PieceBlocks { get; set; }
	PieceBlocks NextPieceBlocks { get; set; } = new();
	(double x, double y) Center { get; set; }

	internal TetrisPiece()
	{
		var choiceOfPieces = new[]
		{
			// the cyan long piece
			(new PieceBlocks {
				(0, 0),
				(1, 0),
				(2, 0),
				(3, 0)
			}, (1.5, 0.5),  ConsoleColor.Cyan),
			// the blue backwards L piece
			(new PieceBlocks {
				(0, 0),
				(0, 1),
				(1, 1),
				(2, 1)
			}, (1, 1), ConsoleColor.Blue),
			// the orange L piece
			(new PieceBlocks {
				(2, 0),
				(0, 1),
				(1, 1),
				(2, 1)
			}, (1, 1), ConsoleColor.DarkYellow),
			// the yellow square piece
			(new PieceBlocks {
				(0, 0),
				(1, 0),
				(0, 1),
				(1, 1)
			}, (0.5, 0.5), ConsoleColor.Yellow),
			// the green lightning piece
			(new PieceBlocks {
				(1, 0),
				(2, 0),
				(0, 1),
				(1, 1)
			}, (1, 1), ConsoleColor.Green),
			// the purple T piece
			(new PieceBlocks  {
				(1, 0),
				(0, 1),
				(1, 1),
				(2, 1)
			}, (1, 1), ConsoleColor.Magenta),
			// the red backwards lightning piece
			(new PieceBlocks  {
				(0, 0),
				(1, 0),
				(1, 1),
				(2, 1)
			}, (1, 1), ConsoleColor.Red)
		};
		(PieceBlocks, Center, Color) = choiceOfPieces[new Random().Next(7)];

		RandomOffset = new Random().Next(
			Game.Width -
			PieceBlocks.Select(block => block.x).Max()
			);
	}

	void Draw()
	{
		Game.IsDrawing = true;

		Console.BackgroundColor = ConsoleColor.Black;
		foreach (var (x, y) in PieceBlocks)
		{
			Utilities.PlaceCursor(x, y);
			Console.Write("[]");
		}

		Console.BackgroundColor = Color;
		foreach (var (x, y) in NextPieceBlocks)
		{
			Utilities.PlaceCursor(x, y);
			Console.Write("  ");
		}

		Game.IsDrawing = false;
	}

	internal void DisplayToSide()
	{
		Game.IsDrawing = true;

		var (offsetX, y) = Game.SideDisplayOffSet;

		Console.BackgroundColor = ConsoleColor.Black;

		Utilities.PlaceCursor(Game.Width + offsetX, y);
		Console.Write("[][][][]");
		Utilities.PlaceCursor(Game.Width + offsetX, y + 1);
		Console.Write("[][][][]");

		Console.BackgroundColor = Color;
		foreach (var (blockX, blockY) in PieceBlocks)
		{
			Utilities.PlaceCursor(Game.Width + offsetX + blockX, y + blockY);
			Console.Write("  ");
		}

		Game.IsDrawing = false;
	}

	#region Control
	readonly System.Timers.Timer timer = new()
	{
		AutoReset = true,
		Interval = Game.FallTime
	};

	bool isDoneControl = false;

	internal bool StartControl()
	{
		PieceBlocks = PieceBlocks
			.Select(block => (block.x + RandomOffset, block.y))
			.ToList();
		Center = (Center.x + RandomOffset, Center.y);
		NextPieceBlocks = new(PieceBlocks);

		Draw();
		DrawDropProjection();
		Console.ForegroundColor = ConsoleColor.DarkGray;

		timer.Elapsed += (object? source, ElapsedEventArgs e) =>
		{
			if (!Game.IsDrawing)
				if (!MoveDown()) EndControl();
		};

		timer.Start();

		while (true)
		{
			while (Console.KeyAvailable == false)
			{
				if (isDoneControl) return true;
			};

			var key = Console.ReadKey(true).Key;

			if (!Game.IsDrawing)
				if (key == ConsoleKey.RightArrow)
				{
					MoveRight();
				}
				else if (key == ConsoleKey.LeftArrow)
				{
					MoveLeft();
				}
				else if (key == ConsoleKey.DownArrow)
				{
					if (!MoveDown()) EndControl();
				}
				else if (key == ConsoleKey.D)
				{
					SpinRight();
				}
				else if (key == ConsoleKey.A)
				{
					SpinLeft();
				}
				else if (key == ConsoleKey.UpArrow)
				{
					InstantDrop();
					EndControl();
				}
				else if (key == ConsoleKey.X)
				{
					timer.Dispose();
					return false;
				}
		}
	}

	void EndControl()
	{
		Game.DroppedBlocks.AddBlock(PieceBlocks, Color);
		timer.Dispose();
		isDoneControl = true;
	}
	#endregion

	#region Instant Drop
	PieceBlocks DropProjection { get; set; } = new();

	internal void DrawDropProjection()
	{
		if (!Game.EnableDropProjection)
		{
			UpdateDropProjection();
			return;
		}

		Game.IsDrawing = true;

		Console.BackgroundColor = ConsoleColor.Black;
		foreach (var (x, y) in
			DropProjection.Where(block => !PieceBlocks.Contains(block)))
		{
			Utilities.PlaceCursor(x, y);
			Console.Write("[]");
		}

		UpdateDropProjection();

		Console.ForegroundColor = Color;
		foreach (var (x, y) in
			DropProjection.Where(block => !PieceBlocks.Contains(block)))
		{
			Utilities.PlaceCursor(x, y);
			Console.Write("[]");
		}
		Console.ForegroundColor = ConsoleColor.DarkGray;

		Game.IsDrawing = false;
	}

	internal void InstantDrop()
	{
		NextPieceBlocks = DropProjection;
		Draw();
		PieceBlocks = NextPieceBlocks;
	}

	internal void UpdateDropProjection()
	{
		(int x, int y) defaultFloorBlock = (0, Game.Height);

		var possibleLeastOffsets = PieceBlocks.Select(block =>
			new
			{
				PieceBlockY = block.y,
				FloorBlockY = Game.DroppedBlocks
					.Select(dropped => (dropped.x, dropped.y))
					.Where(dropped => dropped.x == block.x
									&& dropped.y > block.y)
					.OrderBy(floor => floor.y)
					.FirstOrDefault(defaultFloorBlock).y
			}
		);

		var leastOffsetPair = possibleLeastOffsets
			.MinBy(blockpair => blockpair.FloorBlockY - blockpair.PieceBlockY);

		if (leastOffsetPair == null) return;

		DropProjection = PieceBlocks
			.Select(block =>
			{
				block.y += leastOffsetPair.FloorBlockY - leastOffsetPair.PieceBlockY - 1;
				return block;
			}).ToList();
	}
	#endregion

	#region Collision Checking
	bool CheckNormalCollision()
	{
		foreach (var (x, y) in NextPieceBlocks)
		{
			if (x < 0 ||
				x > Game.Width - 1 ||
				y < 0 ||
				y > Game.Height - 1) return false;
			if (Game.DroppedBlocks.ContainsBlock((x, y))) return false;
		}
		return true;
	}

	bool CheckStoppingCollision()
	{
		foreach (var block in NextPieceBlocks)
		{
			if (block.y > Game.Height - 1 ||
				Game.DroppedBlocks.ContainsBlock(block)) return false;
		}
		return true;
	}
	#endregion

	#region Movement
	void SpinRight()
	{
		var pieceBlocks = PieceBlocks;
		for (int i = 0; i < pieceBlocks.Count; i++)
		{
			(double x, double y) block = pieceBlocks[i];
			block = (-(block.y - Center.y) + Center.x, (block.x - Center.x) + Center.y);
			NextPieceBlocks[i] = ((int x, int y))block;
		}

		if (!CheckNormalCollision()) return;

		Draw();
		PieceBlocks = new(NextPieceBlocks);
		DrawDropProjection();
	}

	void SpinLeft()
	{
		var pieceBlocks = PieceBlocks;
		for (int i = 0; i < pieceBlocks.Count; i++)
		{
			(double x, double y) block = pieceBlocks[i];
			block = ((block.y - Center.y) + Center.x, -(block.x - Center.x) + Center.y);
			NextPieceBlocks[i] = ((int x, int y))block;
		}

		if (!CheckNormalCollision()) return;

		Draw();
		PieceBlocks = new(NextPieceBlocks);
		DrawDropProjection();
	}
	void MoveLeft()
	{
		var pieceBlocks = PieceBlocks;
		for (int i = 0; i < pieceBlocks.Count; i++)
		{
			var block = PieceBlocks[i];
			block.x--;
			NextPieceBlocks[i] = block;
		}

		if (!CheckNormalCollision()) return;

		Center = (Center.x - 1, Center.y);
		Draw();
		PieceBlocks = new(NextPieceBlocks);
		DrawDropProjection();
	}

	void MoveRight()
	{
		var pieceBlocks = PieceBlocks;
		for (int i = 0; i < pieceBlocks.Count; i++)
		{
			var block = pieceBlocks[i];
			block.x++;
			NextPieceBlocks[i] = block;
		}

		if (!CheckNormalCollision()) return;

		Center = (Center.x + 1, Center.y);
		Draw();
		PieceBlocks = new(NextPieceBlocks);
		DrawDropProjection();
	}

	bool MoveDown()
	{
		var pieceBlocks = PieceBlocks;
		for (int i = 0; i < pieceBlocks.Count; i++)
		{
			var block = pieceBlocks[i];
			block.y++;
			NextPieceBlocks[i] = block;
		}

		if (!CheckStoppingCollision()) return false;
		if (!CheckNormalCollision()) return true;

		Center = (Center.x, Center.y + 1);
		Draw();
		PieceBlocks = new(NextPieceBlocks);
		return true;
	}
	#endregion

	public override string ToString()
	{
		return String.Join(", ", PieceBlocks);
	}
}
