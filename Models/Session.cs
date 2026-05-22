using System.Drawing;

namespace RetailShop.Models
{
    public static class Session
    {
        public static int    UserId   { get; set; }
        public static string FIO      { get; set; }
        public static string Role     { get; set; }  // Оператор | Администратор | Товаровед | СтаршийКассир
        public static string Login    { get; set; }

        public static bool IsOperator  => Role == "Оператор";
        public static bool IsAdmin     => Role == "Администратор";
        public static bool IsTovaroved => Role == "Товаровед";
        public static bool IsKassir    => Role == "СтаршийКассир";

        public static void Clear() { UserId = 0; FIO = null; Role = null; Login = null; }
    }

    // ─── Windows 11 colour tokens ─────────────────────────────────────────────
    public static class Clr
    {
        // Backgrounds
        public static readonly Color BgApp       = Color.FromArgb(243, 243, 243); // Win11 #F3F3F3
        public static readonly Color BgWhite     = Color.White;
        public static readonly Color BgRow       = Color.FromArgb(249, 249, 249);

        // Borders
        public static readonly Color Border      = Color.FromArgb(229, 229, 229);
        public static readonly Color BorderDark  = Color.FromArgb(200, 200, 200);
        public static readonly Color BorderFocus = Color.FromArgb(0, 120, 212);   // Win11 blue

        // Text
        public static readonly Color TextPrimary = Color.FromArgb(26, 26, 26);
        public static readonly Color TextSecond  = Color.FromArgb(100, 100, 100);
        public static readonly Color TextHint    = Color.FromArgb(160, 160, 160);

        // Accent – Win11 blue
        public static readonly Color Accent      = Color.FromArgb(0, 120, 212);   // #0078D4
        public static readonly Color AccentHover = Color.FromArgb(24, 108, 190);
        public static readonly Color AccentPress = Color.FromArgb(0, 90, 158);
        public static readonly Color AccentLight = Color.FromArgb(228, 241, 254); // tinted bg

        // Buttons
        public static readonly Color BtnPrimary  = Color.FromArgb(0, 120, 212);
        public static readonly Color BtnHover    = Color.FromArgb(24, 108, 190);

        // Header – light Win11 style
        public static readonly Color Header      = Color.White;
        public static readonly Color HeaderBorder= Color.FromArgb(229, 229, 229);

        // Sidebar – white Win11 style
        public static readonly Color Sidebar     = Color.White;
        public static readonly Color SidebarSel  = Color.FromArgb(228, 241, 254); // AccentLight
        public static readonly Color SidebarBorder = Color.FromArgb(229, 229, 229);

        // Table
        public static readonly Color TableHeader = Color.FromArgb(248, 248, 248);

        // Status
        public static readonly Color StatusGreen = Color.FromArgb(0, 138, 0);
        public static readonly Color StatusGreenBg = Color.FromArgb(223, 246, 221);
        public static readonly Color StatusRed   = Color.FromArgb(196, 43, 28);
        public static readonly Color StatusRedBg = Color.FromArgb(253, 231, 233);
        public static readonly Color StatusBlue  = Color.FromArgb(0, 120, 212);
        public static readonly Color StatusBlueBg= Color.FromArgb(228, 241, 254);
    }
}
