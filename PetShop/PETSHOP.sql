-- 1. Tao bang ACCOUNT
CREATE TABLE ACCOUNT
(
  account_id INT IDENTITY(1,1) PRIMARY KEY,
  username NVARCHAR(100) NOT NULL UNIQUE,
  password NVARCHAR(255) NOT NULL,
  status BIT NOT NULL DEFAULT 1,
  last_login_time DATETIMEOFFSET,
  created_time DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

-- 2. Tao bang ROLE
CREATE TABLE ROLE
(
  role_id INT IDENTITY(1,1) PRIMARY KEY,
  role_name NVARCHAR(100) NOT NULL
);

-- 3. Tao bang CUSTOMER
CREATE TABLE CUSTOMER
(
  customer_id INT IDENTITY(1,1) PRIMARY KEY,
  first_name NVARCHAR(50) NOT NULL,
  middle_name NVARCHAR(50),
  last_name NVARCHAR(50) NOT NULL,
  address NVARCHAR(MAX),
  account_id INT,
  CONSTRAINT FK_Customer_Account FOREIGN KEY (account_id) REFERENCES ACCOUNT(account_id)
);
CREATE UNIQUE INDEX UIX_Customer_Account 
ON CUSTOMER(account_id) 
WHERE account_id IS NOT NULL;

-- 4. Tao bang PET
CREATE TABLE PET
(
  pet_id INT IDENTITY(1,1) PRIMARY KEY,
  pet_name NVARCHAR(100),
  species NVARCHAR(50) NOT NULL,
  breed NVARCHAR(50),
  color NVARCHAR(50),
  weight DECIMAL(5,2) CONSTRAINT CHK_Pet_Weight CHECK (weight > 0),
  birth_date DATE,
  gender NVARCHAR(20),
  note NVARCHAR(MAX),
  customer_id INT NOT NULL,
  CONSTRAINT FK_Pet_Customer FOREIGN KEY (customer_id) REFERENCES CUSTOMER(customer_id)
);

-- 5. Tao bang EMPLOYEE
CREATE TABLE EMPLOYEE
(
  employee_id INT IDENTITY(1,1) PRIMARY KEY,
  first_name NVARCHAR(50) NOT NULL,
  middle_name NVARCHAR(50),
  last_name NVARCHAR(50) NOT NULL,
  address NVARCHAR(MAX),
  monthly_salary DECIMAL(12,2) NOT NULL CONSTRAINT CHK_Employee_Salary CHECK (monthly_salary >= 0),
  is_active BIT NOT NULL DEFAULT 1,
  account_id INT,
  role_id INT NOT NULL,
  CONSTRAINT FK_Employee_Account FOREIGN KEY (account_id) REFERENCES ACCOUNT(account_id),
  CONSTRAINT FK_Employee_Role FOREIGN KEY (role_id) REFERENCES ROLE(role_id)
);
CREATE UNIQUE INDEX UIX_Employee_Account 
ON EMPLOYEE(account_id) 
WHERE account_id IS NOT NULL;

-- 6. Tao bang WORK_SHIFT
CREATE TABLE WORK_SHIFT
(
  workshift_id INT IDENTITY(1,1) PRIMARY KEY,
  start_time DATETIME2 NOT NULL,
  end_time DATETIME2 NOT NULL,
  employee_id INT NOT NULL,
  CONSTRAINT FK_WorkShift_Employee FOREIGN KEY (employee_id) REFERENCES EMPLOYEE(employee_id),
  CONSTRAINT CHK_WorkShift_Time CHECK (start_time < end_time)
);

-- 7. Tao bang APPOINTMENT
CREATE TABLE APPOINTMENT
(
  appointment_id INT IDENTITY(1,1) PRIMARY KEY,
  appointment_time DATETIME2 NOT NULL,
  booking_time DATETIME2 NOT NULL DEFAULT GETDATE(),
  appointment_status NVARCHAR(50) NOT NULL,
  payment_status NVARCHAR(50) NOT NULL,
  pet_id INT NOT NULL,
  CONSTRAINT FK_App_Pet FOREIGN KEY (pet_id) REFERENCES PET(pet_id)
);

-- 8. Tao bang SERVICE
CREATE TABLE SERVICE
(
  service_id INT IDENTITY(1,1) PRIMARY KEY,
  service_name NVARCHAR(255) NOT NULL,
  estimated_duration INT NOT NULL CONSTRAINT CHK_Service_Duration CHECK (estimated_duration > 0),
  current_price DECIMAL(12,2) NOT NULL CONSTRAINT CHK_Service_Price CHECK (current_price >= 0)
);

-- 9. Tao bang PRODUCT
CREATE TABLE PRODUCT
(
  product_id INT IDENTITY(1,1) PRIMARY KEY,
  product_name NVARCHAR(255) NOT NULL,
  category NVARCHAR(100) NOT NULL,
  pet_type_tag NVARCHAR(100),
  stock_quantity INT NOT NULL DEFAULT 0 CONSTRAINT CHK_Prod_Stock CHECK (stock_quantity >= 0),
  purchase_price DECIMAL(12,2) NOT NULL CONSTRAINT CHK_Product_PurchasePrice CHECK (purchase_price >= 0),
  selling_price DECIMAL(12,2) NOT NULL CONSTRAINT CHK_Product_SellingPrice CHECK (selling_price >= 0),
  description NVARCHAR(MAX),
  is_active BIT NOT NULL DEFAULT 1
);

-- 10. Tao bang ORDERS
CREATE TABLE ORDERS
(
  order_id INT IDENTITY(1,1) PRIMARY KEY,
  payment_status NVARCHAR(50) NOT NULL,
  payment_method NVARCHAR(50) NOT NULL,
  created_time DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
  payment_time DATETIMEOFFSET,
  used_reward_points INT DEFAULT 0 CONSTRAINT CHK_Order_Points CHECK (used_reward_points >= 0),
  customer_id INT NOT NULL,
  employee_id INT NOT NULL,
  CONSTRAINT FK_Order_Customer FOREIGN KEY (customer_id) REFERENCES CUSTOMER(customer_id),
  CONSTRAINT FK_Order_Employee FOREIGN KEY (employee_id) REFERENCES EMPLOYEE(employee_id),
  CONSTRAINT CHK_Order_PaymentTime CHECK (payment_time IS NULL OR payment_time >= created_time)
);

-- 11. Tao bang APPOINTMENT_DETAIL
CREATE TABLE APPOINTMENT_DETAIL
(
  appointment_detail_id INT IDENTITY(1,1) PRIMARY KEY,
  price_at_booking DECIMAL(12,2) NOT NULL CONSTRAINT CHK_AppDetail_Price CHECK (price_at_booking >= 0),
  appointment_id INT NOT NULL,
  employee_id INT NOT NULL,
  service_id INT NOT NULL,
  CONSTRAINT FK_AppDetail_App FOREIGN KEY (appointment_id) REFERENCES APPOINTMENT(appointment_id),
  CONSTRAINT FK_AppDetail_Employee FOREIGN KEY (employee_id) REFERENCES EMPLOYEE(employee_id),
  CONSTRAINT FK_AppDetail_Service FOREIGN KEY (service_id) REFERENCES SERVICE(service_id)
);

-- 12. Tao bang ORDER_DETAIL
CREATE TABLE ORDER_DETAIL
(
  order_detail_id INT IDENTITY(1,1) PRIMARY KEY,
  quantity INT NOT NULL DEFAULT 1 CONSTRAINT CHK_OrderDetail_Qty CHECK (quantity > 0),
  price_at_purchase DECIMAL(12,2) NOT NULL CONSTRAINT CHK_OrderDetail_Price CHECK (price_at_purchase >= 0),
  note NVARCHAR(MAX),
  order_id INT NOT NULL,
  product_id INT NOT NULL,
  CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (order_id) REFERENCES ORDERS(order_id),
  CONSTRAINT FK_OrderDetail_Product FOREIGN KEY (product_id) REFERENCES PRODUCT(product_id)
);

-- 13. Tao bang COMMISSION_HISTORY
CREATE TABLE COMMISSION_HISTORY
(
  commission_id INT IDENTITY(1,1) PRIMARY KEY,
  commission_type NVARCHAR(100) NOT NULL,
  applied_percentage DECIMAL(5,2) NOT NULL CONSTRAINT CHK_Comm_Percent CHECK (applied_percentage >= 0 AND applied_percentage <= 100),
  received_amount DECIMAL(12,2) NOT NULL CONSTRAINT CHK_Comm_Amount CHECK (received_amount >= 0),
  recorded_time DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
  appointment_detail_id INT NULL,
  order_detail_id INT NULL,
  CONSTRAINT FK_Comm_AppDetail FOREIGN KEY (appointment_detail_id) REFERENCES APPOINTMENT_DETAIL(appointment_detail_id),
  CONSTRAINT FK_Comm_OrderDetail FOREIGN KEY (order_detail_id) REFERENCES ORDER_DETAIL(order_detail_id),
  CONSTRAINT CHK_Comm_Source CHECK (
      commission_type = 'Thưởng thêm' 
      OR (appointment_detail_id IS NOT NULL AND order_detail_id IS NULL) 
      OR (appointment_detail_id IS NULL AND order_detail_id IS NOT NULL)
  )
);
-- 14. Tao bang PHONE_NUM
CREATE TABLE PHONE_NUM
(
  phone_num_id INT IDENTITY(1,1) PRIMARY KEY,
  num VARCHAR(20) NOT NULL,
  customer_id INT NULL,
  employee_id INT NULL,
  CONSTRAINT FK_Phone_Customer FOREIGN KEY (customer_id) REFERENCES CUSTOMER(customer_id),
  CONSTRAINT FK_Phone_Employee FOREIGN KEY (employee_id) REFERENCES EMPLOYEE(employee_id),
  CONSTRAINT chk_phone_owner CHECK (
        (customer_id IS NOT NULL AND employee_id IS NULL) 
        OR 
        (customer_id IS NULL AND employee_id IS NOT NULL)
    )
);

-- 15. Tao bang STORE_SETTINGS
CREATE TABLE STORE_SETTINGS (
  setting_key NVARCHAR(100) PRIMARY KEY,
  setting_value NVARCHAR(MAX)
);