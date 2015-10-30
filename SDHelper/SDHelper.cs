using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EZGUI;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.AbilityInfo;

using SharpDX;
using SharpDX.Direct3D9;

namespace SDHelper
{
    class SDHelper
    {

        #region Fields
        private static string Release = "0.0.0.1";
        private static string AName = "Shadow Demon Helper";

        private static bool Loaded = false;

        private static EzGUI Interface;
        private static EzElement Enabled;
        
        /* Creeps */
        private static EzElement CreepsFarm;
        private static EzElement CKillIndicator;
        private static EzElement CKillCountIndicator;
        private static EzElement CKillOnlyRange;

        /* Heroes */
        private static EzElement HeroKill;
        private static EzElement HKillIndicator;
        private static EzElement HKillCountIndicator;
        private static EzElement HKillOnlyRange;

        private static Hero MyHero;
        private static Ability ShadowPoison;
        private static Ability ShadowPoisonRelease;

        private static int[] _ShadowPoisonDamage = new int[4] { 20, 35, 50, 65 };

        #endregion

        public static void Init()
        {
            CreateInterface();
            Drawing.OnEndScene += Game_OnUpdate;
        }

        #region Update
        static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || !Enabled.isActive)
            {
                if (Loaded) 
                {
                    Loaded = false;
                    Console.WriteLine("SDHelper successfully unloaded.");
                }
                return;
            }

            if (!Loaded)
            {
                MyHero = ObjectMgr.LocalHero;
                /* ERRORS */
                if (MyHero == null) return;
                if (MyHero.ClassID != ClassID.CDOTA_Unit_Hero_Shadow_Demon)
                {
                    Interface.isVisible = false;
                    return;
                }
                if (MyHero.Spellbook == null) return;
                if (MyHero.Spellbook.SpellE == null) return;
                if (MyHero.Spellbook.SpellD == null) return;
                /* /ERRORS */
                ShadowPoison = MyHero.Spellbook.SpellE;
                ShadowPoisonRelease = MyHero.Spellbook.SpellD;
                Loaded = true;
                //
                Interface.isVisible = true;
                Interface.SetTitle(AName);
                //
                Console.WriteLine("SDHelper successfully loaded.");
            }

            if (ShadowPoison.Level == 0) return;

            if (Utils.SleepCheck("k_delay") && CanRelease() && (CreepsFarm.isActive || HeroKill.isActive))
            {
                if (CreepsFarm.isActive)
                {
                    var creeps = GetCreeps(CKillOnlyRange.isActive ? true : false);

                    foreach (var creep in creeps)
                    {
                        if (CanKill(creep)) ShadowPoison_Release();
                    }
                }

                if (HeroKill.isActive)
                {
                    var heroes = GetHeroes(HKillOnlyRange.isActive ? true : false);

                    foreach (var hero in heroes)
                    {
                        if (CanKill(hero)) ShadowPoison_Release();
                    }
                }

                Utils.Sleep(200, "k_delay");
            }

            if (CKillIndicator.isActive)
            {
                var creeps = GetCreeps(true);

                foreach (var creep in creeps)
                {
                    if (CKillCountIndicator.isActive) DrawInfo(creep, true);
                    else DrawInfo(creep, false);
                }
            }

