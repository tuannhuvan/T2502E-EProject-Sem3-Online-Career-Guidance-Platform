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
            string[] roles = { "Admin", "Student", "Mentor" };
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

            // 3.5. Seed Mentor User & Profile
            var mentorEmail = "mentor@careerpath.vn";
            var mentorUser = await userManager.FindByEmailAsync(mentorEmail);
            if (mentorUser == null)
            {
                mentorUser = new User
                {
                    UserName = mentorEmail,
                    Email = mentorEmail,
                    FullName = "Trần Văn Cố Vấn",
                    Role = "Mentor",
                    EmailConfirmed = true,
                    Status = 1
                };
                var result = await userManager.CreateAsync(mentorUser, "Mentor@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(mentorUser, "Mentor");
                }
            }

            var mentorProfile = await context.MentorProfiles.FirstOrDefaultAsync(mp => mp.UserId == mentorUser.Id);
            if (mentorProfile == null)
            {
                mentorProfile = new MentorProfile
                {
                    UserId = mentorUser.Id,
                    JobTitle = "Senior .NET Developer",
                    Company = "FPT Software",
                    Specialization = "ASP.NET Core, C#, Clean Architecture",
                    Biography = "Hơn 8 năm kinh nghiệm phát triển phần mềm và định hướng nghề nghiệp cho các thế hệ học viên trẻ.",
                    AvailabilityJson = "Thứ 2, Thứ 4: 19:00 - 21:00",
                    LinkedInUrl = "https://linkedin.com/in/mentor-demo",
                    Rating = 5.0m,
                    IsActive = true,
                    IsVerified = true,
                    HourlyRate = 150000m,
                    ExperienceDescription = "Từng tham gia các dự án lớn về Tài chính và Hành chính công của FPT Software. Đào tạo thành công hơn 200 mentees.",
                    Expertise = "C#, Backend Development, Cloud Azure"
                };
                context.MentorProfiles.Add(mentorProfile);
                await context.SaveChangesAsync();
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
            List<CareerPath> careerPaths;
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

                careerPaths = new List<CareerPath>
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
            }
            else
            {
                careerPaths = await context.CareerPaths.OrderBy(c => c.Id).ToListAsync();
            }

            // --- SEEDING SKILLS (20 SKILLS) ---
            List<Skill> skills = new List<Skill>();
            if (!await context.Skills.AnyAsync())
            {
                skills = new List<Skill>
                {
                    new Skill { Name = "Lập trình C#", Description = "Ngôn ngữ C# căn bản và nâng cao", SkillType = "Hard Skill" },
                    new Skill { Name = "Cơ sở dữ liệu SQL Server", Description = "Truy vấn, thiết kế DB và viết Store Procedure", SkillType = "Hard Skill" },
                    new Skill { Name = "HTML/CSS & Bootstrap", Description = "Cắt web, dựng giao diện responsive", SkillType = "Hard Skill" },
                    new Skill { Name = "Kiến trúc ASP.NET Core MVC", Description = "Xây dựng ứng dụng web theo mô hình MVC", SkillType = "Hard Skill" },
                    new Skill { Name = "Git & Version Control", Description = "Quản lý mã nguồn, làm việc nhóm Git", SkillType = "Hard Skill" },
                    new Skill { Name = "Kỹ năng thuyết trình", Description = "Trình bày ý tưởng rõ ràng trước đám đông", SkillType = "Soft Skill" },
                    new Skill { Name = "Quản trị thời gian", Description = "Sắp xếp công việc và kiểm soát deadline", SkillType = "Soft Skill" },
                    new Skill { Name = "Giao tiếp & Làm việc nhóm", Description = "Lắng nghe, phản hồi và kết nối đồng nghiệp", SkillType = "Soft Skill" },
                    new Skill { Name = "Thiết kế UI trong Figma", Description = "Sử dụng Figma để thiết kế giao diện", SkillType = "Hard Skill" },
                    new Skill { Name = "Nghiên cứu UX", Description = "Khảo sát người dùng, vẽ User Flow", SkillType = "Hard Skill" },
                    new Skill { Name = "Cấu trúc dữ liệu & Thuật toán", Description = "Tối ưu hóa giải thuật giải quyết bài toán phức tạp", SkillType = "Hard Skill" },
                    new Skill { Name = "Lập trình Python", Description = "Ngôn ngữ Python ứng dụng cho Data Science", SkillType = "Hard Skill" },
                    new Skill { Name = "Phân tích dữ liệu với Pandas", Description = "Xử lý, trực quan hóa dữ liệu thô", SkillType = "Hard Skill" },
                    new Skill { Name = "Tuyển dụng nhân sự", Description = "Lập kế hoạch và phỏng vấn ứng viên", SkillType = "Hard Skill" },
                    new Skill { Name = "Quản trị dự án Agile/Scrum", Description = "Quản lý tiến độ theo mô hình linh hoạt", SkillType = "Soft Skill" },
                    new Skill { Name = "Kế toán tài chính", Description = "Lập báo cáo tài chính doanh nghiệp", SkillType = "Hard Skill" },
                    new Skill { Name = "Kiểm toán VAS/IFRS", Description = "Kiểm tra số liệu tài chính theo chuẩn", SkillType = "Hard Skill" },
                    new Skill { Name = "Mạng căn bản CCNA", Description = "Thiết lập định tuyến, cấu hình Switch/Router", SkillType = "Hard Skill" },
                    new Skill { Name = "Bảo mật hệ thống", Description = "Phòng ngừa tấn công mạng cơ bản", SkillType = "Hard Skill" },
                    new Skill { Name = "Kỹ năng giải quyết xung đột", Description = "Hòa giải mâu thuẫn trong tập thể", SkillType = "Soft Skill" }
                };
                context.Skills.AddRange(skills);
                await context.SaveChangesAsync();
            }
            else
            {
                skills = await context.Skills.OrderBy(s => s.Id).ToListAsync();
            }

            // --- SEEDING CAREER STAGES (CAREERMAPS) ---
            if (!await context.CareerStages.AnyAsync() || (await context.CareerStages.CountAsync()) < 10)
            {
                var oldStages = await context.CareerStages.ToListAsync();
                context.CareerStages.RemoveRange(oldStages);
                await context.SaveChangesAsync();

                // 1. Software Engineer
                var softwareEngineer = careerPaths.FirstOrDefault(cp => cp.Title.Contains("Software Engineer"));
                if (softwareEngineer != null)
                {
                    var stages = new List<CareerStage>
                    {
                        new CareerStage { CareerPathId = softwareEngineer.Id, Title = "Intern/Freshman", Description = "Tập trung tư duy nền tảng và kỹ năng viết code sạch cơ bản.", SequenceOrder = 1 },
                        new CareerStage { CareerPathId = softwareEngineer.Id, Title = "Junior Developer", Description = "Thực thi kỹ thuật độc lập, giải quyết các task chi tiết dưới sự hướng dẫn.", SequenceOrder = 2 },
                        new CareerStage { CareerPathId = softwareEngineer.Id, Title = "Mid-Senior Developer", Description = "Thiết kế giải pháp, làm việc với hệ thống lớn hơn và review code cho đồng nghiệp.", SequenceOrder = 3 },
                        new CareerStage { CareerPathId = softwareEngineer.Id, Title = "Senior/Lead Developer", Description = "Định hướng kiến trúc hệ thống phức tạp, dẫn dắt đội ngũ kỹ thuật.", SequenceOrder = 4 }
                    };
                    context.CareerStages.AddRange(stages);
                    await context.SaveChangesAsync();

                    if (skills.Count >= 10)
                    {
                        context.CareerStageSkills.AddRange(
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[0].Id, ProficiencyRequired = "Basic" }, // C#
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[1].Id, ProficiencyRequired = "Basic" }, // SQL
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[4].Id, ProficiencyRequired = "Basic" }, // Git
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[7].Id, ProficiencyRequired = "Basic" }, // Communication
                            
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[0].Id, ProficiencyRequired = "Intermediate" }, // C#
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[3].Id, ProficiencyRequired = "Basic" }, // ASP.NET
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[2].Id, ProficiencyRequired = "Intermediate" } // HTML/CSS
                        );
                        await context.SaveChangesAsync();
                    }
                }

                // 2. Network & Security
                var networkSecurity = careerPaths.FirstOrDefault(cp => cp.Title.Contains("Network & Security") || cp.Title.Contains("Mạng"));
                if (networkSecurity != null)
                {
                    var stages = new List<CareerStage>
                    {
                        new CareerStage { CareerPathId = networkSecurity.Id, Title = "Network Support Associate", Description = "Hỗ trợ vận hành mạng cơ bản, tiếp nhận sự cố và kiểm tra dây nối.", SequenceOrder = 1 },
                        new CareerStage { CareerPathId = networkSecurity.Id, Title = "Network Engineer", Description = "Cấu hình định tuyến Router/Switch, thiết lập mạng LAN/WAN.", SequenceOrder = 2 },
                        new CareerStage { CareerPathId = networkSecurity.Id, Title = "Network Architect", Description = "Thiết kế kiến trúc hệ thống mạng quy mô lớn cho doanh nghiệp.", SequenceOrder = 3 },
                        new CareerStage { CareerPathId = networkSecurity.Id, Title = "Information Security Specialist", Description = "Đảm bảo an toàn thông tin hệ thống mạng, rà soát lỗ hổng bảo mật.", SequenceOrder = 4 }
                    };
                    context.CareerStages.AddRange(stages);
                    await context.SaveChangesAsync();

                    if (skills.Count >= 20)
                    {
                        context.CareerStageSkills.AddRange(
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[17].Id, ProficiencyRequired = "Basic" }, // CCNA
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[7].Id, ProficiencyRequired = "Basic" },  // Communication
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[17].Id, ProficiencyRequired = "Intermediate" }, // CCNA
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[18].Id, ProficiencyRequired = "Basic" }, // Security
                            new CareerStageSkill { CareerStageId = stages[2].Id, SkillId = skills[17].Id, ProficiencyRequired = "Advanced" }, // CCNA
                            new CareerStageSkill { CareerStageId = stages[3].Id, SkillId = skills[18].Id, ProficiencyRequired = "Advanced" } // Security
                        );
                        await context.SaveChangesAsync();
                    }
                }

                // 3. Data Scientist
                var dataScientist = careerPaths.FirstOrDefault(cp => cp.Title.Contains("Data Scientist") || cp.Title.Contains("Dữ liệu"));
                if (dataScientist != null)
                {
                    var stages = new List<CareerStage>
                    {
                        new CareerStage { CareerPathId = dataScientist.Id, Title = "Data Analyst Intern", Description = "Thu thập, làm sạch dữ liệu và thực hiện các báo cáo thống kê cơ bản.", SequenceOrder = 1 },
                        new CareerStage { CareerPathId = dataScientist.Id, Title = "Data Analyst / Developer", Description = "Viết kịch bản xử lý dữ liệu tự động, xây dựng trực quan hóa dữ liệu.", SequenceOrder = 2 },
                        new CareerStage { CareerPathId = dataScientist.Id, Title = "Machine Learning Engineer", Description = "Huấn luyện các mô hình học máy phục vụ cho việc dự báo và phân lớp.", SequenceOrder = 3 },
                        new CareerStage { CareerPathId = dataScientist.Id, Title = "Senior Data Scientist", Description = "Nghiên cứu tối ưu giải thuật AI, đưa ra định hướng khai phá dữ liệu lớn.", SequenceOrder = 4 }
                    };
                    context.CareerStages.AddRange(stages);
                    await context.SaveChangesAsync();

                    if (skills.Count >= 20)
                    {
                        context.CareerStageSkills.AddRange(
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[1].Id, ProficiencyRequired = "Basic" }, // SQL
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[11].Id, ProficiencyRequired = "Basic" }, // Python
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[1].Id, ProficiencyRequired = "Intermediate" }, // SQL
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[12].Id, ProficiencyRequired = "Intermediate" }, // Pandas
                            new CareerStageSkill { CareerStageId = stages[2].Id, SkillId = skills[11].Id, ProficiencyRequired = "Advanced" }, // Python
                            new CareerStageSkill { CareerStageId = stages[2].Id, SkillId = skills[10].Id, ProficiencyRequired = "Intermediate" } // Algorithms
                        );
                        await context.SaveChangesAsync();
                    }
                }

                // 4. UI/UX Designer
                var uiuxDesigner = careerPaths.FirstOrDefault(cp => cp.Title.Contains("UI/UX"));
                if (uiuxDesigner != null)
                {
                    var stages = new List<CareerStage>
                    {
                        new CareerStage { CareerPathId = uiuxDesigner.Id, Title = "Junior Designer", Description = "Thiết kế các màn hình tĩnh, làm quen với Figma và chuẩn Component.", SequenceOrder = 1 },
                        new CareerStage { CareerPathId = uiuxDesigner.Id, Title = "Mid Designer", Description = "Thực hiện nghiên cứu UX cơ bản, dựng interactive prototype và test người dùng.", SequenceOrder = 2 },
                        new CareerStage { CareerPathId = uiuxDesigner.Id, Title = "Senior Designer", Description = "Xây dựng Design System, định hình phong cách mỹ thuật của toàn bộ sản phẩm.", SequenceOrder = 3 }
                    };
                    context.CareerStages.AddRange(stages);
                    await context.SaveChangesAsync();

                    if (skills.Count >= 10)
                    {
                        context.CareerStageSkills.AddRange(
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[8].Id, ProficiencyRequired = "Basic" }, // Figma
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[2].Id, ProficiencyRequired = "Basic" }, // HTML/CSS
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[8].Id, ProficiencyRequired = "Intermediate" }, // Figma
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[9].Id, ProficiencyRequired = "Basic" } // UX Research
                        );
                        await context.SaveChangesAsync();
                    }
                }

                // 5. HR Specialist
                var hrSpecialist = careerPaths.FirstOrDefault(cp => cp.Title.Contains("HR") || cp.Title.Contains("Nhân sự"));
                if (hrSpecialist != null)
                {
                    var stages = new List<CareerStage>
                    {
                        new CareerStage { CareerPathId = hrSpecialist.Id, Title = "HR Intern", Description = "Hỗ trợ lọc hồ sơ ứng viên, sắp xếp lịch hẹn phỏng vấn và thực hiện thủ tục cơ bản.", SequenceOrder = 1 },
                        new CareerStage { CareerPathId = hrSpecialist.Id, Title = "Recruitment Specialist", Description = "Chịu trách nhiệm tuyển dụng nhân sự chất lượng cao cho các phòng ban.", SequenceOrder = 2 },
                        new CareerStage { CareerPathId = hrSpecialist.Id, Title = "HR Generalist / HRBP", Description = "Giải quyết các quan hệ nhân sự, tổ chức đánh giá hiệu suất và đào tạo nội bộ.", SequenceOrder = 3 },
                        new CareerStage { CareerPathId = hrSpecialist.Id, Title = "HR Manager", Description = "Xây dựng chiến lược nhân sự, chế độ đãi ngộ và chính sách phát triển tổ chức.", SequenceOrder = 4 }
                    };
                    context.CareerStages.AddRange(stages);
                    await context.SaveChangesAsync();

                    if (skills.Count >= 20)
                    {
                        context.CareerStageSkills.AddRange(
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[7].Id, ProficiencyRequired = "Basic" }, // Communication
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[6].Id, ProficiencyRequired = "Basic" }, // Time Management
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[13].Id, ProficiencyRequired = "Intermediate" }, // Recruitment
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[7].Id, ProficiencyRequired = "Intermediate" }, // Communication
                            new CareerStageSkill { CareerStageId = stages[2].Id, SkillId = skills[19].Id, ProficiencyRequired = "Intermediate" }, // Conflict Resolution
                            new CareerStageSkill { CareerStageId = stages[3].Id, SkillId = skills[19].Id, ProficiencyRequired = "Advanced" } // Conflict Resolution
                        );
                        await context.SaveChangesAsync();
                    }
                }

                // 6. Business Manager
                var businessManager = careerPaths.FirstOrDefault(cp => cp.Title.Contains("Business") || cp.Title.Contains("Kinh doanh") || cp.Title.Contains("Quản trị"));
                if (businessManager != null)
                {
                    var stages = new List<CareerStage>
                    {
                        new CareerStage { CareerPathId = businessManager.Id, Title = "Business Assistant", Description = "Hỗ trợ chuẩn bị văn bản dự án, phối hợp sắp xếp lịch trình cuộc họp.", SequenceOrder = 1 },
                        new CareerStage { CareerPathId = businessManager.Id, Title = "Project Coordinator", Description = "Điều phối công việc trong nhóm Agile, theo dõi tiến độ các task chi tiết.", SequenceOrder = 2 },
                        new CareerStage { CareerPathId = businessManager.Id, Title = "Project Manager", Description = "Chịu trách nhiệm lập kế hoạch, kiểm soát ngân sách và quản lý rủi ro dự án.", SequenceOrder = 3 },
                        new CareerStage { CareerPathId = businessManager.Id, Title = "Business Unit Manager", Description = "Quản lý hoạt động kinh doanh tổng thể, tối ưu doanh thu và lợi nhuận của chi nhánh.", SequenceOrder = 4 }
                    };
                    context.CareerStages.AddRange(stages);
                    await context.SaveChangesAsync();

                    if (skills.Count >= 20)
                    {
                        context.CareerStageSkills.AddRange(
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[6].Id, ProficiencyRequired = "Basic" }, // Time Management
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[7].Id, ProficiencyRequired = "Basic" }, // Communication
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[14].Id, ProficiencyRequired = "Intermediate" }, // Agile/Scrum
                            new CareerStageSkill { CareerStageId = stages[2].Id, SkillId = skills[14].Id, ProficiencyRequired = "Advanced" }, // Agile/Scrum
                            new CareerStageSkill { CareerStageId = stages[2].Id, SkillId = skills[5].Id, ProficiencyRequired = "Advanced" }, // Presentation
                            new CareerStageSkill { CareerStageId = stages[3].Id, SkillId = skills[19].Id, ProficiencyRequired = "Advanced" } // Conflict Resolution
                        );
                        await context.SaveChangesAsync();
                    }
                }

                // 7. Accountant/Auditor
                var accountantAuditor = careerPaths.FirstOrDefault(cp => cp.Title.Contains("Accountant") || cp.Title.Contains("Kế toán") || cp.Title.Contains("Kiểm toán"));
                if (accountantAuditor != null)
                {
                    var stages = new List<CareerStage>
                    {
                        new CareerStage { CareerPathId = accountantAuditor.Id, Title = "Junior Accountant", Description = "Ghi sổ nhật ký chung, đối chiếu chứng từ hóa đơn, kiểm tra quỹ tiền mặt.", SequenceOrder = 1 },
                        new CareerStage { CareerPathId = accountantAuditor.Id, Title = "General Accountant", Description = "Lập báo cáo thuế giá trị gia tăng, thuế thu nhập, lên bảng cân đối kế toán.", SequenceOrder = 2 },
                        new CareerStage { CareerPathId = accountantAuditor.Id, Title = "Chief Accountant", Description = "Tổ chức công tác kế toán, kiểm soát tính hợp lý hợp lệ của các chi phí lớn.", SequenceOrder = 3 },
                        new CareerStage { CareerPathId = accountantAuditor.Id, Title = "Senior Auditor / CFO", Description = "Độc lập kiểm toán tài chính cho doanh nghiệp, tham mưu cấu trúc dòng tiền chiến lược.", SequenceOrder = 4 }
                    };
                    context.CareerStages.AddRange(stages);
                    await context.SaveChangesAsync();

                    if (skills.Count >= 20)
                    {
                        context.CareerStageSkills.AddRange(
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[15].Id, ProficiencyRequired = "Basic" }, // Financial Accounting
                            new CareerStageSkill { CareerStageId = stages[0].Id, SkillId = skills[6].Id, ProficiencyRequired = "Basic" }, // Time Management
                            new CareerStageSkill { CareerStageId = stages[1].Id, SkillId = skills[15].Id, ProficiencyRequired = "Intermediate" }, // Financial Accounting
                            new CareerStageSkill { CareerStageId = stages[2].Id, SkillId = skills[15].Id, ProficiencyRequired = "Advanced" }, // Financial Accounting
                            new CareerStageSkill { CareerStageId = stages[2].Id, SkillId = skills[16].Id, ProficiencyRequired = "Intermediate" }, // Audit
                            new CareerStageSkill { CareerStageId = stages[3].Id, SkillId = skills[16].Id, ProficiencyRequired = "Advanced" } // Audit
                        );
                        await context.SaveChangesAsync();
                    }
                }
            }

            // --- SEEDING GOALS & GOAL MILESTONES ---
            var student = await userManager.FindByEmailAsync("student@careerpath.vn");
            if (student != null && (!await context.Goals.AnyAsync() || !await context.GoalMilestones.AnyAsync()) && careerPaths.Count >= 1 && skills.Count >= 5)
            {
                var oldGoals = await context.Goals.ToListAsync();
                context.Goals.RemoveRange(oldGoals);
                await context.SaveChangesAsync();

                var goal = new Goal
                {
                    StudentId = student.Id,
                    CareerPathId = careerPaths[0].Id,
                    Title = "Trở thành Junior Software Engineer trong 6 tháng",
                    GoalType = "ShortTerm",
                    Progress = 33, // 1 out of 3 completed
                    TargetDate = DateTime.Now.AddMonths(6),
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = 1
                };
                context.Goals.Add(goal);
                await context.SaveChangesAsync();

                var milestones = new List<GoalMilestone>
                {
                    new GoalMilestone
                    {
                        GoalId = goal.Id,
                        Title = "Hoàn thành kỹ năng Lập trình C# cơ bản",
                        Status = "Completed",
                        SequenceOrder = 1,
                        SkillId = skills[0].Id,
                        UpdatedAt = DateTime.Now.AddDays(-10)
                    },
                    new GoalMilestone
                    {
                        GoalId = goal.Id,
                        Title = "Học xong khóa C# Fullstack trên hệ thống",
                        Status = "In Progress",
                        SequenceOrder = 2,
                        SkillId = skills[0].Id
                    },
                    new GoalMilestone
                    {
                        GoalId = goal.Id,
                        Title = "Xây dựng dự án cá nhân Portfolio đạt chuẩn",
                        Status = "In Progress",
                        SequenceOrder = 3,
                        SkillId = skills[4].Id // Git
                    }
                };
                context.GoalMilestones.AddRange(milestones);
                await context.SaveChangesAsync();
            }

            // Seed CareerPathCourses
            if (!await context.CareerPathCourses.AnyAsync())
            {
                var softwareEngineer = await context.CareerPaths
                    .FirstOrDefaultAsync(c => c.Title.Contains("Software Engineer"));

                if (softwareEngineer != null)
                {
                    context.CareerPathCourses.AddRange(
                        new CareerPathCourse
                        {
                            CareerPathId = softwareEngineer.Id,
                            Title = "C# Fundamentals",
                            Description = "Học cú pháp C#, biến, kiểu dữ liệu, điều kiện, vòng lặp.",
                            EstimatedDays = 7,
                            SortOrder = 1,
                            Status = 1
                        },
                        new CareerPathCourse
                        {
                            CareerPathId = softwareEngineer.Id,
                            Title = "Object Oriented Programming",
                            Description = "Lập trình hướng đối tượng với C#.",
                            EstimatedDays = 7,
                            SortOrder = 2,
                            Status = 1
                        },
                        new CareerPathCourse
                        {
                            CareerPathId = softwareEngineer.Id,
                            Title = "SQL Server",
                            Description = "Thiết kế cơ sở dữ liệu và truy vấn SQL.",
                            EstimatedDays = 5,
                            SortOrder = 3,
                            Status = 1
                        },
                        new CareerPathCourse
                        {
                            CareerPathId = softwareEngineer.Id,
                            Title = "ASP.NET Core MVC",
                            Description = "Xây dựng ứng dụng web bằng ASP.NET Core MVC.",
                            EstimatedDays = 10,
                            SortOrder = 4,
                            Status = 1
                        }
                    );

                    await context.SaveChangesAsync();
                }
            }
            // 6. Seed Base Test
            var baseTest = await context.Tests.FirstOrDefaultAsync(t => t.Title == "Bài đánh giá định hướng nghề nghiệp nền tảng (Base Test)");
            if (baseTest == null)
            {
                baseTest = new Test
                {
                    Title = "Bài đánh giá định hướng nghề nghiệp nền tảng (Base Test)",
                    Description = "Khảo sát diện rộng 20 câu hỏi tổng quan giúp phân tích hành vi, tư duy chuyên môn nhằm đề xuất Lộ trình sự nghiệp (Career Path) thích hợp nhất.",
                    Status = 1
                };
                context.Tests.Add(baseTest);
            }
            else
            {
                baseTest.Status = 1;
                context.Tests.Update(baseTest);
            }
            await context.SaveChangesAsync();

            // Deactivate all other active tests to ensure only the 20-question Base Test is used
            var otherTests = await context.Tests
                .Where(t => t.Id != baseTest.Id && t.Status == 1)
                .ToListAsync();
            foreach (var ot in otherTests)
            {
                ot.Status = 0;
                context.Tests.Update(ot);
            }
            await context.SaveChangesAsync();

            var hasSubQuestions = await context.QuestionTests.AnyAsync(q => q.TestId == baseTest.Id && q.Content.Contains("Câu hỏi phụ"));
            var existingQuestionsCount = await context.QuestionTests.CountAsync(q => q.TestId == baseTest.Id);
            if (existingQuestionsCount != 104 || hasSubQuestions)
            {
                var existingQuestions = await context.QuestionTests.Where(q => q.TestId == baseTest.Id).ToListAsync();
                foreach (var q in existingQuestions)
                {
                    var options = await context.QuestionOptions.Where(o => o.QuestionId == q.Id).ToListAsync();
                    foreach (var opt in options)
                    {
                        var weights = await context.OptionCareerPaths.Where(oc => oc.OptionId == opt.Id).ToListAsync();
                        context.OptionCareerPaths.RemoveRange(weights);
                    }
                    context.QuestionOptions.RemoveRange(options);
                }
                context.QuestionTests.RemoveRange(existingQuestions);
                await context.SaveChangesAsync();

                // --- SEEDING 104 UNIQUE QUESTIONS FOR BASE TEST ---
                var questionsData = new List<(string Content, string TestType, List<string> Options, List<(int PathIdx, int W)> Weights)>();

                var pathTexts = new Dictionary<string, string[]>()
                {
                    { "Interests", new[] {
                        "Nghiên cứu công nghệ mới, tự viết mã nguồn phần mềm.",
                        "Thiết lập, quản trị hệ thống mạng và thiết bị phần cứng.",
                        "Khám phá dữ liệu, trực quan hóa biểu đồ và tìm kiếm quy luật.",
                        "Phác thảo giao diện, phối màu và cải tiến trải nghiệm người dùng.",
                        "Tuyển dụng, trò chuyện hỗ trợ nhân sự và xây dựng văn hóa doanh nghiệp.",
                        "Lập kế hoạch kinh doanh, quản lý tiến độ và tổ chức vận hành nhóm.",
                        "Kiểm tra hóa đơn chứng từ, phân tích cân đối thu chi tài chính."
                    }},
                    { "Skills", new[] {
                        "Khả năng gỡ lỗi phần mềm và tư duy lập trình logic.",
                        "Khả năng cấu hình Router/Switch và phát hiện xâm nhập mạng.",
                        "Khả năng thống kê toán học và viết truy vấn dữ liệu SQL.",
                        "Khả năng sử dụng Figma và tạo các bản mẫu tương tác (Prototype).",
                        "Khả năng phỏng vấn tuyển dụng và đánh giá năng lực con người.",
                        "Khả năng quản trị thời gian, đàm phán và thuyết trình dự án.",
                        "Khả năng lập báo cáo tài chính và kiểm soát ngân sách nội bộ."
                    }},
                    { "Values", new[] {
                        "Giá trị của sản phẩm phần mềm mang lại tiện ích cho cộng đồng.",
                        "Sự an toàn, ổn định và bảo mật thông tin tuyệt đối của hệ thống.",
                        "Tính chính xác, khách quan của các quyết định dựa trên dữ liệu.",
                        "Vẻ đẹp thẩm mỹ, tính dễ sử dụng và thân thiện với người dùng.",
                        "Môi trường hòa đồng, phát triển tiềm năng con người tối đa.",
                        "Hiệu suất công việc, tính linh hoạt và tinh thần khởi nghiệp.",
                        "Tính minh bạch, trung thực và tuân thủ pháp luật về tài chính."
                    }},
                    { "Personality", new[] {
                        "Kiên nhẫn, thích làm việc độc lập với máy tính và giải quyết lỗi.",
                        "Cẩn thận, nhạy bén trước các nguy cơ bảo mật hệ thống.",
                        "Tò mò, có đầu óc phân tích logic và yêu thích toán học.",
                        "Bay bổng, sáng tạo nghệ thuật và có gu thẩm mỹ tốt.",
                        "Thấu hiểu, biết lắng nghe chia sẻ và có trí tuệ cảm xúc cao.",
                        "Quyết đoán, thích dẫn dắt đội nhóm và có mục tiêu rõ ràng.",
                        "Tỉ mỉ, coi trọng quy củ, sự ngăn nắp và tính chính xác cao."
                    }}
                };

                var aspectQuestions = new Dictionary<string, string[]>()
                {
                    { "Interests", new[] {
                        "Bạn thích tìm hiểu về chủ đề công nghệ hay nghệ thuật hơn?",
                        "Khi rảnh rỗi, bạn muốn tham gia hoạt động nào nhất?",
                        "Bạn thích làm việc với các con số, hình ảnh hay con người hơn?",
                        "Đọc sách về chủ đề nào khiến bạn cảm thấy hào hứng nhất?",
                        "Bạn thích dành thời gian để nghiên cứu phần mềm hay thiết kế đồ họa?",
                        "Bạn muốn tìm hiểu sâu về mảng nào trong doanh nghiệp?",
                        "Bạn thích học một ngôn ngữ lập trình mới hay học cách quản lý tài chính?",
                        "Bạn có hứng thú hơn với việc lắp ráp phần cứng hay phân tích dữ liệu?",
                        "Khi tham gia một sự kiện, bạn thích quan sát điều gì nhất?",
                        "Bạn thích sáng tạo nội dung hình ảnh hay viết code giải quyết bài toán?",
                        "Bạn thích nghiên cứu hành vi con người hay phân tích thị trường kinh doanh?",
                        "Chủ đề nào trong các buổi hội thảo thu hút sự chú ý của bạn nhất?",
                        "Bạn thích khám phá các công cụ thiết kế mới hay các công cụ quản lý dự án?",
                        "Bạn muốn dành thời gian để lập kế hoạch tài chính hay tối ưu hóa hệ thống mạng?",
                        "Bạn thích nghiên cứu về an ninh mạng hay xây dựng quy trình nhân sự?",
                        "Bạn thích tìm tòi về trí tuệ nhân tạo hay các xu hướng thiết kế giao diện?",
                        "Khi lướt mạng xã hội, bạn thường đọc bài viết về chủ đề nào?",
                        "Bạn thích học hỏi về kỹ thuật kiểm toán hay kỹ năng lãnh đạo đội nhóm?",
                        "Hoạt động ngoại khóa nào bạn cảm thấy tự tin và hứng thú tham gia nhất?",
                        "Bạn thích nghiên cứu về cấu trúc dữ liệu hay vẽ minh họa sản phẩm?",
                        "Bạn thích học cách tối ưu hóa công cụ tìm kiếm hay xây dựng kế hoạch nhân sự?",
                        "Bạn thích tìm hiểu cách vận hành của một chuỗi cung ứng hay thiết kế hệ thống?",
                        "Bạn có hứng thú với việc phân tích dòng tiền hay đào tạo nhân sự mới?",
                        "Bạn thích tìm hiểu về các giải pháp bảo mật hay các mô hình kinh doanh?",
                        "Bạn có muốn học cách tổ chức một sự kiện cộng đồng lớn không?",
                        "Bạn thích nghiên cứu về trải nghiệm người dùng trên thiết bị di động hay máy tính?"
                    }},
                    { "Skills", new[] {
                        "Bạn tự tin nhất vào khả năng nào khi làm việc nhóm?",
                        "Kỹ năng kỹ thuật nào bạn đã làm chủ tốt nhất?",
                        "Khi gặp một lỗi kỹ thuật phức tạp, bạn giải quyết bằng cách nào?",
                        "Bạn có khả năng tự học một công nghệ mới nhanh đến mức nào?",
                        "Bạn đánh giá thế nào về khả năng thuyết trình và truyền đạt của mình?",
                        "Bạn có khả năng quản lý thời gian và sắp xếp công việc tốt không?",
                        "Bạn tự tin nhất vào khả năng viết code hay thiết kế giao diện?",
                        "Bạn có kỹ năng phân tích và làm sạch dữ liệu tốt không?",
                        "Khả năng thiết lập và cấu hình hệ thống mạng của bạn ở mức nào?",
                        "Bạn có tự tin vào khả năng lập báo cáo tài chính chính xác không?",
                        "Kỹ năng giao tiếp và thuyết phục người khác của bạn như thế nào?",
                        "Bạn có khả năng phỏng vấn và đánh giá năng lực của người khác không?",
                        "Bạn tự tin vào khả năng sử dụng các công cụ thiết kế như Figma không?",
                        "Khả năng giải quyết các mâu thuẫn trong nhóm của bạn như thế nào?",
                        "Bạn có kỹ năng quản lý ngân sách và kiểm soát chi phí không?",
                        "Bạn tự tin nhất vào khả năng phát hiện lỗi bảo mật hay lỗi phần mềm?",
                        "Khả năng làm việc độc lập của bạn so với làm việc nhóm thế nào?",
                        "Bạn có kỹ năng điều phối dự án theo mô hình Agile/Scrum không?",
                        "Kỹ năng nghiên cứu trải nghiệm và khảo sát người dùng của bạn ra sao?",
                        "Bạn tự tin vào khả năng viết các câu lệnh truy vấn SQL phức tạp không?",
                        "Bạn có kỹ năng tối ưu hóa các thuật toán xử lý dữ liệu lớn không?",
                        "Khả năng thích ứng với sự thay đổi quy trình làm việc của bạn thế nào?",
                        "Bạn tự tin vào kỹ năng viết tài liệu kỹ thuật rõ ràng, dễ hiểu không?",
                        "Bạn có khả năng hướng dẫn và hỗ trợ chuyên môn cho người mới không?",
                        "Kỹ năng phân tích thị trường và đối thủ cạnh tranh của bạn ra sao?",
                        "Bạn tự tin vào khả năng xử lý các tình huống khẩn cấp trong công việc không?"
                    }},
                    { "Values", new[] {
                        "Yếu tố nào quan trọng nhất đối với bạn khi chọn một công việc?",
                        "Động lực lớn nhất giúp bạn nỗ lực hoàn thành công việc mỗi ngày là gì?",
                        "Môi trường làm việc lý tưởng đối với bạn có đặc điểm gì?",
                        "Bạn coi trọng sự ổn định lâu dài hay cơ hội bứt phá thử thách hơn?",
                        "Bạn mong muốn sản phẩm mình làm ra mang lại giá trị gì cho xã hội?",
                        "Đối với bạn, một người quản lý lý tưởng cần có phẩm chất gì?",
                        "Bạn coi trọng thu nhập cao hay sự cân bằng giữa công việc và cuộc sống?",
                        "Bạn muốn làm việc ở một tập đoàn lớn có quy trình hay startup linh hoạt?",
                        "Tính minh bạch và trung thực trong tổ chức có ý nghĩa thế nào với bạn?",
                        "Bạn mong đợi nhận được sự công nhận từ cấp trên như thế nào?",
                        "Bạn ưu tiên làm việc ở nơi có cơ hội thăng tiến rõ ràng không?",
                        "Mức độ tự do sáng tạo trong công việc quan trọng với bạn thế nào?",
                        "Bạn mong muốn đồng nghiệp của mình là những người như thế nào?",
                        "Việc đóng góp ý kiến cá nhân vào chiến lược chung quan trọng ra sao?",
                        "Bạn nghĩ sao về việc làm thêm giờ (OT) để đạt được mục tiêu chung?",
                        "Bạn coi trọng việc học hỏi kiến thức mới hay áp dụng kỹ năng sẵn có?",
                        "Một văn hóa làm việc cởi mở, không khoảng cách có quan trọng với bạn?",
                        "Bạn có muốn làm việc ở nơi có nhiều hoạt động gắn kết cộng đồng?",
                        "Sự rõ ràng trong chế độ đãi ngộ và phúc lợi tác động thế nào đến bạn?",
                        "Bạn mong muốn công việc của mình mang tính chất phục vụ cộng đồng không?",
                        "Sự an toàn thông tin và bảo mật dữ liệu của công ty quan trọng thế nào?",
                        "Bạn thích nhận phản hồi trực tiếp, thẳng thắn hay nhẹ nhàng, khích lệ?",
                        "Bạn coi trọng việc tự do quyết định cách làm việc của mình như thế nào?",
                        "Tinh thần khởi nghiệp và dám chấp nhận rủi ro có cuốn hút bạn không?",
                        "Bạn mong muốn công việc mang lại danh tiếng cá nhân ở mức độ nào?",
                        "Bạn ưu tiên môi trường tôn trọng sự đa dạng và khác biệt của cá nhân?"
                    }},
                    { "Personality", new[] {
                        "Khi đối mặt với áp lực công việc lớn, phản ứng tự nhiên của bạn là gì?",
                        "Bạn là người hướng nội thích làm việc độc lập hay hướng ngoại thích giao lưu?",
                        "Cách bạn thường đưa ra quyết định quan trọng trong cuộc sống là gì?",
                        "Bạn thường giải quyết các bất đồng quan điểm trong nhóm như thế nào?",
                        "Khi bắt đầu một dự án mới, bạn là người lập kế hoạch hay hành động ngay?",
                        "Bạn tự nhận thấy mình là người cẩn thận, tỉ mỉ hay nhanh nhẹn, linh hoạt?",
                        "Bạn phản ứng thế nào khi nhận được những lời góp ý phê bình từ người khác?",
                        "Khi làm việc nhóm, bạn thường đảm nhận vai trò nào một cách tự nhiên?",
                        "Bạn có dễ dàng đồng cảm và chia sẻ khó khăn với người xung quanh không?",
                        "Bạn thích làm việc theo quy trình sẵn có hay tự tìm lối đi riêng?",
                        "Khi một kế hoạch bị thay đổi đột ngột vào phút chót, bạn sẽ làm gì?",
                        "Bạn có xu hướng tập trung vào chi tiết nhỏ hay bức tranh tổng thể hơn?",
                        "Khi phải thuyết phục người khác đồng ý với mình, bạn thường làm gì?",
                        "Bạn có phải là người dễ dàng chấp nhận và thử nghiệm các ý tưởng mới?",
                        "Bạn thường kiểm soát cảm xúc của mình thế nào trong các cuộc tranh luận?",
                        "Bạn có phải là người thích cạnh tranh để đạt được kết quả tốt nhất?",
                        "Khi hoàn thành công việc, bạn có xu hướng tự kiểm tra lại nhiều lần không?",
                        "Bạn thích sự ngăn nắp, quy củ hay sự linh hoạt, ngẫu hứng hơn?",
                        "Cách bạn tiếp cận một vấn đề phức tạp chưa từng có tiền lệ là gì?",
                        "Bạn có thường xuyên chia sẻ tiến độ công việc với mọi người không?",
                        "Bạn phản ứng thế nào khi đồng nghiệp không hoàn thành đúng tiến độ?",
                        "Bạn có thích dẫn dắt và chịu trách nhiệm cho kết quả của cả đội không?",
                        "Bạn thấy mình là người thực tế, lý trí hay mơ mộng, giàu cảm xúc hơn?",
                        "Bạn thường dành thời gian để tự phản tỉnh và đánh giá bản thân khi nào?",
                        "Bạn có dễ bị phân tâm bởi các yếu tố môi trường xung quanh khi làm việc?",
                        "Bạn có phải là người luôn giữ lời hứa và hoàn thành cam kết đúng hạn?"
                    }}
                };

                var aspectTypes = new[] { "Interests", "Skills", "Values", "Personality" };
                foreach (var aspect in aspectTypes)
                {
                    var questionsList = aspectQuestions[aspect];
                    for (int i = 0; i < questionsList.Length; i++)
                    {
                        var content = questionsList[i];
                        var opts = new List<string>();
                        var weights = new List<(int, int)>();
                        
                        for (int j = 0; j < 4; j++)
                        {
                            int pathIdx = (i + j) % 7;
                            opts.Add(pathTexts[aspect][pathIdx]);
                            weights.Add((pathIdx, 5));
                        }

                        questionsData.Add((content, aspect, opts, weights));
                    }
                }

                foreach (var qData in questionsData)
                {
                    var question = new QuestionTest
                    {
                        TestId = baseTest.Id,
                        QuestionTypeId = singleChoiceType.Id,
                        Content = qData.Content,
                        TestType = qData.TestType
                    };
                    context.QuestionTests.Add(question);
                    await context.SaveChangesAsync(); // Lưu để sinh QuestionId

                    var optionsList = new List<QuestionOption>();
                    foreach (var optContent in qData.Options)
                    {
                        var option = new QuestionOption
                        {
                            QuestionId = question.Id,
                            Content = optContent
                        };
                        optionsList.Add(option);
                    }
                    context.QuestionOptions.AddRange(optionsList);
                    await context.SaveChangesAsync(); // Lưu để sinh OptionId

                    // Gắn trọng số Weight dựa trên Index của CareerPath đã khởi tạo ở trên
                    for (int i = 0; i < optionsList.Count; i++)
                    {
                        var weightConfig = qData.Weights[i];
                        var optionCareerPath = new OptionCareerPath
                        {
                            OptionId = optionsList[i].Id,
                            CareerPathId = careerPaths[weightConfig.PathIdx].Id,
                            Weight = weightConfig.W
                        };
                        context.OptionCareerPaths.Add(optionCareerPath);
                    }
                    await context.SaveChangesAsync();
                }
            }

            // 7. Seed Team Members
            if (!await context.TeamMembers.AnyAsync())
            {
                var teamMembers = new List<TeamMember>
                {
                    new TeamMember
                    {
                        Name = "Mr Nhu Van Tuan",
                        Role = "Leader",
                        AvatarUrl = "",
                        Bio = "Hơn 10 năm kinh nghiệm quản lý dự án công nghệ, dẫn dắt đội ngũ phát triển các giải pháp hướng nghiệp toàn cầu.",
                        Email = "tuan.nv@careerpath.vn"
                    },
                    new TeamMember
                    {
                        Name = "Mr Nguyen Tuan Dung",
                        Role = "Member BE",
                        AvatarUrl = "",
                        Bio = "Chuyên gia phát triển hệ thống backend tối ưu hiệu năng, xây dựng kiến trúc dữ liệu và xử lý thuật toán phân tích.",
                        Email = "dung.nt@careerpath.vn"
                    },
                    new TeamMember
                    {
                        Name = "Mr Nguyen Van Linh",
                        Role = "Member FE & SM",
                        AvatarUrl = "",
                        Bio = "Đam mê thiết kế giao diện tinh tế, tối ưu hóa trải nghiệm người dùng Front-End và điều phối dự án theo Agile/Scrum.",
                        Email = "linh.nv@careerpath.vn"
                    },
                    new TeamMember
                    {
                        Name = "Mr Nguyen Huu Tri",
                        Role = "Member Service",
                        AvatarUrl = "",
                        Bio = "Trưởng bộ phận dịch vụ khách hàng, kết nối doanh nghiệp và hỗ trợ học sinh định hướng lộ trình học tập hiệu quả.",
                        Email = "tri.nh@careerpath.vn"
                    }
                };
                context.TeamMembers.AddRange(teamMembers);
                await context.SaveChangesAsync();
            }

            // 8. Seed 30 FAQ Items
            if (!await context.FaqItems.AnyAsync())
            {
                var faqs = new List<FaqItem>
                {
                    // Category: General
                    new FaqItem { Category = "General", Question = "CareerPath là gì?", Answer = "CareerPath là nền tảng hướng nghiệp thông minh hỗ trợ người dùng khám phá bản thân, đề xuất lộ trình sự nghiệp phù hợp và kết nối với các doanh nghiệp, mentor." },
                    new FaqItem { Category = "General", Question = "Làm thế nào để bắt đầu sử dụng CareerPath?", Answer = "Bạn chỉ cần đăng ký một tài khoản, thực hiện bài kiểm tra đánh giá nghề nghiệp nền tảng (Base Test) để nhận được các gợi ý lộ trình nghề nghiệp chi tiết." },
                    new FaqItem { Category = "General", Question = "Nền tảng này dành cho đối tượng nào?", Answer = "CareerPath được thiết kế đặc biệt cho học sinh trung học, sinh viên đại học và người đi làm muốn chuyển hướng sự nghiệp." },
                    new FaqItem { Category = "General", Question = "Sử dụng nền tảng CareerPath có mất phí không?", Answer = "Các tính năng cơ bản như làm bài kiểm tra trắc nghiệm, xem lộ trình sơ bộ và tin tuyển dụng là hoàn toàn miễn phí. Các gói Premium hỗ trợ phân tích chuyên sâu chi tiết và đặt lịch Mentor nâng cao." },
                    new FaqItem { Category = "General", Question = "CareerPath hoạt động trên thiết bị nào?", Answer = "Hệ thống hoạt động mượt mà trên mọi thiết bị có kết nối Internet như máy tính để bàn, laptop, máy tính bảng và điện thoại di động thông qua trình duyệt web." },
                    new FaqItem { Category = "General", Question = "Tôi có thể liên hệ hỗ trợ kỹ thuật bằng cách nào?", Answer = "Bạn có thể gửi yêu cầu hỗ trợ qua trang Liên hệ trực tuyến hoặc email đến support@careerpath.vn để nhận phản hồi trong vòng 24 giờ." },

                    // Category: Account
                    new FaqItem { Category = "Account", Question = "Đăng ký tài khoản yêu cầu những thông tin gì?", Answer = "Bạn chỉ cần cung cấp Họ tên, địa chỉ Email hợp lệ và tạo Mật khẩu bảo mật để thiết lập tài khoản cá nhân." },
                    new FaqItem { Category = "Account", Question = "Tôi có thể thay đổi email đăng ký không?", Answer = "Hiện tại để bảo mật tài khoản, email đăng ký là cố định. Nếu có nhu cầu thay đổi đặc biệt, bạn cần liên hệ admin để hỗ trợ xác minh." },
                    new FaqItem { Category = "Account", Question = "Làm sao để thay đổi mật khẩu?", Answer = "Bạn truy cập vào trang Quản lý tài khoản cá nhân, chọn mục 'Đổi mật khẩu', nhập mật khẩu hiện tại và mật khẩu mới để cập nhật." },
                    new FaqItem { Category = "Account", Question = "Tôi có thể đăng nhập bằng các tài khoản mạng xã hội không?", Answer = "Tính năng đăng nhập qua Google và Facebook đang được phát triển và dự kiến sẽ ra mắt trong phiên bản cập nhật sắp tới." },
                    new FaqItem { Category = "Account", Question = "Làm thế nào để xóa tài khoản cá nhân?", Answer = "Bạn có thể gửi yêu cầu xóa tài khoản cùng lý do qua email hỗ trợ của chúng tôi. Dữ liệu cá nhân của bạn sẽ được gỡ bỏ hoàn toàn khỏi hệ thống sau khi xác nhận." },
                    new FaqItem { Category = "Account", Question = "Hệ thống bảo vệ mật khẩu của tôi như thế nào?", Answer = "Mật khẩu của bạn được mã hóa một chiều bằng thuật toán Identity PasswordHasher trước khi lưu trữ vào cơ sở dữ liệu." },

                    // Category: Test
                    new FaqItem { Category = "Test", Question = "Bài kiểm tra trắc nghiệm kéo dài bao lâu?", Answer = "Bài kiểm tra hướng nghiệp nền tảng gồm 20 câu hỏi, thường chỉ mất khoảng 10-15 phút để hoàn thành một cách thoải mái." },
                    new FaqItem { Category = "Test", Question = "Mô hình khoa học nào đứng sau bài kiểm tra?", Answer = "Thuật toán của bài kiểm tra được thiết kế dựa trên lý thuyết trắc nghiệm tâm lý học hành vi và phân tích xu hướng năng lực kết hợp với trọng số kỹ năng thực tế." },
                    new FaqItem { Category = "Test", Question = "Kết quả bài kiểm tra có chính xác không?", Answer = "Kết quả dựa trên câu trả lời trung thực của bạn và mang tính chất định hướng, giúp bạn thu hẹp phạm vi lựa chọn nghề nghiệp dựa trên năng lực và sở thích." },
                    new FaqItem { Category = "Test", Question = "Tôi có thể lưu lại kết quả kiểm tra không?", Answer = "Có. Toàn bộ lịch sử và kết quả bài đánh giá của bạn đều được tự động lưu trữ và hiển thị trực quan trong hồ sơ cá nhân." },
                    new FaqItem { Category = "Test", Question = "Tại sao kết quả của tôi chỉ gợi ý một số ngành nhất định?", Answer = "Thuật toán tính điểm ưu tiên các ngành nghề có trọng số tương thích cao nhất với câu trả lời của bạn để tránh làm bạn bị phân tâm bởi quá nhiều lựa chọn." },
                    new FaqItem { Category = "Test", Question = "Tôi có thể làm bài kiểm tra lại từ đầu không?", Answer = "Hoàn toàn được. Bạn có thể nhấn nút làm lại bài kiểm tra bất cứ lúc nào để làm mới các gợi ý lộ trình nghề nghiệp." },

                    // Category: Mentor
                    new FaqItem { Category = "Mentor", Question = "Mentor trên CareerPath là ai?", Answer = "Đội ngũ Mentor là các chuyên gia có nhiều năm kinh nghiệm thực chiến trong các lĩnh vực như Kỹ nghệ phần mềm, Thiết kế UI/UX, Quản trị nhân sự và Tài chính." },
                    new FaqItem { Category = "Mentor", Question = "Làm sao để đặt lịch hẹn với Mentor?", Answer = "Sau khi có kết quả bài kiểm tra, hệ thống sẽ tiến cử các Mentor phù hợp. Bạn chọn hồ sơ Mentor mong muốn, chọn lịch trống và điền lý do cần tư vấn để gửi yêu cầu." },
                    new FaqItem { Category = "Mentor", Question = "Đặt lịch Mentor có tốn phí không?", Answer = "Đặt lịch hỗ trợ giải đáp nhanh thường miễn phí hoặc được bao gồm trong gói Premium. Một số Mentor đặc biệt có thể có mức phí tư vấn chuyên sâu riêng." },
                    new FaqItem { Category = "Mentor", Question = "Tôi có thể hủy lịch hẹn đã đặt không?", Answer = "Có, bạn có thể hủy lịch trước ít nhất 12 giờ diễn ra cuộc hẹn thông qua bảng quản lý lịch trình cá nhân." },
                    new FaqItem { Category = "Mentor", Question = "Buổi gặp mặt Mentor diễn ra ở đâu?", Answer = "Các buổi tư vấn chủ yếu diễn ra trực tuyến thông qua cuộc gọi video tích hợp sẵn trên nền tảng (Zoom hoặc Google Meet)." },
                    new FaqItem { Category = "Mentor", Question = "Làm thế nào để trở thành Mentor trên hệ thống?", Answer = "Nếu bạn có trên 3 năm kinh nghiệm làm việc và mong muốn chia sẻ, hãy gửi hồ sơ đăng ký qua trang Hợp tác Mentor để ban quản trị phê duyệt." },

                    // Category: Community
                    new FaqItem { Category = "Community", Question = "Không gian Cộng đồng hoạt động thế nào?", Answer = "Cộng đồng là nơi người dùng chia sẻ kinh nghiệm học tập, đặt câu hỏi về ngành nghề và thảo luận các chủ đề nóng về tuyển dụng." },
                    new FaqItem { Category = "Community", Question = "Tôi có được đăng bài viết tự do không?", Answer = "Có, bạn có thể đăng các bài viết thảo luận. Tuy nhiên, bài viết cần tuân thủ tiêu chuẩn cộng đồng và sẽ được kiểm duyệt để tránh spam." },
                    new FaqItem { Category = "Community", Question = "Làm sao để báo cáo bài viết vi phạm?", Answer = "Bên cạnh mỗi bài đăng đều có nút 'Báo cáo vi phạm'. Ban quản trị sẽ rà soát và xử lý bài viết vi phạm trong vòng 2 giờ." },
                    new FaqItem { Category = "Community", Question = "Các sự kiện nghề nghiệp diễn ra khi nào?", Answer = "Lịch sự kiện nghề nghiệp được cập nhật liên tục hàng tuần trên mục 'Tin tức & Sự kiện', bao gồm các buổi tọa đàm trực tuyến và ngày hội việc làm." },
                    new FaqItem { Category = "Community", Question = "Làm thế nào để đăng ký tham gia sự kiện?", Answer = "Bạn nhấp vào sự kiện quan tâm trên giao diện, nhấn nút 'Đăng ký tham gia' để nhận liên kết Zoom và lịch nhắc qua email." },
                    new FaqItem { Category = "Community", Question = "Lợi ích khi tham gia các sự kiện của CareerPath?", Answer = "Bạn sẽ được giao lưu trực tiếp với các nhà tuyển dụng, nhận tài liệu độc quyền và có cơ hội nhận voucher học bổng từ các đối tác đào tạo." }
                };
                context.FaqItems.AddRange(faqs);
                await context.SaveChangesAsync();
            }

            // 9. Seed NewsArticles
            if (!await context.NewsArticles.AnyAsync())
            {
                var news = new List<NewsArticle>
                {
                    new NewsArticle
                    {
                        Title = "Xu hướng tuyển dụng ngành Công nghệ thông tin năm 2026",
                        Summary = "Khám phá những kỹ năng cốt lõi và các vị trí lập trình viên được săn đón nhất trong kỷ nguyên trí tuệ nhân tạo phát triển vượt bậc.",
                        Content = "Ngành Công nghệ thông tin trong năm 2026 đang chứng kiến sự chuyển dịch mạnh mẽ. Sự phát triển của AI tạo sinh không làm giảm nhu cầu tuyển dụng mà trái lại đặt ra bài toán về việc lập trình viên phải trang bị thêm kỹ năng sử dụng công cụ AI để tối ưu năng suất. Các vị trí như Kỹ sư đám mây, Chuyên gia bảo mật thông tin và Kỹ sư AI/Machine Learning tiếp tục là những điểm nóng thu hút nhân tài với mức đãi ngộ cực kỳ hấp dẫn...",
                        Author = "Mr Nguyen Tuan Dung",
                        PublishedDate = DateTime.Now.AddDays(-5),
                        Category = "Xu hướng nghề nghiệp",
                        ImageUrl = ""
                    },
                    new NewsArticle
                    {
                        Title = "Kỹ năng giao tiếp và làm việc nhóm: Chìa khóa vàng cho sinh viên mới ra trường",
                        Summary = "Tại sao kiến thức chuyên môn là chưa đủ để bạn bứt phá trong môi trường doanh nghiệp hiện đại? Đọc ngay cẩm nang nâng cấp soft-skills.",
                        Content = "Nhiều khảo sát từ các tập đoàn lớn cho thấy trên 70% lý do nhân viên mới gặp khó khăn trong việc hòa nhập không nằm ở kiến thức kỹ thuật mà ở các kỹ năng mềm. Kỹ năng giao tiếp hiệu quả, trình bày ý tưởng rõ ràng và lắng nghe đồng nghiệp giúp tối ưu hóa hiệu suất làm việc nhóm. Bài viết này hướng dẫn bạn cách vượt qua rào cản tâm lý khi giao tiếp và làm việc nhóm hiệu quả...",
                        Author = "Mr Nhu Van Tuan",
                        PublishedDate = DateTime.Now.AddDays(-10),
                        Category = "Kỹ năng mềm",
                        ImageUrl = ""
                    },
                    new NewsArticle
                    {
                        Title = "Cẩm nang chuẩn bị CV ấn tượng thu hút nhà tuyển dụng ngay từ cái nhìn đầu tiên",
                        Summary = "Chi tiết các bước thiết kế CV chuẩn ATS giúp tăng cơ hội được gọi phỏng vấn lên gấp 3 lần dành cho sinh viên.",
                        Content = "Hệ thống lọc hồ sơ tự động (ATS) hiện được sử dụng bởi hầu hết các công ty lớn. Để CV của bạn không bị loại từ 'vòng gửi xe', việc tối ưu hóa từ khóa chuyên ngành, cấu trúc CV đơn giản, khoa học và làm nổi bật các dự án thực tế là cực kỳ quan trọng. Tránh viết CV quá dài hoặc sử dụng quá nhiều biểu đồ cột đánh giá kỹ năng không rõ ràng...",
                        Author = "Mr Nguyen Van Linh",
                        PublishedDate = DateTime.Now.AddDays(-15),
                        Category = "Cẩm nang xin việc",
                        ImageUrl = ""
                    }
                };
                context.NewsArticles.AddRange(news);
                await context.SaveChangesAsync();
            }

            // 10. Seed CareerEvents
            if (!await context.CareerEvents.AnyAsync())
            {
                var events = new List<CareerEvent>
                {
                    new CareerEvent
                    {
                        Title = "Webinar: Định hình lộ trình sự nghiệp ngành Software Engineer",
                        Description = "Buổi chia sẻ chuyên sâu từ Mentor Nguyễn Tuấn Dũng về các công nghệ cốt lõi cần học và cách xây dựng dự án cá nhân nổi bật.",
                        EventDate = DateTime.Now.AddDays(3).Date.AddHours(19).AddMinutes(30),
                        Location = "Trực tuyến qua Zoom",
                        Speaker = "Mr Nguyen Tuan Dung (Member BE)",
                        RegistrationUrl = "https://zoom.us/webinar/register/123"
                    },
                    new CareerEvent
                    {
                        Title = "Hội thảo: Xu hướng UI/UX Designer và trải nghiệm sản phẩm số",
                        Description = "Giao lưu trực tiếp cùng chuyên gia thiết kế Nguyễn Văn Linh về tư duy thiết kế lấy người dùng làm trung tâm trong kỷ nguyên mới.",
                        EventDate = DateTime.Now.AddDays(7).Date.AddHours(14).AddMinutes(0),
                        Location = "Hội trường tầng 3, CareerPath Office, Hà Nội",
                        Speaker = "Mr Nguyen Van Linh (Member FE & SM)",
                        RegistrationUrl = "https://careerpath.vn/events/register/456"
                    },
                    new CareerEvent
                    {
                        Title = "Talkshow: Định hướng nghề nghiệp bản thân cùng Top Leader",
                        Description = "Lắng nghe chia sẻ của anh Nhữ Văn Tuấn về cách thấu hiểu bản thân để đưa ra những quyết định thay đổi cuộc đời.",
                        EventDate = DateTime.Now.AddDays(12).Date.AddHours(9).AddMinutes(30),
                        Location = "Trực tuyến qua Microsoft Teams",
                        Speaker = "Mr Nhu Van Tuan (Leader)",
                        RegistrationUrl = "https://teams.live.com/register/789"
                    }
                };
                context.CareerEvents.AddRange(events);
                await context.SaveChangesAsync();
            }

            // 11. Seed CommunityPosts
            if (!await context.CommunityPosts.AnyAsync())
            {
                var posts = new List<CommunityPost>
                {
                    new CommunityPost
                    {
                        Title = "Nên học ASP.NET Core hay Node.js cho Backend Web Developer?",
                        Content = "Chào mọi người, mình đang là sinh viên năm 2 ngành CNTT. Mình đang phân vân giữa việc theo đuổi C# ASP.NET Core hay JavaScript/Node.js để phát triển Backend. Mong các anh chị đi trước cho lời khuyên về cơ hội việc làm và mức lương của 2 hướng này ạ.",
                        AuthorName = "Trần Minh Hoàng",
                        CreatedAt = DateTime.Now.AddHours(-4),
                        LikesCount = 28,
                        RepliesCount = 12,
                        Category = "Hỏi đáp"
                    },
                    new CommunityPost
                    {
                        Title = "Trải nghiệm lần đầu tiên đi phỏng vấn vị trí UI/UX Designer Intern",
                        Content = "Vừa qua mình có tham gia phỏng vấn thực tập sinh UI/UX tại một công ty công nghệ lớn ở HN. Mình muốn chia sẻ lại bộ câu hỏi phỏng vấn thực tế và một số lưu ý chuẩn bị portfolio giúp các bạn có cùng định hướng chuẩn bị tốt hơn...",
                        AuthorName = "Lê Thị Thu Hà",
                        CreatedAt = DateTime.Now.AddDays(-1),
                        LikesCount = 45,
                        RepliesCount = 8,
                        Category = "Góc chia sẻ"
                    },
                    new CommunityPost
                    {
                        Title = "Chia sẻ tài liệu tự học kỹ năng lập trình hướng đối tượng (OOP) cực kỳ dễ hiểu",
                        Content = "Mình sưu tầm được một bộ slide bài giảng trực quan hóa và các bài tập thực hành OOP bằng C++ và C# từ một trường đại học hàng đầu Mỹ. Tài liệu rất thích hợp cho những bạn bắt đầu học lập trình hoặc muốn củng cố lại tư duy thiết kế phần mềm...",
                        AuthorName = "Phạm Hữu Nam",
                        CreatedAt = DateTime.Now.AddDays(-3),
                        LikesCount = 62,
                        RepliesCount = 15,
                        Category = "Tài nguyên học tập"
                    }
                };
                context.CommunityPosts.AddRange(posts);
                await context.SaveChangesAsync();
            }

            // 12. Seed Learning Resources
            if (!await context.Resources.AnyAsync(r => r.CategoryId != null))
            {
                // Clear old resources to ensure clean seeding of the new structure
                var oldResources = await context.Resources.ToListAsync();
                context.Resources.RemoveRange(oldResources);
                await context.SaveChangesAsync();

                var dbCareerPaths = await context.CareerPaths.OrderBy(c => c.Id).ToListAsync();
                if (dbCareerPaths.Count >= 7)
                {
                    if (skills == null || !skills.Any())
                    {
                        skills = await context.Skills.OrderBy(s => s.Id).ToListAsync();
                    }

                    var techCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Kỹ thuật") || c.Name.Contains("Công nghệ"));
                    var businessCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Kinh doanh") || c.Name.Contains("Kinh tế"));
                    var socialCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Xã hội"));

                    var techCatId = techCategory?.Id ?? 1;
                    var bizCatId = businessCategory?.Id ?? 2;
                    var socCatId = socialCategory?.Id ?? 3;

                    // We will create parent courses (nested parents)
                    var csharpCourse = new Resource 
                    { 
                        PathId = dbCareerPaths[0].Id, 
                        CategoryId = techCatId,
                        SkillId = skills.Count > 0 ? skills[0].Id : (int?)null, // Lập trình C#
                        ResourceType = "Course", 
                        Title = "Khóa học ASP.NET Core MVC Full Stack", 
                        Url = "https://example.com/courses/aspnet-fullstack",
                        Description = "Học lập trình Backend Web với ASP.NET Core từ căn bản đến nâng cao.",
                        Status = 1
                    };

                    var figmaCourse = new Resource
                    {
                        PathId = dbCareerPaths[3].Id,
                        CategoryId = techCatId,
                        SkillId = skills.Count > 8 ? skills[8].Id : (int?)null, // Figma
                        ResourceType = "Course",
                        Title = "Khóa học Thiết kế UI/UX với Figma chuyên nghiệp",
                        Url = "https://example.com/courses/figma-mastery",
                        Description = "Làm chủ Figma, thiết kế giao diện web/app chuẩn UI/UX.",
                        Status = 1
                    };

                    var ccnaCourse = new Resource
                    {
                        PathId = dbCareerPaths[1].Id,
                        CategoryId = techCatId,
                        SkillId = skills.Count > 17 ? skills[17].Id : (int?)null, // CCNA
                        ResourceType = "Course",
                        Title = "Học mạng căn bản CCNA từ con số 0",
                        Url = "https://example.com/courses/ccna-networking",
                        Description = "Kiến thức mạng toàn diện, chuẩn bị thi chứng chỉ CCNA.",
                        Status = 1
                    };

                    context.Resources.AddRange(csharpCourse, figmaCourse, ccnaCourse);
                    await context.SaveChangesAsync(); // generate parent IDs

                    var resources = new List<Resource>
                    {
                        // Software Engineer (PathIdx 0)
                        new Resource { PathId = dbCareerPaths[0].Id, CategoryId = techCatId, SkillId = skills.Count > 0 ? skills[0].Id : (int?)null, ParentResourceId = csharpCourse.Id, ResourceType = "PDF", Title = "C# & .NET Core Developer Handbook", Url = "https://example.com/books/csharp-handbook.pdf", Description = "Tài liệu tra cứu nhanh ngôn ngữ C# và .NET Core.", Status = 1 },
                        new Resource { PathId = dbCareerPaths[0].Id, CategoryId = techCatId, SkillId = skills.Count > 3 ? skills[3].Id : (int?)null, ParentResourceId = csharpCourse.Id, ResourceType = "Video", Title = "Mastering Clean Architecture in ASP.NET Core", Url = "https://example.com/videos/clean-architecture", Description = "Video hướng dẫn thiết kế Clean Architecture.", Status = 1 },
                        
                        // Network & Security (PathIdx 1)
                        new Resource { PathId = dbCareerPaths[1].Id, CategoryId = techCatId, SkillId = skills.Count > 17 ? skills[17].Id : (int?)null, ParentResourceId = ccnaCourse.Id, ResourceType = "Doc", Title = "Cisco CCNA Networking Lab Guide", Url = "https://example.com/docs/ccna-networking-guide", Description = "Tài liệu hướng dẫn thực hành lab CCNA.", Status = 1 },
                        new Resource { PathId = dbCareerPaths[1].Id, CategoryId = techCatId, SkillId = skills.Count > 18 ? skills[18].Id : (int?)null, ResourceType = "PDF", Title = "Introduction to Cyber Security Standards 2026", Url = "https://example.com/books/cybersecurity-standards.pdf", Description = "Sách giới thiệu các tiêu chuẩn bảo mật hệ thống mạng.", Status = 1 },
                        
                        // Data Scientist (PathIdx 2)
                        new Resource { PathId = dbCareerPaths[2].Id, CategoryId = techCatId, SkillId = skills.Count > 11 ? skills[11].Id : (int?)null, ResourceType = "Video", Title = "Python for Data Analysis and Visualization", Url = "https://example.com/videos/python-data-science", Description = "Video hướng dẫn Python, Pandas, Matplotlib.", Status = 1 },
                        new Resource { PathId = dbCareerPaths[2].Id, CategoryId = techCatId, SkillId = skills.Count > 12 ? skills[12].Id : (int?)null, ResourceType = "PDF", Title = "Practical Machine Learning Guide", Url = "https://example.com/books/machine-learning-guide.pdf", Description = "Sách hướng dẫn thuật toán Machine Learning thực chiến.", Status = 1 },
                        
                        // UI/UX Designer (PathIdx 3)
                        new Resource { PathId = dbCareerPaths[3].Id, CategoryId = techCatId, SkillId = skills.Count > 8 ? skills[8].Id : (int?)null, ParentResourceId = figmaCourse.Id, ResourceType = "Doc", Title = "Figma Design System Component Standards", Url = "https://example.com/docs/figma-design-system", Description = "Quy chuẩn thiết kế Component và Auto-layout.", Status = 1 },
                        new Resource { PathId = dbCareerPaths[3].Id, CategoryId = techCatId, SkillId = skills.Count > 9 ? skills[9].Id : (int?)null, ParentResourceId = figmaCourse.Id, ResourceType = "Video", Title = "User Experience (UX) Research Methodology", Url = "https://example.com/videos/ux-research", Description = "Video phương pháp nghiên cứu trải nghiệm người dùng.", Status = 1 },
                        
                        // HR Specialist (PathIdx 4)
                        new Resource { PathId = dbCareerPaths[4].Id, CategoryId = socCatId, SkillId = skills.Count > 13 ? skills[13].Id : (int?)null, ResourceType = "PDF", Title = "Modern Human Resources Management Guide", Url = "https://example.com/books/modern-hr-management.pdf", Description = "Cẩm nang quản trị nhân sự hiện đại.", Status = 1 },
                        new Resource { PathId = dbCareerPaths[4].Id, CategoryId = socCatId, ResourceType = "Doc", Title = "KPI & Performance Review Template", Url = "https://example.com/docs/kpi-performance-review", Description = "Mẫu đánh giá KPI nhân sự.", Status = 1 },
                        
                        // Business Manager (PathIdx 5)
                        new Resource { PathId = dbCareerPaths[5].Id, CategoryId = bizCatId, SkillId = skills.Count > 14 ? skills[14].Id : (int?)null, ResourceType = "Video", Title = "Strategic Thinking & Project Management", Url = "https://example.com/videos/strategic-thinking", Description = "Video tư duy chiến lược và quản lý dự án Agile.", Status = 1 },
                        new Resource { PathId = dbCareerPaths[5].Id, CategoryId = bizCatId, ResourceType = "PDF", Title = "Startup Business Model Planning Handbook", Url = "https://example.com/books/business-model-planning.pdf", Description = "Cẩm nang lập kế hoạch mô hình kinh doanh khởi nghiệp.", Status = 1 },
                        
                        // Accountant/Auditor (PathIdx 6)
                        new Resource { PathId = dbCareerPaths[6].Id, CategoryId = bizCatId, SkillId = skills.Count > 15 ? skills[15].Id : (int?)null, ResourceType = "Doc", Title = "Vietnamese Accounting Standards (VAS) Cheat Sheet", Url = "https://example.com/docs/vas-cheat-sheet", Description = "Bảng tóm tắt hệ thống tài khoản kế toán Việt Nam.", Status = 1 },
                        new Resource { PathId = dbCareerPaths[6].Id, CategoryId = bizCatId, SkillId = skills.Count > 16 ? skills[16].Id : (int?)null, ResourceType = "PDF", Title = "Corporate Taxation & Auditing Principles", Url = "https://example.com/books/corporate-taxation-principles.pdf", Description = "Tài liệu nguyên lý kế toán và thuế doanh nghiệp.", Status = 1 }
                    };
                    context.Resources.AddRange(resources);
                    await context.SaveChangesAsync();
                }
            }

            // 13. Seed Mentors (Users with Role "Mentor" and their MentorProfiles)
            var mentorEmail1 = "mentor1@careerpath.vn";
            var mentorUser1 = await userManager.FindByEmailAsync(mentorEmail1);
            if (mentorUser1 == null)
            {
                mentorUser1 = new User
                {
                    UserName = mentorEmail1,
                    Email = mentorEmail1,
                    FullName = "Trần Quốc Bảo",
                    Role = "Mentor",
                    EmailConfirmed = true,
                    Status = 1
                };
                var result = await userManager.CreateAsync(mentorUser1, "Mentor@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(mentorUser1, "Mentor");
                    
                    var profile = new MentorProfile
                    {
                        UserId = mentorUser1.Id,
                        JobTitle = "Software Architect",
                        Company = "TechCorp Global",
                        Specialization = "Phát triển phần mềm, C#, .NET Core, Cloud Computing, Clean Architecture",
                        Biography = "Hơn 10 năm kinh nghiệm thiết kế hệ thống lớn và phát triển ứng dụng Web. Từng dẫn dắt nhiều dự án chuyển đổi số quy mô lớn.",
                        Rating = 4.9m,
                        LinkedInUrl = "https://linkedin.com/in/quocbao-developer",
                        AvailabilityJson = "[\"Thứ 2 (19h - 21h)\", \"Thứ 7 (9h - 11h)\"]"
                    };
                    context.MentorProfiles.Add(profile);
                    await context.SaveChangesAsync();
                }
            }

            var mentorEmail2 = "mentor2@careerpath.vn";
            var mentorUser2 = await userManager.FindByEmailAsync(mentorEmail2);
            if (mentorUser2 == null)
            {
                mentorUser2 = new User
                {
                    UserName = mentorEmail2,
                    Email = mentorEmail2,
                    FullName = "Vũ Hoàng My",
                    Role = "Mentor",
                    EmailConfirmed = true,
                    Status = 1
                };
                var result = await userManager.CreateAsync(mentorUser2, "Mentor@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(mentorUser2, "Mentor");
                    
                    var profile = new MentorProfile
                    {
                        UserId = mentorUser2.Id,
                        JobTitle = "UI/UX Design Lead",
                        Company = "Creative Studio",
                        Specialization = "Thiết kế sản phẩm, Figma, User Research, Mobile App UX",
                        Biography = "Nhà thiết kế sản phẩm số đam mê tạo ra các trải nghiệm người dùng tinh tế, có kinh nghiệm làm việc với nhiều khách hàng đa quốc gia.",
                        Rating = 4.85m,
                        LinkedInUrl = "https://linkedin.com/in/hoangmy-design",
                        AvailabilityJson = "[\"Thứ 4 (14h - 16h)\", \"Chủ nhật (15h - 17h)\"]"
                    };
                    context.MentorProfiles.Add(profile);
                    await context.SaveChangesAsync();
                }
            }

            var mentorEmail3 = "mentor3@careerpath.vn";
            var mentorUser3 = await userManager.FindByEmailAsync(mentorEmail3);
            if (mentorUser3 == null)
            {
                mentorUser3 = new User
                {
                    UserName = mentorEmail3,
                    Email = mentorEmail3,
                    FullName = "Lâm Minh Hưng",
                    Role = "Mentor",
                    EmailConfirmed = true,
                    Status = 1
                };
                var result = await userManager.CreateAsync(mentorUser3, "Mentor@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(mentorUser3, "Mentor");
                    
                    var profile = new MentorProfile
                    {
                        UserId = mentorUser3.Id,
                        JobTitle = "Chief Accountant",
                        Company = "Finance Group Vina",
                        Specialization = "Kế toán tài chính, Thuế doanh nghiệp, Kiểm toán nội bộ",
                        Biography = "Chuyên gia tài chính với chứng chỉ CPA, hơn 8 năm quản lý hệ thống kế toán doanh nghiệp lớn.",
                        Rating = 4.95m,
                        LinkedInUrl = "https://linkedin.com/in/minhhung-cpa",
                        AvailabilityJson = "[\"Thứ 3 (20h - 22h)\", \"Thứ 5 (20h - 22h)\"]"
                    };
                    context.MentorProfiles.Add(profile);
                    await context.SaveChangesAsync();
                }
            }

            // 14. Seed JobPostings
            if (!await context.JobPostings.AnyAsync())
            {
                var dbCareerPaths = await context.CareerPaths.ToListAsync();
                var softwarePath = dbCareerPaths.FirstOrDefault(cp => cp.Title.Contains("Software Engineer"));
                var uiuxPath = dbCareerPaths.FirstOrDefault(cp => cp.Title.Contains("UI/UX Designer"));
                var accountantPath = dbCareerPaths.FirstOrDefault(cp => cp.Title.Contains("Accountant") || cp.Title.Contains("Kế toán"));

                var jobs = new List<JobPosting>
                {
                    new JobPosting
                    {
                        CareerPathId = softwarePath?.Id,
                        Title = "Junior .NET Developer (C#)",
                        CompanyName = "FPT Software",
                        JobType = "FullTime",
                        Location = "Hà Nội",
                        ExperienceLevel = "1-2 năm kinh nghiệm",
                        Salary = 15000000,
                        Description = "Tham gia phát triển các dự án Web Application lớn của khách hàng Nhật Bản và Âu Mỹ. Sử dụng C#, ASP.NET Core MVC, SQL Server.",
                        Status = 1
                    },
                    new JobPosting
                    {
                        CareerPathId = softwarePath?.Id,
                        Title = "Backend Engineer (ASP.NET Core)",
                        CompanyName = "VNG Corporation",
                        JobType = "FullTime",
                        Location = "Hồ Chí Minh",
                        ExperienceLevel = "2-3 năm kinh nghiệm",
                        Salary = 26000000,
                        Description = "Phát triển các API dịch vụ hiệu năng cao, tối ưu cơ sở dữ liệu lớn và triển khai sản phẩm phần mềm lên môi trường Kubernetes/Docker.",
                        Status = 1
                    },
                    new JobPosting
                    {
                        CareerPathId = uiuxPath?.Id,
                        Title = "UI/UX Designer (Figma)",
                        CompanyName = "Viettel Group",
                        JobType = "FullTime",
                        Location = "Hà Nội",
                        ExperienceLevel = "1-3 năm kinh nghiệm",
                        Salary = 18000000,
                        Description = "Thiết kế wireframe, mockup và prototype cho các sản phẩm ứng dụng di động và website của Tập đoàn. Đọc hiểu tài liệu UX Research.",
                        Status = 1
                    },
                    new JobPosting
                    {
                        CareerPathId = accountantPath?.Id,
                        Title = "Nhân viên Kế toán tổng hợp",
                        CompanyName = "VinGroup",
                        JobType = "FullTime",
                        Location = "Hà Nội",
                        ExperienceLevel = "1-2 năm kinh nghiệm",
                        Salary = 12000000,
                        Description = "Hỗ trợ ghi nhận doanh thu, chi phí, kiểm tra đối chiếu hóa đơn chứng từ và lập báo cáo thuế định kỳ cho ban giám đốc.",
                        Status = 1
                    }
                };

                context.JobPostings.AddRange(jobs);
                await context.SaveChangesAsync();
            }

            // 15. Seed PaymentHistories and Premium Users
            if (!await context.PaymentHistories.AnyAsync())
            {
                var studentsData = new List<(string Email, string Name)>
                {
                    ("student1@careerpath.vn", "Lê Minh Anh"),
                    ("student2@careerpath.vn", "Phạm Thảo Vy"),
                    ("student3@careerpath.vn", "Nguyễn Huy Hoàng"),
                    ("student4@careerpath.vn", "Đỗ Tuấn Kiệt")
                };

                var mockStudents = new List<User>();

                foreach (var s in studentsData)
                {
                    var user = await userManager.FindByEmailAsync(s.Email);
                    if (user == null)
                    {
                        user = new User
                        {
                            UserName = s.Email,
                            Email = s.Email,
                            FullName = s.Name,
                            Role = "Student",
                            EmailConfirmed = true,
                            Status = 1,
                            IsPremium = true
                        };
                        var result = await userManager.CreateAsync(user, "Student@123456");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "Student");
                            mockStudents.Add(user);
                        }
                    }
                    else
                    {
                        user.IsPremium = true;
                        await userManager.UpdateAsync(user);
                        mockStudents.Add(user);
                    }
                }

                // Make sure we have student@careerpath.vn in the list
                var baseStudent = await userManager.FindByEmailAsync("student@careerpath.vn");
                if (baseStudent != null)
                {
                    baseStudent.IsPremium = true;
                    await userManager.UpdateAsync(baseStudent);
                    mockStudents.Add(baseStudent);
                }

                if (mockStudents.Any())
                {
                    var random = new Random();
                    var payments = new List<PaymentHistory>();

                    // Generate payments for the last 6 months
                    for (int i = 0; i < 30; i++)
                    {
                        var date = DateTime.Now.AddDays(-random.Next(0, 180));
                        var selectedStudent = mockStudents[random.Next(mockStudents.Count)];
                        
                        payments.Add(new PaymentHistory
                        {
                            UserId = selectedStudent.Id,
                            PaypalOrderId = "MOCK-PAYPAL-" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper(),
                            Amount = 1.00m,
                            Currency = "USD",
                            PaymentStatus = "Completed",
                            CreatedAt = date
                        });
                    }

                    context.PaymentHistories.AddRange(payments);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}