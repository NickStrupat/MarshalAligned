using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NickStrupat
{
	public static class MarshalEx
	{
		private static readonly Boolean IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		private static readonly Boolean IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		private static readonly Boolean IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

		/// <summary>
		/// Free aligned memory
		/// </summary>
		/// <param name="alignedMemoryHandle">Handle to aligned memory to be freed</param>
		public static void FreeHGlobalAligned(IntPtr alignedMemoryHandle)
		{
			if (IsWindows)
				_aligned_free(alignedMemoryHandle);
			else if (IsMacOS | IsLinux)
				free(alignedMemoryHandle);
			else
				throw new PlatformNotSupportedException();
		}

		/// <summary>
		/// Allocate aligned memory
		/// </summary>
		/// <param name="size">Size of the requested memory allocation, in bytes.</param>
		/// <param name="alignment">The alignment value, which must be an integer power of 2.</param>
		/// <returns>A handle to aligned memory. Possibly zero if something has gone wrong.</returns>
		public static IntPtr AllocHGlobalAligned(int size, int alignment)
		{
			if (size <= 0)
				throw new ArgumentOutOfRangeException(nameof(size));
			if (alignment <= 0)
				throw new ArgumentOutOfRangeException(nameof(alignment));
			return AllocHGlobalAligned((uint)size, (uint)alignment);
		}

		/// <summary>
		/// Allocate aligned memory
		/// </summary>
		/// <param name="size">Size of the requested memory allocation, in bytes.</param>
		/// <param name="alignment">The alignment value, which must be an integer power of 2.</param>
		/// <returns></returns>
		public static IntPtr AllocHGlobalAligned(uint size, uint alignment)
		{
			if (!IsPowerOfTwo(alignment))
				throw new ArgumentException("Alignment must be a power of two", nameof(alignment));
			var p = AlignedMalloc((UIntPtr)size, (UIntPtr)alignment);
			Debug.Assert(p != IntPtr.Zero, "Aligned malloc failed.");
			Debug.Assert((UInt64)p % (UInt64)alignment == 0, "Aligned malloc returned memory that is not aligned to " + alignment + " bytes.");
			return p;
		}

		private static unsafe IntPtr AlignedMalloc(UIntPtr size, UIntPtr alignment)
		{
			if (IsWindows)
				return _aligned_malloc(size, alignment);
			if (IsLinux | IsMacOS)
			{
				IntPtr p;
				posix_memalign(&p, alignment, size);
				return p;
			}
			throw new PlatformNotSupportedException();
		}

		private static bool IsPowerOfTwo(ulong x)
		{
			return x != 0 & (x & (x - 1)) == 0;
		}

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr _aligned_malloc(UIntPtr size, UIntPtr alignment);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr _aligned_free(IntPtr memblock);

		[DllImport("libc")]
		private static extern unsafe IntPtr posix_memalign(IntPtr* memptr, UIntPtr alignment, UIntPtr size);

		[DllImport("libc")]
		private static extern void free(IntPtr ptr);
	}
}