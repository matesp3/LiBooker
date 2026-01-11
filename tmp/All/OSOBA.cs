using System;
using System.Collections.Generic;

namespace LiBookerWebApi.Models.Scaffolded.All;

public partial class OSOBA
{
    public decimal ID_OSOBY { get; set; }

    public string MENO { get; set; } = null!;

    public string? PRIEZVISKO { get; set; }

    public DateTime DAT_NARODENIA { get; set; }

    public DateTime DAT_REGISTRACIE { get; set; }

    public string EMAIL { get; set; } = null!;

    public string POHLAVIE { get; set; } = null!;

    public string? TEL_CISLO { get; set; }

    public virtual ICollection<REZERVACIum> REZERVACIa { get; set; } = new List<REZERVACIum>();

    public virtual ICollection<VYPOZICANIE> VYPOZICANIEs { get; set; } = new List<VYPOZICANIE>();
}
