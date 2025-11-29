using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Models;
using System.Text.RegularExpressions;

namespace QuanLyPhongTro.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // Apply pending migrations
            await db.Database.MigrateAsync();

            // Seed Rooms if empty
            if (!await db.Rooms.AnyAsync())
            {
                var rooms = new List<Room>
                {
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = "Phòng 101 - Quận 1",
                        Address = "123 Lý Tự Trọng, Q1, TP.HCM",
                        Area = 18,
                        Price = 3500000,
                        Status = "Trống",
                        Description = "Gần trung tâm, có ban công, giờ giấc tự do"
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = "Phòng 202 - Quận 7",
                        Address = "45 Nguyễn Trãi, Q7, TP.HCM",
                        Area = 22,
                        Price = 4200000,
                        Status = "Tr?ng",
                        Description = "Full nôi thất"
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = "Phòng 303 - Th? Ð?c",
                        Address = "12 Võ Van Ngân, TP. Thủ Đức",
                        Area = 16,
                        Price = 2800000,
                        Status = "Tróng",
                        Description = "Gần ÐH, an ninh tốt"
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Name = "Phòng 404 - Bình Thạnh",
                        Address = "89 Ðiện Biên Phủ",
                        Area = 20,
                        Price = 3900000,
                        Status = "Trống",
                        Description = "Có chỗ để xe, thang máy"
                    }
                };

                await db.Rooms.AddRangeAsync(rooms);

                // Seed images for rooms with correct naming format: Room{Number}_P{Number}.png
                var images = rooms.Select(r => new RoomImage
                {
                    Id = Guid.NewGuid(),
                    IdRoom = r.Id,
                    ImageUrl = $"/images/rooms/{GenerateImageFileName(r.Name)}" // e.g., /images/rooms/Room101_P101.png
                });
                await db.RoomImages.AddRangeAsync(images);

                await db.SaveChangesAsync();
            }
        }

        private static string GenerateImageFileName(string roomName)
        {
            // Extract first numeric sequence (room number) from the room name
            var match = Regex.Match(roomName, "(\\d{2,4})");
            var number = match.Success ? match.Value : "000";
            return $"Room{number}_P{number}.png";
        }
    }
}
