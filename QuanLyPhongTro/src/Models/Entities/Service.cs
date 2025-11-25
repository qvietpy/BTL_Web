using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongTro.Models;

[Table("Service")]
public partial class Service
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string? Unit { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PricePerUnit { get; set; }

    [InverseProperty("IdServiceNavigation")]
    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
}