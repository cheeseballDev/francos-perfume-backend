using InventorySystemBackend.Data;
using InventorySystemBackend.DTOs.POSDTOs;
using InventorySystemBackend.Models.Entities;
using InventorySystemBackend.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class POSController : ControllerBase
    {
        private readonly POSService posService;

        public POSController(POSService posService)
        {
            this.posService = posService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(POSDTO posdto)
        {
            var claims = new ClaimsGetter(User);

            var result = await posService.CheckoutAsync(posdto, claims);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}
