using CV_3_project.EventSystem;
using CV_3_project.Models;

namespace CV_3_project.Observers
{
    public class ShiftEventData : EventData
    {
        public Shift Shift { get; }
        public Account? Account { get; }

        public ShiftEventData(Shift shift, Account? account)
        {
            Shift = shift;
            Account = account;
        }
    }
}
