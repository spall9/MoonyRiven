using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace MoonyRiven
{
    internal static class RivenMenu
    {
        public static Menu menu;
        public static void Init()
        {
            menu = MainMenu.AddMenu("MoonyRiven", "MoonyRiven");
            menu.AddGroupLabel("Combo");
            menu.Add("useE", new CheckBox("Use E Combo"));
            menu.Add("useR1", new KeyBind("Use R1", false, KeyBind.BindTypes.PressToggle));
            menu.Add("useR2", new CheckBox("Use R2 Combo"));
            menu.Add("burst", new KeyBind("DebugMode", false, KeyBind.BindTypes.HoldActive));
            menu.AddGroupLabel("Drawing");
            menu.Add("drawBurst", new CheckBox("Draw burst range"));
            menu.Add("drawRExpire", new CheckBox("Draw R expiry"));
            menu.AddGroupLabel("Misc");
            menu.Add("qDelay", new Slider("Q cancel delay", 29, 20, 100));
            menu.Add("q3Delay", new Slider("Q3 cancel delay", 42, 20, 100));
        }
    }
}
