using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyPhongTro.Models;

namespace QuanLyPhongTro.Models
{
    [Table("FavoriteRoom")]
    public class FavoriteRoom
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? PersonId { get; set; }
        public Guid RoomId { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [ForeignKey("PersonId")]
        public virtual Person? Person { get; set; }
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;
    }
}
