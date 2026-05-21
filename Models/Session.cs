namespace RetailShop.Models
{
    public static class Session
    {
        public static int UserId { get; set; }
        public static string UserName { get; set; }
        public static string UserRole { get; set; }
        public static string UserFIO { get; set; }

        public static bool IsOperator => UserRole == "Оператор";
        public static bool IsAdmin => UserRole == "Администратор";
        public static bool IsTovaroved => UserRole == "Товаровед";
        public static bool IsKassir => UserRole == "СтаршийКассир";

        public static void Clear()
        {
            UserId = 0;
            UserName = null;
            UserRole = null;
            UserFIO = null;
        }
    }
}
