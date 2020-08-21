using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;
using Shiny.BluetoothLE.Central;
using Shiny.Infrastructure;

namespace Shiny.BluetoothLE
{
    internal class L2CapChannel : Stream, IChannel
    {
        private readonly CBL2CapChannel nativeChannel;
        private readonly AsyncManualResetEvent inputStreamEvent = new AsyncManualResetEvent();
        private readonly AsyncManualResetEvent outputStreamEvent = new AsyncManualResetEvent();
        private bool streamEnded;
        private bool streamError;

        public L2CapChannel(CBL2CapChannel nativeChannel)
        {
            this.nativeChannel = nativeChannel;
            this.nativeChannel.InputStream.OnEvent += this.OnInputStreamEvent;
            this.nativeChannel.InputStream.Schedule(NSRunLoop.Current, NSRunLoopMode.Default);
            this.nativeChannel.InputStream.Open();
            this.nativeChannel.OutputStream.OnEvent += this.OnOutputStreamEvent;
            this.nativeChannel.OutputStream.Schedule(NSRunLoop.Current, NSRunLoopMode.Default);
            this.nativeChannel.OutputStream.Open();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.nativeChannel.InputStream.Close();
                this.nativeChannel.InputStream.Unschedule(NSRunLoop.Current, NSRunLoopMode.Default);
                this.nativeChannel.InputStream.OnEvent -= this.OnInputStreamEvent;
                this.nativeChannel.OutputStream.Close();
                this.nativeChannel.OutputStream.Unschedule(NSRunLoop.Current, NSRunLoopMode.Default);
                this.nativeChannel.OutputStream.OnEvent -= this.OnOutputStreamEvent;
            }
        }

        public Guid PeerUuid => this.nativeChannel.Peer.Identifier.ToGuid();
        public int Psm => this.nativeChannel.Psm;
        public Stream Stream => this;

        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.CheckError();
            if (this.streamEnded)
                return 0;

            while (!this.nativeChannel.InputStream.HasBytesAvailable())
            {
                // TODO: Allow cancellation-token for the WaitAsync method
                cancellationToken.ThrowIfCancellationRequested();
                await this.inputStreamEvent.WaitAsync().ConfigureAwait(false);
                var hasBytesAvailable = this.nativeChannel.InputStream.HasBytesAvailable();
                if (!hasBytesAvailable || this.streamEnded)
                {
                    this.CheckError();
                    if (this.streamError)
                        return 0;
                }
            }

            this.inputStreamEvent.Reset();
            var bytesRead = Convert.ToInt32(this.nativeChannel.InputStream.Read(buffer, offset, new nuint((uint)count)));
            return bytesRead;
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.CheckError();

            while (count > 0)
            {
                await this.outputStreamEvent.WaitAsync().ConfigureAwait(false);
                this.outputStreamEvent.Reset();

                if (this.streamEnded)
                {
                    this.CheckError();
                    throw new L2CapException("Cannot write to a closed stream.");
                }

                var bytesWritten = Convert.ToInt32(this.nativeChannel.OutputStream.Write(buffer, offset, new nuint((uint)count)));
                if (bytesWritten == 0)
                {
                    this.CheckError();
                    return;
                }

                offset += bytesWritten;
                count -= bytesWritten;
            }
        }

        public override void Flush()
        {
            // NOP
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
            => Convert.ToInt32(this.nativeChannel.InputStream.Read(buffer, offset, new nuint((uint)count)));

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                var bytesWritten = Convert.ToInt32(this.nativeChannel.OutputStream.Write(buffer, offset, new nuint((uint)count)));
                offset += bytesWritten;
                count -= bytesWritten;
            }
        }

        private void OnInputStreamEvent(object sender, NSStreamEventArgs e)
        {
            var streamEvent = e.StreamEvent;
            if (streamEvent.HasFlag(NSStreamEvent.HasBytesAvailable))
                this.inputStreamEvent.Set();
            if (streamEvent.HasFlag(NSStreamEvent.EndEncountered) || streamEvent.HasFlag(NSStreamEvent.ErrorOccurred))
            {
                this.streamEnded = true;
                if (streamEvent.HasFlag(NSStreamEvent.ErrorOccurred))
                    this.streamError = true;
                this.inputStreamEvent.Set();
            }
        }

        private void OnOutputStreamEvent(object sender, NSStreamEventArgs e)
        {
            var streamEvent = e.StreamEvent;
            if (streamEvent.HasFlag(NSStreamEvent.HasSpaceAvailable) || streamEvent.HasFlag(NSStreamEvent.EndEncountered) || streamEvent.HasFlag(NSStreamEvent.ErrorOccurred))
            {
                if (streamEvent.HasFlag(NSStreamEvent.ErrorOccurred))
                    this.streamError = true;
                this.outputStreamEvent.Set();
            }
        }

        private void CheckError()
        {
            if (this.streamError)
                throw new L2CapException("The L2CAP channel is in the error state.");
        }
    }
}
