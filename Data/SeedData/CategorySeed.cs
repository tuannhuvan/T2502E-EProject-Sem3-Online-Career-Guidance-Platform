

using Career_Guidance_Platform.Models;

namespace Career_Guidance_Platform.Data.SeedData;

public static class CategorySeed
{
    public static readonly DateTime SeedDate =
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static List<Category> Data => new()
    {
        new Category
        {
            Id = 1,
            Name = "Công nghệ thông tin",
            Description = "Lĩnh vực phần mềm, dữ liệu, hệ thống và mạng máy tính",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new Category
        {
            Id = 2,
            Name = "Kinh tế & Kinh doanh",
            Description = "Lĩnh vực quản trị, kinh doanh, phân tích và vận hành",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new Category
        {
            Id = 3,
            Name = "Marketing & Truyền thông",
            Description = "Lĩnh vực tiếp thị số, nội dung và truyền thông thương hiệu",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new Category
        {
            Id = 4,
            Name = "Thiết kế & Sáng tạo",
            Description = "Lĩnh vực thiết kế giao diện, đồ họa và sản phẩm sáng tạo",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new Category
        {
            Id = 5,
            Name = "Tài chính & Ngân hàng",
            Description = "Lĩnh vực tài chính, kế toán, đầu tư và ngân hàng",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        }
    };
}