using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongTro.Models;

[Table("Report")]
public partial class Report
{
    [Key]
    public Guid Id { get; set; }

    public Guid? IdReporter { get; set; }

    public Guid? IdRoom { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateCreated { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [ForeignKey("IdReporter")]
    [InverseProperty("Reports")]
    public virtual Person? IdReporterNavigation { get; set; }

    [ForeignKey("IdRoom")]
    [InverseProperty("Reports")]
    public virtual Room? IdRoomNavigation { get; set; }

    // Quan hệ 1-1 với Notice
    [InverseProperty("IdreportNavigation")]
    public virtual Notice? Notice { get; set; }
}