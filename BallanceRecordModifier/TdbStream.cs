using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BallanceRecordModifier
{
    public class TdbStream : Stream
    {
        private readonly Stream _rawStream;
        public bool ReadAsEncoded { get; set; }
        public bool WriteAsEncoded { get; set; }

        private readonly bool _streamEncoded;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Decode(byte a)
        {
            a = (byte) (a << 3 | a >> 5);
            a = (byte) -(a ^ 0xAF);
            
            return a;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Encode(byte a)
        {
            a = (byte) (-a ^ 0xAF);
            a = (byte) (a << 5 | a >> 3);

            return a;
        }
        
        public TdbStream(bool readAsEncoded, bool writeAsEncoded, Stream rawStream)
        {
            _rawStream = rawStream;
            ReadAsEncoded = readAsEncoded;
            WriteAsEncoded = writeAsEncoded;
            _streamEncoded = writeAsEncoded;
        }

        public TdbStream(bool readAsEncoded, bool writeAsEncoded, byte[]? chunk = null)
        {
            _rawStream = chunk is null ? new MemoryStream() : new MemoryStream(chunk!);
            ReadAsEncoded = readAsEncoded;
            WriteAsEncoded = writeAsEncoded;
            _streamEncoded = writeAsEncoded;
        }

        public override void Flush()
            => _rawStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var ret = _rawStream.Read(buffer, offset, count);
            if (ReadAsEncoded == _streamEncoded) 
                return ret; // No matter it's encoded or not, just read is fine
            
            Debug.Assert(ReadAsEncoded != _streamEncoded);
            for (var i = offset; i < count; i++)
                // Since we just ruled out the situation that read mode is the same as the state in the stream
                // For the time being, the read mode MUST be the opposite of the state in the stream
                // So we can just know what (have) to do next depending on one of the states
                buffer[i] = _streamEncoded   // implies ReadAsEncoded == false. aka. encoded stream -> decoded param
                    ? Decode(buffer[i]) : Encode(buffer[i]);

            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
            => _rawStream.Seek(offset, origin);

        public override void SetLength(long value)
            => _rawStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_streamEncoded == WriteAsEncoded)
            {
                _rawStream.Write(buffer, offset, count);
                return;
            }

            byte[] bytes = new byte[count];
            for (var i = offset; i < offset + count; i++)
                bytes[i] = _streamEncoded   // decoded param -> encoded stream
                    ? Encode(buffer[i]) : Decode(buffer[i]);
                
            _rawStream.Write(bytes, offset, count);
        }

        public override bool CanRead => _rawStream.CanRead;
        public override bool CanSeek => _rawStream.CanSeek;
        public override bool CanWrite => _rawStream.CanWrite;
        public override long Length => _rawStream.Length;

        public override long Position
        {
            get => _rawStream.Position;
            set => _rawStream.Position = value;
        }

        public override bool CanTimeout => _rawStream.CanTimeout;
        public override void Close() => _rawStream.Close();

        public override int Read(Span<byte> buffer)
        {
            var ret = _rawStream.Read(buffer);
            if (ReadAsEncoded == _streamEncoded)
                return ret;
            
            Debug.Assert(_streamEncoded != ReadAsEncoded);
            for (var i = 0; i < ret; i++)
                buffer[i] = _streamEncoded // encoded stream -> decoded param
                    ? Decode(buffer[i]) : Encode(buffer[i]);
            
            return ret;
        }

        public override void Write(ReadOnlySpan<byte> buffer)
            // => _rawStream.Write(buffer);
        {
            if (_streamEncoded == WriteAsEncoded)
            {
                _rawStream.Write(buffer);
                return;
            }

            Debug.Assert(_streamEncoded != WriteAsEncoded);
            byte[] bytes = new byte[buffer.Length];
            for (var i = 0; i < buffer.Length; i++)
                bytes[i] = _streamEncoded   // decoded param -> encoded stream
                    ? Encode(buffer[i]) : Decode(buffer[i]);
                 
            _rawStream.Write(bytes);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback,
            object? state)
            => _rawStream.BeginRead(buffer, offset, count, callback, state);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback,
            object? state)
            => _rawStream.BeginRead(buffer, offset, count, callback, state);

        public override void CopyTo(Stream destination, int bufferSize)
            // => _rawStream.CopyTo(destination, bufferSize);
        {
            if (destination is TdbStream stream)
                stream.WriteAsEncoded = _streamEncoded;
            
            _rawStream.CopyTo(destination, bufferSize);
        }
        public override ValueTask DisposeAsync() => _rawStream.DisposeAsync();
        public override int EndRead(IAsyncResult asyncResult) => _rawStream.EndRead(asyncResult);
        public override void EndWrite(IAsyncResult asyncResult) => _rawStream.EndWrite(asyncResult);

        public override Task FlushAsync(CancellationToken cancellationToken) =>
            _rawStream.FlushAsync(cancellationToken);

        // public new Task<int> ReadAsync(byte[] buffer, int offset, int count)
        // {
        //     return ReadAsync(buffer, offset, count, CancellationToken.None);
        // }
        
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_streamEncoded == ReadAsEncoded)
                return _rawStream.ReadAsync(buffer, offset, count, cancellationToken);
            
            byte[] bytes = new byte[count];
            var ret = _rawStream.ReadAsync(bytes.AsMemory(offset, count), cancellationToken);

            for (var i = 0; i < count; i++) 
                bytes[i] = _streamEncoded   // encoded stream -> decoded param
                    ? Decode(bytes[i]) : Encode(bytes[i]);
                
            bytes.CopyTo(buffer, 0);
            return ret.AsTask();
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (_streamEncoded == ReadAsEncoded)
                return _rawStream.ReadAsync(buffer, cancellationToken);
            
            var bytes = new Memory<byte>(buffer.ToArray()); 
            var ret = _rawStream.ReadAsync(bytes, cancellationToken); 
            for (var i = 0; i < buffer.Length; i++) 
                bytes.Span[i] = _streamEncoded  // encoded stream -> decoded param
                    ? Decode(bytes.Span[i]) : Encode(bytes.Span[i]);
                
            bytes.CopyTo(buffer);
            
            return ret;
        }

        public override int ReadByte()
        {
            if (_streamEncoded == ReadAsEncoded)
                return _rawStream.ReadByte();
            
            var ret = _rawStream.ReadByte();
            return ret == -1 ? -1 : _streamEncoded // encoded stream -> decoded byte
                ? Decode((byte) ret) : Encode((byte) ret);
        }

        public override int ReadTimeout
        {
            get => _rawStream.ReadTimeout;
            set => _rawStream.ReadTimeout = value;
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            // => _rawStream.WriteAsync(buffer, offset, count, cancellationToken);
        {
            if (count == 0)
                return Task.CompletedTask;

            if (_streamEncoded == WriteAsEncoded)
                return _rawStream.WriteAsync(buffer, offset, count, cancellationToken);
            
            Debug.Assert(_streamEncoded != WriteAsEncoded);
            var bytes = new byte[count];
            for (var i = offset; i < offset + count; i++)
                bytes[i] = _streamEncoded ? Encode(buffer[i]) : Decode(buffer[i]);
            return _rawStream.WriteAsync(bytes, 0, count - offset, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = new CancellationToken())
            // => _rawStream.WriteAsync(buffer, cancellationToken);
        {
            if (_streamEncoded == WriteAsEncoded)
                return _rawStream.WriteAsync(buffer, cancellationToken);

            var memory = new Memory<byte>(buffer.ToArray());
            var span = memory.Span;
            for (var i = 0; i < buffer.Length; i++)
                span[i] = _streamEncoded ? Encode(span[i]) : Decode(span[i]);
            return _rawStream.WriteAsync(memory, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            if (_streamEncoded == WriteAsEncoded)
            {
                _rawStream.WriteByte(value);
                return;
            }

            value = _streamEncoded ? Encode(value) : Decode(value);
            // byte[] bytes = {value};
            // bytes[0] = Encode(value);
            _rawStream.WriteByte(value);
        }

        public override int WriteTimeout
        {
            get => _rawStream.WriteTimeout;
            set => _rawStream.WriteTimeout = value;
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (destination is TdbStream stream)
                stream.WriteAsEncoded = _streamEncoded;
                
            return _rawStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }
    }
}