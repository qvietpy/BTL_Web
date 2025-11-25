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
    public class BookingRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid IdRenter { get; set; } 
        [ForeignKey("IdRenter")]
        public Person Renter { get; set; }

        public Guid IdRoom { get; set; } 
        [ForeignKey("IdRoom")]
        public Room Room { get; set; }

        public DateTime DesiredStartDate { get; set; }
        public int DesiredDurationMonths { get; set; } 

        public string Note { get; set; } 

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }

}