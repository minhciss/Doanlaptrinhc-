using System.Collections.Generic;
using Minh2.Models;

namespace Minh2.Services
{
    public class POIRepository
    {
        // In-memory seeded POIs for Vĩnh Khánh food street
        public List<POI> GetAll()
        {
            return new List<POI>
            {
                new POI
                {
                    Name = "Bún riêu Bà Hằng",
                    Latitude = 10.773500,
                    Longitude = 106.688200,
                    RadiusMeters = 60,
                    Priority = 10,
                    Description = "Quán bún riêu truyền thống nổi tiếng...",
                    AudioScript = "Đây là Bún riêu Bà Hằng, món ăn đặc trưng..."
                },
                new POI
                {
                    Name = "Ốc cô Lan",
                    Latitude = 10.774100,
                    Longitude = 106.688900,
                    RadiusMeters = 50,
                    Priority = 8,
                    Description = "Ốc tươi và nước chấm đặc biệt...",
                    AudioScript = "Bạn đang đến gần Ốc cô Lan..."
                },
                new POI
                {
                    Name = "Kem Tràng Tiền (chi nhánh)",
                    Latitude = 10.772800,
                    Longitude = 106.689500,
                    RadiusMeters = 40,
                    Priority = 5,
                    Description = "Kem mát lạnh...",
                    AudioScript = "Một quán kem nổi tiếng..."
                }
            };
        }
    }
}