using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongTro.src.Models.ViewModels
{
    // ViewModel hiển thị danh sách phòng
    public class RoomListViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Area { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxOccupants { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
    }

    // ViewModel chi tiết phòng
    public class RoomDetailViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Area { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxOccupants { get; set; }

        // Thông tin chủ nhà
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;

        // Hình ảnh
        public List<string> ImageUrls { get; set; } = new List<string>();

        // Dịch vụ
        public List<ServiceInfo> Services { get; set; } = new List<ServiceInfo>();
    }

    public class ServiceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal PricePerUnit { get; set; }
    }

    // ViewModel gửi yêu cầu thuê phòng
    public class RentalRequestViewModel
    {
        public Guid RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [Display(Name = "Ngày bắt đầu thuê")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [Display(Name = "Ngày kết thúc thuê")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(6);

        [Required(ErrorMessage = "Vui lòng nhập số tiền đặt cọc")]
        [Display(Name = "Số tiền đặt cọc")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public decimal Deposit { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? Note { get; set; }
    }

    // ViewModel tìm kiếm/lọc phòng
    public class RoomSearchViewModel
    {
        [Display(Name = "Từ khóa")]
        public string? Keyword { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Giá từ")]
        public decimal? MinPrice { get; set; }

        [Display(Name = "Giá đến")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Diện tích từ")]
        public decimal? MinArea { get; set; }

        [Display(Name = "Diện tích đến")]
        public decimal? MaxArea { get; set; }

        [Display(Name = "Số người tối đa")]
        public int? MaxOccupants { get; set; }

        // Kết quả tìm kiếm
        public List<RoomListViewModel> Results { get; set; } = new List<RoomListViewModel>();

        // Phân trang
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalRooms { get; set; } = 0;
    }

    // ViewModel hợp đồng của renter
    public class RenterContractViewModel
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string RoomAddress { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Deposit { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
    }

    // ViewModel báo cáo sự cố
    public class ReportViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng mô tả vấn đề")]
        [Display(Name = "Mô tả chi tiết")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Phòng liên quan")]
        public Guid? RoomId { get; set; }
    }
}