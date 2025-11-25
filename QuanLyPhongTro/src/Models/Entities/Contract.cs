using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongTro.Models;

[Table("Contract")]
public partial class Contract
{
    [Key]
    public Guid Id { get; set; }

    public Guid? IdRoom { get; set; }

    public Guid? IdRenter { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Deposit { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    // Gia hạn: số tháng người thuê đề xuất (chờ duyệt); nếu duyệt sẽ cộng vào EndDate
    public int? RequestedExtensionMonths { get; set; }
    // Trạng thái yêu cầu gia hạn: Pending/Approved/Rejected/null (null nếu chưa có yêu cầu)
    [StringLength(15)]
    public string? ExtensionStatus { get; set; }

    [ForeignKey("IdRenter")]
    [InverseProperty("Contracts")]
    public virtual Person? IdRenterNavigation { get; set; }

    [ForeignKey("IdRoom")]
    [InverseProperty("Contracts")]
    public virtual Room? IdRoomNavigation { get; set; }
}