using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APITDS;
using APITDS.Models;
using APITDS.DTO;
using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace APITDS.Controllers
{
    [Route("api/Inventory")]
    [ApiController]
    public class InventoriesController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public InventoriesController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Inventories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetInventories()
        {
          if (_context.Inventories == null)
          {
              return NotFound();
          }
            return await _context.Inventories.ToListAsync();
        }

        // GET: api/Inventories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory>> GetInventory(int id)
        {
          if (_context.Inventories == null)
          {
              return NotFound();
          }
            var inventory = await _context.Inventories.FindAsync(id);

            if (inventory == null)
            {
                return NotFound();
            }

            return inventory;
        }

        // PUT: api/Inventories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventory(int id, Inventory inventory)
        {
            if (id != inventory.Id)
            {
                return BadRequest();
            }

            _context.Entry(inventory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

   

        // POST: api/Inventories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Inventory>> PostInventory(Inventory inventory)
        {
          if (_context.Inventories == null)
          {
              return Problem("Entity set 'ApiDbContext.Inventories'  is null.");
          }
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInventory", new { id = inventory.Id }, inventory);
        }

        // DELETE: api/Inventories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            if (_context.Inventories == null)
            {
                return NotFound();
            }
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InventoryExists(int id)
        {
            return (_context.Inventories?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadInventory(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length <= 0)
            {
                return BadRequest("Invalid file.");
            }

            if (!Path.GetExtension(csvFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid file format. Please upload a CSV file.");
            }

            var inventories = new List<InventoryDto>();
            using (var parser = new TextFieldParser(csvFile.OpenReadStream()))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                if (!parser.EndOfData)
                {
                    parser.ReadFields(); // Skip header
                }

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    if (fields != null && fields.Length >= 6)
                    {
                        var dateString = fields[1]?.Trim();
                        if (string.IsNullOrEmpty(dateString))
                        {
                            continue;
                        }

                        var inventory = new InventoryDto
                        {
                            Store = fields[0]?.Trim(),
                            Date = DateOnly.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                            Flavor = fields[2]?.Trim(),
                            IsSeasonFlavor = string.Equals(fields[3]?.Trim(), "Yes", StringComparison.OrdinalIgnoreCase),
                            Quantity = int.TryParse(fields[4]?.Trim(), out var quantity) ? quantity : 0,
                            ListedBy = fields[5]?.Trim()
                        };
                        inventories.Add(inventory);
                    }
                }
            }

            foreach (var inventoryDto in inventories)
            {
                var store = await _context.Stores.FirstOrDefaultAsync(s => s.Name == inventoryDto.Store);
                if (store == null)
                {
                    store = new Store { Name = inventoryDto.Store };
                    _context.Stores.Add(store);
                    await _context.SaveChangesAsync(); // Guarda inmediatamente para obtener el ID.
                }

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Name == inventoryDto.ListedBy);
                if (employee == null)
                {
                    employee = new Employee { Name = inventoryDto.ListedBy };
                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync(); // Guarda inmediatamente para obtener el ID.
                }

                var inventory = new Inventory
                {
                    StoreId = store.Id,
                    EmployeeId = employee.Id,
                    Date = inventoryDto.Date,
                    Flavor = inventoryDto.Flavor,
                    IsSeasonFlavor = inventoryDto.IsSeasonFlavor,
                    Quantity = inventoryDto.Quantity,
                };
                _context.Inventories.Add(inventory);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(UploadInventory), new { Message = "Inventory uploaded successfully" });
        }



    }

}
