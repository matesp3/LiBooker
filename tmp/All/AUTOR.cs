using System;
using System.Collections.Generic;

namespace LiBookerWebApi.Models.Scaffolded.All;

public partial class AUTOR
{
    public decimal ID_AUTORA { get; set; }

    public string MENO { get; set; } = null!;

    public string PRIEZVISKO { get; set; } = null!;

    public string? NARODNOST { get; set; }

    public virtual ICollection<KNIHA> ID_KNIHies { get; set; } = new List<KNIHA>();
}
