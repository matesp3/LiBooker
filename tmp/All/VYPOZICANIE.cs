using System;
using System.Collections.Generic;

namespace LiBookerWebApi.Models.Scaffolded.All;

public partial class VYPOZICANIE
{
    public decimal ID_VYPOZICANIA { get; set; }

    public decimal OSOBA_ID_OSOBY { get; set; }

    public decimal VYTLACOK_ID_VYTLACKA { get; set; }

    public DateTime DAT_VYPOZICANIA { get; set; }

    public DateTime DAT_KONCA_VYPOZ { get; set; }

    public DateTime? DAT_VRATENIA { get; set; }

    public virtual OSOBA OSOBA_ID_OSOBYNavigation { get; set; } = null!;

    public virtual VYTLACOK VYTLACOK_ID_VYTLACKANavigation { get; set; } = null!;
}
