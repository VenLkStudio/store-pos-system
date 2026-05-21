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

    // ─── Wireframe colour tokens ──────────────────────────────────────────────
    public static class Clr
    {
        // exact colours from the SVG wireframe
        public static readonly Color BgApp       = Color.FromArgb(243, 243, 243); // #F3F3F3
        public static readonly Color BgWhite     = Color.White;
        public static readonly Color BgRow       = Color.FromArgb(248, 248, 248);
        public static readonly Color Border      = Color.FromArgb(224, 224, 224); // #E0E0E0
        public static readonly Color BorderDark  = Color.FromArgb(209, 209, 209); // #D1D1D1
        public static readonly Color TextPrimary = Color.FromArgb(26, 26, 26);   // #1A1A1A
        public static readonly Color TextSecond  = Color.FromArgb(94, 94, 94);   // #5E5E5E
        public static readonly Color TextHint    = Color.FromArgb(180,180,180);
        public static readonly Color Accent      = Color.FromArgb(26, 26, 26);   // header bar – dark
        public static readonly Color BtnPrimary  = Color.FromArgb(26, 26, 26);
        public static readonly Color BtnHover    = Color.FromArgb(50, 50, 50);
        public static readonly Color Sidebar     = Color.FromArgb(30, 30, 30);
        public static readonly Color SidebarSel  = Color.FromArgb(60, 60, 60);
        public static readonly Color TableHeader = Color.FromArgb(26, 26, 26);
    }
}