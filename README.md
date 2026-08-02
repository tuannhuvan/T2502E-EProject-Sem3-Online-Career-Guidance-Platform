# Career Guidance Platform (Nền Tảng Hướng Nghiệp & Lập Lộ Trình Học Tập)

Career Guidance Platform là hệ thống toàn diện hỗ trợ sinh viên và người dùng định hướng nghề nghiệp, làm các bài đánh giá năng lực, lập lộ trình học tập chi tiết, kết nối với cố vấn (Mentor), tìm kiếm việc làm và tự động tạo CV dựa trên các kỹ năng đã tích lũy.

---

## 🌟 Tính Năng Cốt Lõi

1. **Trắc Nghiệm Định Hướng (Career Tests)**
   - Cung cấp các bài đánh giá tính cách và sở thích nghề nghiệp (như Holland, MBTI).
   - Tự động chấm điểm và gợi ý lộ trình nghề nghiệp (Career Path) phù hợp dựa trên kết quả trắc nghiệm.

2. **Bản Đồ Lộ Trình Học Tập (Roadmaps & Career Paths)**
   - Hiển thị thông tin chi tiết về từng ngành nghề, mức lương trung bình, và cơ hội phát triển.
   - Lộ trình học được chia thành các giai đoạn (Stages) từ cơ bản đến nâng cao với danh sách khóa học đề xuất chi tiết.

3. **Thiết Lập Mục Tiêu & Kế Hoạch (Goal & Milestone Tracking)**
   - Cho phép người dùng đặt mục tiêu học tập cụ thể theo lộ trình đề xuất.
   - Theo dõi tiến độ hoàn thành các kỹ năng và cột mốc phát triển.

4. **Kết Nối Cố Vấn (Mentorship)**
   - Tìm kiếm, kết nối và đặt lịch hẹn tư vấn (1:1 hoặc nhóm) với các Mentor có kinh nghiệm.
   - Nhắn tin trực tiếp giữa Học viên và Mentor. Đánh giá chất lượng tư vấn của Mentor.

5. **Thiết Kế CV Tự Động (Resume Builder)**
   - Đồng bộ hóa các kỹ năng đã hoàn thành từ lộ trình học tập để đưa vào CV.
   - Xuất CV theo các mẫu template chuyên nghiệp được cấu hình bởi Admin.

6. **Tuyển Dụng & Việc Làm (Job Board)**
   - Hiển thị danh sách việc làm từ các doanh nghiệp đối tác tương ứng với từng lộ trình học.
   - Ứng tuyển trực tiếp và lưu các công việc quan tâm.

7. **Tài Khoản Premium (VIP Membership)**
   - Thanh toán nâng cấp tài khoản qua PayPal Sandbox.
   - Mở khóa toàn bộ tài liệu học tập nâng cao, không giới hạn lượt làm bài test và tương tác không giới hạn với Mentor.

---

## 💻 Công Nghệ Sử Dụng

