using System;
using System.Collections.Generic;

namespace QuanLyPhongTro.src.Models.ViewModels
{
    public class RoomViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public decimal Area { get; set; }

        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxOccupants { get; set; }

        public string MainImage { get; set; } = "/images/Room103_P01.png"; 

        public List<string> AllImages { get; set; } = new List<string>();
        public string PriceDisplay
        {
            get
            {
                if (Price >= 1000000)
                    return $"{(Price / 1000000m):0.##} Tr/th";
                return $"{Price:N0} đ/th";
            }
        }

        public string FullAddress { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
    }
}