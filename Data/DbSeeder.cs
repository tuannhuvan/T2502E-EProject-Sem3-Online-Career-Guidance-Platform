using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Career_Guidance_Platform.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            // 1. Seed Roles
            string[] roles = { "Admin", "Student" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@careerpath.vn";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    Role = "Admin",
                    EmailConfirmed = true,
                    Status = 1
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Student User
            var studentEmail = "student@careerpath.vn";
            var studentUser = await userManager.FindByEmailAsync(studentEmail);
            if (studentUser == null)
            {
                studentUser = new User
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FullName = "Nguyễn Văn Học Sinh",
                    Role = "Student",
                    EmailConfirmed = true,
                    Status = 1
                };
                var result = await userManager.CreateAsync(studentUser, "Student@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
            }

            // 4. Seed QuestionType
            var singleChoiceType = await context.Set<QuestionType>().FirstOrDefaultAsync(qt => qt.Name == "Single Choice");
            if (singleChoiceType == null)
            {
                singleChoiceType = new QuestionType { Name = "Single Choice", Description = "Chọn một đáp án" };
                context.Set<QuestionType>().Add(singleChoiceType);
                await context.SaveChangesAsync();
            }

            // 5. Seed Categories & CareerPaths
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Kỹ thuật & Công nghệ", Description = "Nhóm kỹ thuật, phát triển phần mềm và mạng máy tính" },
                    new Category { Name = "Khoa học & Nghiên cứu", Description = "Nhóm nghiên cứu, phân tích dữ liệu và khoa học" },
                    new Category { Name = "Thiết kế & Nghệ thuật", Description = "Nhóm sáng tạo nghệ thuật, thiết kế đồ họa và UI/UX" },
                    new Category { Name = "Giáo dục & Xã hội", Description = "Nhóm chăm sóc khách hàng, nhân sự và giáo dục" },
                    new Category { Name = "Kinh doanh & Quản lý", Description = "Nhóm quản trị kinh doanh, marketing và quản lý" },
                    new Category { Name = "Tài chính & Hành chính", Description = "Nhóm tài chính, kế toán và hành chính văn phòng" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                // Seed CareerPaths for each Category
                var tech = categories[0];
                var science = categories[1];
                var art = categories[2];
                var social = categories[3];
                var biz = categories[4];
                var finance = categories[5];

                var careerPaths = new List<CareerPath>
                {
                    new CareerPath { CategoryId = tech.Id, Title = "Kỹ sư phần mềm (Software Engineer)", Content = "Lập trình, thiết kế, phát triển và bảo trì phần mềm." },
                    new CareerPath { CategoryId = tech.Id, Title = "Chuyên gia mạng & bảo mật (Network & Security)", Content = "Thiết lập hạ tầng mạng và bảo đảm an toàn thông tin hệ thống." },
                    new CareerPath { CategoryId = science.Id, Title = "Nhà khoa học dữ liệu (Data Scientist)", Content = "Phân tích, trực quan hóa và xây dựng mô hình dự báo từ dữ liệu lớn." },
                    new CareerPath { CategoryId = art.Id, Title = "Thiết kế đồ họa & UI/UX (UI/UX Designer)", Content = "Thiết kế trải nghiệm người dùng và giao diện cho các sản phẩm số." },
                    new CareerPath { CategoryId = social.Id, Title = "Chuyên viên nhân sự (HR Specialist)", Content = "Tuyển dụng, đào tạo, quản lý nhân sự và gắn kết nhân viên." },
                    new CareerPath { CategoryId = biz.Id, Title = "Quản lý kinh doanh (Business Manager)", Content = "Điều hành, lập kế hoạch kinh doanh và thúc đẩy tăng trưởng doanh nghiệp." },
                    new CareerPath { CategoryId = finance.Id, Title = "Kế toán / Kiểm toán viên (Accountant / Auditor)", Content = "Kiểm tra sổ sách, lập báo cáo tài chính và tuân thủ thuế." }
                };
                context.CareerPaths.AddRange(careerPaths);
                await context.SaveChangesAsync();

                // Seed JobPostings for businesses connection
                var jobs = new List<JobPosting>
                {
                    new JobPosting { CareerPathId = careerPaths[0].Id, Title = "Software Engineer Intern", CompanyName = "VNG Corporation", Salary = 8000000, Description = "Tham gia phát triển các ứng dụng Web/Mobile trên nền tảng .NET/ReactJS." },
                    new JobPosting { CareerPathId = careerPaths[0].Id, Title = "Junior .NET Developer", CompanyName = "FPT Software", Salary = 15000000, Description = "Phát triển dự án phần mềm cho thị trường Nhật Bản và Châu Âu." },
                    new JobPosting { CareerPathId = careerPaths[2].Id, Title = "Data Analyst", CompanyName = "Tiki", Salary = 18000000, Description = "Phân tích số liệu kinh doanh, xây dựng báo cáo Dashboard cho Ban Giám Đốc." },
                    new JobPosting { CareerPathId = careerPaths[3].Id, Title = "UI/UX Designer", CompanyName = "Shopee Vietnam", Salary = 20000000, Description = "Thiết kế giao diện ứng dụng mua sắm Shopee mang lại trải nghiệm mượt mà." },
                    new JobPosting { CareerPathId = careerPaths[4].Id, Title = "HR Executive", CompanyName = "Grab Vietnam", Salary = 14000000, Description = "Phụ trách tuyển dụng các vị trí thuộc bộ phận Vận hành và Chăm sóc khách hàng." }
                };
                context.Set<JobPosting>().AddRange(jobs);
                await context.SaveChangesAsync();

                // 6. Seed Holland Test
                var hollandTest = new Test
                {
                    Title = "Bài đánh giá định hướng nghề nghiệp Holland (RIASEC)",
                    Description = "Hỗ trợ bạn xác định nhóm tính cách và các nghề nghiệp có độ tương thích cao nhất dựa trên thang đo Holland."
                };
                context.Tests.Add(hollandTest);
                await context.SaveChangesAsync();

                // Questions
                var questions = new List<QuestionTest>
                {
                    new QuestionTest
                    {
                        TestId = hollandTest.Id,
                        QuestionTypeId = singleChoiceType.Id,
                        Content = "Bạn thích làm việc với máy tính và thiết bị công nghệ hơn hay tương tác trực tiếp với con người?"
                    },
                    new QuestionTest
                    {
                        TestId = hollandTest.Id,
                        QuestionTypeId = singleChoiceType.Id,
                        Content = "Trong một dự án nhóm học tập hoặc làm việc, vai trò nào khiến bạn cảm thấy tự tin và hào hứng nhất?"
                    },
                    new QuestionTest
                    {
                        TestId = hollandTest.Id,
                        QuestionTypeId = singleChoiceType.Id,
                        Content = "Khi đối mặt với một vấn đề phức tạp trong cuộc sống hoặc công việc, bạn thường chọn cách tiếp cận nào?"
                    },
                    new QuestionTest
                    {
                        TestId = hollandTest.Id,
                        QuestionTypeId = singleChoiceType.Id,
                        Content = "Bạn thích dành thời gian rảnh rỗi của mình để làm việc gì nhất?"
                    }
                };
                context.QuestionTests.AddRange(questions);
                await context.SaveChangesAsync();

                // Options for Q1
                var q1Opts = new List<QuestionOption>
                {
                    new QuestionOption { QuestionId = questions[0].Id, Content = "Thích lắp ráp, lập trình hoặc sửa chữa máy tính." },
                    new QuestionOption { QuestionId = questions[0].Id, Content = "Thích nghiên cứu lý thuyết, phân tích dữ liệu phức tạp." },
                    new QuestionOption { QuestionId = questions[0].Id, Content = "Thích thiết kế đồ họa, sáng tạo giao diện sản phẩm." },
                    new QuestionOption { QuestionId = questions[0].Id, Content = "Thích hướng dẫn, giảng dạy công nghệ cho người khác." }
                };
                context.QuestionOptions.AddRange(q1Opts);
                await context.SaveChangesAsync();

                // Link Q1 options to career paths
                context.OptionCareerPaths.AddRange(
                    new OptionCareerPath { OptionId = q1Opts[0].Id, CareerPathId = careerPaths[0].Id, Weight = 5 }, // Software Engineer
                    new OptionCareerPath { OptionId = q1Opts[0].Id, CareerPathId = careerPaths[1].Id, Weight = 5 }, // Net & Sec
                    new OptionCareerPath { OptionId = q1Opts[1].Id, CareerPathId = careerPaths[2].Id, Weight = 5 }, // Data Scientist
                    new OptionCareerPath { OptionId = q1Opts[2].Id, CareerPathId = careerPaths[3].Id, Weight = 5 }, // UI/UX Designer
                    new OptionCareerPath { OptionId = q1Opts[3].Id, CareerPathId = careerPaths[4].Id, Weight = 3 }  // HR Specialist
                );

                // Options for Q2
                var q2Opts = new List<QuestionOption>
                {
                    new QuestionOption { QuestionId = questions[1].Id, Content = "Sáng tạo ý tưởng hình ảnh, thiết kế slide thuyết trình." },
                    new QuestionOption { QuestionId = questions[1].Id, Content = "Lập kế hoạch hành động, phân công và dẫn dắt cả nhóm." },
                    new QuestionOption { QuestionId = questions[1].Id, Content = "Kiểm tra dữ liệu, số liệu, lập tiến độ chi tiết tỉ mỉ." },
                    new QuestionOption { QuestionId = questions[1].Id, Content = "Hỗ trợ các thành viên, giữ tinh thần đoàn kết giải quyết mâu thuẫn." }
                };
                context.QuestionOptions.AddRange(q2Opts);
                await context.SaveChangesAsync();

                // Link Q2 options
                context.OptionCareerPaths.AddRange(
                    new OptionCareerPath { OptionId = q2Opts[0].Id, CareerPathId = careerPaths[3].Id, Weight = 5 }, // UI/UX Designer
                    new OptionCareerPath { OptionId = q2Opts[1].Id, CareerPathId = careerPaths[5].Id, Weight = 5 }, // Business Manager
                    new OptionCareerPath { OptionId = q2Opts[2].Id, CareerPathId = careerPaths[6].Id, Weight = 5 }, // Accountant
                    new OptionCareerPath { OptionId = q2Opts[3].Id, CareerPathId = careerPaths[4].Id, Weight = 5 }  // HR Specialist
                );

                // Options for Q3
                var q3Opts = new List<QuestionOption>
                {
                    new QuestionOption { QuestionId = questions[2].Id, Content = "Đọc tài liệu, nghiên cứu kỹ lý thuyết khoa học." },
                    new QuestionOption { QuestionId = questions[2].Id, Content = "Bắt tay làm thử trực tiếp để rút kinh nghiệm." },
                    new QuestionOption { QuestionId = questions[2].Id, Content = "Nghĩ ra giải pháp khác biệt, phá cách mang tính nghệ thuật." },
                    new QuestionOption { QuestionId = questions[2].Id, Content = "Tổ chức họp bàn, lắng nghe ý kiến của mọi người." }
                };
                context.QuestionOptions.AddRange(q3Opts);
                await context.SaveChangesAsync();

                // Link Q3 options
                context.OptionCareerPaths.AddRange(
                    new OptionCareerPath { OptionId = q3Opts[0].Id, CareerPathId = careerPaths[2].Id, Weight = 5 }, // Data Scientist
                    new OptionCareerPath { OptionId = q3Opts[1].Id, CareerPathId = careerPaths[0].Id, Weight = 5 }, // Software Engineer
                    new OptionCareerPath { OptionId = q3Opts[2].Id, CareerPathId = careerPaths[3].Id, Weight = 4 }, // UI/UX Designer
                    new OptionCareerPath { OptionId = q3Opts[3].Id, CareerPathId = careerPaths[5].Id, Weight = 4 }  // Business Manager
                );

                // Options for Q4
                var q4Opts = new List<QuestionOption>
                {
                    new QuestionOption { QuestionId = questions[3].Id, Content = "Quản lý tài chính cá nhân, lập kế hoạch chi tiêu hoặc chơi cờ." },
                    new QuestionOption { QuestionId = questions[3].Id, Content = "Lên ý tưởng kinh doanh, bán hàng online thử nghiệm." },
                    new QuestionOption { QuestionId = questions[3].Id, Content = "Hoạt động thiện nguyện, tư vấn hỗ trợ cộng đồng." },
                    new QuestionOption { QuestionId = questions[3].Id, Content = "Tìm tòi thiết bị công nghệ mới hoặc nghiên cứu vũ trụ." }
                };
                context.QuestionOptions.AddRange(q4Opts);
                await context.SaveChangesAsync();

                // Link Q4 options
                context.OptionCareerPaths.AddRange(
                    new OptionCareerPath { OptionId = q4Opts[0].Id, CareerPathId = careerPaths[6].Id, Weight = 5 }, // Accountant
                    new OptionCareerPath { OptionId = q4Opts[1].Id, CareerPathId = careerPaths[5].Id, Weight = 5 }, // Business Manager
                    new OptionCareerPath { OptionId = q4Opts[2].Id, CareerPathId = careerPaths[4].Id, Weight = 5 }, // HR Specialist
                    new OptionCareerPath { OptionId = q4Opts[3].Id, CareerPathId = careerPaths[2].Id, Weight = 4 }  // Data Scientist
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
