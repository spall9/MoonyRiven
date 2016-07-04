using System;
using System.Collections.Generic;
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
        protected static Spell.Skillshot Q;
        protected static Spell.Active W;
        protected static Spell.Skillshot E;
        protected static Spell.Active R;
        protected static Spell.Skillshot R2;
        protected static Spell.Targeted Flash;

        static Item Hydra;
        static Item Tiamat;
        private static readonly AIHeroClient me = ObjectManager.Player;

        static int QStacks
        {
            get { return ObjectManager.Player.HasBuff("RivenTriCleave") ? ObjectManager.Player.GetBuff("RivenTriCleave").Count : 0; }
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
                R2 = new Spell.Skillshot(SpellSlot.R, 800, SkillShotType.Cone, 250, 1600, 125);
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
                Game.OnWndProc += GameOnOnWndProc;
            };
        }

        private static void GameOnOnWndProc(WndEventArgs args)
        {
            if (RivenMenu.combo["burst"].Cast<KeyBind>().CurrentValue && Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo &&
                args.WParam == 0x0101)
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
        }

        private static void GapcloserOnOnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloserEventArgs)
        {
            if (!sender.IsEnemy)
                return;

            if (gapcloserEventArgs.End.Distance(me) <= W.Range && RivenMenu.misc["antiGapW"].Cast<CheckBox>().CurrentValue)
                W.Cast();
        }

        private static bool drawException;
        private static void DrawingOnOnDraw(EventArgs args)
        {
            try
            {
                bool useR1 = RivenMenu.combo["useR1.Combo"].Cast<KeyBind>().CurrentValue;
                var heropos = Drawing.WorldToScreen(ObjectManager.Player.Position);
                Drawing.DrawText(heropos.X - 40, heropos.Y + 20, Color.LightBlue, "Always R  [        ]");
                Drawing.DrawText(heropos.X + 40, heropos.Y + 20, useR1 ? Color.LightGreen : Color.Red,
                    useR1 ? "On" : "Off");

                if (me.Level > 5)
                    if (IsSecondR && RivenMenu.drawings["drawRExpire"].Cast<CheckBox>().CurrentValue)
                    {
                        float rCD_Sec = (15000 - (float) (Environment.TickCount - LastR))/1000;
                        string rCd_Str = rCD_Sec.ToString("0.0");

                        Text rCdText = new Text(rCd_Str, new Font("Gill Sans MT Pro Book", 20f, FontStyle.Bold))
                        {
                            Color = rCD_Sec <= 5 && rCD_Sec > 2 ? Color.DarkOrange : Color.Red
                        };
                        rCdText.Position = heropos - new Vector2((float) rCdText.Bounding.Width/2, -100);

                        if (rCD_Sec <= 5 && rCD_Sec > 0)
                            rCdText.Draw();
                    }

                if (RivenMenu.drawings["drawBurst"].Cast<CheckBox>().CurrentValue)
                {
                    float maxRange = !Flash.IsReady()
                        ? E.Range + me.GetAutoAttackRange(TargetSelector.SelectedTarget)
                        : 700;
                    new Circle(new ColorBGRA(new Vector4(255, 0, 0, 1)), maxRange).Draw(me.Position);
                }

                if (RivenMenu.drawings["drawUltimateCone"].Cast<CheckBox>().CurrentValue)
                {
                    UltimateCone.Draw();
                }
            }
            catch
            {
                if (!drawException && Game.Time <= 50)
                {
                    drawException = true;
                    for (int i = 0; i < 5; i++)
                    {
                        Chat.Print("Please reload the addons");
                    }
                }
            }

        }

        private static void OnProcCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            bool inFightMode = Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None &&
                               Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LastHit &&
                               Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Flee;
            if (args.SData.Name.Contains("RivenMartyr") && inFightMode && Enabled("useQ") && Q.IsReady())
            {
                ForceItem();
            }

            bool inBurstMode = RivenMenu.combo["burst"].Cast<KeyBind>().CurrentValue;
            if (inBurstMode && args.SData.Name.Contains("ItemTiamatCleave"))
            {
                var target = TargetSelector.SelectedTarget;
                R2.Cast(me.Position.Extend(target, R2.Range).To3D());
                ForceR2();
            }

            if (args.SData.Name.Contains("ItemTiamatCleave")) forceItem = false;
            if (args.SData.Name.Contains("RivenTriCleave")) { forceQ = false;}
            if (args.SData.Name.Contains("RivenMartyr")) forceW = false;
            if (args.SData.Name == "RivenFengShuiEngine") forceR = false;
            if (args.SData.Name == "RivenIzunaBlade") forceR2 = false;
            if (args.SData.IsAutoAttack()) ;
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            ForceSkills();
            if (Environment.TickCount - LastQ >= 3600 && QStacks > 0 &&
                !me.IsRecalling() && Q.IsReady())
            {
                Q.Cast(me.Position);
            }

            if (RivenMenu.combo["burst"].Cast<KeyBind>().CurrentValue)
                Burst();
            else
                Combo();

            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Flee)
            {
                E.Cast(me.Position.Extend(Game.CursorPos, E.Range).To3D());
                if (!E.IsReady() && Environment.TickCount - LastE > 100) Q.Cast(me.Position.Extend(Game.CursorPos, Q.Range).To3D());
            }
        }

        private static void Combo()
        {
            bool checkForUltimateCone = RivenMenu.ultimate["rDmgMethod"].Cast<ComboBox>().CurrentValue == 1 && IsSecondR &&
                                        R.IsReady();
            if (checkForUltimateCone || RivenMenu.drawings["debugDraw"].Cast<CheckBox>().CurrentValue)
            {
                CheckUltimateHits();
            }

            bool inFightMode = Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None &&
                               Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LastHit &&
                               Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Flee;
            var target = GetTarget();
            if (target == null || !target.IsValid || !inFightMode)
                return;

            bool useW = InWRange(target) && W.IsReady() && Enabled("useW");
            if (useW)
                ForceW();

            if (RivenMenu.combo["useR2.Combo"].Cast<CheckBox>().CurrentValue && target.Distance(me) > me.GetAutoAttackRange() + 70)
            {
                if (target is AIHeroClient && !target.IsZombie && !target.IsDead && IsSecondR && R.IsReady() &&
                        Dmg.IsKillableR((AIHeroClient)target) && RivenMenu.combo["useR2.Combo"].Cast<CheckBox>().CurrentValue)
                    ForceR2();
            }
            
            bool targetOutOfAA = me.Distance(target.Position) > me.GetAutoAttackRange();
            bool useEGap = E.IsReady() && RivenMenu.combo["gapE.Combo"].Cast<CheckBox>().CurrentValue;
            bool useQGap = QStacks == 0 && RivenMenu.combo["gapQ1.Combo"].Cast<CheckBox>().CurrentValue;

            if (targetOutOfAA && useEGap && !useW)
                E.Cast(target.Position);
            if (targetOutOfAA && useQGap && !useEGap && !useW)
                ForceCastQ(target);
        }

        private static void CheckUltimateHits()
        {
            int maxHitCount = 0;
            Vector2 bestEndVec = Vector2.Zero;
            Geometry.Polygon BestCone = null;

            List<Vector2> hitPositions = new List<Vector2>();

            for (int i = 0; i < 360; i += 21 - RivenMenu.ultimate["coneAngleStep"].Cast<Slider>().CurrentValue)
            {
                var endVec = PointOnCircle(i);
                Geometry.Polygon Cone = CreateUltimateCone(endVec);

                int currentHits = EntityManager.Heroes.Enemies.Where(x => x.IsValid && !x.IsDead && !x.IsZombie).Count(
                    x =>
                    {
                        int rSpeed = 1000;
                        var pred = Prediction.Position.PredictUnitPosition(x, (int)x.Distance(me) / rSpeed * 1000);

                        if (Cone.IsInside(pred))
                            hitPositions.Add(pred);

                        return Cone.IsInside(pred);
                    });
                if (currentHits > maxHitCount)
                {
                    maxHitCount = currentHits;
                    bestEndVec = endVec;

                    BestCone = Cone;
                }
            }

            if (bestEndVec != Vector2.Zero &&
                maxHitCount >= RivenMenu.ultimate["rmaxDmgHitCount"].Cast<Slider>().CurrentValue)
            {
                UltimateCone.LastBestCone = BestCone;
                UltimateCone.LastPredictedInsidePositions = hitPositions;

                if (IsSecondR && R.IsReady())
                {
                    ForceR2(me.Position.Extend(bestEndVec, 100));
                }
            }
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

            if (args.SData.Name.Contains("ItemTiamatCleave"))
            {
                HandleTiamatCast();
            }
            else if (args.SData.Name.Contains("RivenMartyr"))
            {
                ForceAA();
            }

            if (!args.IsAutoAttack())
                return;

            if (Orbwalker.DisableMovement)
                Orbwalker.DisableMovement = false;

            ExecuteSpellsAfterAA();

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

        private static void HandleTiamatCast()
        {
            if (RivenMenu.combo["burst"].Cast<KeyBind>().CurrentValue)
            {
                var target = TargetSelector.SelectedTarget;
                R2.Cast(me.Position.Extend(target, R2.Range).To3D());
                ForceR2();
            }
            else if (GetTarget() != null && GetTarget().IsValid)
            {
                ForceAA();
            }
        }

        static bool Enabled(string id)
        {
            return (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo &&
                    RivenMenu.combo[id + ".Combo"].Cast<CheckBox>().CurrentValue) ||
                   (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear &&
                    RivenMenu.waveClear[id + ".WaveClear"].Cast<CheckBox>().CurrentValue) ||
                   (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear &&
                    RivenMenu.jungleClear[id + ".JungleClear"].Cast<CheckBox>().CurrentValue);
        }

        /// <summary>
        /// op vec
        /// </summary>
        static Vector2 PointOnCircle(float angleInDegrees)
        {
            float x = me.Position.X + (float)(R2.Range * Math.Cos(angleInDegrees * Math.PI / 180));
            float y = me.Position.Y + (float)(R2.Range * Math.Sin(angleInDegrees * Math.PI / 180));

            return new Vector2(x, y);
        }

        static Vector2 RotateAroundPoint(Vector2 center, Vector2 vec, float theta)
        {
            float px = vec.X, py = vec.Y;
            float ox = center.X, oy = center.Y;

            float x = (float)Math.Cos(theta)*(px - ox) - (float)Math.Sin(theta)*(py - oy) + ox;
            float y = (float)Math.Sin(theta)*(px - ox) + (float)Math.Cos(theta)*(py - oy) + oy;
            return new Vector2(x,y);
        }

        static Geometry.Polygon CreateUltimateCone(Vector2 endVec)
        {
            Geometry.Polygon cone = new Geometry.Polygon();
            var edgePoint1 = endVec + (endVec - me.Position.To2D()).Perpendicular2().Normalized() * 200;
            //var edgePoint2 = endVec + (endVec - me.Position.To2D()).Perpendicular().Normalized()*200;
            cone.Points.Add(me.Position.To2D());

            for (int currentAngle = 0; currentAngle <= 40; currentAngle += 1)
            {
                var newP = RotateAroundPoint(me.Position.To2D(), edgePoint1, currentAngle* ((float)Math.PI/180));
                cone.Points.Add(newP);
            }

            return cone;
        }

        private static void ExecuteSpellsAfterAA()
        {
            /*set target*/
            Obj_AI_Base target = GetTarget();

            bool inBurstMode = RivenMenu.combo["burst"].Cast<KeyBind>().CurrentValue;
            if (inBurstMode)
            {
                ForceItem(true);
                Core.DelayAction(() => ForceCastQ(target), 100);
                return;
            }

            if (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Combo && Orbwalker.ActiveModesFlags !=
            Orbwalker.ActiveModes.LaneClear && Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.JungleClear)
            return;

            if (target is AIHeroClient && !target.IsZombie && !target.IsDead && IsSecondR && R.IsReady() &&
                    Dmg.IsKillableR((AIHeroClient)target) && RivenMenu.combo["useR2.Combo"].Cast<CheckBox>().CurrentValue)
                ForceR2();

            if (RivenMenu.combo["useR1.Combo"].Cast<KeyBind>().CurrentValue && IsFirstR && R.IsReady() &&
                Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                ForceR();
            }

            bool castQ = Enabled("useQ") && Q.IsReady();
            bool castW = Enabled("useW") && W.IsReady();
            bool castE = Enabled("useE") && E.IsReady();
            bool castItem = Enabled("useItem");

            if (target != null && target.IsValid)
            {
                if (W.IsReady() && E.IsReady() && !InWRange(target) && me.Distance(target) < E.Range && castW && castE)
                {
                    E.Cast(target.Position);
                    if (castItem)
                        Core.DelayAction(() => ForceItem(), 10);

                    Core.DelayAction(() => ForceW(), 240);
                }
                else if (Q.IsReady() && castQ)
                {
                    if (castItem && ItemReady)
                    {
                        ForceItem();
                        Core.DelayAction(() => ForceCastQ(target), 100);
                    }
                    else
                        ForceCastQ(target);
                }
                else if (castE)
                    E.Cast(target.ServerPosition);
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
            if (RivenMenu.combo["burst"].Cast<KeyBind>().CurrentValue)
                target = TargetSelector.SelectedTarget;

            return target;
        }

        static bool IsFirstR
        {
            get { return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.Name.Contains("RivenFengShuiEngine"); }
        }

        static bool IsSecondR
        {
            get { return !ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.Name.Contains("RivenFengShuiEngine"); }
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
                R2.Cast(R2ForcePos.To3D());
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

            int QD = RivenMenu.misc["qDelay"].Cast<Slider>().CurrentValue,
                QLD = RivenMenu.misc["q3Delay"].Cast<Slider>().CurrentValue;
            bool inFightMode = Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None &&
                               Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LastHit &&
                               Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Flee;
            switch (args.Animation)
            {
                case "Spell1a":
                    Dmg.Qstack = 2;
                    LastQ = Environment.TickCount;
                    if (inFightMode)
                        Core.DelayAction(Reset, QD * 10 + 1);
                    break;
                case "Spell1b":
                    Dmg.Qstack = 3;
                    LastQ = Environment.TickCount;
                    if (inFightMode)
                        Core.DelayAction(Reset, QD * 10 + 1);
                    break;
                case "Spell1c":
                    Dmg.Qstack = 1;
                    LastQ = Environment.TickCount;
                    if (inFightMode)
                        Core.DelayAction(Reset, QLD * 10 + 3);
                    break;
                case "Spell2":
                    LastW = Environment.TickCount;
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
                    if (RivenMenu.combo["burst"].Cast<KeyBind>().CurrentValue)
                        Core.DelayAction(() => Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Combo, 200);
                    break;
            }
        }

        private static void ForceItem(bool burst = false)
        {
            forceItem = true;
            Core.DelayAction(() => forceItem = false, !burst ? 500 : 2000);
        }
        private static void ForceR()
        {
            forceR = R.IsReady() && IsFirstR;
            Core.DelayAction(() => forceR = false, 500);
        }

        private static Vector2 R2ForcePos;
        private static void ForceR2(Vector2 pos = new Vector2())
        {
            bool gotTarget = GetTarget() != null && !GetTarget().IsDead && GetTarget().IsValid;
            if (gotTarget || pos != new Vector2())
            {
                R2ForcePos = pos == new Vector2() ? me.Position.Extend(GetTarget(), 100) : pos;
                forceR2 = R.IsReady() && IsSecondR;
                Core.DelayAction(() => forceR2 = false, 1500);
            }
        }

        static void ForceAA()
        {
            Player.DoEmote(Emote.Dance);
            Orbwalker.ResetAutoAttack();
            Player.IssueOrder(GameObjectOrder.MoveTo, me.Position.Extend(Game.CursorPos, me.Distance(Game.CursorPos) + 10).To3D());
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
        static void Burst()
        {
            var target = TargetSelector.SelectedTarget;
            Orbwalker.OrbwalkTo(Game.CursorPos);
            if (target == null || !target.IsValidTarget() || target.IsZombie || target.IsInvulnerable) return;

            if (Orbwalker.CanAutoAttack && me.IsInAutoAttackRange(target))
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);          

            if (Flash.IsReady())
            {
                if (me.Distance(target.Position) > 700 || me.Distance(target) < E.Range + me.AttackRange) return;

                if (!R.IsReady() || !E.IsReady() || !W.IsReady() || !IsFirstR) return;

                E.Cast(target.Position);
                ForceR();
                Core.DelayAction(() =>
                {
                    var targett = TargetSelector.SelectedTarget;
                    if (targett != null && target.IsValidTarget() && !target.IsZombie)
                    {
                        Core.DelayAction(() => 
                        Flash.Cast(targett.Distance(me) >= Flash.Range ? 
                                me.Position.Extend(targett, Flash.Range).To3D() : targett.Position), 10);
                        Core.RepeatAction(() => ForceW(), 170, 1000);
                    }
                }, 180);
            }
            else
            {
                if (me.Distance(target) > E.Range) return;

                if (E.IsReady() && R.IsReady())
                {
                    E.Cast(target.ServerPosition);
                    ForceR();
                    Core.RepeatAction(() => ForceW(), 160, 1000);
                }
            }
        }
    }
}
