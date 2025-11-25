using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongTro.Models;

[Table("Person")]
public partial class Person
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string Password { get; set; } = null!;

    [StringLength(20)]
    public string? Role { get; set; }

    public Guid? IdDetail { get; set; }

    [InverseProperty("IdPersonNavigation")]
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    [InverseProperty("IdRenterNavigation")]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    [ForeignKey("IdDetail")]
    [InverseProperty("People")]
    public virtual PersonDetail? IdDetailNavigation { get; set; }

    [InverseProperty("IdReporterNavigation")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [InverseProperty("IdOwnerNavigation")]
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}