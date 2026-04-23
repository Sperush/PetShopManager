namespace PetShop.DTO
{

    public class CustomerDisplay
    {
        public int customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public int? account_id { get; set; }
        public int reward_points { get; set; }
        public string FullName => $"{first_name} {last_name}".Trim();
    }

    public class AppointmentDisplay
    {
        public int pet_id { get; set; }
        public int appointment_id { get; set; }
        public DateTime appointment_time { get; set; }
        public DateTime booking_time { get; set; }
        public string appointment_status { get; set; }
        public string payment_status { get; set; }
        public decimal total_amount { get; set; }
        public string description { get; set; }
        public int workshift_id { get; set; }
        public int? employee_id { get; set; }
        public string customer_name { get; set; }
        public int customer_id { get; set; }
        public string customer_address { get; set; }
        public string pet_info { get; set; }
        public decimal? pet_weight { get; set; }
        public string employee_name { get; set; }
        public int? used_reward_points { get; set; }
        public string service_names { get; set; }
        public List<AppointmentDetailItem> Details { get; set; } = new List<AppointmentDetailItem>();
    }
    public class AppointmentDetailItem
    {
        public int appointment_detail_id { get; set; }
        public int service_id { get; set; }
        public string service_name { get; set; }
        public int? employee_id { get; set; }
        public string detail_employee_name { get; set; }
        public decimal price_at_booking { get; set; }
    }

    public class OrderDisplay
    {
        public int order_id { get; set; }
        public decimal total_amount { get; set; }
        public string payment_status { get; set; }
        public string payment_method { get; set; }
        public DateTimeOffset created_time { get; set; }
        public DateTimeOffset? payment_time { get; set; }
        public int? used_reward_points { get; set; }
        public int customer_id { get; set; }
        public int workshift_id { get; set; }
        public int employee_id { get; set; }
        public string customer_name { get; set; }
        public string customer_phone { get; set; }
    }

    public class OrderFullDisplay : OrderDisplay
    {
        public string customer_address { get; set; }
        public string employee_name { get; set; }
        public List<OrderDetailItemDisplay> Details { get; set; } = new();
    }

    public class OrderDetailItemDisplay
    {
        public int order_detail_id { get; set; }
        public int product_id { get; set; }
        public string product_name { get; set; }
        public int quantity { get; set; }
        public decimal price_at_purchase { get; set; }
        public string note { get; set; }
        public decimal TotalPrice => quantity * price_at_purchase;
    }
    public class PetDisplay
    {
        public int pet_id { get; set; }
        public string pet_name { get; set; }
        public string species { get; set; }
        public string breed { get; set; }
        public string color { get; set; }
        public decimal? weight { get; set; }
        public DateTime? birth_date { get; set; }
        public string gender { get; set; }
        public string note { get; set; }
        public int customer_id { get; set; }
        public string owner_name { get; set; }
        public string owner_phone { get; set; }
    }
    public class ProductDisplay
    {
        public int product_id { get; set; }
        public string product_name { get; set; }
        public string category { get; set; }
        public string pet_type_tag { get; set; }
        public int stock_quantity { get; set; }
        public decimal purchase_price { get; set; }
        public decimal selling_price { get; set; }
        public string description { get; set; }
        public bool is_active { get; set; } = true;
    }
    public class ServiceDisplay
    {
        public int service_id { get; set; }
        public string service_name { get; set; }
        public int estimated_duration { get; set; }
        public decimal current_price { get; set; }
    }
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AccountCreateRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class AccountDisplay
    {
        public int account_id { get; set; }
        public string username { get; set; }
        public string role { get; set; }
        public int role_id { get; set; }
        public int employee_id { get; set; }
        public bool status { get; set; }
        public bool is_active { get; set; } = true;
    }
    public class RoleDisplay
    {
        public int role_id { get; set; }
        public string role_name { get; set; }
    }

    public class EmployeeDisplay
    {
        public int employee_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public decimal monthly_salary { get; set; }
        public bool is_active { get; set; } = true;
        public int? account_id { get; set; }
        public int role_id { get; set; }
        public string role_name { get; set; }
        public string FullName => $"{first_name} {middle_name} {last_name}".Replace("  ", " ").Trim();
    }

    public class OrderCreateRequest
    {
        public int customer_id { get; set; }
        public int employee_id { get; set; }
        public string payment_method { get; set; } = "Tiền mặt";
        public string payment_status { get; set; } = "Pending";
        public int? used_reward_points { get; set; }
        public List<OrderDetailCreate> Details { get; set; } = new();
        public bool auto_commission { get; set; } = false;
        public decimal commission_rate { get; set; } = 5.0m;
    }

    public class OrderDetailCreate
    {
        public int product_id { get; set; }
        public int quantity { get; set; }
        public decimal price_at_purchase { get; set; }
        public string note { get; set; }
    }

    public class AppointmentCreateRequest
    {
        public int pet_id { get; set; }
        public int? employee_id { get; set; }
        public DateTime appointment_time { get; set; }
        public string payment_status { get; set; } = "Pending";
        public int? used_reward_points { get; set; }
        public List<AppointmentDetailCreate> Details { get; set; } = new();
        public bool auto_commission { get; set; } = false;
        public decimal commission_rate { get; set; } = 5.0m;
    }

    public class AppointmentDetailCreate
    {
        public int service_id { get; set; }
        public int? employee_id { get; set; }
        public decimal price_at_booking { get; set; }
    }
    public class WorkShiftDisplay
    {
        public int workshift_id { get; set; }
        public int employee_id { get; set; }
        public DateTime start_time { get; set; } = DateTime.Now;
        public DateTime end_time { get; set; } = DateTime.Now;
        public string employee_name { get; set; }
    }
    public class CommissionDisplay
    {
        public int commission_id { get; set; }
        public string commission_type { get; set; }
        public decimal applied_percentage { get; set; }
        public decimal received_amount { get; set; }
        public DateTimeOffset recorded_time { get; set; } = DateTimeOffset.Now;
        public int employee_id { get; set; }
        public int? appointment_detail_id { get; set; }
        public int? appointment_id { get; set; }
        public int? order_detail_id { get; set; }
        public int? order_id { get; set; }
        public string employee_name { get; set; }
        public string employee_phone { get; set; }
    }

    public class CustomerPortalUser 
    { 
        public int CustomerId { get; set; } 
        public int AccountId { get; set; }
        public string Name { get; set; } 
        public int Points { get; set; } 
    }

    public class CustomerRegisterRequest 
    { 
        public string Phone { get; set; } 
        public string Password { get; set; } 
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
    }
}

