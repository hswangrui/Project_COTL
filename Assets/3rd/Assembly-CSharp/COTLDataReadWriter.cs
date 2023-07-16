using System;

public class COTLDataReadWriter<T> : MMDataReadWriterBase<T>
{
	protected MMDataReadWriterBase<T> _readWriter;

	public COTLDataReadWriter()
	{
		_readWriter = new MMJsonDataReadWriter<T>();
		MMDataReadWriterBase<T> readWriter = _readWriter;
		readWriter.OnWriteCompleted = (Action)Delegate.Combine(readWriter.OnWriteCompleted, (Action)delegate
		{
			Action onWriteCompleted = OnWriteCompleted;
			if (onWriteCompleted != null)
			{
				onWriteCompleted();
			}
		});
		MMDataReadWriterBase<T> readWriter2 = _readWriter;
		readWriter2.OnReadCompleted = (Action<T>)Delegate.Combine(readWriter2.OnReadCompleted, (Action<T>)delegate(T data)
		{
			Action<T> onReadCompleted = OnReadCompleted;
			if (onReadCompleted != null)
			{
				onReadCompleted(data);
			}
		});
		MMDataReadWriterBase<T> readWriter3 = _readWriter;
		readWriter3.OnCreateDefault = (Action)Delegate.Combine(readWriter3.OnCreateDefault, (Action)delegate
		{
			Action onCreateDefault = OnCreateDefault;
			if (onCreateDefault != null)
			{
				onCreateDefault();
			}
		});
		MMDataReadWriterBase<T> readWriter4 = _readWriter;
		readWriter4.OnDeletionComplete = (Action)Delegate.Combine(readWriter4.OnDeletionComplete, (Action)delegate
		{
			Action onDeletionComplete = OnDeletionComplete;
			if (onDeletionComplete != null)
			{
				onDeletionComplete();
			}
		});
		MMDataReadWriterBase<T> readWriter5 = _readWriter;
		readWriter5.OnReadError = (Action<MMReadWriteError>)Delegate.Combine(readWriter5.OnReadError, (Action<MMReadWriteError>)delegate(MMReadWriteError error)
		{
			Action<MMReadWriteError> onReadError = OnReadError;
			if (onReadError != null)
			{
				onReadError(error);
			}
		});
		MMDataReadWriterBase<T> readWriter6 = _readWriter;
		readWriter6.OnWriteError = (Action<MMReadWriteError>)Delegate.Combine(readWriter6.OnWriteError, (Action<MMReadWriteError>)delegate(MMReadWriteError error)
		{
			Action<MMReadWriteError> onWriteError = OnWriteError;
			if (onWriteError != null)
			{
				onWriteError(error);
			}
		});
	}

	public override void Write(T data, string filename, bool encrypt = true, bool backup = true)
	{
		_readWriter.Write(data, filename, encrypt, backup);
	}

	public override void Read(string filename)
	{
		_readWriter.Read(filename);
	}

	public override void Delete(string filename)
	{
		_readWriter.Delete(filename);
	}

	public override bool FileExists(string filename)
	{
		return _readWriter.FileExists(filename);
	}
}
