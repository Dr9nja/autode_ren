using System.Linq;
using System.Threading.Tasks;
using KooliProjekt.Application.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KooliProjekt.WebAPI.Controllers
{
    [ApiController]
    [Route("api/shop")]
    public class ShopController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ShopController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var shop = await _dbContext.Shops
                .AsNoTracking()
                .Select(item => new
                {
                    item.Name,
                    item.Street,
                    item.Email,
                    item.Country
                })
                .FirstOrDefaultAsync();

            return shop == null ? NotFound() : Ok(shop);
        }
    }
}
