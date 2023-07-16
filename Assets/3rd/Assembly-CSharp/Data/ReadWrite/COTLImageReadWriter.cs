using System;
using UnityEngine;

namespace Data.ReadWrite
{
	public class COTLImageReadWriter : MMImageReadWriterBase<Texture2D>
	{
		protected MMImageReadWriterBase<Texture2D> _readWriter;

		public COTLImageReadWriter()
		{
			_readWriter = new MMImageDataReadWriter();
			MMImageReadWriterBase<Texture2D> readWriter = _readWriter;
			readWriter.OnWriteCompleted = (Action)Delegate.Combine(readWriter.OnWriteCompleted, (Action)delegate
			{
				OnWriteCompleted?.Invoke();
			});
			MMImageReadWriterBase<Texture2D> readWriter2 = _readWriter;
			readWriter2.OnReadCompleted = (Action<Texture2D>)Delegate.Combine(readWriter2.OnReadCompleted, (Action<Texture2D>)delegate(Texture2D data)
			{
				OnReadCompleted?.Invoke(data);
			});
			MMImageReadWriterBase<Texture2D> readWriter3 = _readWriter;
			readWriter3.OnCreateDefault = (Action)Delegate.Combine(readWriter3.OnCreateDefault, (Action)delegate
			{
				OnCreateDefault?.Invoke();
			});
			MMImageReadWriterBase<Texture2D> readWriter4 = _readWriter;
			readWriter4.OnDeletionComplete = (Action)Delegate.Combine(readWriter4.OnDeletionComplete, (Action)delegate
			{
				OnDeletionComplete?.Invoke();
			});
			MMImageReadWriterBase<Texture2D> readWriter5 = _readWriter;
			readWriter5.OnReadError = (Action<MMReadWriteError>)Delegate.Combine(readWriter5.OnReadError, (Action<MMReadWriteError>)delegate(MMReadWriteError error)
			{
				OnReadError?.Invoke(error);
			});
			MMImageReadWriterBase<Texture2D> readWriter6 = _readWriter;
			readWriter6.OnWriteError = (Action<MMReadWriteError>)Delegate.Combine(readWriter6.OnWriteError, (Action<MMReadWriteError>)delegate(MMReadWriteError error)
			{
				OnWriteError?.Invoke(error);
			});
		}

		public override void Write(Texture2D data, string filename)
		{
			_readWriter.Write(data, filename);
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

		public override string[] GetFiles()
		{
			return _readWriter.GetFiles();
		}
	}
}
