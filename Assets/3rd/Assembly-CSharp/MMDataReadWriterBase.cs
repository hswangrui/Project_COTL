using System;

public abstract class MMDataReadWriterBase<T>
{
	public Action<T> OnReadCompleted;

	public Action OnCreateDefault;

	public Action OnWriteCompleted;

	public Action OnDeletionComplete;

	public Action<MMReadWriteError> OnReadError;

	public Action<MMReadWriteError> OnWriteError;

	public abstract void Write(T data, string filename, bool encrypt = true, bool backup = true);

	public abstract void Read(string filename);

	public abstract void Delete(string filename);

	public abstract bool FileExists(string filename);
}
