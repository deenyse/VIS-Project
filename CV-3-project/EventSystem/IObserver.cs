namespace CV_3_project.EventSystem
{
    public interface IObserver
    {
        void Update(string eventType, EventData? data);
    }
}
