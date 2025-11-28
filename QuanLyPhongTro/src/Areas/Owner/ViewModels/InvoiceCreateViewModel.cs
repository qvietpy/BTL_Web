using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongTro.Areas.Owner.ViewModels
{
    public class InvoiceCreateViewModel1
    {
        [Display(Name = "Mã hóa đơn")]
        public string InvoiceCode { get; set; } = string.Empty;

        [Display(Name = "Hợp đồng")]
        [Required(ErrorMessage = "Vui lòng chọn hợp đồng")]
        public Guid? ContractId { get; set; }

        [Display(Name = "Phòng")]
        public Guid? RoomId { get; set; }

        [Display(Name = "Khách thuê")]
        public Guid? RenterId { get; set; }

        [Required]
        [Range(1, 12)]
        [Display(Name = "Tháng")]
        public int Month { get; set; }

        [Required]
        [Range(2000, 2100)]
        [Display(Name = "Năm")]
        public int Year { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hạn thanh toán")]
        public DateTime? DueDate { get; set; }

        // Tiền phòng
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền phòng phải là số dương")]
        [Display(Name = "Tiền phòng")]
        public decimal Rent { get; set; }

        // Điện
        [Range(0, double.MaxValue)]
        [Display(Name = "Số điện đầu kì")]
        public int ElecStart { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Số điện cuối kì")]
        public int ElecEnd { get; set; }

        [Display(Name = "Số điện sử dụng")]
        public int ElecUsed => Math.Max(0, ElecEnd - ElecStart);

        [Range(0, double.MaxValue)]
        [Display(Name = "Tiền điện")]
        public decimal Electricity { get; set; }

        // Nước
        [Range(0, double.MaxValue)]
        [Display(Name = "Số nước đầu kì")]
        public int WaterStart { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Số nước cuối kì")]
        public int WaterEnd { get; set; }

        [Display(Name = "Số nước sử dụng")]
        public int WaterUsed => Math.Max(0, WaterEnd - WaterStart);

        [Range(0, double.MaxValue)]
        [Display(Name = "Tiền nước")]
        public decimal Water { get; set; }

        // Dịch vụ
        [Range(0, double.MaxValue)]
        [Display(Name = "Phí gửi xe")]
        public decimal ParkingFee { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Phí rác")]
        public decimal GarbageFee { get; set; }

        [Display(Name = "Tổng phí dịch vụ")]
        public decimal Services
        {
            get => ParkingFee + GarbageFee;
            set { /* EF cần setter */ }
        }

        // Tổng tiền
        [Display(Name = "Tổng tiền")]
        public decimal Total => Rent + Electricity + Water + Services;

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string Note { get; set; } = string.Empty;
    }
}
