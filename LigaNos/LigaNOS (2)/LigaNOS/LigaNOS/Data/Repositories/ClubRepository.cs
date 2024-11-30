using LigaNOS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LigaNOS.Data.Repositories
{
    public class ClubRepository : GenericRepository<Club>, IClubRepository
    {
        private readonly DataContext _context;
        public ClubRepository(DataContext context) : base(context)
        {
            _context = context;
        }
        public IQueryable GetAllWithUsers()
        {
            return _context.Clubs.Include(c => c.User);
        }
        public IEnumerable<SelectListItem> GetComboClubs()
        {
            var list = _context.Clubs.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Name
            }).ToList();
            list.Insert(0, new SelectListItem
            {
                Text = "(Select a club...)",
                Value = "0"
            });
            return list;
        }
        public async Task<bool> HasMatchesAsync(int clubId)
        {

            return await _context.Matches.AnyAsync(m => m.HomeClubId == clubId || m.AwayClubId == clubId);
        }

        public async Task<Club> GetByIdAsync(int id)
        {
            return await _context.Clubs.FindAsync(id);
        }
        public static byte[] ConvertImageToBytes(string imagePath)
        {
            return File.ReadAllBytes(imagePath);
        }
        public async Task SaveImageAsync(int clubId, byte[] imageData, string imageType)
        {
            var club = await _context.Clubs.FindAsync(clubId);
            if (club == null)
            {
                throw new InvalidOperationException($"Club with ID {clubId} not found.");
            }

            club.ImageData = imageData; // Assign image data
            club.ImageType = imageType; // Assign image MIME type (if applicable)

            _context.Clubs.Update(club); // Mark the club entity as modified
            await _context.SaveChangesAsync(); // Save changes to the database
        }
        public async Task<(byte[], string)> GetImageAsync(int clubId)
        {

            var club = await _context.Clubs.FindAsync(clubId);
            if (club != null && club.ImageData != null)
            {
                return (club.ImageData, club.ImageType);
            }

            throw new FileNotFoundException("Image not found for the specified club.");
        }

        

        Task<bool> IClubRepository.HasMatchesAsync(int clubId)
        {
            throw new NotImplementedException();
        }

        Task IClubRepository.SaveImageAsync(int clubId, string filePath)
        {
            throw new NotImplementedException();
        }

        Task<(byte[], string)> IClubRepository.GetImageAsync(int clubId)
        {
            throw new NotImplementedException();
        }
       
    }
}