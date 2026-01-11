using System;
using System.Collections.Generic;

namespace LiBookerWebApi.Models.Scaffolded.All;

public partial class KNIHA
{
    public decimal ID_KNIHY { get; set; }

    public string NAZOV { get; set; } = null!;

    public string? POPIS { get; set; }

    public virtual ICollection<AUTOR> ID_AUTORAs { get; set; } = new List<AUTOR>();

    public virtual ICollection<KATEGORIum> ID_KATEGORIEs { get; set; } = new List<KATEGORIum>();

    public virtual ICollection<ZANER> ID_ZANRAs { get; set; } = new List<ZANER>();
}
