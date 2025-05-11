using Microsoft.AspNetCore.Mvc;
using Tutorial9.DTO;
using Tutorial9.Exceptions;
using Tutorial9.Repositories;

namespace Tutorial9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : Controller
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Fulfill(FulfillOrderDTO fulfillOrderDto)
    {
        try
        {
            return new ObjectResult(await _warehouseService.Fulfill(fulfillOrderDto))
            {
                StatusCode = StatusCodes.Status201Created
            };
        }
        catch (ProductNotFoundException)
        {
            return NotFound(new { message = "Product not found" });
        }
        catch (WarehouseNotFoundException)
        {
            return NotFound(new { message = "Warehouse not found" });
        }
        catch (OrderNotFoundException)
        {
            return NotFound(new { message = "Order not found" });
        }
        catch (InvalidAmountException)
        {
            return BadRequest(new { message = "Amount must be a positive integer" });
        }
        catch (OrderAlreadyCompletedException)
        {
            return Conflict(new { message = "Order is already completed" });
        }
    }

    [HttpPost("procedure")]
    public async Task<IActionResult> FulfillProcedure(FulfillOrderDTO fulfillOrderDto)
    {
        try
        {
            return new ObjectResult(await _warehouseService.FulfillProcedure(fulfillOrderDto))
            {
                StatusCode = StatusCodes.Status201Created
            };
        }
        catch (ProductNotFoundException)
        {
            return NotFound(new { message = "Product not found" });
        }
        catch (WarehouseNotFoundException)
        {
            return NotFound(new { message = "Warehouse not found" });
        }
        catch (OrderNotFoundException)
        {
            return NotFound(new { message = "Order not found" });
        }
        catch (InvalidAmountException)
        {
            return BadRequest(new { message = "Amount must be a positive integer" });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new ObjectResult(new { message = "Unknown error" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}