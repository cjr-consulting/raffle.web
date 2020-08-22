using Dapper;

using Raffle.Core.Models;

using Raffle.Core.Repositories;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Raffle.Core.Data
{
    public class RaffleItemRepository : IRaffleItemRepository
    {
        readonly string connectionString;

        public RaffleItemRepository(RaffleDbConfiguration config)
        {
            connectionString = config.ConnectionString;
        }

        public IReadOnlyList<RaffleItem> GetAll()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                const string query = "SELECT " +
                    "ri.*, " +
                    "TotalTicketsEntered = (SELECT SUM(roii.Count) " +
                    "   FROM RaffleOrders ro JOIN RaffleOrderLineItems roii " +
                    "       ON ro.Id = roii.RaffleOrderId " +
                    "   WHERE ro.TicketNumber <> '' AND roii.RaffleItemId = ri.Id), " +
                    " rii.ImageRoute " +
                    "FROM RaffleItems ri " +
                    "LEFT JOIN RaffleItemImages rii ON ri.Id = rii.RaffleItemId";

                var raffleItemDictionary = new Dictionary<int, RaffleItem>();
                var list = conn.Query<RaffleItem, string, RaffleItem>(
                    query,
                    (raffleItem, imageRoute) =>
                    {
                        RaffleItem raffleItemEntry;

                        if (!raffleItemDictionary.TryGetValue(raffleItem.Id, out raffleItemEntry))
                        {
                            raffleItemEntry = raffleItem;
                            raffleItemEntry.ImageUrls = new List<string>();
                            raffleItemDictionary.Add(raffleItemEntry.Id, raffleItemEntry);
                        }

                        if (imageRoute != null)
                        {
                            raffleItemEntry.ImageUrls.Add(imageRoute);
                        }

                        return raffleItemEntry;
                    },
                    splitOn: "ImageRoute")
                .Distinct()
                .ToList();

                return list;
            }
        }

        public RaffleItem GetById(int id)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                const string query = "SELECT " +
                    "ri.*, " +
                    "TotalTicketsEntered = (SELECT SUM(roii.Count) " +
                    "   FROM RaffleOrders ro JOIN RaffleOrderLineItems roii " +
                    "       ON ro.Id = roii.RaffleOrderId " +
                    "   WHERE ro.TicketNumber <> '' AND roii.RaffleItemId = ri.Id), " +
                    " rii.ImageRoute " +
                    "FROM RaffleItems ri " +
                    "LEFT JOIN RaffleItemImages rii ON ri.Id = rii.RaffleItemId " +
                    "WHERE ri.Id = @id";

                var raffleItemDictionary = new Dictionary<int, RaffleItem>();
                var result = conn.Query<RaffleItem, string, RaffleItem>(
                        query,
                        (raffleItem, imageRoute) =>
                        {
                            RaffleItem raffleItemEntry;

                            if (!raffleItemDictionary.TryGetValue(raffleItem.Id, out raffleItemEntry))
                            {
                                raffleItemEntry = raffleItem;
                                raffleItemEntry.ImageUrls = new List<string>();
                                raffleItemDictionary.Add(raffleItemEntry.Id, raffleItemEntry);
                            }

                            if (imageRoute != null)
                            {
                                raffleItemEntry.ImageUrls.Add(imageRoute);
                            }

                            return raffleItemEntry;
                        },
                        new { Id = id },
                        splitOn: "ImageRoute")
                    .Distinct()
                    .SingleOrDefault();

                return result;
            }
        }

        public IReadOnlyList<string> GetUsedCategories()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<string>("SELECT DISTINCT RTRIM(LTRIM(Category)) FROM RaffleItems;").ToList();
            }
        }
    }
}
