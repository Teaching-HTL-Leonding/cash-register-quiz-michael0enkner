using CashRegister.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CashRegister.WebApi.Controllers
{
    [ApiController]
    [Route("api/receipts")]
    public class ReceiptsController : ControllerBase
    {
        private readonly CashRegisterDataContext DataContext;

        public ReceiptsController(CashRegisterDataContext dataContext)
        {
            DataContext = dataContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] List<ReceiptLineDto> receiptLineDtos)
        {
            if (receiptLineDtos == null || receiptLineDtos.Count < 1) return BadRequest();

            // Read product data from DB for incoming product IDs
            var products = new Dictionary<int, Product>();

            // Here you have to add code that reads all products referenced by product IDs
            // in receiptDto.Lines and store them in the `products` dictionary.
            foreach (var line in receiptLineDtos)
            {
                products[line.ProductID] = await DataContext.Products.FirstOrDefaultAsync(p => p.ID == line.ProductID);
                if (products[line.ProductID] == null) return BadRequest();
            }

            // Build receipt from DTO
            var newReceipt = new Receipt
            {
                ReceiptTimestamp = DateTime.UtcNow,
                ReceiptLines = receiptLineDtos.Select(rl => new ReceiptLine
                {
                    ID = 0,
                    Product = products[rl.ProductID],
                    Amount = rl.Amount,
                    TotalPrice = rl.Amount * products[rl.ProductID].UnitPrice
                }).ToList()
            };
            newReceipt.TotalPrice = newReceipt.ReceiptLines.Sum(rl => rl.TotalPrice);

            await DataContext.Receipts.AddAsync(newReceipt);
            await DataContext.SaveChangesAsync();

            return StatusCode((int)HttpStatusCode.Created, newReceipt); //From Solution
        }
    }
}
