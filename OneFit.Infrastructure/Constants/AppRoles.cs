namespace OneFit.Infrastructure.Constants
{
    /// <summary>
    /// Constants for application roles
    /// </summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Brand = "Brand";
        public const string User = "User";

        /// <summary>
        /// Get all available roles
        /// </summary>
        public static readonly string[] AllRoles = { Admin, Brand, User };
    }
}
