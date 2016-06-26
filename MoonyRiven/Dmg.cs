using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;

namespace MoonyRiven
{
    class Dmg : Program
    {
        public static int Qstack = 1;
        public static double BasicDmg(Obj_AI_Base target)
        {
            if (target != null)
            {
                double dmg = 0;
                double passivenhan;
                if (ObjectManager.Player.Level >= 18)
                    passivenhan = 0.5;
                else if (ObjectManager.Player.Level >= 15)
                    passivenhan = 0.45;
                else if (ObjectManager.Player.Level >= 12)
                    passivenhan = 0.4;
                else if (ObjectManager.Player.Level >= 9)
                    passivenhan = 0.35;
                else if (ObjectManager.Player.Level >= 6)
                    passivenhan = 0.3;
                else if (ObjectManager.Player.Level >= 3)
                    passivenhan = 0.25;
                else
                    passivenhan = 0.2;
                if (W.IsReady()) dmg = dmg + ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
                if (Q.IsReady())
                {
                    var qnhan = 4 - Qstack;
                    dmg = dmg + ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) * qnhan +
                        ObjectManager.Player.GetAutoAttackDamage(target) * qnhan * (1 + passivenhan);
                }
                dmg = dmg + ObjectManager.Player.GetAutoAttackDamage(target) * (1 + passivenhan);
                return dmg;
            }
            return 0;
        }


        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            if (enemy != null)
            {
                float damage = 0;
                float passivenhan;
                if (ObjectManager.Player.Level >= 18)
                    passivenhan = 0.5f;
                else if (ObjectManager.Player.Level >= 15)
                    passivenhan = 0.45f;
                else if (ObjectManager.Player.Level >= 12)
                    passivenhan = 0.4f;
                else if (ObjectManager.Player.Level >= 9)
                    passivenhan = 0.35f;
                else if (ObjectManager.Player.Level >= 6)
                    passivenhan = 0.3f;
                else if (ObjectManager.Player.Level >= 3)
                    passivenhan = 0.25f;
                else
                    passivenhan = 0.2f;

                if (W.IsReady()) damage = damage + ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);
                if (Q.IsReady())
                {
                    var qnhan = 4 - Qstack;
                    damage = damage + ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q) * qnhan +
                        ObjectManager.Player.GetAutoAttackDamage(enemy) * qnhan * (1 + passivenhan);
                }
                damage = damage + (float)ObjectManager.Player.GetAutoAttackDamage(enemy) * (1 + passivenhan);
                if (R.IsReady())
                {
                    return damage * 1.2f + ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R);
                }
                return damage;
            }
            return 0;
        }

        public static bool IsKillableR(AIHeroClient target)
        {
            return !target.IsInvulnerable && TotalDmg(target) >= target.Health &&
                   BasicDmg(target) <= target.Health;
        }

        public static double TotalDmg(Obj_AI_Base target)
        {
            if (target == null) return 0;
            double dmg = 0;
            double passivenhan;
            if (ObjectManager.Player.Level >= 18)
                passivenhan = 0.5;
            else if (ObjectManager.Player.Level >= 15)
                passivenhan = 0.45;
            else if (ObjectManager.Player.Level >= 12)
                passivenhan = 0.4;
            else if (ObjectManager.Player.Level >= 9)
                passivenhan = 0.35;
            else if (ObjectManager.Player.Level >= 6)
                passivenhan = 0.3;
            else if (ObjectManager.Player.Level >= 3)
                passivenhan = 0.25;
            else
                passivenhan = 0.2;

            if (W.IsReady()) dmg = dmg + ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
            if (Q.IsReady())
            {
                var qnhan = 4 - Qstack;
                dmg = dmg + ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q)
                    * qnhan + ObjectManager.Player.GetAutoAttackDamage(target) * qnhan * (1 + passivenhan);
            }
            dmg = dmg + ObjectManager.Player.GetAutoAttackDamage(target) * (1 + passivenhan);
            if (!R.IsReady()) return dmg;
            var rdmg = Rdmg(target, target.Health - dmg * 1.2);
            return dmg * 1.2 + rdmg;
        }
        public static bool IsLethal(Obj_AI_Base unit)
        {
            return GetComboDamage(unit) / 1.65 >= unit.Health;
        }
        public static double Rdmg(Obj_AI_Base target, double health)
        {
            if (target != null)
            {
                var missinghealth = (target.MaxHealth - health) / target.MaxHealth > 0.75 ? 0.75 : (target.MaxHealth - health) / target.MaxHealth;
                var pluspercent = missinghealth * 2;
                var rawdmg = new double[] { 80, 120, 160 }[R.Level - 1] + 0.6 * ObjectManager.Player.FlatPhysicalDamageMod;
                return ObjectManager.Player.CalculateDamageOnUnit(target, DamageType.Physical, (float)rawdmg * (float)(1 + pluspercent));
            }
            return 0;
        }
    }
}
