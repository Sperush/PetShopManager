namespace PetShop.Models
{
    public class Account
    {
        public int account_id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public bool status { get; set; }
        public DateTimeOffset? last_login_time { get; set; }
        public DateTimeOffset created_time { get; set; }
    }

    public class Role
    {
        public int role_id { get; set; }
        public string role_name { get; set; }
    }

    public class Customer
    {
        public int customer_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string address { get; set; }
        public string membership_tier { get; set; }
        public int? account_id { get; set; }
        public int reward_points { get; set; }
    }

    public class Pet
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
    }

    public class Employee
    {
        public int employee_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string address { get; set; }
        public decimal monthly_salary { get; set; }
        public bool is_active { get; set; } = true;
        public int? account_id { get; set; }
        public int role_id { get; set; }
    }

    public class WorkShift
    {
        public int workshift_id { get; set; }
        public int employee_id { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
    }

    public class Appointment
    {
        public int appointment_id { get; set; }
        public DateTime appointment_time { get; set; }
        public DateTime booking_time { get; set; }
        public string appointment_status { get; set; }
        public string payment_status { get; set; }
        public string description { get; set; }
        public int pet_id { get; set; }
        public int customer_id { get; set; }
        public int workshift_id { get; set; }
        public int employee_id { get; set; }
        public int used_reward_points { get; set; }
    }

    public class Service
    {
        public int service_id { get; set; }
        public string service_name { get; set; }
        public int estimated_duration { get; set; }
        public decimal current_price { get; set; }
    }

    public class Product
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

    public class Order
    {
        public int order_id { get; set; }
        public string payment_status { get; set; }
        public string payment_method { get; set; }
        public DateTimeOffset created_time { get; set; }
        public DateTimeOffset? payment_time { get; set; }
        public int? used_reward_points { get; set; }
        public int customer_id { get; set; }
        public int workshift_id { get; set; }
        public int employee_id { get; set; }
    }

    public class AppointmentDetail
    {
        public int appointment_detail_id { get; set; }
        public int appointment_id { get; set; }
        public decimal price_at_booking { get; set; }
        public int employee_id { get; set; }
        public int service_id { get; set; }
    }

    public class OrderDetail
    {
        public int order_detail_id { get; set; }
        public int order_id { get; set; }
        public int quantity { get; set; }
        public decimal price_at_purchase { get; set; }
        public string note { get; set; }
        public int product_id { get; set; }
    }

    public class CommissionHistory
    {
        public int commission_id { get; set; }
        public string commission_type { get; set; }
        public decimal applied_percentage { get; set; }
        public decimal received_amount { get; set; }
        public DateTimeOffset recorded_time { get; set; }
        public int employee_id { get; set; }
        public int? appointment_detail_id { get; set; }
        public int? appointment_id { get; set; }
        public int? order_detail_id { get; set; }
        public int? order_id { get; set; }
    }

    public class PhoneNum
    {
        public int phone_num_id { get; set; }
        public string num { get; set; }
        public int? customer_id { get; set; }
        public int? employee_id { get; set; }
    }
}

