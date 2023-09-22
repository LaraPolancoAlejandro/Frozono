using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APITDS;
using APITDS.Models;
using CsvHelper.Configuration;

namespace APITDS.DTO
{
    public class InventoryDto
    {
        public string Store { get; set; }
        public DateOnly Date { get; set; }
        public string Flavor { get; set; }
        public bool IsSeasonFlavor { get; set; }
        public int Quantity { get; set; }
        public string ListedBy { get; set; }
    }
    public sealed class InventoryMap : ClassMap<InventoryDto>
    {
        public InventoryMap()
        {
            Map(m => m.Store).Name("Store".Trim());
            Map(m => m.Date).Name("Date".Trim());
            Map(m => m.Flavor).Name("Flavor".Trim());
            Map(m => m.IsSeasonFlavor).Name("Is Season Flavor".Trim());
            Map(m => m.Quantity).Name("Quantity".Trim());
            Map(m => m.ListedBy).Name("Listed By".Trim());
        }
    }


}
