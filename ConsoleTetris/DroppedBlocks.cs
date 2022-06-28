using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTetris;

class DroppedBlocks : List<(int x, int y, ConsoleColor color)>
{
	internal void AddBlock(List<(int x, int y)> blocks, ConsoleColor color)
	{
		AddRange(
			blocks
			.Select(block => (block.x, block.y, color))
		);
	}

	internal bool ContainsBlock((int x, int y) block)
	{
		return this
			.Select(block => (block.x, block.y))
			.Contains(block);
	}

	internal bool CheckFailure()
	{
		var ys = this
			.Select(block => block.y);
		return ys.Contains(0) || ys.Contains(1);
	}

	int destroyedLines = 0;

	internal void CheckForLine()
	{
		Game.IsDrawing = true;

		var fullLine = Enumerable.Range(0, Game.Width);

		var blocksByLine = this
			.GroupBy(block => block.y)
			.OrderBy(line => line.Key);

		var firstPossibleFullLine = blocksByLine.FirstOrDefault(
			line => line
					.OrderBy(block => block)
					.Select(block => block.x)
					.SequenceEqual(fullLine)
		);

		if (firstPossibleFullLine != null)
		{
			DestroyLine(firstPossibleFullLine.Key);
			destroyedLines++;
			CheckForLine();
		}
		else
		{
			Game.SetScore(destroyedLines);
			destroyedLines = 0;
		}
	}

	void DestroyLine(int lineY)
	{
		var newBlocks = this
			.Where(block => block.y > lineY)
			.Concat(this
				.Where(block => block.y < lineY)
				.Select(block =>
				{
					block.y += 1;
					return block;
				})
			).ToList();

		Clear();

		AddRange(newBlocks);

		Console.BackgroundColor = ConsoleColor.Black;
		for (int i = 0; i < Game.Width; i++)
		{
			Utilities.PlaceCursor(i, lineY);
			Console.WriteLine("[]");
		}

		var blocksToDraw = this.Where(block => block.y <= lineY);
		foreach (var (x, y, color) in blocksToDraw)
		{
			Utilities.PlaceCursor(x, y - 1);
			Console.WriteLine("[]");
		}

		foreach (var (x, y, color) in blocksToDraw)
		{
			Console.BackgroundColor = color;
			Utilities.PlaceCursor(x, y);
			Console.WriteLine("  ");
		}
	}
}
