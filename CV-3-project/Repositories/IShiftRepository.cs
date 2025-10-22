using CV_3_project.Models;

namespace CV_3_project.Repositories
{
    public interface IShiftRepository
    {
        void Add(Shift shift);
        void Update(Shift shift);
        Shift? GetById(string mongoId); // Теперь принимаем string
        List<Shift> GetAll();
    }
}