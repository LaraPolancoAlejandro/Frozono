using System;
using System.Collections.Generic;

namespace APITDS.Models;

public partial class Store
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