- **Framework Chính**: ASP.NET Core 8.0 MVC (C#)
- **Database ORM**: Entity Framework Core 8.0, Pomelo MySQL Provider
- **Hệ Quản Trị CSDL**: MySQL, Azure
- **Frontend**: Razor Pages/Views, HTML5, CSS Vanilla, JavaScript, Bootstrap Icons
- **Tích Hợp Dịch Vụ**:
  - **Cloudinary**: Quản lý hình ảnh và tải lên ảnh đại diện (avatar).
  - **Gmail SMTP**: Gửi thư xác thực tài khoản, khôi phục mật khẩu.
  - **PayPal Sandbox REST API**: Tích hợp cổng thanh toán trực tuyến nâng cấp Premium.
  - **SignalR / Hubs**: Real-time thông báo và trạng thái hoạt động.

---

## 📂 Cấu Trúc Thư Mục Dự Án

```text
T2502E_EProject_SEM3/
│
├── Career_Guidance_Platform/               # Thư mục mã nguồn chính (ASP.NET Core Web App)
│   ├── Areas/                              # Phân khu chức năng
│   │   ├── Admin/                          # Trang quản trị hệ thống
│   │   └── Mentor/                         # Trang dành riêng cho Cố vấn
│   ├── Controllers/                        # Bộ điều hướng xử lý logic nghiệp vụ
│   ├── Data/                               # AppDbContext và cấu hình dữ liệu
│   ├── Dtos/                               # Data Transfer Objects cho việc truyền nhận dữ liệu
│   ├── Filters/                            # Các bộ lọc phân quyền (ví dụ: PremiumAccessFilter)
│   ├── Hubs/                               # SignalR Hubs phục vụ cập nhật thời gian thực
│   ├── Models/                             # Các thực thể cơ sở dữ liệu (Entities)
│   ├── Repository/                         # Lớp tương tác CSDL trực tiếp (Repositories)
│   ├── Service/                            # Lớp chứa logic nghiệp vụ chính (Services)
│   ├── Views/                              # Các trang giao diện Razor (HTML/CSS/JS)
│   ├── wwwroot/                            # Tài nguyên tĩnh (CSS, JS, hình ảnh, thư viện client-side)
│   ├── appsettings.json                    # Tệp cấu hình môi trường, DB, API Keys
│   └── Program.cs                          # File khởi chạy ứng dụng & cấu hình Services
│
├── Career_Guidance_Platform.Tests/         # Thư mục chứa các Unit Tests (xUnit, Moq)
│   └── UnitTest1.cs                        # Các kịch bản test chức năng cốt lõi
│
└── T2502E_EProject_SEM3.sln                # File Solution của dự án
```

---

## ⚙️ Hướng Dẫn Cài Đặt Cục Bộ (Local Setup)

### Yêu Cầu Hệ Thống
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên
- [MySQL Server 8.x](https://dev.mysql.com/downloads/mysql/)

### Các Bước Cài Đặt
1. **Clone dự án về máy**:
   ```bash
   git clone <URL_REPRESENTING_YOUR_REPO>
   cd T2502E_EProject_SEM3
   ```

2. **Cấu hình Cơ sở dữ liệu**:
   Mở tệp `Career_Guidance_Platform/appsettings.json` và điều chỉnh chuỗi kết nối MySQL cho phù hợp với máy của bạn:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "server=localhost;port=3306;database=careerguidancedb;user=root;password=YourPasswordHere"
   }
   ```

3. **Cấu hình API Keys (Tùy chọn)**:
   Để các chức năng gửi mail, upload ảnh và thanh toán hoạt động đầy đủ, cập nhật thông tin tương ứng trong `appsettings.json`:
   - `CloudinarySettings`: Các thông số tài khoản Cloudinary.
   - `EmailSettings`: SMTP Server, cổng và tài khoản Email gửi đi (cần dùng App Password nếu sử dụng Gmail).
   - `PayPal`: Điền `ClientId` và `Secret` lấy từ tài khoản Developer PayPal.

4. **Khởi chạy ứng dụng**:
   Khi chạy lần đầu, ứng dụng sẽ tự động áp dụng các Migration và tạo các bảng cần thiết kèm dữ liệu mẫu mẫu (Seeder).
   ```bash
   cd Career_Guidance_Platform
   dotnet run
   ```
   Ứng dụng sẽ chạy tại địa chỉ mặc định: `http://localhost:5088`

5. **Chạy Unit Tests**:
   Để chạy bộ kiểm thử tự động của dự án:
   ```bash
   cd ..
   dotnet test
   ```

---

## ☁️ Hướng Dẫn Triển Khai Lên Azure (Azure Deployment Guide)

Hệ thống có thể được triển khai dễ dàng lên nền tảng đám mây Microsoft Azure theo các bước sau:

### Bước 1: Chuẩn Bị Cơ Sở Dữ Liệu MySQL Trên Azure
1. Truy cập Azure Portal, tạo dịch vụ **Azure Database for MySQL flexible server**.
2. Cấu hình quy tắc tường lửa (Firewall Rules) để cho phép các dịch vụ Azure truy cập (`Allow public access from any Azure service within Azure to this server`).
3. Lấy chuỗi kết nối (Connection String) định dạng:
   ```text
   server=your-azure-mysql.mysql.database.azure.com;port=3306;database=careerguidancedb;user=yourusername;password=yourpassword;SSL Mode=Required;
   ```

### Bước 2: Tạo Azure App Service
1. Trên Azure Portal, tạo một **Web App** mới.
2. Chọn cấu hình:
   - **Publish**: Code
   - **Runtime stack**: .NET 8 (LTS)
   - **Operating System**: Linux (khuyên dùng để tiết kiệm chi phí) hoặc Windows.
3. Chọn Service Plan phù hợp với nhu cầu.

### Bước 3: Cấu Hình Biến Môi Trường (App Settings)
Thay vì lưu thông tin nhạy cảm như Client Secret hay Connection String trong code, cấu hình chúng trong mục **Settings > Configuration** của Azure App Service:
- `ConnectionStrings__DefaultConnection` = *Chuỗi kết nối Azure MySQL ở Bước 1*
- `PayPal__ClientId` = *PayPal Client ID*
- `PayPal__Secret` = *PayPal Client Secret*
- `CloudinarySettings__ApiKey` = *Cloudinary Api Key*
- `CloudinarySettings__ApiSecret` = *Cloudinary Api Secret*
- `EmailSettings__Password` = *App Password của Gmail*

### Bước 4: Triển Khai Code (Deploy)
Bạn có thể deploy qua các cách sau:
- **Deploy bằng Git / GitHub Actions**: Cấu hình Continuous Deployment (CD) liên kết trực tiếp với nhánh `main` của kho lưu trữ GitHub của bạn. Azure sẽ tự động sinh file workflow để build và deploy mỗi khi có code mới.
- **Deploy trực tiếp từ IDE (Rider / Visual Studio)**:
  1. Click chuột phải vào Project `Career_Guidance_Platform` -> Chọn **Publish**.
  2. Chọn Target là **Azure** -> **Azure App Service (Linux/Windows)**.
  3. Đăng nhập tài khoản Azure của bạn, chọn Web App đã tạo và nhấn **Publish**.

### Bước 5: Cấu Hình Tên Miền Riêng (Custom Domain)
1. Trong trang Web App của bạn trên Azure, chọn mục **Settings > Custom domains**.
2. Nhấp vào **Add custom domain**.
3. Cấu hình các bản ghi **CNAME** và **TXT** (hoặc A record) tại nhà cung cấp tên miền của bạn để trỏ về địa chỉ `<app-name>.azurewebsites.net` theo hướng dẫn hiển thị trên màn hình.
4. Sau khi xác thực thành công, nhấn **Add**.
5. Nhấp vào **Add binding** để cấu hình chứng chỉ bảo mật SSL miễn phí (App Service Managed Certificate) giúp trang chạy dưới giao thức an toàn `https://`.
