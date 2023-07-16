using System;
using System.IO;

public sealed class NotClosingMemoryStream : MemoryStream
{
	private MemoryStream stream;

	private bool closed;

	public Stream BaseStream
	{
		get
		{
			return stream;
		}
	}

	public override bool CanRead
	{
		get
		{
			if (!closed)
			{
				return stream.CanRead;
			}
			return false;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (!closed)
			{
				return stream.CanSeek;
			}
			return false;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (!closed)
			{
				return stream.CanWrite;
			}
			return false;
		}
	}

	public override long Length
	{
		get
		{
			CheckClosed();
			return stream.Length;
		}
	}

	public override long Position
	{
		get
		{
			CheckClosed();
			return stream.Position;
		}
		set
		{
			CheckClosed();
			stream.Position = value;
		}
	}

	public NotClosingMemoryStream(MemoryStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		this.stream = stream;
	}

	private void CheckClosed()
	{
		if (closed)
		{
			throw new InvalidOperationException("Wrapper has been closed or disposed");
		}
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		CheckClosed();
		return stream.BeginRead(buffer, offset, count, callback, state);
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		CheckClosed();
		return stream.BeginWrite(buffer, offset, count, callback, state);
	}

	public override void Close()
	{
		if (!closed)
		{
			stream.Flush();
		}
	}

	public override int EndRead(IAsyncResult asyncResult)
	{
		CheckClosed();
		return stream.EndRead(asyncResult);
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		CheckClosed();
		stream.EndWrite(asyncResult);
	}

	public override void Flush()
	{
		CheckClosed();
		stream.Flush();
	}

	public override object InitializeLifetimeService()
	{
		throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		CheckClosed();
		return stream.Read(buffer, offset, count);
	}

	public override int ReadByte()
	{
		CheckClosed();
		return stream.ReadByte();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		CheckClosed();
		return stream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		CheckClosed();
		stream.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		CheckClosed();
		stream.Write(buffer, offset, count);
	}

	public override void WriteByte(byte value)
	{
		CheckClosed();
		stream.WriteByte(value);
	}

	public override byte[] GetBuffer()
	{
		return stream.GetBuffer();
	}
}
