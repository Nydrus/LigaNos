using LigaNOS.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using System.IO;


namespace LigaNOS.Controllers.API
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClubsController : Controller
    {
        private readonly IClubRepository _clubRepository;

        public ClubsController(IClubRepository clubRepository)
        {
            _clubRepository = clubRepository;
        }
        [HttpGet]
        public IActionResult GetClubs()
        {
            return Ok(_clubRepository.GetAllWithUsers());

        }
        [HttpPost("upload/{clubId}")]
        public async Task<IActionResult> UploadImage(int clubId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            await _clubRepository.SaveImageAsync(clubId, filePath);

            return Ok("Image uploaded successfully.");
        }
        [HttpGet("image/{clubId}")]
        public async Task<IActionResult> GetImage(int clubId)
        {
            try
            {
                var (imageData, imageType) = await _clubRepository.GetImageAsync(clubId);
                return File(imageData, imageType);
            }
            catch (FileNotFoundException)
            {
                return NotFound("Image not found.");
            }

        }

    }
}
