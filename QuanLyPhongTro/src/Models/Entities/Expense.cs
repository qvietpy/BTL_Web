using QuanLyPhongTro.Models;
//using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace QuanLyPhongTro.Models
{
    public class Expense
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid IdOwner { get; set; }
        [ForeignKey("IdOwner")]
        public Person Owner { get; set; }

        public Guid? IdRoom { get; set; }
        [ForeignKey("IdRoom")]
        public Room Room { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; } // Số tiền chi

        public DateTime DateIncurred { get; set; } // Ngày chi

        public string Description { get; set; } // Ghi chú 

    }
}