using System;
using System.Collections.Generic;

namespace WebDongHoLG.Models;

public partial class Faq
{
    public int IdFaq { get; set; }

    public string? CauHoi { get; set; }

    public string? CauTraLoi { get; set; }

    public string? TrangThai { get; set; }
}
