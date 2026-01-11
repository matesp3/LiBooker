using System;
using System.Collections.Generic;

namespace LiBookerWebApi.Models.Scaffolded.All;

public partial class VYTLACOK
{
    public decimal ID_VYTLACKA { get; set; }

    public decimal ID_VYDANIA { get; set; }

    public string? STAV { get; set; }

    public virtual ICollection<VYPOZICANIE> VYPOZICANIEs { get; set; } = new List<VYPOZICANIE>();
}
