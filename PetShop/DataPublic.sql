-- 7. SERVICE
INSERT INTO SERVICE (service_name, estimated_duration, current_price) VALUES 
(N'Tắm sấy cơ bản', 60, 150000), (N'Cắt tỉa tạo kiểu', 120, 300000), (N'Vệ sinh tai móng', 30, 70000),
(N'Nhuộm lông nghệ thuật', 150, 450000), (N'Combo Full Spa', 180, 600000), (N'Trông giữ thú cưng', 1440, 200000),
(N'Lấy cao răng', 60, 250000), (N'Diệt ve rận', 90, 180000), (N'Massage thảo dược', 45, 200000), (N'Vắt tuyến hôi', 20, 50000);
-- 8. PRODUCT
INSERT INTO PRODUCT (product_name, category, pet_type_tag, stock_quantity, purchase_price, selling_price, is_active) VALUES 
(N'Hạt Royal Canin', N'Thức ăn', N'Chó', 50, 180000, 250000, 1), (N'Pate Me-O', N'Thức ăn', N'Mèo', 100, 10000, 18000, 1),
(N'Sữa tắm SOS', N'Vệ sinh', N'Chung', 30, 85000, 130000, 1), (N'Cát đậu nành', N'Vệ sinh', N'Mèo', 40, 70000, 110000, 1),
(N'Đồ chơi gà', N'Đồ chơi', N'Chó', 60, 15000, 35000, 1), (N'Vòng cổ chuông', N'Phụ kiện', N'Chung', 200, 5000, 15000, 1),
(N'Bát ăn đôi', N'Dụng cụ', N'Chung', 45, 30000, 65000, 1), (N'Lược chải lông', N'Dụng cụ', N'Chung', 25, 45000, 90000, 1),
(N'Xương gặm', N'Đồ chơi', N'Chó', 120, 12000, 25000, 1), (N'Túi vận chuyển', N'Phụ kiện', N'Mèo', 10, 150000, 280000, 1);