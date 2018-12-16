using System.Threading.Tasks;

namespace raumPlayer.Interfaces
{
    public interface IFirstRunDisplayService
    {
        Task ShowIfAppropriateAsync();
    }
}
