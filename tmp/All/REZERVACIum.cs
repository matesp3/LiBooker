using System;
using System.Collections.Generic;

namespace LiBookerWebApi.Models.Scaffolded.All;

public partial class REZERVACIum
{
    public decimal ID_REZERVACIE { get; set; }

    public decimal VYDANIE_ID_VYDANIA { get; set; }

    public decimal OSOBA_ID_OSOBY { get; set; }

    public DateTime DAT_REZERVACIE { get; set; }

    public DateTime? DAT_DO { get; set; }

    public string? STAV { get; set; }

    public virtual OSOBA OSOBA_ID_OSOBYNavigation { get; set; } = null!;
}
