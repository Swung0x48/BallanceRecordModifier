using System;
using System.Text;
using System.Threading.Tasks;

namespace BallanceRecordModifier
{
    /// <summary>
    /// The ByteManipulator.
    /// Contains all methods to encode and decode files, read data from raw bytes array, etc.
    /// </summary>
    public class ByteManipulator
    {
        public byte[] Array { get; }
        private int _index;

        private ByteManipulator(byte[] array)
        {
            Array = array;
            _index = 0;
        }
        
        public static async Task<ByteManipulator> Create(byte[]? encoded)
        {
            if (encoded is null) throw new NullReferenceException("Attempting to create ByteManipulator with null array.");

            return await Task.Run(() => new ByteManipulator(Decode(encoded)));
        }
        
        public static byte[] Decode(byte[]? array)
        {
            if (array is null) throw new NullReferenceException("Array cannot be null.");
            if (array.Length == 0) throw new ArgumentNullException(nameof(array), "Array cannot be empty.");
            
            for (int index = 0; index < array.Length; index++)
            {
                array[index] = (byte) (array[index] << 3 | array[index] >> 5);
                array[index] = (byte) (-(array[index] ^ 0xAF));
            }

            return array;
        }

        public static byte[] Encode(byte[]? array)
        {
            if (array is null) throw new NullReferenceException();
            if (array.Length == 0) throw new ArgumentNullException();

            for (var index = 0; index < array.Length; index++)
            {
                array[index] = (byte) (-(array[index]) ^ 0xAF);
                array[index] = (byte) (array[index] << 5 | array[index] >> 3);
            }

            return array;
        }

        public string ReadString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (_index >= Array.Length) throw new InvalidOperationException("Byte array has been exhausted.");
            
            for (; _index < Array.Length; _index++)
            {
                if (Array[_index] == 0)
                {
                    _index++;
                    break;
                }
                
                stringBuilder.Append((char)Array[_index]);
            }

            return stringBuilder.ToString();
        }

        public int ReadInt()
        {
            if (_index + 4 > Array.Length) throw new InvalidOperationException("The end of byte array has been reached.");

            var ret = BitConverter.ToInt32(Array, _index);
            _index += 4;
            return ret;
        }
        
        public float ReadFloat()
        {
            if (_index + 4 > Array.Length) throw new InvalidOperationException("The end of byte array has been reached.");
            
            var ret = BitConverter.ToSingle(Array, _index);
            _index += 4;
            return ret;
        }

        public void Reset() => _index = 0;
    }
}