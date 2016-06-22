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
            menu.Add("useQ.Combo", new CheckBox("Use Q"));
            menu.Add("useW.Combo", new CheckBox("Use W"));
            menu.Add("useE.Combo", new CheckBox("Use E"));
            menu.Add("gapQ1.Combo", new CheckBox("Use Q1 to gap"));
            menu.Add("gapE.Combo", new CheckBox("Use E to gap"));
            menu.Add("useItem.Combo", new CheckBox("Use Tiamat and Hydra"));
            menu.AddSeparator(10);
            menu.Add("useR1.Combo", new KeyBind("Use R1", false, KeyBind.BindTypes.PressToggle));
            menu.Add("useR2.Combo", new CheckBox("Use R2"));
            menu.AddSeparator(10);
            menu.Add("burst", new KeyBind("Burst Mode", false, KeyBind.BindTypes.HoldActive));
            menu.AddSeparator();
            menu.AddGroupLabel("WaveClear");
            menu.Add("useQ.WaveClear", new CheckBox("Use Q"));
            menu.Add("useW.WaveClear", new CheckBox("Use W"));
            menu.Add("useE.WaveClear", new CheckBox("Use E"));
            menu.Add("useItem.WaveClear", new CheckBox("Use Tiamat and Hydra"));
            menu.AddSeparator();
            menu.AddGroupLabel("JungleClear");
            menu.Add("useQ.JungleClear", new CheckBox("Use Q"));
            menu.Add("useW.JungleClear", new CheckBox("Use W"));
            menu.Add("useE.JungleClear", new CheckBox("Use E"));
            menu.Add("useItem.JungleClear", new CheckBox("Use Tiamat and Hydra"));
            menu.AddSeparator();
            menu.AddGroupLabel("Drawing");
            menu.Add("drawBurst", new CheckBox("Draw burst range"));
            menu.Add("drawRExpire", new CheckBox("Draw R expiry"));
            menu.AddSeparator();
            menu.AddGroupLabel("Misc");
            menu.Add("qDelay", new Slider("Q cancel delay", 29, 20, 100));
            menu.Add("q3Delay", new Slider("Q3 cancel delay", 42, 20, 100));
            menu.Add("antiGapW", new CheckBox("AntiGapcloser W"));
            menu.Add("rDmgMethod", new ComboBox("R damage method", 1, "Kill only", "Max damage"));
        }
    }
}
