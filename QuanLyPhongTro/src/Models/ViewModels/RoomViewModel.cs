using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.src.Models.ViewModels
{
    public class RoomViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Price { get; set; }
        public double Area { get; set; }
        public string Status { get; set; } // "Còn trống", "Đã thuê"
        public string Description { get; set; }

        public string MainImage { get; set; }

        public List<string> AllImages { get; set; }

        public string PriceDisplay
        {
            get
            {
                if (Price >= 1000000)
                    return $"{(Price / 1000000m):0.##} Tr/th";
                return $"{Price:N0} VND/th";
            }
        }

        public string FullAddress { get; set; }
        public string OwnerName { get; set; }
    }
}