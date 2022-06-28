using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTetris;

internal static class Utilities
{
	internal static void PlaceCursor(int left, int top)
	{
		Console.SetCursorPosition(left * 2 + 1, top + 1);
	}

	internal static string Repeat(this string str, int times)
	{
		return string.Concat(Enumerable.Repeat(str, times));
	}

	internal static IEnumerable<string> Segments(this string str, int chunkSize)
	{
		var result = Enumerable.Range(0, str.Length / chunkSize)
			.Select(i => str.Substring(i * chunkSize, chunkSize));

		var possibleNotFullLength = result.Count() * chunkSize;
		if (possibleNotFullLength < str.Length)
		{
			result = result.Append(str[possibleNotFullLength..]);
		}

		return result;
	}

	internal static string GetKVPString(this KeyValuePair<string, string>[] dictionary, int i)
	{
		return $"{dictionary[i].Key}: {dictionary[i].Value}";
	}
}
