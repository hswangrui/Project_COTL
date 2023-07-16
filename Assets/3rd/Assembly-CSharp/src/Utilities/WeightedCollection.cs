using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace src.Utilities
{
	public class WeightedCollection<T> : IEnumerable, IEnumerator<T>, IEnumerator, IDisposable
	{
		private class WeightedElement
		{
			public T Element;

			public float Weight;
		}

		private List<WeightedElement> _contents = new List<WeightedElement>();

		private int _position = -1;

		public int Count
		{
			get
			{
				return _contents.Count;
			}
		}

		public T this[int index]
		{
			get
			{
				return _contents[index].Element;
			}
			set
			{
				_contents[index].Element = value;
			}
		}

		public T Current
		{
			get
			{
				return _contents[_position].Element;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}

		public void Add(T element, float weight)
		{
			Add(new WeightedElement
			{
				Element = element,
				Weight = weight
			});
		}

		private void Add(WeightedElement weightedElement)
		{
			if (Contains(weightedElement))
			{
				_contents.Find((WeightedElement e) => e.Element.Equals(weightedElement.Element)).Weight += weightedElement.Weight;
			}
			else
			{
				_contents.Add(weightedElement);
			}
		}

		public void Remove(T element)
		{
			if (Contains(element))
			{
				_contents.Remove(_contents.Find((WeightedElement e) => e.Element.Equals(element)));
			}
		}

		private bool Contains(WeightedElement weightedElement)
		{
			return Contains(weightedElement.Element);
		}

		public bool Contains(T element)
		{
			return _contents.Any((WeightedElement e) => e.Element.Equals(element));
		}

		public T GetRandomItem()
		{
			return _contents[GetRandomIndex()].Element;
		}

		public int GetRandomIndex()
		{
			List<float> distributedWeights = GetDistributedWeights();
			float max = distributedWeights.Last();
			float rand = UnityEngine.Random.Range(0f, max);
			return distributedWeights.IndexOf(distributedWeights.First((float weight) => rand < weight));
		}

		private List<float> GetDistributedWeights()
		{
			List<float> list = new List<float> { _contents[0].Weight };
			for (int i = 1; i < Count; i++)
			{
				list.Add(list[i - 1] + _contents[i].Weight);
			}
			return list;
		}

		public void Clear()
		{
			_contents.Clear();
		}

		public IEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			_position++;
			return _position < _contents.Count;
		}

		public void Reset()
		{
			_position = -1;
		}

		public void Dispose()
		{
		}
	}
}
