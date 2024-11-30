using LigaNOS.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace LigaNOS.Data.Repositories
{
    public interface IClubRepository : IGenericRepository<Club>
    {
        public IQueryable GetAllWithUsers();
        Task<bool> HasMatchesAsync(int clubId);
        Task SaveImageAsync(int clubId, string filePath);
        Task<(byte[], string)> GetImageAsync(int clubId);
        Task SaveImageAsync(int clubId, byte[] imageData, string imageType);
    }
}