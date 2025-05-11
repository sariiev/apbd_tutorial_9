using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[Route("api/warehouse")]
[ApiController]
public class WarehouseController : Controller
{
    private IOrderService _orderService;

    public WarehouseController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> Fulfill(OrderDTO orderDto)
    {
    }
}