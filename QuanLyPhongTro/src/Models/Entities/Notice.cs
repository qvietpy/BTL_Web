using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongTro.Models;

[Table("Notice")]
public partial class Notice
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Nội dung không được để trống")]
    public string? Description { get; set; }

    public byte[]? Image { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public string? Status { get; set; } = "Chưa đọc";

    [Column("IDReport")]
    public Guid? Idreport { get; set; }

    [ForeignKey("Idreport")]
    [InverseProperty("Notice")]
    public virtual Report? IdreportNavigation { get; set; }
}