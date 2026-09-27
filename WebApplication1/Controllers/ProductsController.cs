using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace WebApplication1.data
{
    [ApiController]
    [Route("api/[controller]")] // Route will be: api/products
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inject the DB Context
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProducts(int id)
        {
            // Fetch the product with the given id using raw SQL from hoteldata.dbo.inventory
            var product = await _context.Products
                .FromSqlInterpolated($"SELECT Id, rawmaterial, quantity, category FROM hoteldata.dbo.inventory WHERE Id = {id}")
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            // Return the product as JSON
            return Ok(product);
        }

        // GET: api/helloyou
        [HttpGet("/api/helloyou")]
        public ActionResult<string> helloprint()
        {
            // Return the greeting string for GET /api/helloyou
            return Ok("hello world");
        }


    }
}