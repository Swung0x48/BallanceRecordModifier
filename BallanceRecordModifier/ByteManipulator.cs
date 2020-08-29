using System;
using System.IO;
using System.Threading.Tasks;

namespace BallanceRecordModifier
{
    public class ByteManipulator
    {
        public byte[] Encoded { get; private set; }
        public byte[] Decoded { get; private set; }

        private ByteManipulator(byte[] encoded, byte[] decoded)
        {
            Encoded = encoded;
            Decoded = decoded;
        }

        internal static byte[] Decode(byte[]? arr)
        {
            if (arr is null) throw new NullReferenceException();
            if (arr.Length == 0) throw new ArgumentNullException();
            
            for (int index = 0; index < arr.Length; index++)
            {
                arr[index] = (byte) (arr[index] << 3 | arr[index] >> 5);
                arr[index] = (byte) (-(arr[index] ^ 0xAF));
            }

            return arr;
        }

        internal static byte[] Encode(byte[]? arr)
        {
            if (arr is null) throw new NullReferenceException();
            if (arr.Length == 0) throw new ArgumentNullException();

            for (int index = 0; index < arr.Length; index++)
            {
                arr[index] = (byte) (-(arr[index]) ^ 0xAF);
                arr[index] = (byte) (arr[index] << 5 | arr[index] >> 3);
            }

            return arr;
        }

        public static async Task<ByteManipulator> Create(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException();
            var encoded = await File.ReadAllBytesAsync(path);
            
            return new ByteManipulator(encoded, Decode(encoded));
        }
        
        public async Task SaveToFile(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException();
            await File.WriteAllBytesAsync(path, Encoded);
        }

        public byte[] ByteArray
        {
            set
            {
                if (value[0] == 'D' && value[1] == 'B') // Check if the array is already decoded.
                {
                    Decoded = value;
                    Encoded = Encode(value);
                }
                else
                {
                    Encoded = value;
                    Decoded = Decode(value);
                }
            }
        }
    }
}