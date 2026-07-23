
using Career_Guidance_Platform.Models;

namespace Career_Guidance_Platform.Models.SeedData;

public static class CareerPathSeed
{
    public static readonly DateTime SeedDate =
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static List<CareerPath> Data => new()
    {
        new CareerPath
        {
            Id = 1,
            CategoryId = 1,
            Title = "Web Developer",
            Content = "Phát triển website và ứng dụng web bằng HTML, CSS, JavaScript, C# và ASP.NET Core.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 2,
            CategoryId = 1,
            Title = "Mobile Developer",
            Content = "Phát triển ứng dụng di động trên Android, iOS hoặc nền tảng đa nền tảng.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 3,
            CategoryId = 1,
            Title = "Data Analyst",
            Content = "Phân tích dữ liệu, xây dựng báo cáo và hỗ trợ ra quyết định kinh doanh.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 4,
            CategoryId = 2,
            Title = "Business Analyst",
            Content = "Phân tích yêu cầu nghiệp vụ, mô tả quy trình và kết nối khách hàng với đội phát triển.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 5,
            CategoryId = 2,
            Title = "Project Coordinator",
            Content = "Điều phối dự án, theo dõi tiến độ và hỗ trợ giao tiếp giữa các bên liên quan.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 6,
            CategoryId = 2,
            Title = "Sales Executive",
            Content = "Tìm kiếm khách hàng, tư vấn sản phẩm và phát triển doanh số.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 7,
            CategoryId = 3,
            Title = "Digital Marketer",
            Content = "Lập kế hoạch và triển khai chiến dịch marketing trên các nền tảng số.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 8,
            CategoryId = 3,
            Title = "Content Creator",
            Content = "Sáng tạo nội dung cho mạng xã hội, website, blog và chiến dịch truyền thông.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 9,
            CategoryId = 3,
            Title = "SEO Specialist",
            Content = "Tối ưu hóa website để cải thiện thứ hạng tìm kiếm trên Google.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 10,
            CategoryId = 4,
            Title = "UI/UX Designer",
            Content = "Thiết kế giao diện và trải nghiệm người dùng cho website, ứng dụng và sản phẩm số.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 11,
            CategoryId = 4,
            Title = "Graphic Designer",
            Content = "Thiết kế hình ảnh, banner, poster, nhận diện thương hiệu và tài liệu truyền thông.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 12,
            CategoryId = 4,
            Title = "Motion Designer",
            Content = "Thiết kế chuyển động, video animation và hiệu ứng đồ họa.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 13,
            CategoryId = 5,
            Title = "Financial Analyst",
            Content = "Phân tích tài chính, đánh giá hiệu quả kinh doanh và lập báo cáo tài chính.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 14,
            CategoryId = 5,
            Title = "Investment Consultant",
            Content = "Tư vấn đầu tư, phân tích rủi ro và hỗ trợ khách hàng xây dựng danh mục đầu tư.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        },
        new CareerPath
        {
            Id = 15,
            CategoryId = 5,
            Title = "Accountant",
            Content = "Quản lý sổ sách, chứng từ, báo cáo thuế và báo cáo tài chính doanh nghiệp.",
            Status = 1,
            CreatedAt = SeedDate,
            CreatedBy = "System_Seed"
        }
    };
}