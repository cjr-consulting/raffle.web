using CsvHelper;

using Dapper;

using Microsoft.Extensions.Configuration;

using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LoadRaffleFromCsv
{
    public class Program
    {
        static string ConnectionString;

        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            ConnectionString = config["ConnectionStrings:DefaultConnection"];

            using (var reader = new StreamReader(@"c:\projects\dfd_raffle_2020\DFD-Q-Prizes.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<RaffleItemRecord>().ToList();
                var raffleItems = records.Select(Map);
                
                foreach(var item in raffleItems)
                {
                    Save(item);
                }
            }
        }
        static RaffleItem Map(RaffleItemRecord record)
        {
            return new RaffleItem
            {
                ItemNumber = int.Parse(record.Number.Trim()),
                Title = record.Item.Trim(),
                Description = record.Description.Trim(),
                Category = record.Category.Trim(),
                ItemValue = record.Value.Trim(),
                Sponsor = record.Sponsor.Trim(),
                Cost = int.Parse(record.Cost.Trim()),
                ForOver21 = record.Over21.Trim() == "T",
                LocalPickupOnly = record.LocalPickup.Trim() == "T"
            };
        }

        static void Save(RaffleItem item)
        {
            const string query = "INSERT INTO [RaffleItems] " +
                "(ItemNumber, Title, Description, ImageUrl, Category, Sponsor, ItemValue, Cost, IsAvailable, ForOver21, LocalPickupOnly, NumberOfDraws) " +
                "VALUES " +
                "(@ItemNumber, @Title, @Description, '', @Category, @Sponsor, @ItemValue, @Cost, @IsAvailable, @ForOver21, @LocalPickupOnly, @NumberOfDraws)";
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(query, item);
            }
        }
    }

    public class RaffleItemRecord
    {
        /*
         * Number,Item,Description,Sponsor,Value,Cost,Category,Available,Over21,LocalPickup,Multiple,WinningTicket
         */
        public string Number { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string Sponsor { get; set; }
        public string Value { get; set; }
        public string Cost { get; set; }
        public string Category { get; set; }
        public string Available { get; set; }
        public string Over21 { get; set; }
        public string LocalPickup { get; set; }
        public string Multiple { get; set; }
        public string WinningTicket { get; set; }
    }

    public class RaffleItem
    {
        public int ItemNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public string ItemValue { get; set; }
        public string Sponsor { get; set; }
        public int Cost { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool ForOver21 { get; set; }
        public bool LocalPickupOnly { get; set; }
        public int NumberOfDraws { get; set; } = 1;
    }
}
