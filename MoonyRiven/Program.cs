using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace MoonyRiven
{
    class Program
    {
        static Spell.Skillshot Q;
        static Spell.Active W;
        static Spell.Skillshot E;
        static Spell.Active R;
        static Spell.Skillshot R2;
        static Spell.Targeted Flash;

        static Item Hydra;
        static Item Tiamat;
        private static readonly AIHeroClient me = ObjectManager.Player;

        static int QStacks
        {
            get { return me.HasBuff("RivenTriCleave") ? me.GetBuff("RivenTriCleave").Count : 0; }
        }

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += eventArgs =>
            {
                RivenMenu.Init();
                Q = new Spell.Skillshot(SpellSlot.Q, 275, SkillShotType.Circular, 250, 2200, 100);
                W = new Spell.Active(SpellSlot.W, 250);
                E = new Spell.Skillshot(SpellSlot.E, 310, SkillShotType.Linear);
                R = new Spell.Active(SpellSlot.R);
                R2 = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Cone, 250, 1600, 125);
                if (Player.Instance.Spellbook.GetSpell(SpellSlot.Summoner1).Name == "SummonerFlash")
                {
                    Flash = new Spell.Targeted(SpellSlot.Summoner1, 425);
                }
                else if (Player.Instance.Spellbook.GetSpell(SpellSlot.Summoner2).Name == "SummonerFlash")
                {
                    Flash = new Spell.Targeted(SpellSlot.Summoner2, 425);
                }

                Hydra = new Item((int)ItemId.Ravenous_Hydra, 350);
                Tiamat = new Item((int)ItemId.Tiamat, 350);

                Obj_AI_Base.OnPlayAnimation += ObjAiBaseOnOnPlayAnimation;
                AIHeroClient.OnSpellCast += AiHeroClientOnOnSpellCast;
                AIHeroClient.OnProcessSpellCast += OnProcCast;
                Game.OnUpdate += GameOnOnUpdate;
                Drawing.OnDraw += DrawingOnOnDraw;
                Gapcloser.OnGapcloser += GapcloserOnOnGapcloser;
            };
        }

        private static void GapcloserOnOnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloserEventArgs)
        {
            if (!sender.IsEnemy)
                return;

            if (gapcloserEventArgs.End.Distance(me) <= W.Range && RivenMenu.menu["antiGapW"].Cast<CheckBox>().CurrentValue)
                W.Cast();
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            bool useR1 = RivenMenu.menu["useR1"].Cast<KeyBind>().CurrentValue;
            var heropos = Drawing.WorldToScreen(ObjectManager.Player.Position);
            Drawing.DrawText(heropos.X - 40, heropos.Y + 20, Color.LightBlue, "Always R  [        ]");
            Drawing.DrawText(heropos.X + 40, heropos.Y + 20, useR1 ? Color.LightGreen : Color.Red, useR1 ? "On" : "Off");

            if (IsSecondR && RivenMenu.menu["drawRExpire"].Cast<CheckBox>().CurrentValue && R.IsReady() && me.Level > 5)
            {
                float rCD_Sec = (15000 - (float) (Environment.TickCount - LastR))/1000;
                string rCd_Str = rCD_Sec.ToString("0.0");

                Text rCdText = new Text(rCd_Str, new Font("Euphemia", 18F, FontStyle.Bold))
                {
                    Color = Color.Orange
                };
                rCdText.Position = Player.Instance.Position.WorldToScreen() -
                                   new Vector2((float) rCdText.Bounding.Width/2, -50);
                rCdText.Draw();
            }

            if (RivenMenu.menu["drawBurst"].Cast<CheckBox>().CurrentValue)
            {
                float maxRange = !Flash.IsReady() ?  350 : 350+425;
                new Circle(new ColorBGRA(new Vector4(255, 0, 0, 1)), maxRange).Draw(me.Position);
            }
        }

        private static void OnProcCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            if (args.SData.Name.Contains("ItemTiamatCleave")) forceItem = false;
            if (args.SData.Name.Contains("RivenTriCleave")) forceQ = false;
            if (args.SData.Name.Contains("RivenMartyr")) forceW = false;
            if (args.SData.Name == "RivenFengShuiEngine") forceR = false;
            if (args.SData.Name == "RivenIzunaBlade") forceR2 = false;
        }

        public static double GetUltDamage(Obj_AI_Base target, double health)
        {
            if (target != null && target.IsValid)
            {
                var missinghealth = (target.MaxHealth - health) / target.MaxHealth > 0.75 ? 0.75 : (target.MaxHealth - health) / target.MaxHealth;
                var pluspercent = missinghealth * 8 / 3;
                var rawdmg = new double[] { 80, 120, 160 }[R.Level - 1] + 0.6 * me.FlatPhysicalDamageMod;
                return me.CalculateDamageOnUnit(target, DamageType.Physical, (float)rawdmg * (float)(1 + pluspercent));
            }
            return 0;
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            ForceSkills();
            if (Environment.TickCount - LastQ >= 3550 && QStacks > 0 &&
                !me.IsRecalling() && Q.IsReady())
            {
                Q.Cast(me.Position);
            }

            if (RivenMenu.menu["burst"].Cast<KeyBind>().CurrentValue)
                Burst();

            var target = GetTarget();
            if (target != null && target.IsValid)
                if (RivenMenu.menu["useR2"].Cast<CheckBox>().CurrentValue && target.Distance(me) > me.GetAutoAttackRange())
                {
                    if (target is AIHeroClient && !target.IsZombie && !target.IsDead && IsSecondR && R.IsReady() &&
                            target.Health < GetUltDamage(target, target.Health) && RivenMenu.menu["useR2"].Cast<CheckBox>().CurrentValue)
                        R2.Cast(R2.GetPrediction(target).CastPosition);
                }

            Combo(target);

            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Flee)
            {
                E.Cast(me.Position.Extend(Game.CursorPos, E.Range).To3D());
                if (!E.IsReady() && Environment.TickCount - LastE > 100) Q.Cast(me.Position.Extend(Game.CursorPos, Q.Range).To3D());
            }
        }

        private static void Combo(Obj_AI_Base target)
        {
            if (target == null || !target.IsValid)
                return;

            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo && target.Distance(me) > me.GetAutoAttackRange() &&
                    E.IsReady() && RivenMenu.menu["gapE"].Cast<CheckBox>().CurrentValue)
                E.Cast(target.Position);
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo && target.Distance(me) > me.GetAutoAttackRange() &&
                QStacks == 0 && RivenMenu.menu["gapQ1"].Cast<CheckBox>().CurrentValue)
                ForceCastQ(target);
        }

        private static bool InWRange(GameObject target)
        {
            if (target == null || !target.IsValid)
                return false;

            return me.HasBuff("RivenFengShuiEngine") 
                ? me.Distance(target.Position) <= 330
                : me.Distance(target.Position) <= 265;
        }

        private static void AiHeroClientOnOnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (args.SData.Name.Contains("Tiamat"))
            {
                Core.DelayAction(() => Player.IssueOrder(GameObjectOrder.AttackTo, GetTarget()), 
                    (int)Orbwalker.AttackCastDelay*1000 + Orbwalker.ExtraWindUpTime+90);
                if (RivenMenu.menu["burst"].Cast<KeyBind>().CurrentValue)
                    Core.DelayAction(ForceR2, (int)Orbwalker.AttackCastDelay * 1000 + Orbwalker.ExtraWindUpTime + 90);
            }

            if (!args.IsAutoAttack())
                return;

            ExectuteSpellsAfterAA();

            if (!sender.IsEnemy || sender.Type != me.Type || Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LastHit) return;

            var epos = me.ServerPosition +
                       (me.ServerPosition - sender.ServerPosition).Normalized() * 300;

            if (me.Distance(sender.ServerPosition) <= args.SData.CastRange)
            {
                #region SPAM
                switch (args.SData.TargettingType)
                {
                    case SpellDataTargetType.Unit:

                        if (args.Target.NetworkId == me.NetworkId)
                        {
                            if (!args.SData.Name.Contains("NasusW"))
                            {
                                if (E.IsReady()) E.Cast(epos);
                            }
                        }

                        break;
                    case SpellDataTargetType.SelfAoe:

                        if (E.IsReady()) E.Cast(epos);

                        break;
                }
                if (args.SData.Name.Contains("IreliaEquilibriumStrike"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (W.IsReady() && InWRange(sender)) W.Cast();
                        else if (E.IsReady()) E.Cast(epos);
                    }
                }
                if (args.SData.Name.Contains("TalonCutthroat"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (W.IsReady()) W.Cast();
                    }
                }
                if (args.SData.Name.Contains("RenektonPreExecute"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (W.IsReady()) W.Cast();
                    }
                }
                if (args.SData.Name.Contains("GarenRPreCast"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast(epos);
                    }
                }
                if (args.SData.Name.Contains("GarenQAttack"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("XenZhaoThrust3"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (W.IsReady()) W.Cast();
                    }
                }
                if (args.SData.Name.Contains("RengarQ"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("RengarPassiveBuffDash"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("RengarPassiveBuffDashAADummy"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("TwitchEParticle"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("FizzPiercingStrike"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("HungeringStrike"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("YasuoDash"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("KatarinaRTrigger"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (W.IsReady() && InWRange(sender)) W.Cast();
                        else if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("YasuoDash"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("KatarinaE"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (W.IsReady()) W.Cast();
                    }
                }
                if (args.SData.Name.Contains("MonkeyKingQAttack"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("MonkeyKingSpinToWin"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                        else if (W.IsReady()) W.Cast();
                    }
                }
                if (args.SData.Name.Contains("MonkeyKingQAttack"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("MonkeyKingQAttack"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                if (args.SData.Name.Contains("MonkeyKingQAttack"))
                {
                    if (args.Target.NetworkId == me.NetworkId)
                    {
                        if (E.IsReady()) E.Cast();
                    }
                }
                #endregion
            }
        }

        private static void ExectuteSpellsAfterAA()
        {
            if (RivenMenu.menu["burst"].Cast<KeyBind>().CurrentValue)
            {
                if (!W.IsReady())
                    ForceR2();
                return;
            }

            if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Combo && Orbwalker.ActiveModesFlags !=
                Orbwalker.ActiveModes.LaneClear && Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.JungleClear)
                return;

            /*set target*/
            Obj_AI_Base target = GetTarget();

            bool castE = Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Combo ||
                         Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo &&
                         RivenMenu.menu["useE"].Cast<CheckBox>().CurrentValue;
            castE = castE && E.IsReady();
            if (target != null && target.IsValid)
            {
                if (W.IsReady() && InWRange(target))
                {
                    ForceItem();
                    Core.DelayAction(() => W.Cast(), 1);
                }
                else if (W.IsReady() && E.IsReady() && !InWRange(target) && me.Distance(target) < E.Range)
                {
                    E.Cast(target.Position);
                    Core.DelayAction(ForceItem, 10);
                    Core.DelayAction(() => ForceW(), 240);
                }
                else if (Q.IsReady())
                {
                    ForceItem();
                    Core.DelayAction(() => ForceCastQ(target), 1);
                }
                else if (castE)
                    E.Cast(target.ServerPosition);

                if (target is AIHeroClient && !target.IsZombie && !target.IsDead && IsSecondR && R.IsReady() &&
                    target.Health < GetUltDamage(target, target.Health) && RivenMenu.menu["useR2"].Cast<CheckBox>().CurrentValue)
                    ForceR2();

                if (RivenMenu.menu["useR1"].Cast<KeyBind>().CurrentValue && IsFirstR && R.IsReady() && Orbwalker.ActiveModesFlags ==
                    Orbwalker.ActiveModes.Combo)
                {
                    ForceR();
                }
            }
        }

        private static Obj_AI_Base GetTarget()
        {
            Obj_AI_Base target = null;
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
                target = TargetSelector.GetTarget(250 + me.AttackRange + 70, DamageType.Physical);
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {
                var Mobs =
                    EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, me.Position, 250 + me.AttackRange + 70)
                        .OrderByDescending(x => x.MaxHealth).ToList();
                target = Mobs.FirstOrDefault();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear)
            {
                var Mobs =
                    EntityManager.MinionsAndMonsters.GetJungleMonsters(me.Position, 250 + me.AttackRange + 70)
                        .OrderByDescending(x => x.MaxHealth).ToList();
                target = Mobs.FirstOrDefault();
            }
            

            return target;
        }

        static bool IsFirstR
        {
            get { return me.Spellbook.GetSpell(SpellSlot.R).SData.Name.Contains("RivenFengShuiEngine"); }
        }

        static bool IsSecondR
        {
            get { return !me.Spellbook.GetSpell(SpellSlot.R).SData.Name.Contains("RivenFengShuiEngine"); }
        }

        static bool forceQ, forceW, forceR, forceItem, forceR2;
        private static Obj_AI_Base ForceQTarget;
        private static void ForceSkills()
        {
            if (forceQ && ForceQTarget != null && ForceQTarget.IsValidTarget(E.Range + me.BoundingRadius + 70) &&
                Q.IsReady())
            {
                Q.Cast(ForceQTarget.Position);
            }
            if (forceW) W.Cast();
            if (forceR && IsFirstR) R.Cast();
            if (forceItem && Hydra.IsOwned() && Hydra.IsReady()) Hydra.Cast();
            if (forceItem && Tiamat.IsOwned() && Tiamat.IsReady()) Tiamat.Cast();
            if (forceR2 && IsSecondR)
            {
                var target = TargetSelector.SelectedTarget;
                if (target != null && target.IsValid) R2.Cast(target.Position);
            }
        }

        private static void Reset()
        {
            Player.DoEmote(Emote.Dance);
            Orbwalker.ResetAutoAttack();
            Player.IssueOrder(GameObjectOrder.MoveTo, me.Position.Extend(Game.CursorPos, me.Distance(Game.CursorPos) + 10).To3D());
        }

        static bool ItemReady
        {
            get { return (Hydra.IsOwned() && Hydra.IsReady()) || (Tiamat.IsOwned() && Tiamat.IsReady()); }
        }
        
        private static void ObjAiBaseOnOnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe)
                return;

            int QD = RivenMenu.menu["qDelay"].Cast<Slider>().CurrentValue, 
                QLD = RivenMenu.menu["q3Delay"].Cast<Slider>().CurrentValue;
            bool inFightMode = Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None &&
                               Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LastHit &&
                               Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Flee;
            switch (args.Animation)
            {
                case "Spell1a":
                    LastQ = Environment.TickCount;
                    if (inFightMode)
                        Core.DelayAction(Reset, QD * 10 + 1);
                    break;
                case "Spell1b":
                    LastQ = Environment.TickCount;
                    if (inFightMode)
                        Core.DelayAction(Reset, QD * 10 + 1);
                    break;
                case "Spell1c":
                    LastQ = Environment.TickCount;
                    if (inFightMode)
                        Core.DelayAction(Reset, QLD * 10 + 3);
                    break;
                case "Spell2":
                    LastW = Environment.TickCount;
                    if (inFightMode || RivenMenu.menu["burst"].Cast<KeyBind>().CurrentValue)
                    {
                        Core.DelayAction(Reset, 50*10 + 3);
                        if (ItemReady)
                            ForceItem();
                        if (Q.IsReady())
                        {
                            ForceQTarget = GetTarget();
                            forceQ = true;
                        }
                    }
                    break;
                case "Spell3":
                    LastE = Environment.TickCount;
                    if (inFightMode)
                        Core.DelayAction(Reset, 45 * 10 + 3);
                    break;
                case "Spell4a":
                    LastR = Environment.TickCount;
                    break;
                case "Spell4b":
                    var target = TargetSelector.SelectedTarget;
                    if (Q.IsReady() && target.IsValidTarget()) ForceCastQ(target);
                    break;
            }
        }

        private static void ForceItem()
        {
            forceItem = true;
            Core.DelayAction(() => forceItem = false, 500);
        }
        private static void ForceR()
        {
            forceR = R.IsReady() && IsFirstR;
            Core.DelayAction(() => forceR = false, 500);
        }
        private static void ForceR2()
        {
            forceR2 = R.IsReady() && IsSecondR;
            Core.DelayAction(() => forceR2 = false, 500);
        }
        private static void ForceW(bool burst = false)
        {
            forceW = W.IsReady(); 
            Core.DelayAction(() => forceW = false, burst ? 1000 : 500);
        }

        private static void ForceCastQ(AttackableUnit target)
        {
            forceQ = true;
            ForceQTarget = (Obj_AI_Base)target;
        }

        static int LastR { get; set; }

        static int LastQ { get; set; }
        private static int LastE { get; set; }

        static int LastW { get; set; }
        private static void Burst()
        {
            Orbwalker.OrbwalkTo(Game.CursorPos);

            var target = TargetSelector.SelectedTarget;
            if (target == null || !target.IsValidTarget() || target.IsZombie || target.IsDead)
            {
                return;
            }

            bool distTooHigh = Flash.IsReady()
                ? me.Distance(target.Position) > 350 + 425 : me.Distance(target.Position) > 350;
            bool needFlash = me.Distance(target.Position) > 350 && me.Distance(target.Position) < 350 + 425;

            if (needFlash && !distTooHigh)
                Flash.Cast(me.Position.Extend(target, Flash.Range).To3D());

            if (R.IsReady() && IsFirstR && E.IsReady() && W.IsReady() && Q.IsReady() && !distTooHigh)
            {
                E.Cast(target.Position);
                ForceR();
                Core.DelayAction(() => ForceCastQ(target), 150);
                Core.DelayAction(() => ForceW(true), 160);
            }
        }
    }
}
