using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BallanceRecordModifier
{
    public class TdbStream : Stream
    {
        private readonly Stream _rawStream;

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
            a = (byte) -(a ^ 0xAF);
            a = (byte) (a << 5 | a >> 3);

            return a;
        }

        public TdbStream(Stream rawStream)
        {
            _rawStream = rawStream;
        }

        public TdbStream(byte[]? chunk = null)
        {
            _rawStream = chunk is null ? new MemoryStream() : new MemoryStream(chunk!);
        }

        public override void Flush()
            => _rawStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var ret = _rawStream.Read(buffer, offset, count);
            for (var i = offset; i < count; i++)
                buffer[i] = Decode(buffer[i]);

            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
            => _rawStream.Seek(offset, origin);

        public override void SetLength(long value)
            => _rawStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
            => _rawStream.Write(buffer, offset, count);
        // {
        //     byte[] bytes = new byte[count];
        //     for (var i = 0; i < count; i++)
        //         bytes[i] = Encode(buffer[i]);
        //     
        //     _rawStream.Write(bytes, offset, count);
        // }

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
            for (var i = 0; i < ret; i++)
                buffer[i] = Decode(buffer[i]);
            
            return ret;
        }

        public override void Write(ReadOnlySpan<byte> buffer) => _rawStream.Write(buffer);
        // {
        //     byte[] bytes = new byte[buffer.Length];
        //     buffer.CopyTo(bytes);
        //     for (var i = 0; i < buffer.Length; i++)
        //         bytes[i] = Encode(buffer[i]);
        //     
        //     _rawStream.Write(bytes);
        // }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback,
            object? state)
            => _rawStream.BeginRead(buffer, offset, count, callback, state);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback,
            object? state)
            => _rawStream.BeginRead(buffer, offset, count, callback, state);

        public override void CopyTo(Stream destination, int bufferSize) 
            => _rawStream.CopyTo(destination, bufferSize);
        public override ValueTask DisposeAsync() => _rawStream.DisposeAsync();
        public override int EndRead(IAsyncResult asyncResult) => _rawStream.EndRead(asyncResult);
        public override void EndWrite(IAsyncResult asyncResult) => _rawStream.EndWrite(asyncResult);

        public override Task FlushAsync(CancellationToken cancellationToken) =>
            _rawStream.FlushAsync(cancellationToken);

        public new Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None);
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            byte[] bytes = new byte[count]; 
            var ret = _rawStream.ReadAsync(bytes.AsMemory(offset, count), cancellationToken); 
            for (var i = 0; i < count; i++) 
                bytes[i] = Decode(bytes[i]);
                
            bytes.CopyTo(buffer, 0);
            return ret.AsTask();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var bytes = new Memory<byte>(buffer.ToArray()); 
            var ret = await _rawStream.ReadAsync(bytes, cancellationToken); 
            for (var i = 0; i < buffer.Length; i++) 
                bytes.Span[i] = Decode(bytes.Span[i]);
                
            bytes.CopyTo(buffer);
            
            return ret;
        }

        public override int ReadByte()
        {
            var ret = _rawStream.ReadByte();
            return ret == -1 ? -1 : Decode((byte) ret);
        }

        public override int ReadTimeout
        {
            get => _rawStream.ReadTimeout;
            set => _rawStream.ReadTimeout = value;
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _rawStream.WriteAsync(buffer, offset, count, cancellationToken);
        // {
        //     if (count == 0)
        //         return Task.CompletedTask;
        //         
        //     var bytes = new byte[count - offset];
        //     buffer[offset..(offset + count - 1)].CopyTo(bytes, offset);
        //     for (var i = 0; i < count; i++)
        //         bytes[i] = Encode(bytes[i]);
        //     return _rawStream.WriteAsync(bytes, 0, count - offset, cancellationToken);
        // }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = new CancellationToken())
            => _rawStream.WriteAsync(buffer, cancellationToken);
        // {
        //     var memory = new Memory<byte>(buffer.ToArray());
        //     var span = memory.Span;
        //     for (var i = 0; i < buffer.Length; i++)
        //         span[i] = Encode(span[i]);
        //     return _rawStream.WriteAsync(memory, cancellationToken);
        // }

        public override void WriteByte(byte value)
        {
            byte[] bytes = new byte[1];
            bytes[0] = Encode(value);
            _rawStream.Write(bytes, 0, 1);
        }

        public override int WriteTimeout
        {
            get => _rawStream.WriteTimeout;
            set => _rawStream.WriteTimeout = value;
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => _rawStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }
}