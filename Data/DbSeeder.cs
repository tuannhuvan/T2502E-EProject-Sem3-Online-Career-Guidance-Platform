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

                // 6. Seed Base Test
                var baseTest = new Test
                {
                    Title = "Bài đánh giá định hướng nghề nghiệp nền tảng (Base Test)",
                    Description = "Khảo sát diện rộng 20 câu hỏi tổng quan giúp phân tích hành vi, tư duy chuyên môn nhằm đề xuất Lộ trình sự nghiệp (Career Path) thích hợp nhất."
                };
                context.Tests.Add(baseTest);
                await context.SaveChangesAsync();

                // --- SEEDING 20 QUESTIONS FOR BASE TEST ---
                var questionsData = new List<(string Content, List<string> Options, List<(int PathIdx, int W)> Weights)>
                {
                    // Q1
                    ("Bạn thích dành thời gian rảnh của mình để làm việc nào nhất?",
                     new List<string> { "Đọc tài liệu công nghệ hoặc tự mày mò viết code mẫu.", "Gặp gỡ, kết nối và mở rộng mối quan hệ xã hội.", "Vẽ tranh, thiết kế đồ họa hoặc sáng tạo tự do.", "Sắp xếp lại bảng chi tiêu, lên kế hoạch tuần mới cụ thể." },
                     new List<(int, int)> { (0, 5), (4, 5), (3, 5), (6, 5) }),

                    // Q2
                    ("Khi đối mặt với một thiết bị công nghệ mới bị hỏng, bạn sẽ làm gì?",
                     new List<string> { "Tự tháo linh kiện ra xem cấu trúc phần cứng bên trong để sửa.", "Lên mạng tìm các bài viết/video phân tích bản chất lỗi kỹ thuật.", "Tìm kiếm sự giúp đỡ từ những người xung quanh hoặc mang ra tiệm.", "Ghi chép lại lỗi này vào file log để theo dõi thiết bị định kỳ." },
                     new List<(int, int)> { (1, 5), (2, 5), (4, 4), (6, 4) }),

                    // Q3
                    ("Trong một cuộc thảo luận nhóm, vai trò tự nhiên của bạn thường là gì?",
                     new List<string> { "Đưa ra các ý tưởng cấu trúc, giải pháp đột phá, mới mẻ.", "Lắng nghe, hòa giải mâu thuẫn và kết nối các thành viên.", "Phân tích chuyên sâu tính khả thi và logic của các ý tưởng.", "Thúc giục tiến độ, quản lý thời gian và phân chia nhiệm vụ." },
                     new List<(int, int)> { (3, 5), (4, 5), (2, 5), (5, 5) }),

                    // Q4
                    ("Khi một người bạn đồng nghiệp đang gặp áp lực hoặc chuyện buồn, bạn sẽ:",
                     new List<string> { "Tìm các giải pháp thực tế giúp họ xử lý dứt điểm công việc đang nghẽn.", "Lắng nghe một cách đồng cảm, chia sẻ và khích lệ tinh thần.", "Rủ họ tham gia các buổi Workshop kết nối cộng đồng để giải tỏa.", "Giữ khoảng cách tôn trọng giúp họ có không gian riêng để tĩnh tâm." },
                     new List<(int, int)> { (0, 4), (4, 5), (4, 4), (6, 3) }),

                    // Q5
                    ("Bạn cảm thấy bị thu hút mạnh mẽ bởi môi trường làm việc nào?",
                     new List<string> { "Hạ tầng kỹ thuật phòng Lab hiện đại, quy trình làm việc chuẩn hóa.", "Không gian làm việc mở, tự do sáng tạo cao, linh hoạt thời gian.", "Môi trường sôi nổi, liên tục di chuyển gặp gỡ đối tác ngoại giao.", "Hệ thống văn phòng ổn định, phúc lợi rõ ràng, quản lý chặt chẽ." },
                     new List<(int, int)> { (1, 5), (3, 5), (5, 5), (6, 5) }),

                    // Q6
                    ("Khi phải xử lý các bảng số liệu phức tạp hoặc khối dữ liệu thô, bạn thấy:",
                     new List<string> { "Hào hứng tìm kiếm bản chất mô hình toán học ẩn sau chúng.", "Làm theo đúng biểu mẫu tài chính chuyên môn một cách cẩn thận.", "Nhanh chóng thiết kế các biểu đồ trực quan hóa màu sắc cho dễ hiểu.", "Cảm thấy nhàm chán và muốn ủy quyền cho người khác xử lý." },
                     new List<(int, int)> { (2, 5), (6, 5), (3, 4), (4, 3) }),

                    // Q7
                    ("Nếu được tài trợ để viết một cuốn sách, bạn sẽ viết về chủ đề nào?",
                     new List<string> { "Chiến lược quản trị và dẫn dắt doanh nghiệp khởi nghiệp.", "Cẩm nang hướng dẫn chuyên sâu về mỹ thuật số nghệ thuật.", "Nghiên cứu cấu trúc tâm lý học và cách quản trị nhân sự.", "Các thuật toán thông minh cải tiến hệ thống cơ sở dữ liệu." },
                     new List<(int, int)> { (5, 5), (3, 5), (4, 5), (0, 5) }),

                    // Q8
                    ("Cách bạn đang tự quản lý tài chính cá nhân hàng tháng là gì?",
                     new List<string> { "Sử dụng ứng dụng/Excel ghi chép nghiêm túc không sót một đồng.", "Chi tiêu linh hoạt theo cảm xúc, ước lượng sơ bộ trong đầu.", "Chia nhỏ ngân sách vào các quỹ: Tiết kiệm, Đầu tư, Nâng cấp bản thân.", "Tìm cách tối ưu hóa, đầu tư sinh lời từ các khoản tiền nhàn rỗi." },
                     new List<(int, int)> { (6, 5), (3, 3), (6, 4), (5, 5) }),

                    // Q9
                    ("Khi bất chợt nảy ra một ý tưởng kinh doanh mới, bạn làm gì trước tiên?",
                     new List<string> { "Phân tích, xây dựng bản kế hoạch chi tiết và khảo sát thị trường.", "Liên hệ ngay với bạn bè có thế mạnh chuyên môn khác để lập Team.", "Tìm một Mentor có uy tín lớn trong ngành để xin ý kiến tư vấn.", "Lập tức xây dựng thử nghiệm sản phẩm phiên bản Demo (MVP)." },
                     new List<(int, int)> { (5, 5), (5, 4), (4, 4), (0, 5) }),

                    // Q10
                    ("Bạn đánh giá khả năng xử lý công việc dồn dập (áp lực Deadline lớn) của mình thế nào?",
                     new List<string> { "Rất tốt, áp lực công nghệ kích thích sự tập trung giải bài toán khó của tôi.", "Tốt, tôi sẽ thiết lập kế hoạch chia nhỏ công việc, sắp xếp độ ưu tiên rõ ràng.", "Dễ bị áp lực tâm lý, tôi cần sự động viên hoặc chỉ dẫn từ đồng nghiệp.", "Tôi thích làm việc tuần tự và sẽ chủ động đàm phán kéo dài thời gian." },
                     new List<(int, int)> { (1, 4), (5, 5), (4, 4), (6, 4) }),

                    // Q11
                    ("Khi truy cập vào một trang web mới, yếu tố nào tác động tới bạn đầu tiên?",
                     new List<string> { "Hệ màu sắc bố cục, độ mượt mà của trải nghiệm UI/UX.", "Tốc độ tải trang, kiến trúc tính năng hệ thống thông minh.", "Độ chính xác logic của nội dung truyền tải và thông tin liên hệ.", "Mô hình chuyển đổi kinh doanh, cách thức tối ưu doanh thu của trang." },
                     new List<(int, int)> { (3, 5), (0, 5), (2, 4), (5, 4) }),

                    // Q12
                    ("Bạn mong muốn đối tượng tương tác chính trong công việc hàng ngày của mình là gì?",
                     new List<string> { "Hệ thống câu lệnh mã code, máy móc cấu hình mạng hoặc thuật toán.", "Con người (Học viên, ứng viên, đối tác chiến lược hoặc khách hàng).", "Sản phẩm sáng tạo nghệ thuật, hình ảnh truyền thông, giao diện số.", "Hồ sơ chứng từ tài chính, quy trình kiểm toán, văn bản hành chính." },
                     new List<(int, int)> { (0, 5), (4, 5), (3, 5), (6, 5) }),

                    // Q13
                    ("Khi xảy ra xung đột nghiêm trọng về quan điểm kỹ thuật trong nội bộ dự án, bạn sẽ:",
                     new List<string> { "Đưa ra các dẫn chứng số liệu, logic thực tế để bảo vệ luận điểm chuyên môn.", "Lắng nghe các bên, tìm giải pháp dung hòa tinh thần đồng đội.", "Đề xuất một cuộc họp lấy biểu quyết đồng thuận theo đa số hành chính.", "Mời chuyên gia/Mentor có kinh nghiệm dày dặn đứng ra phân xử khách quan." },
                     new List<(int, int)> { (2, 5), (4, 5), (6, 4), (5, 4) }),

                    // Q14
                    ("Bạn tự tin nhất vào siêu năng lực bẩm sinh nào của bản thân?",
                     new List<string> { "Tư duy logic, giải quyết các bài toán chuỗi dữ liệu hóc búa.", "Giao tiếp thuyết phục, tạo tầm ảnh hưởng dẫn dắt đám đông.", "Sự tỉ mỉ, quan sát chi tiết siêu nhỏ, kiểm lỗi dữ liệu chính xác.", "Thích ứng công nghệ cực nhanh, đổi mới tư duy linh hoạt." },
                     new List<(int, int)> { (0, 5), (5, 5), (6, 5), (1, 5) }),

                    // Q15
                    ("Khi cấp trên giao phó một nhiệm vụ công nghệ hoàn toàn mới chưa từng có tài liệu hướng dẫn:",
                     new List<string> { "Rất hào hứng, tự tra cứu tài liệu quốc tế chuyên sâu để xử lý.", "Cần có một Mentor giàu kinh nghiệm cầm tay chỉ việc giai đoạn đầu.", "Tìm kiếm các biểu mẫu (Template) tương đồng để áp dụng có quy trình.", "Đề xuất chuyển giao nhiệm vụ cho người có chuyên môn phù hợp hơn." },
                     new List<(int, int)> { (0, 5), (4, 4), (6, 4), (5, 3) }),

                    // Q16
                    ("Đâu là động lực cốt lõi lớn nhất thúc đẩy bạn làm việc mỗi ngày?",
                     new List<string> { "Mức thu nhập cao, cơ hội thăng tiến lên cấp quản lý điều hành.", "Được tự do sáng tạo nghệ thuật, không chịu sự gò bó hành chính.", "Mang lại giá trị lớn cho xã hội, hỗ trợ phát triển năng lực con người.", "Sự ổn định lâu dài, môi trường làm việc ít biến động rủi ro tài chính." },
                     new List<(int, int)> { (5, 5), (3, 5), (4, 5), (6, 5) }),

                    // Q17
                    ("Bạn có xu hướng đưa ra quyết định cuối cùng dựa trên nền tảng nào?",
                     new List<string> { "Số liệu thống kê thực tế, chứng cứ khoa học kiểm định rõ ràng.", "Trực giác nhạy bén kết hợp cảm xúc nghệ thuật tại thời điểm đó.", "Sự thống nhất, đồng lòng của toàn bộ thành viên trong tập thể.", "Các hướng dẫn của chuyên gia đầu ngành hoặc quy chuẩn pháp lý có sẵn." },
                     new List<(int, int)> { (2, 5), (3, 5), (4, 4), (6, 4) }),

                    // Q18
                    ("Khi tham gia setup một không gian làm việc mới cho Team, bạn ưu tiên tiêu chí nào?",
                     new List<string> { "Tối ưu hóa công năng kỹ thuật, đường truyền mạng tốc độ cao ổn định.", "Thiết kế độc đáo mang tính nghệ thuật cao, khơi nguồn cảm hứng sáng tạo.", "Gần gũi thiên nhiên, có khu vực kết nối trò chuyện mở giữa các thành viên.", "Tiết kiệm chi phí đầu tư vật liệu, tối ưu hóa ngân sách quản lý." },
                     new List<(int, int)> { (1, 5), (3, 5), (4, 5), (6, 5) }),

                    // Q19
                    ("Tần suất bạn chủ động tự học để cập nhật các kiến thức công nghệ/xu hướng mới là:",
                     new List<string> { "Mỗi ngày, tôi liên tục đọc báo cáo thị trường chuyên ngành quốc tế.", "Vài lần một tuần khi bắt gặp các nguồn bài viết phân tích uy tín.", "Chỉ học tập khi hệ thống công việc bắt buộc hoặc có kỳ kiểm tra định kỳ.", "Rất ít khi, tôi ưu tiên tối ưu hóa tốt các kỹ năng hiện có của bản thân." },
                     new List<(int, int)> { (0, 5), (2, 4), (6, 4), (5, 3) }),

                    // Q20
                    ("Đối với bạn, một người Mentor đồng hành lý tưởng nhất thiết phải có tố chất nào?",
                     new List<string> { "Kỹ năng chuyên môn kỹ thuật thượng thừa, giải quyết lỗi hệ thống cực nhanh.", "Định hướng tầm nhìn chiến lược phát triển sự nghiệp dài hạn.", "Biết lắng nghe tâm tư, truyền cảm hứng bứt phá năng lực.", "Nghiêm khắc, quản lý tiến độ học tập một cách quy chuẩn kỷ luật." },
                     new List<(int, int)> { (0, 5), (5, 5), (4, 5), (6, 5) })
                };

                foreach (var qData in questionsData)
                {
                    var question = new QuestionTest
                    {
                        TestId = baseTest.Id,
                        QuestionTypeId = singleChoiceType.Id,
                        Content = qData.Content
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
        }
    }
}