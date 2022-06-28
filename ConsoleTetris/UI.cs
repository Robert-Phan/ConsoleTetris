using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumberStyles = System.Globalization.NumberStyles;

namespace ConsoleTetris;

class InputSelection
{
	KeyValuePair<string, string>[] Selections { get; }
	int Index { get; set; } = 0;
	(int x, int y) Offset { get; set; }

	internal InputSelection(Dictionary<string, string> selection, (int x, int y) offset)
	{
		Selections = selection.ToArray();
		Offset = offset;
	}

	internal Dictionary<string, string> Start()
	{
		var (x, y) = Offset;

		for (int i = 0; i < Selections.Length; i++)
		{
			Console.SetCursorPosition(x, y + i);
			Console.Write(Selections.GetKVPString(i));
		}

		Console.SetCursorPosition(x, y + Selections.Length + 1);
		Console.Write("Press Tab to input.");

		Console.SetCursorPosition(x, y);
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.Write("> " + Selections.GetKVPString(0));

		while (true)
		{
			var key = Console.ReadKey(true).Key;

			if (key == ConsoleKey.DownArrow)
			{
				if (Index == Selections.Length - 1) continue;

				Console.ResetColor();
				Console.SetCursorPosition(x, y + Index);
				Console.Write(Selections.GetKVPString(Index) + "  ");

				Index++;

				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.SetCursorPosition(x, y + Index);
				Console.Write("> " + Selections.GetKVPString(Index));
			}
			else if (key == ConsoleKey.UpArrow)
			{
				if (Index == 0) continue;

				Console.ResetColor();
				Console.SetCursorPosition(x, y + Index);
				Console.Write(Selections.GetKVPString(Index) + "  ");

				Index--;

				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.SetCursorPosition(x, y + Index);
				Console.Write("> " + Selections.GetKVPString(Index));
			}
			else if (key == ConsoleKey.Enter)
			{
				Console.ResetColor();
				Index = 0;
				return Selections
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			}
			else if (key == ConsoleKey.Tab)
			{
				Console.SetCursorPosition(x, y + Selections.Length);
				Console.Write("Input: ");

				var input = Console.ReadLine();

				Console.SetCursorPosition(x, y + Selections.Length);
				Console.Write("       " +
					" ".Repeat(input is null ? 0 : input.Length));

				if (input is null) continue;

				var kvp = Selections[Index];

				Console.SetCursorPosition(x, y + Index);
				Console.Write(" ".Repeat(
					Selections.GetKVPString(Index).Length + 2
					));

				Selections[Index] = new KeyValuePair<string, string>(kvp.Key, input);

				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.SetCursorPosition(x, y + Index);
				Console.Write("> " + Selections.GetKVPString(Index));
			}
		}
	}
}

class Selection
{
	string[] SelectionsArray { get; }
	int Index { get; set; } = 0;
	(int x, int y) Offset { get; set; }

	internal Selection(string[] selection, (int x, int y) offset)
	{
		SelectionsArray = selection;
		Offset = offset;
	}

	internal string Start()
	{
		var (x, y) = Offset;

		for (int i = 0; i < SelectionsArray.Length; i++)
		{
			Console.SetCursorPosition(x, y + i);
			Console.Write(SelectionsArray[i]);
		}

		Console.SetCursorPosition(x, y);
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.Write("> " + SelectionsArray[0]);

		while (true)
		{
			var key = Console.ReadKey(true).Key;

			if (key == ConsoleKey.DownArrow)
			{
				if (Index == SelectionsArray.Length - 1) continue;

				Console.ForegroundColor = ConsoleColor.Gray;
				Console.SetCursorPosition(x, y + Index);
				Console.Write(SelectionsArray[Index] + "  ");

				Index++;

				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.SetCursorPosition(x, y + Index);
				Console.Write("> " + SelectionsArray[Index]);
			}
			else if (key == ConsoleKey.UpArrow)
			{
				if (Index == 0) continue;

				Console.ForegroundColor = ConsoleColor.Gray;
				Console.SetCursorPosition(x, y + Index);
				Console.Write(SelectionsArray[Index] + "  ");

				Index--;

				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.SetCursorPosition(x, y + Index);
				Console.Write("> " + SelectionsArray[Index]);
			}
			else if (key == ConsoleKey.Enter)
			{
				Console.ResetColor();
				var i = Index;
				Index = 0;
				return SelectionsArray[i];
			}
		}
	}
}

static internal class UI
{
	static Selection StartScreen { get; } = new(new[]
	{
		"Play",
		"Settings",
		"Help"
	}, (26, 7));

	static InputSelection GameSettings { get; } = new(new Dictionary<string, string>
	{
		{"Width", "10"},
		{"Height", "20"},
		{"Fall Time", "800"},
		{"Enable Drop Projection", "True" }
	}, (20, 7));

	internal static void Start()
	{
		Console.CursorVisible = false;
		Console.Clear();
		DisplayLogo();

		var selection = StartScreen.Start();
		if (selection == "Play")
		{
			Game.Start();
			Game.ResetGame();
			Start();
		}
		else if (selection == "Settings")
		{
			Console.Clear();
			var settings = GameSettings.Start();
			ParseSettings(settings);
			Start();
		}
		else if (selection == "Help") Start();
	}

	static void ParseSettings(Dictionary<string, string> settings)
	{
		var styles =
			NumberStyles.AllowThousands |
			NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;

		if (int.TryParse(settings["Width"],
			styles,
			null,
			out int width)) Game.Width = width;

		if (int.TryParse(settings["Height"],
			styles,
			null,
			out int height)) Game.Height = height;

		if (int.TryParse(settings["Fall Time"],
			styles,
			null,
			out int fallTime)) Game.FallTime = fallTime;

		Game.EnableDropProjection =
			new[] { 't', 'T', 'y', 'Y' }
			.Any(ch => settings["Enable Drop Projection"].Contains(ch));
	}

	static string Logo { get; } = @" 
 _____  _____  _____  ____   ___  ____  
|_   _|| ____||_   _||  _ \ |_ _|/ ___| 
  | |  |  _|    | |  | |_) | | | \___ \ 
  | |  | |___   | |  |  _ <  | |  ___) |
  |_|  |_____|  |_|  |_| \_\|___||____/ 
";

	static void DisplayLogo()
	{
		if (OperatingSystem.IsWindows())
			Console.WindowWidth = 60;

		var logoByLine = Logo.Split('\n');
		for (int i = 0; i < logoByLine.Length; i++)
		{
			Console.SetCursorPosition(10, i);
			Console.WriteLine(logoByLine[i]);
		}
	}
}
