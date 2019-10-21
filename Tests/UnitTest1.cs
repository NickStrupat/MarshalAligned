using NickStrupat;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using Xunit;

namespace Tests
{
	public class UnitTests
	{
		[Theory]
		[InlineData(64, 128)]
		[InlineData(4096, 128)]
		[InlineData(4096, 4096)]
		[InlineData(4096, 4321)]
		[InlineData(1, 16)]
		[InlineData(2, 16)]
		[InlineData(3, 16)]
		[InlineData(4, 16)]
		[InlineData(5, 16)]
		[InlineData(6, 16)]
		[InlineData(7, 16)]
		[InlineData(8, 16)]
		[InlineData(9, 16)]
		public void AllocateAlignedMemory(int alignment, int size)
		{
			var isNotWindows = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows); 
			var alignmentIsPowerOfTwo = Popcnt.PopCount((uint)alignment) == 1;
			if (!alignmentIsPowerOfTwo || (isNotWindows && (alignment % IntPtr.Size) != 0))
				Assert.Throws<ArgumentException>(() => MarshalEx.AllocHGlobalAligned(size, alignment));
			else
				Assert.True(MarshalEx.AllocHGlobalAligned(size, alignment).ToInt64() % alignment == 0);
		}

		[Fact]
		public void FreeAligned()
		{
			var aligned = MarshalEx.AllocHGlobalAligned(16, 64);
			MarshalEx.FreeHGlobalAligned(aligned);
		}
	}
}
