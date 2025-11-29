using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongTro.Models
{
    [Table("Bill")]
    public partial class Bill
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? IdRoom { get; set; }

        public Guid? IdPerson { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalMoney { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Chưa thanh toán";

        [Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime? PaymentDate { get; set; }

        [InverseProperty("IdBillNavigation")]
        public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();

        [ForeignKey("IdPerson")]
        [InverseProperty("Bills")]
        public virtual Person? IdPersonNavigation { get; set; }

        [ForeignKey("IdRoom")]
        [InverseProperty("Bills")]
        public virtual Room? IdRoomNavigation { get; set; }

        [InverseProperty("IdPaymentNavigation")]
        public virtual Payment? Payment { get; set; }

    }
}