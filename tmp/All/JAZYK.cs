using System;
using System.Collections.Generic;

namespace LiBookerWebApi.Models.Scaffolded.All;

public partial class JAZYK
{
    public decimal ID_JAZYKA { get; set; }

    public string NAZOV { get; set; } = null!;

    public string SKRATKA { get; set; } = null!;

    public string? ISO_A3 { get; set; }
}
