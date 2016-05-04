using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace TileGame {
	public static class Authentication {
		private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static ulong CurrentCounter {
			get {
				return (ulong) (DateTime.UtcNow - Epoch).TotalSeconds / 30;
			}
		}

		public static string GenerateToken(string secret, ulong iteration) {
			byte[] counter = BitConverter.GetBytes(iteration);
			if ( BitConverter.IsLittleEndian ) {
				Array.Reverse(counter);
			}
			byte[] key = Encoding.ASCII.GetBytes(secret);
			HMACSHA1 hmac = new HMACSHA1(key, true);
			byte[] hash = hmac.ComputeHash(counter);
			int offset = hash[hash.Length - 1] & 0x0F;
			int binary = ((hash[offset] & 0x7F) << 24) | ((hash[offset + 1] & 0xFF) << 16) | ((hash[offset + 2] & 0xFF) << 8) | (hash[offset + 3] & 0xFF);
			int password = binary % 1000000;
			return password.ToString("000000");
		}

		public static string GenerateToken(string secret) {
			return GenerateToken(secret, CurrentCounter);
		}

		public static bool IsValid(string secret, string token) {
			if ( token == GenerateToken(secret) ) {
				return true;
			} else for ( uint i = 1u; i < 3u; ++i ) {
				if ( token == GenerateToken(secret, CurrentCounter + i) || token == GenerateToken(secret, CurrentCounter - i) ) {
					return true;
				}
			}
			return false;
		}
	}
}

