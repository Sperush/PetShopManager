-- 1. ACCOUNT
INSERT INTO ACCOUNT (username, password) VALUES 
('admin', '123456'), ('staff1', '123456'), ('staff2', '123456'), ('staff3', '123456'), 
('groomer1', '123456'), ('groomer2', '123456'), ('customer1', '123456'), 
('customer2', '123456'), ('customer3', '123456'), ('customer4', '123456');
-- 2. ROLE
INSERT INTO ROLE (role_name) VALUES (N'Quản lý'), (N'Nhân viên CSKH'), (N'Nhân viên chăm sóc pet'), (N'Nhân viên bán hàng'), (N'Lễ tân');
-- 3. CUSTOMER
INSERT INTO CUSTOMER (first_name, middle_name, last_name, address, account_id) VALUES 
(N'Nguyễn', N'Thị', N'Lan', N'Quận 1, HCM', 7), (N'Trần', N'Minh', N'Tuấn', N'Quận 3, HCM', 8),
(N'Lê', N'Văn', N'Nam', N'Quận 5, HCM', 9), (N'Phạm', N'Thanh', N'Hà', N'Quận 7, HCM', 10),
(N'Hoàng', N'Anh', N'Dũng', N'Thủ Đức, HCM', NULL), (N'Vũ', N'Thị', N'Mai', N'Gò Vấp, HCM', NULL),
(N'Bùi', N'Văn', N'Khoa', N'Hóc Môn, HCM', NULL), (N'Đặng', N'Thu', N'Thủy', N'Bình Chánh, HCM', NULL),
(N'Đỗ', N'Gia', N'Bảo', N'Quận 10, HCM', NULL), (N'Lý', N'Văn', N'Đông', N'Quận 11, HCM', NULL);
-- 4. EMPLOYEE
INSERT INTO EMPLOYEE (first_name, middle_name, last_name, address, monthly_salary, role_id, account_id) VALUES 
(N'Phạm', N'Quốc', N'Admin', N'Quận 1, HCM', 20000000, 1, 1),
(N'Nguyễn', N'Văn', N'Nhân', N'Quận 3, HCM', 9000000, 4, 2),
(N'Trần', N'Thị', N'Viên', N'Quận 5, HCM', 9000000, 4, 3),
(N'Lê', N'Thị', N'Thắm', N'Quận 7, HCM', 9000000, 5, 4),
(N'Lý', N'Minh', N'Groom', N'Tân Bình, HCM', 12000000, 3, 5),
(N'Vũ', N'Văn', N'Cắt', N'Quận 12, HCM', 11000000, 3, 6),
(N'Bùi', N'Thị', N'Nữ', N'Quận 4, HCM', 8500000, 2, NULL),
(N'Lê', N'Quốc', N'Huy', N'Quận 8, HCM', 8500000, 2, NULL),
(N'Phan', N'Anh', N'Tú', N'Quận 9, HCM', 10000000, 3, NULL),
(N'Hồ', N'Minh', N'Trí', N'Bình Tân, HCM', 8500000, 2, NULL);
-- 5. PHONE_NUM
INSERT INTO PHONE_NUM (num, customer_id, employee_id) VALUES 
('0901234567', 1, NULL), ('0902234567', 2, NULL), ('0903234567', 3, NULL), ('0904234567', 4, NULL), ('0905234567', 5, NULL),
('0981234567', NULL, 1), ('0982234567', NULL, 2), ('0983234567', NULL, 3), ('0984234567', NULL, 5), ('0985234567', NULL, 6);
-- 6. PET
INSERT INTO PET (pet_name, species, breed, color, weight, customer_id) VALUES 
(N'LuLu', N'Chó', N'Poodle', N'Nâu', 5.5, 1), (N'Miu', N'Mèo', N'Anh lông ngắn', N'Xám', 4.2, 2),
(N'Bơ', N'Chó', N'Corgi', N'Vàng trắng', 12.0, 3), (N'Kem', N'Mèo', N'Ta', N'Trắng', 3.5, 4),
(N'Bi', N'Chó', N'Phốc', N'Đen', 2.8, 5), (N'Na', N'Mèo', N'Ba Tư', N'Vàng', 5.0, 6),
(N'Gấu', N'Chó', N'Golden', N'Vàng đậm', 25.0, 7), (N'Mochi', N'Mèo', N'Ragdoll', N'Xám trắng', 6.2, 8),
(N'Bin', N'Chó', N'Chihuahua', N'Trắng', 2.1, 9), (N'Susu', N'Mèo', N'Xiêm', N'Đen xám', 4.0, 10);
-- 9. ORDERS
INSERT INTO ORDERS (payment_status, payment_method, payment_time, customer_id, employee_id) VALUES 
('Completed', N'Tiền mặt', DATEADD(minute, 15, SYSDATETIMEOFFSET()), 1, 2), 
('Completed', N'Chuyển khoản', DATEADD(minute, 25, SYSDATETIMEOFFSET()), 2, 3), 
('Pending', N'Tiền mặt', NULL, 3, 2),
('Completed', N'Chuyển khoản', DATEADD(minute, 35, SYSDATETIMEOFFSET()), 4, 3), 
('Pending', N'Tiền mặt', NULL, 5, 2), 
('Pending', N'Tiền mặt', NULL, 6, 3),
('Pending', N'Chuyển khoản', NULL, 7, 2), 
('Pending', N'Tiền mặt', NULL, 8, 3), 
('Pending', N'Chuyển khoản', NULL, 9, 2);
-- 10. ORDER_DETAIL
INSERT INTO ORDER_DETAIL (quantity, price_at_purchase, order_id, product_id) VALUES 
(1, 250000, 1, 1), (2, 18000, 1, 2), (1, 130000, 2, 3), (2, 110000, 3, 4), (5, 35000, 4, 5),
(2, 15000, 5, 6), (1, 65000, 6, 7), (1, 90000, 7, 8), (4, 25000, 8, 9), (1, 280000, 9, 10);
-- 11. APPOINTMENT
INSERT INTO APPOINTMENT (appointment_time, appointment_status, payment_status, pet_id) VALUES 
(GETDATE(), 'Confirmed', 'Completed', 1),
(DATEADD(hour, 1, GETDATE()), 'Completed', 'Completed', 2),
(DATEADD(hour, 2, GETDATE()), 'Confirmed', 'Completed', 3),
(DATEADD(hour, 3, GETDATE()), 'Cancelled', 'Pending', 4),
(DATEADD(hour, 4, GETDATE()), 'Confirmed', 'Pending', 5),
(DATEADD(hour, 5, GETDATE()), 'Confirmed', 'Pending', 6),
(DATEADD(hour, 6, GETDATE()), 'Confirmed', 'Pending', 7),
(DATEADD(hour, 7, GETDATE()), 'Completed', 'Pending', 8),
(DATEADD(hour, 8, GETDATE()), 'Confirmed', 'Pending', 9),
(DATEADD(hour, 9, GETDATE()), 'Confirmed', 'Pending', 10);
-- 12. APPOINTMENT_DETAIL
INSERT INTO APPOINTMENT_DETAIL (price_at_booking, appointment_id, employee_id, service_id) VALUES 
(150000, 1, 5, 1), (300000, 2, 6, 2), (70000, 3, 5, 3), (450000, 4, 6, 4), (600000, 5, 5, 5),
(200000, 6, 6, 6), (250000, 7, 5, 7), (180000, 8, 6, 8), (200000, 9, 5, 9), (50000, 10, 6, 10);
-- 13. WORK_SHIFT
INSERT INTO WORK_SHIFT (start_time, end_time, employee_id) VALUES 
('2024-04-23 08:00:00', '2024-04-23 17:00:00', 1), ('2024-04-23 08:00:00', '2024-04-23 17:00:00', 2),
('2024-04-23 08:00:00', '2024-04-23 17:00:00', 3), ('2024-04-23 08:00:00', '2024-04-23 17:00:00', 5),
('2024-04-23 08:00:00', '2024-04-23 17:00:00', 6), ('2024-04-24 08:00:00', '2024-04-24 17:00:00', 1),
('2024-04-24 08:00:00', '2024-04-24 17:00:00', 2), ('2024-04-24 08:00:00', '2024-04-24 17:00:00', 3),
('2024-04-24 08:00:00', '2024-04-24 17:00:00', 5), ('2024-04-24 08:00:00', '2024-04-24 17:00:00', 6);
-- 14. COMMISSION_HISTORY
INSERT INTO COMMISSION_HISTORY (commission_type, applied_percentage, received_amount, order_detail_id) VALUES 
(N'Bán hàng', 5, 14300, 1), (N'Bán hàng', 5, 6500, 3), (N'Bán hàng', 5, 8750, 4);
INSERT INTO COMMISSION_HISTORY (commission_type, applied_percentage, received_amount, appointment_detail_id) VALUES 
(N'Dịch vụ', 10, 15000, 1), (N'Dịch vụ', 10, 30000, 2), (N'Dịch vụ', 10, 7000, 3);
-- 15. STORE_SETTINGS
INSERT INTO STORE_SETTINGS (setting_key, setting_value) VALUES 
('store_name', N'Pet Shop'), ('qr_secret', 'PETSHOP_2026'), ('bank_id', 'VCB'),
('account_no', '0011001234567'), ('account_name', 'NGUYEN VAN A');