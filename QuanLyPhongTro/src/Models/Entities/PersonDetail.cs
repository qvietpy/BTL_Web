using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongTro.Models;

[Table("PersonDetail")]
public partial class PersonDetail
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }

    [Column("CCCD")]
    [StringLength(20)]
    public string? Cccd { get; set; }

    public byte[]? CccdImage { get; set; }

    [StringLength(15)]
    public string? Phone { get; set; }

    public byte[]? Avatar { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(100)]
    public string? Gmail { get; set; }

    [InverseProperty("IdDetailNavigation")]
    public virtual ICollection<Person> People { get; set; } = new List<Person>();
}