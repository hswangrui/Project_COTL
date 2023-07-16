using System;
using System.Collections;
using System.Collections.Generic;

namespace Ara
{
	public class ElasticArray<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private T[] data = new T[16];

		private int count;

		public int Count
		{
			get
			{
				return count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public T this[int index]
		{
			get
			{
				return data[index];
			}
			set
			{
				data[index] = value;
			}
		}

		public T[] Data
		{
			get
			{
				return data;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			int i = 0;
			while (i < count)
			{
				yield return data[i];
				int num = i + 1;
				i = num;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T item)
		{
			EnsureCapacity(count + 1);
			data[count++] = item;
		}

		public void Clear()
		{
			count = 0;
		}

		public bool Contains(T item)
		{
			for (int i = 0; i < count; i++)
			{
				if (data[i].Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (array.Length - arrayIndex < count)
			{
				throw new ArgumentException();
			}
			Array.Copy(data, 0, array, arrayIndex, count);
		}

		public bool Remove(T item)
		{
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				if (!flag && data[i].Equals(item))
				{
					flag = true;
				}
				if (flag && i < count - 1)
				{
					data[i] = data[i + 1];
				}
			}
			if (flag)
			{
				count--;
			}
			return flag;
		}

		public int IndexOf(T item)
		{
			return Array.IndexOf(data, item);
		}

		public void Insert(int index, T item)
		{
			if (index < 0 || index > count)
			{
				throw new ArgumentOutOfRangeException();
			}
			EnsureCapacity(++count);
			for (int i = count - 1; i > index; i++)
			{
				data[i] = data[i - 1];
			}
			data[index] = item;
		}

		public void RemoveAt(int index)
		{
			for (int i = index; i < count; i++)
			{
				if (i < count - 1)
				{
					data[i] = data[i + 1];
				}
			}
			count--;
		}

		public void SetCount(int count)
		{
			EnsureCapacity(count);
			this.count = count;
		}

		public void EnsureCapacity(int capacity)
		{
			if (capacity >= data.Length)
			{
				Array.Resize(ref data, capacity * 2);
			}
		}

		public void Reverse()
		{
			int num = 0;
			int num2 = count - 1;
			while (num < num2)
			{
				T val = data[num];
				data[num++] = data[num2];
				data[num2--] = val;
			}
		}
	}
}
