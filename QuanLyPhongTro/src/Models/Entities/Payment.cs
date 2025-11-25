using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongTro.Models;

[Table("Payment")]
public partial class Payment
{
    [Key]
    public Guid IdBill { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Amount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaymentDate { get; set; }

    [StringLength(255)]
    public string? Note { get; set; }

    [ForeignKey("IdBill")]
    [InverseProperty("Payment")]
    public virtual Bill IdPaymentNavigation { get; set; } = null!;
}