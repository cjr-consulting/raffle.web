using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raffle.Core.Queries;

namespace Raffle.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        readonly IMediator mediator;

        public OrdersController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<GetRaffleOrder>>> Get()
        {
            var ordersResult = await mediator.Send(new GetRaffleOrdersQuery());

            return ordersResult.Orders.ToList();
        }
    } 
}
