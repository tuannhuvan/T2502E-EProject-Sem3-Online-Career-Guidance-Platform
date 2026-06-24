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

            var existingQuestionsCount = await context.QuestionTests.CountAsync(q => q.TestId == baseTest.Id);
            if (existingQuestionsCount != 20)
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
            if (!await context.Resources.AnyAsync())
            {
                var dbCareerPaths = await context.CareerPaths.OrderBy(c => c.Id).ToListAsync();
                if (dbCareerPaths.Count >= 7)
                {
                    var resources = new List<Resource>
                    {
                        // Software Engineer (PathIdx 0)
                        new Resource { PathId = dbCareerPaths[0].Id, ResourceType = "PDF", Title = "C# & .NET Core Developer Handbook", Url = "https://example.com/books/csharp-handbook.pdf" },
                        new Resource { PathId = dbCareerPaths[0].Id, ResourceType = "Video", Title = "Mastering Clean Architecture in ASP.NET Core", Url = "https://example.com/videos/clean-architecture" },
                        // Network & Security (PathIdx 1)
                        new Resource { PathId = dbCareerPaths[1].Id, ResourceType = "Doc", Title = "Cisco CCNA Networking Lab Guide", Url = "https://example.com/docs/ccna-networking-guide" },
                        new Resource { PathId = dbCareerPaths[1].Id, ResourceType = "PDF", Title = "Introduction to Cyber Security Standards 2026", Url = "https://example.com/books/cybersecurity-standards.pdf" },
                        // Data Scientist (PathIdx 2)
                        new Resource { PathId = dbCareerPaths[2].Id, ResourceType = "Video", Title = "Python for Data Analysis and Visualization", Url = "https://example.com/videos/python-data-science" },
                        new Resource { PathId = dbCareerPaths[2].Id, ResourceType = "PDF", Title = "Practical Machine Learning Guide", Url = "https://example.com/books/machine-learning-guide.pdf" },
                        // UI/UX Designer (PathIdx 3)
                        new Resource { PathId = dbCareerPaths[3].Id, ResourceType = "Doc", Title = "Figma Design System Component Standards", Url = "https://example.com/docs/figma-design-system" },
                        new Resource { PathId = dbCareerPaths[3].Id, ResourceType = "Video", Title = "User Experience (UX) Research Methodology", Url = "https://example.com/videos/ux-research" },
                        // HR Specialist (PathIdx 4)
                        new Resource { PathId = dbCareerPaths[4].Id, ResourceType = "PDF", Title = "Modern Human Resources Management Guide", Url = "https://example.com/books/modern-hr-management.pdf" },
                        new Resource { PathId = dbCareerPaths[4].Id, ResourceType = "Doc", Title = "KPI & Performance Review Template", Url = "https://example.com/docs/kpi-performance-review" },
                        // Business Manager (PathIdx 5)
                        new Resource { PathId = dbCareerPaths[5].Id, ResourceType = "Video", Title = "Strategic Thinking & Project Management", Url = "https://example.com/videos/strategic-thinking" },
                        new Resource { PathId = dbCareerPaths[5].Id, ResourceType = "PDF", Title = "Startup Business Model Planning Handbook", Url = "https://example.com/books/business-model-planning.pdf" },
                        // Accountant/Auditor (PathIdx 6)
                        new Resource { PathId = dbCareerPaths[6].Id, ResourceType = "Doc", Title = "Vietnamese Accounting Standards (VAS) Cheat Sheet", Url = "https://example.com/docs/vas-cheat-sheet" },
                        new Resource { PathId = dbCareerPaths[6].Id, ResourceType = "PDF", Title = "Corporate Taxation & Auditing Principles", Url = "https://example.com/books/corporate-taxation-principles.pdf" }
                    };
                    context.Resources.AddRange(resources);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}