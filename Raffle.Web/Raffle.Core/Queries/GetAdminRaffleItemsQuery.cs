using Dapper;
using MediatR;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Queries
{
    public class GetAdminRaffleItemsQuery : IRequest<GetRaffleItemsRequest>
    {

    }

    public class GetRaffleItemsRequest
    {
        public IReadOnlyList<AdminListRaffleItem> RaffleItems { get; set; } = new List<AdminListRaffleItem>();
    }

    public class AdminListRaffleItem
    {
        public int Id { get; set; }
        public int TotalTicketsEntered { get; set; }
        public int ItemNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public string ItemValue { get; set; }
        public string Sponsor { get; set; }
        public int Cost { get; set; }
        public bool IsAvailable { get; set; }
        public bool ForOver21 { get; set; }
        public bool LocalPickupOnly { get; set; }
        public int NumberOfDraws { get; set; }
        public string WinningTickets { get; set; }
    }

    public class GetAdminRaffleItemsQueryHandler : IRequestHandler<GetAdminRaffleItemsQuery, GetRaffleItemsRequest>
    {
        readonly string connectionString;

        public GetAdminRaffleItemsQueryHandler(RaffleDbConfiguration config)
        {
            connectionString = config.ConnectionString;
        }

        public async Task<GetRaffleItemsRequest> Handle(GetAdminRaffleItemsQuery request, CancellationToken cancellationToken)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                const string query = "SELECT " +
                    "ri.*, " +
                    "TotalTicketsEntered = (SELECT SUM(roii.Count) " +
                    "   FROM RaffleOrders ro JOIN RaffleOrderLineItems roii " +
                    "       ON ro.Id = roii.RaffleOrderId " +
                    "   WHERE ro.TicketNumber <> '' AND roii.RaffleItemId = ri.Id) " +
                    "FROM RaffleItems ri";

                var raffleItems = (await conn.QueryAsync<AdminListRaffleItem>(query))
                .Distinct()
                .ToList();

                return new GetRaffleItemsRequest { RaffleItems = raffleItems };
            }
        }
    }
}
