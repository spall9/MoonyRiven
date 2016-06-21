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
            menu.Add("gapQ1", new CheckBox("Use Q1 to gap"));
            menu.Add("gapE", new CheckBox("Use E to gap"));
            menu.Add("useE", new CheckBox("Use E"));
            menu.Add("useR1", new KeyBind("Use R1", false, KeyBind.BindTypes.PressToggle));
            menu.Add("useR2", new CheckBox("Use R2"));
            menu.Add("burst", new KeyBind("Burst Mode", false, KeyBind.BindTypes.HoldActive));
            menu.AddGroupLabel("Drawing");
            menu.Add("drawBurst", new CheckBox("Draw burst range"));
            menu.Add("drawRExpire", new CheckBox("Draw R expiry"));
            menu.AddGroupLabel("Misc");
            menu.Add("qDelay", new Slider("Q cancel delay", 29, 20, 100));
            menu.Add("q3Delay", new Slider("Q3 cancel delay", 42, 20, 100));
            menu.Add("antiGapW", new CheckBox("AntiGap W"));
        }
    }
}
