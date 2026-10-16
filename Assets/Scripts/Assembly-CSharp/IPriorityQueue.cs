public interface IPriorityQueue<TElement, TPriority>
{
	void Enqueue(TElement element, TPriority priority);

	TElement Dequeue();

	TPriority GetFirstKey();
}
