namespace MyFirstApi.Domain.Constants;

public static class Permissions
{
         // 2. موديول التصنيفات
    public static class Categories
    {
        public const string View = "Permissions.Categories.View";
        public const string Create = "Permissions.Categories.Create";
        public const string Edit = "Permissions.Categories.Edit";
        public const string Delete = "Permissions.Categories.Delete";
    }

    // 3. موديول الأدوات
    public static class Tools
    {
        public const string View = "Permissions.Tools.View";
        public const string Create = "Permissions.Tools.Create";
        public const string Edit = "Permissions.Tools.Edit";
        public const string Delete = "Permissions.Tools.Delete";
    }
}
