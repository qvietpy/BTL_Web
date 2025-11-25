using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongTro.Models;

[Table("RoomImage")]
public partial class RoomImage
{
    [Key]
    public Guid Id { get; set; }

    public Guid? IdRoom { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [ForeignKey("IdRoom")]
    [InverseProperty("RoomImages")]
    public virtual Room? IdRoomNavigation { get; set; }
}