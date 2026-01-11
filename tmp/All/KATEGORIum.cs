using System;
using System.Collections.Generic;

namespace LiBookerWebApi.Models.Scaffolded.All;

public partial class KATEGORIum
{
    public decimal ID_KATEGORIE { get; set; }

    public string NAZOV { get; set; } = null!;

    public virtual ICollection<KNIHA> ID_KNIHies { get; set; } = new List<KNIHA>();
}
