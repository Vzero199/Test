using System;
using System.Collections.Generic;
using System.Linq;

public class PriorityQueue<TElement, TPriority>
{
	private readonly SortedDictionary<TPriority, Queue<TElement>> _dict = new SortedDictionary<TPriority, Queue<TElement>>();

	public int Count => _dict.Values.Sum((Queue<TElement> queue) => queue.Count);

	public void Enqueue(TElement element, TPriority priority)
	{
		if (!_dict.TryGetValue(priority, out var value))
		{
			value = new Queue<TElement>();
			_dict[priority] = value;
		}
		value.Enqueue(element);
	}

	public TElement Dequeue()
	{
		if (_dict.Count == 0)
		{
			throw new InvalidOperationException("The queue is empty.");
		}
		TPriority firstKey = GetFirstKey();
		Queue<TElement> queue = _dict[firstKey];
		TElement result = queue.Dequeue();
		if (queue.Count == 0)
		{
			_dict.Remove(firstKey);
		}
		return result;
	}

	public bool Contains(TElement element)
	{
		return _dict.Values.Any((Queue<TElement> queue) => queue.Contains(element));
	}

	public TPriority GetFirstKey()
	{
		if (_dict.Count == 0)
		{
			throw new InvalidOperationException("The queue is empty.");
		}
		return _dict.Keys.Min();
	}
}