            if (HKillIndicator.isActive)
            {
                var heroes = GetHeroes(true);

                foreach (var hero in heroes)
                {
                    if (HKillCountIndicator.isActive) DrawInfo(hero, true);
                    else DrawInfo(hero, false);
                }
            }

        }
        #endregion

        #region Methods
        private static void CreateInterface()
        {
            Interface = new EzGUI(Drawing.Width - 350, 60, AName);
            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Main", false));
            Enabled = new EzElement(ElementType.CHECKBOX, "Enabled", true);
            Interface.AddMainElement(Enabled);
            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Options", false));

            // Creeps
            var creepsCategory = new EzElement(ElementType.CATEGORY, "Creeps", false);
            CreepsFarm = new EzElement(ElementType.CHECKBOX, "Farm", true);
            CKillIndicator = new EzElement(ElementType.CHECKBOX, "Marker [-FPS]", true);
            CKillCountIndicator = new EzElement(ElementType.CHECKBOX, "Show required stacks [-FPS]", false);
            CKillOnlyRange = new EzElement(ElementType.CHECKBOX, "Farm only in shadow poison range [+FPS]", true);
            creepsCategory.AddElement(CreepsFarm);
            creepsCategory.AddElement(CKillIndicator);
            creepsCategory.AddElement(CKillCountIndicator);
            creepsCategory.AddElement(CKillOnlyRange);
            // Heroes
            var heroesCategory = new EzElement(ElementType.CATEGORY, "Heroes", false);
            HeroKill = new EzElement(ElementType.CHECKBOX, "Kill", false);
            HKillIndicator = new EzElement(ElementType.CHECKBOX, "Marker [-FPS]", false);
            HKillCountIndicator = new EzElement(ElementType.CHECKBOX, "Show required stacks [-FPS]", false);
            HKillOnlyRange = new EzElement(ElementType.CHECKBOX, "Kill only in shadow poison range [+FPS]", true);
            heroesCategory.AddElement(HeroKill);
            heroesCategory.AddElement(HKillIndicator);
            heroesCategory.AddElement(HKillCountIndicator);
            heroesCategory.AddElement(HKillOnlyRange);
            //

            Interface.AddMainElement(creepsCategory);
            Interface.AddMainElement(heroesCategory);

            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Version: " + Release, false)); // offset :3
        }

        private static List<Creep> GetCreeps(bool inAction)
        {
            if (inAction) return ObjectMgr.GetEntities<Creep>().Where(x => x.Team != MyHero.Team && x.Distance2D(MyHero) <= 1450 && x.Health > 0).ToList();
            return ObjectMgr.GetEntities<Creep>().ToList();
        }

        private static List<Hero> GetHeroes(bool inAction)
        {
            if (inAction) return ObjectMgr.GetEntities<Hero>().Where(x => x.Team != MyHero.Team && x.Distance2D(MyHero) <= 1450 && x.Health > 0).ToList();
            return ObjectMgr.GetEntities<Hero>().ToList();
        }

        private static void DrawInfo(Unit unit, bool showCount)
        {
            var dPos = Drawing.WorldToScreen(unit.Position);

            if (CanKill(unit)) Drawer.DrawCircle(dPos, 10, Color.Red, 10);
            else
            {
                if (showCount)
                {
                    var needStacks = 0;
                    for (int i = 0; i <= 19; i++)
                    {
                        var lvlDamage = _ShadowPoisonDamage[ShadowPoison.Level - 1];
                        var damage = needStacks * lvlDamage;
                        if (unit.DamageTaken(damage, DamageType.Magical, MyHero) < unit.Health) needStacks++;
                        else break;
                    }
                    Drawer.DrawShadowText(needStacks.ToString(), dPos.X - 5, dPos.Y - 5, Color.White);
                }
                Drawer.DrawCircle(dPos, 10, new ColorBGRA((Vector3)Color.Green, 120), 10);
            }
        }

        private static bool CanKill(Unit unit)
        {
            var shadowPoisonModifier = unit.Modifiers.FirstOrDefault(x => x.Name == "modifier_shadow_demon_shadow_poison");
            if (shadowPoisonModifier != null)
            {
                var lvlDamage = _ShadowPoisonDamage[ShadowPoison.Level - 1];
                var damage = shadowPoisonModifier.StackCount * lvlDamage;
                if (unit.DamageTaken(damage, DamageType.Magical, MyHero) >= unit.Health)
                    return true;
            }
            return false;
        }
        private static bool CanRelease()
        {
            if (MyHero.IsChanneling() || !MyHero.CanCast()) return false;
            return true;
        }

        private static void ShadowPoison_Release()
        {
            ShadowPoisonRelease.UseAbility();
        }
        #endregion

    }
}
