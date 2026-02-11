-- Tạo Database
CREATE DATABASE PetShopDB;
GO
USE PetShopDB;
GO

-- 1. Bảng Khách hàng (Customer)
CREATE TABLE Customer (
    Id INT PRIMARY KEY IDENTITY(1,1), -- Tự tăng 1, 2, 3...
    Name NVARCHAR(100) NOT NULL,
    Phone VARCHAR(15) UNIQUE,         -- SĐT không được trùng
    Address NVARCHAR(255)
);

-- 2. Bảng Loại thú cưng (PetType) - Để chuẩn hóa dữ liệu
-- Thay vì lưu chữ "Chó", "Mèo" lặp lại, ta lưu ID.
CREATE TABLE PetType (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(50) NOT NULL -- Ví dụ: Chó, Mèo, Hamster
);

-- 3. Bảng Thú cưng (Pet)
CREATE TABLE Pet (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,          -- Khóa ngoại trỏ về Customer
    PetTypeId INT NOT NULL,           -- Khóa ngoại trỏ về PetType
    Name NVARCHAR(100) NOT NULL,
    Weight FLOAT,
    FOREIGN KEY (CustomerId) REFERENCES Customer(Id) ON DELETE CASCADE,
    FOREIGN KEY (PetTypeId) REFERENCES PetType(Id)
);

-- 4. Bảng Dịch vụ (Service)
CREATE TABLE Service (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(18, 0) NOT NULL     -- Dùng Decimal cho tiền tệ chính xác hơn Double
);

-- 5. Bảng Sản phẩm (Product) - Quản lý kho
CREATE TABLE Product (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50),
    Price DECIMAL(18, 0) NOT NULL,
    Stock INT DEFAULT 0               -- Tồn kho
);

-- 6. Bảng Đặt lịch (Booking) - Đơn hàng tổng
CREATE TABLE Booking (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    PetId INT NOT NULL,
    BookingDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT N'Chờ xử lý', -- Có thể tách bảng Status riêng nếu muốn chuẩn hóa cao hơn
    TotalAmount DECIMAL(18, 0) DEFAULT 0,     -- Tổng tiền của lịch này
    FOREIGN KEY (CustomerId) REFERENCES Customer(Id),
    FOREIGN KEY (PetId) REFERENCES Pet(Id) -- Không dùng Cascade để giữ lịch sử nếu xóa Pet
);

-- 7. Bảng Chi tiết đặt lịch (BookingDetail) - QUAN TRỌNG
-- Giải quyết vấn đề: 1 Lịch làm nhiều Dịch vụ
CREATE TABLE BookingDetail (
    BookingId INT NOT NULL,
    ServiceId INT NOT NULL,
    PriceAtBooking DECIMAL(18, 0), -- Lưu giá tại thời điểm đặt (đề phòng sau này tăng giá)
    PRIMARY KEY (BookingId, ServiceId),
    FOREIGN KEY (BookingId) REFERENCES Booking(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Service(Id)
);
INSERT INTO Customer (Name, Phone, Address) VALUES 
(N'Nguyễn Văn A', '0988123456', N'Hà Nội'),
(N'Trần Thị B', '0912345678', N'Đà Nẵng');

INSERT INTO PetType (TypeName) VALUES (N'Chó'), (N'Mèo');

INSERT INTO Pet (CustomerId, PetTypeId, Name, Weight) VALUES 
(1, 1, N'Lu', 5.5), 
(2, 2, N'Misa', 3.0);

INSERT INTO Service (Name, Price) VALUES (N'Tắm gội', 100000), (N'Cắt tỉa', 150000);