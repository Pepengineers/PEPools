using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PEPEngineers.PEPools.Extensions
{
	internal static class CollectionUtilities
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastRemove<T>(this IList<T> list, int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException();
			var lastIndex = list.Count - 1;
			if (index > lastIndex)
				throw new ArgumentOutOfRangeException();
			list[index] = list[lastIndex];
			list.RemoveAt(lastIndex);
		}
	}
}