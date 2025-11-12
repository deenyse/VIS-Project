namespace CV_3_project.EventSystem
{
    public class EventManager
    {
        private readonly List<IObserver> _observers = new List<IObserver>();

        public void Register(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void Unregister(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(string eventType, EventData? data)
        {
            foreach (var observer in _observers)
            {
                observer.Update(eventType, data);
            }
        }
    }
}
