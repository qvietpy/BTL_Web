using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongTro.Models;

[Table("Room")]
public partial class Room
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    public Guid? IdOwner { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Area { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Price { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [InverseProperty("IdRoomNavigation")]
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    [InverseProperty("IdRoomNavigation")]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    [ForeignKey("IdOwner")]
    [InverseProperty("Rooms")]
    public virtual Person? IdOwnerNavigation { get; set; }

    [InverseProperty("IdRoomNavigation")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [InverseProperty("IdRoomNavigation")]
    public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();
}