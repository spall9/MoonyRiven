using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace MoonyRiven
{
    internal static class RivenMenu
    {
        public static Menu menu, combo, ultimate, waveClear, jungleClear, drawings, misc;
        public static void Init()
        {
            menu = MainMenu.AddMenu("MoonyRiven", "MoonyRiven");
            menu.AddGroupLabel("by DanThePman");

            combo = menu.AddSubMenu("Combo", "moonyRivenCombo");
            combo.Add("useQ.Combo", new CheckBox("Use Q"));
            combo.Add("useW.Combo", new CheckBox("Use W"));
            combo.Add("useE.Combo", new CheckBox("Use E"));
            combo.Add("gapQ1.Combo", new CheckBox("Use Q1 to gap", false));
            combo.Add("gapE.Combo", new CheckBox("Use E to gap"));
            combo.Add("useItem.Combo", new CheckBox("Use Tiamat and Hydra"));
            combo.AddSeparator(10);
            combo.Add("useR1.Combo", new KeyBind("Use R1", false, KeyBind.BindTypes.PressToggle));
            combo.Add("useR2.Combo", new CheckBox("Use R2"));
            menu.AddSeparator(10);
            combo.Add("burst", new KeyBind("Shy Burst", false, KeyBind.BindTypes.HoldActive));

            ultimate = menu.AddSubMenu("Ultimate", "moonyRivenUltimate");
            ultimate.Add("rDmgMethod", new ComboBox("R damage method", 1, "Kill only", "Max damage or Killable"));
            ultimate.AddSeparator();
            ultimate.Add("rmaxDmgHitCount", new Slider("Min Enemies to hit at MaxDamage", 1, 1, 5));
            ultimate.AddSeparator();
            ultimate.Add("coneAngleStep", new Slider("MaxDamage searching accuracy", 3, 0, 20));
            ultimate.AddLabel("Reduce this value to get more FPS");

            waveClear = menu.AddSubMenu("WaveClear", "moonyRivenWC");
            waveClear.Add("useQ.WaveClear", new CheckBox("Use Q"));
            waveClear.Add("useW.WaveClear", new CheckBox("Use W"));
            waveClear.Add("useE.WaveClear", new CheckBox("Use E"));
            waveClear.Add("useItem.WaveClear", new CheckBox("Use Tiamat and Hydra"));

            jungleClear = menu.AddSubMenu("JungleClear", "moonyRivenJC");
            jungleClear.Add("useQ.JungleClear", new CheckBox("Use Q"));
            jungleClear.Add("useW.JungleClear", new CheckBox("Use W"));
            jungleClear.Add("useE.JungleClear", new CheckBox("Use E"));
            jungleClear.Add("useItem.JungleClear", new CheckBox("Use Tiamat and Hydra"));

            drawings = menu.AddSubMenu("Drawings", "moonyRivenDrawings");
            drawings.Add("drawBurst", new CheckBox("Draw burst range"));
            drawings.Add("drawRExpire", new CheckBox("Draw R expiry"));
            drawings.Add("drawUltimateCone", new CheckBox("Draw Ultimate Cone"));
            drawings.AddLabel("Disable to achieve more fps");
            drawings.Add("debugDraw", new CheckBox("Debug Mode"));

            misc = menu.AddSubMenu("Misc", "moonyRivenMisc");
            misc.Add("qDelay", new Slider("AA reset delay after Q", 29, 20, 100));
            misc.Add("q3Delay", new Slider("AA reset delay after Q3", 42, 20, 100));
            misc.Add("itemDelay", new Slider("AA reset delay after Tiamat/Hydra", 300, 250, 500));
            misc.Add("wDelay", new Slider("AA reset delay after W if aa possible", 500, 200, 600));
            misc.Add("antiGapW", new CheckBox("AntiGapcloser W"));
        }
    }
}
