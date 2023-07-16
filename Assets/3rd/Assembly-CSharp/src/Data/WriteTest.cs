using System;
using UnityEngine;

namespace src.Data
{
	public class WriteTest : BaseMonoBehaviour
	{
		private const string kMMJsonTestFilename = "mmJsonTestFile";

		private const string kUnifyTestFilename = "unifyTestFile";

		private MMDataReadWriterBase<SerializeTest> _mmJsonTest = new MMJsonDataReadWriter<SerializeTest>();

		private MMDataReadWriterBase<SerializeTest> _unifyTest = new UnifyDataReadWriter<SerializeTest>();

		[SerializeField]
		private SerializeTest _mmTestData = new SerializeTest();

		[SerializeField]
		private SerializeTest _unifyTestData = new SerializeTest();

		private void Start()
		{
			MMDataReadWriterBase<SerializeTest> mmJsonTest = _mmJsonTest;
			mmJsonTest.OnCreateDefault = (Action)Delegate.Combine(mmJsonTest.OnCreateDefault, (Action)delegate
			{
				_mmTestData = new SerializeTest();
			});
			MMDataReadWriterBase<SerializeTest> mmJsonTest2 = _mmJsonTest;
			mmJsonTest2.OnReadCompleted = (Action<SerializeTest>)Delegate.Combine(mmJsonTest2.OnReadCompleted, (Action<SerializeTest>)delegate(SerializeTest data)
			{
				_mmTestData = data;
			});
			MMDataReadWriterBase<SerializeTest> unifyTest = _unifyTest;
			unifyTest.OnCreateDefault = (Action)Delegate.Combine(unifyTest.OnCreateDefault, (Action)delegate
			{
				_unifyTestData = new SerializeTest();
			});
			MMDataReadWriterBase<SerializeTest> unifyTest2 = _unifyTest;
			unifyTest2.OnReadCompleted = (Action<SerializeTest>)Delegate.Combine(unifyTest2.OnReadCompleted, (Action<SerializeTest>)delegate(SerializeTest data)
			{
				_unifyTestData = data;
			});
		}

		public void Write()
		{
			_mmJsonTest.Write(_mmTestData, "mmJsonTestFile");
			_unifyTest.Write(_unifyTestData, "unifyTestFile");
		}

		public void Read()
		{
		}

		public void Delete()
		{
			_mmJsonTest.Delete("mmJsonTestFile");
			_unifyTest.Delete("unifyTestFile");
		}
	}
}
