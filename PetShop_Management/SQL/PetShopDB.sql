-- 1. Bảng Tài khoản (Đăng nhập, Phân quyền)
CREATE TABLE Accounts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) DEFAULT 'Staff', -- 'Admin' hoặc 'Staff'
    IsActive BIT DEFAULT 1
);

-- 2. Bảng Khách hàng
CREATE TABLE Customer (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Phone VARCHAR(15) UNIQUE NULL,
    Address NVARCHAR(255) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0 -- 0: Đang hoạt động, 1: Đã xóa mềm
);

-- 3. Bảng Loại thú cưng
CREATE TABLE PetType (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(50) NOT NULL
);

-- 4. Bảng Thú cưng
CREATE TABLE Pet (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    PetTypeId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Weight FLOAT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_Pet_Customer FOREIGN KEY (CustomerId) REFERENCES Customer(Id),
    CONSTRAINT FK_Pet_PetType FOREIGN KEY (PetTypeId) REFERENCES PetType(Id)
);

-- 5. Bảng Dịch vụ
CREATE TABLE Service (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(18, 0) NOT NULL,
    IsDeleted BIT DEFAULT 0
);

-- 6. Bảng Sản phẩm
CREATE TABLE Product (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NULL,
    Price DECIMAL(18, 0) NOT NULL,
    Stock INT DEFAULT 0,
    IsDeleted BIT DEFAULT 0
);

-- 7. Bảng Đặt lịch
CREATE TABLE Booking (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    PetId INT NOT NULL,
    BookingDate DATETIME2 DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT N'Chờ xử lý', 
    PaymentStatus NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    PaymentMethod NVARCHAR(50) DEFAULT N'Tiền mặt',
    TotalAmount DECIMAL(18, 0) DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Booking_Customer FOREIGN KEY (CustomerId) REFERENCES Customer(Id),
    CONSTRAINT FK_Booking_Pet FOREIGN KEY (PetId) REFERENCES Pet(Id)
);

-- 8. Bảng Chi tiết hóa đơn đặt lịch
CREATE TABLE BookingDetail (
    BookingId INT NOT NULL,
    ServiceId INT NOT NULL,
    PriceAtBooking DECIMAL(18, 0) NOT NULL,
    PRIMARY KEY (BookingId, ServiceId),
    CONSTRAINT FK_BookingDetail_Booking FOREIGN KEY (BookingId) REFERENCES Booking(Id),
    CONSTRAINT FK_BookingDetail_Service FOREIGN KEY (ServiceId) REFERENCES Service(Id)
);

-- 9. Bảng Hóa đơn bán lẻ (POS)
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    OrderDate DATETIME2 DEFAULT GETDATE(),
    TotalAmount DECIMAL(18, 0) DEFAULT 0,
    PaymentStatus NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    PaymentMethod NVARCHAR(50) DEFAULT N'Tiền mặt',
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Orders_Customer FOREIGN KEY (CustomerId) REFERENCES Customer(Id)
);

-- 10. Bảng Chi tiết hóa đơn bán lẻ
CREATE TABLE OrderDetails (
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPriceAtPurchase DECIMAL(18, 0) NOT NULL,
    PRIMARY KEY (OrderId, ProductId),
    CONSTRAINT FK_OrderDetails_Order FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT FK_OrderDetails_Product FOREIGN KEY (ProductId) REFERENCES Product(Id)
);

-- Chèn thêm tài khoản ADMIN để đăng nhập
INSERT INTO Accounts (Username, PasswordHash, FullName, Role, IsActive) 
VALUES ('admin', '123456', N'Quản trị viên', 'Admin', 1);