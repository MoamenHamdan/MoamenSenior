using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M_Suite.Data;
using M_Suite.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M_Suite.Controllers
{
    [Route("api/Item")]
    [ApiController]
    public class ItemApiController : ControllerBase
    {
        private readonly MSuiteContext _context;

        public ItemApiController(MSuiteContext context)
        {
            _context = context;
        }

        // GET: api/Item
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetItems()
        {
            return await _context.Items
                .Where(i => i.ItActive == 1)
                .Select(i => new
                {
                    i.ItId,
                    i.ItCode,
                    i.ItDescriptionLan1
                })
                .OrderBy(i => i.ItDescriptionLan1)
                .ToListAsync();
        }

        // GET: api/Item/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetItem(int id)
        {
            var item = await _context.Items
                .Include(i => i.ItCdIdItgNavigation)
                .Include(i => i.ItUom)
                .Where(i => i.ItId == id)
                .Select(i => new
                {
                    i.ItId,
                    i.ItCode,
                    i.ItDescriptionLan1,
                    i.ItDescriptionLan2,
                    i.ItWeight,
                    UomName = i.ItUom.UomNameLan1,
                    ItemGroupName = i.ItCdIdItgNavigation.CdDescriptionLan1,
                    i.ItActive,
                    i.ItIsSaleable,
                    i.ItIsService
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // GET: api/Item/Search/{searchTerm}
        [HttpGet("Search/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<object>>> SearchItems(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Search term cannot be empty");
            }

            return await _context.Items
                .Where(i => i.ItActive == 1 && 
                           (i.ItCode.Contains(searchTerm) || 
                            i.ItDescriptionLan1.Contains(searchTerm) ||
                            i.ItDescriptionLan2.Contains(searchTerm)))
                .Select(i => new
                {
                    i.ItId,
                    i.ItCode,
                    i.ItDescriptionLan1
                })
                .OrderBy(i => i.ItDescriptionLan1)
                .Take(20)
                .ToListAsync();
        }
    }
} 