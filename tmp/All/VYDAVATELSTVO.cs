using System;
using System.Collections.Generic;

namespace LiBookerWebApi.Models.Scaffolded.All;

public partial class VYDAVATELSTVO
{
    public decimal ID_VYDAVATELSTVA { get; set; }

    public string NAZOV { get; set; } = null!;

    public string? POPIS { get; set; }
}
