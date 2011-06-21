using System;
using System.Security.Cryptography;

namespace Glav.CacheAdapter.Distributed.memcached
{

	/// <summary>
	/// Implements a modified FNV hash with good distributiuon (better distribution than FNV1)
	/// </summary>
	/// <remarks>Algorithm found at http://bretm.home.comcast.net/hash/6.html</remarks>
	public class DistributedFNV : HashAlgorithm
	{
		const uint PRIME = 16777619;
		const uint INIT = 2166136261;

		public DistributedFNV()
		{
			this.HashSizeValue = 32;
		}


		public override void Initialize()
		{
			CurrentHashValue = INIT;
		}
		
		protected uint CurrentHashValue;
		
		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			int end = ibStart + cbSize;

			for (int i = ibStart; i < end; i++)
			{
				CurrentHashValue ^= array[i];
				CurrentHashValue *= PRIME;
			}
		}
		
		protected override byte[] HashFinal()
		{
			this.CurrentHashValue += this.CurrentHashValue << 13;
			this.CurrentHashValue ^= this.CurrentHashValue >> 7;
			this.CurrentHashValue += this.CurrentHashValue << 3;
			this.CurrentHashValue ^= this.CurrentHashValue >> 17;
			this.CurrentHashValue += this.CurrentHashValue << 5;

			return BitConverter.GetBytes(this.CurrentHashValue);
		}
	}
}
