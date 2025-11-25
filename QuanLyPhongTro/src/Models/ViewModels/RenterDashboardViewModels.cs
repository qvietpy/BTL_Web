using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.src.Models.ViewModels
{
    public class RenterDashboardViewModel
    {
        public Guid ContractId { get; set; }
        public Guid RoomId { get; set; }
        public string RoomName { get; set; } = "";
        public string? RoomAddress { get; set; }
        public decimal? RoomPrice { get; set; }
        public decimal? RoomArea { get; set; }
        public string? RoomStatus { get; set; }
        public DateOnly? ContractStart { get; set; }
        public DateOnly? ContractEnd { get; set; }
        public decimal? Deposit { get; set; }
        public string ContractStatus { get; set; } = "";
        public decimal OutstandingAmount { get; set; }
        public List<BillItem> UnpaidBills { get; set; } = new();
    }

    public class BillItem
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
    }

    public class ContractDetailViewModel
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; } = "";
        public string? RoomAddress { get; set; }
        public decimal? Price { get; set; }
        public decimal? Area { get; set; }
        public DateOnly? Start { get; set; }
        public DateOnly? End { get; set; }
        public decimal? Deposit { get; set; }
        public string Status { get; set; } = "";
        public string OwnerName { get; set; } = "";
        public string OwnerPhone { get; set; } = "";
    }

    public class PersonalInfoViewModel
    {
        public string? Name { get; set; }
        public string? Cccd { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string? Gmail { get; set; }
        public string Username { get; set; } = "";
    }

    public class SendNoticeInput
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
