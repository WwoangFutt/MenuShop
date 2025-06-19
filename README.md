# MenuShop - Hệ thống quản lý menu và sản phẩm

## Mô tả
MenuShop là một ứng dụng web được xây dựng bằng ASP.NET Core MVC với giao diện responsive, hỗ trợ hiển thị tốt trên cả máy tính và điện thoại di động.

## Tính năng chính

### Cho người dùng:
- **Trang chủ**: Hiển thị danh sách sản phẩm với tìm kiếm và lọc theo danh mục
- **Danh mục sản phẩm**: Xem sản phẩm theo từng danh mục
- **Chi tiết sản phẩm**: Xem thông tin chi tiết, hình ảnh và video sản phẩm
- **Tìm kiếm**: Tìm kiếm sản phẩm theo tên hoặc mô tả
- **Giao diện responsive**: Tối ưu cho điện thoại di động

### Cho quản trị viên:
- **Đăng nhập**: Hệ thống xác thực với tài khoản admin
- **Quản lý danh mục**: Thêm, sửa, xóa danh mục sản phẩm
- **Quản lý sản phẩm**: Thêm, sửa, xóa sản phẩm
- **Upload media**: Hỗ trợ upload hình ảnh và video từ thư viện điện thoại
- **Bảo mật**: Chỉ admin mới có thể truy cập trang quản trị

## Công nghệ sử dụng

- **Backend**: ASP.NET Core 9.0 MVC
- **Database**: SQL Server LocalDB với Entity Framework Core
- **Frontend**: Bootstrap 5, Font Awesome, HTML5, CSS3, JavaScript
- **Authentication**: Cookie-based authentication
- **File Upload**: Hỗ trợ upload hình ảnh và video

## Cài đặt và chạy ứng dụng

### Yêu cầu hệ thống:
- .NET 9.0 SDK
- SQL Server LocalDB (được cài đặt với Visual Studio)

### Các bước cài đặt:

1. **Clone repository:**
   ```bash
   git clone <repository-url>
   cd MenuShop
   ```

2. **Khôi phục packages:**
   ```bash
   dotnet restore
   ```

3. **Tạo database:**
   ```bash
   dotnet ef database update
   ```

4. **Chạy ứng dụng:**
   ```bash
   dotnet run
   ```

5. **Truy cập ứng dụng:**
   - Mở trình duyệt và truy cập: `https://localhost:7001` hoặc `http://localhost:5000`

## Tài khoản mặc định

- **Username**: admin
- **Password**: admin123

## Cấu trúc dự án

```
MenuShop/
├── Controllers/          # Controllers xử lý logic
│   ├── HomeController.cs
│   ├── AccountController.cs
│   └── AdminController.cs
├── Models/              # Models dữ liệu
│   ├── Category.cs
│   ├── Product.cs
│   └── User.cs
├── Views/               # Views giao diện
│   ├── Home/
│   ├── Account/
│   ├── Admin/
│   └── Shared/
├── Data/                # Database context
│   └── ApplicationDbContext.cs
├── wwwroot/             # Static files
│   ├── uploads/         # Thư mục lưu file upload
│   ├── css/
│   └── js/
└── Program.cs           # Entry point
```

## Tính năng responsive

Ứng dụng được thiết kế responsive với Bootstrap 5:
- **Mobile-first**: Tối ưu cho điện thoại di động
- **Grid system**: Tự động điều chỉnh layout theo kích thước màn hình
- **Touch-friendly**: Các nút và form dễ sử dụng trên màn hình cảm ứng
- **Upload từ điện thoại**: Hỗ trợ chọn file từ thư viện điện thoại

## Upload file

### Hình ảnh:
- Định dạng: JPG, PNG, GIF, WebP
- Kích thước tối đa: Không giới hạn (có thể cấu hình)
- Lưu trữ: `/wwwroot/uploads/categories/` và `/wwwroot/uploads/products/`

### Video:
- Định dạng: MP4, WebM, OGV
- Hỗ trợ: HTML5 video player
- Lưu trữ: `/wwwroot/uploads/products/`

## Bảo mật

- **Authentication**: Cookie-based với session timeout
- **Authorization**: Chỉ admin mới truy cập được trang quản trị
- **File upload**: Validate file type và size
- **SQL Injection**: Sử dụng Entity Framework để tránh SQL injection
- **XSS Protection**: ASP.NET Core built-in protection

## Phát triển thêm

### Thêm tính năng mới:
1. Tạo model trong thư mục `Models/`
2. Thêm DbSet trong `ApplicationDbContext.cs`
3. Tạo migration: `dotnet ef migrations add MigrationName`
4. Cập nhật database: `dotnet ef database update`
5. Tạo controller và views tương ứng

### Tùy chỉnh giao diện:
- CSS: Chỉnh sửa file `wwwroot/css/site.css`
- Layout: Chỉnh sửa `Views/Shared/_Layout.cshtml`
- Bootstrap theme: Có thể thay đổi theme Bootstrap

## Hỗ trợ

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra logs trong console
2. Đảm bảo SQL Server LocalDB đang chạy
3. Kiểm tra quyền truy cập thư mục `wwwroot/uploads/`

## License

Dự án này được phát triển cho mục đích học tập và demo. 