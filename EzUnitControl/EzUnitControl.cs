using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.AbilityInfo;

using SharpDX;
using SharpDX.Direct3D9;

using EZGUI;

namespace Ez_Unit_Control
{
    class EzUnitControl
    {
        #region Fields
        private static bool Loaded = false;

        private static EzGUI Interface;
        private static EzElement Enabled;
        private static EzElement MoveTheSameWayAsHero;
        private static EzElement FollowIfNoTarget;

        private static Player MyPlayer;
        private static Hero MyHero;

        private static List<ClassID> SupportedUnits = new List<ClassID>();

        private static EzElement Target;

        private static Vector3 LastMoving = Vector3.Zero;


        private static EzElement PriestsHealHero;
        private static EzElement PriestsHealAlliesCreeps;
        #endregion

        #region Init
        public static void Init()
        {
            Interface = new EzGUI(Drawing.Width - 350, 60, "Ez Unit Control");
            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Main", false));
            Enabled = new EzElement(ElementType.CHECKBOX, "Enabled", true);
            Interface.AddMainElement(Enabled);
            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Other", false));

            var Moving = new EzElement(ElementType.CATEGORY, "Moving", false);
            MoveTheSameWayAsHero = new EzElement(ElementType.CHECKBOX, "Move the same way as hero if no target", true);
            FollowIfNoTarget = new EzElement(ElementType.CHECKBOX, "Follow my hero if no target", false);
            Moving.AddElement(MoveTheSameWayAsHero);
            Moving.AddElement(FollowIfNoTarget);
            Interface.AddMainElement(Moving);

            var Abilities = new EzElement(ElementType.CATEGORY, "Abilities", false);
            PriestsHealHero = new EzElement(ElementType.CHECKBOX, "Priests heal hero", true);
            PriestsHealAlliesCreeps = new EzElement(ElementType.CHECKBOX, "Priests heal allied units (controllable)", false);
            Abilities.AddElement(PriestsHealHero);
            Abilities.AddElement(PriestsHealAlliesCreeps);
            Interface.AddMainElement(Abilities);

            Target = new EzElement(ElementType.TEXT, "Current Target: None", false);
            Interface.AddMainElement(Target);
            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Version: 1.0.0.0", true));

            SupportedUnits.Add(ClassID.CDOTA_BaseNPC_Invoker_Forged_Spirit);
            SupportedUnits.Add(ClassID.CDOTA_BaseNPC_Warlock_Golem);
            SupportedUnits.Add(ClassID.CDOTA_BaseNPC_Venomancer_PlagueWard);
            SupportedUnits.Add(ClassID.CDOTA_Unit_Brewmaster_PrimalEarth);
            SupportedUnits.Add(ClassID.CDOTA_Unit_Brewmaster_PrimalFire);
            SupportedUnits.Add(ClassID.CDOTA_Unit_Brewmaster_PrimalStorm);
            SupportedUnits.Add(ClassID.CDOTA_Unit_Broodmother_Spiderling);
            SupportedUnits.Add(ClassID.CDOTA_BaseNPC_Creep);
            SupportedUnits.Add(ClassID.CDOTA_BaseNPC_Creep_Lane);
            SupportedUnits.Add(ClassID.CDOTA_Unit_VisageFamiliar);
            SupportedUnits.Add(ClassID.CDOTA_BaseNPC_Tusk_Sigil);
            SupportedUnits.Add(ClassID.CDOTA_Unit_SpiritBear);
            SupportedUnits.Add(ClassID.CDOTA_NPC_WitchDoctor_Ward);
            SupportedUnits.Add(ClassID.CDOTA_BaseNPC_ShadowShaman_SerpentWard);
            SupportedUnits.Add(ClassID.CDOTA_BaseNPC_Creep_Neutral);

            Game.OnUpdate += Game_OnUpdate;
            Player.OnExecuteOrder += Player_OnExecuteOrder;
        }
        #endregion

        #region ExecuteOrder
        static void Player_OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (!Game.IsInGame || !Loaded || !Enabled.isActive) return;

            if (sender == MyPlayer)
            {
                if (args.Order == Order.MoveLocation)
                {
                    LastMoving = args.TargetPosition;
                }
            }
        }
        #endregion

        #region Update
        static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || !Enabled.isActive)
            {
                if (Loaded) { Loaded = false; }
                return;
            }

            if (!Loaded)
            {
                MyPlayer = ObjectMgr.LocalPlayer;
                MyHero = ObjectMgr.LocalHero;
                LastMoving = Vector3.Zero;
                Loaded = true;
            }

            if (Utils.SleepCheck("delay"))
            {
                var controllable_units = ObjectMgr.GetEntities<Unit>().Where(x => (SupportedUnits.Contains(x.ClassID) || x.IsIllusion) && x.IsControllable && x.IsAlive && x.Team == MyHero.Team);

                if (controllable_units == null) { SetTargetString("None"); return; }

                if (controllable_units != null)
                {
                    var target = GetClosestToMouseTarget(MyHero, 800f);

                    SetTargetString( (target == null) ? "None" : target.Name.Replace("npc_dota_hero_", "").ToUpper());

                    foreach (var unit in controllable_units)
                    {
                        if (target == null) 
                        {
                            if (Utils.SleepCheck(unit.Handle.ToString()+"moving") && unit.ClassID != ClassID.CDOTA_BaseNPC_ShadowShaman_SerpentWard && unit.ClassID != ClassID.CDOTA_NPC_WitchDoctor_Ward)
                            {
                                if (FollowIfNoTarget.isActive) unit.Follow(MyHero);
                                if (MoveTheSameWayAsHero.isActive && LastMoving != Vector3.Zero && unit.Distance2D(LastMoving) >= 300) unit.Move(LastMoving);
                                Utils.Sleep(300, unit.Handle.ToString() + "moving");
                            }
                            continue; 
                        } 

                        switch (unit.ClassID)
                        {
                            case ClassID.CDOTA_BaseNPC_Tusk_Sigil:
                                unit.Move(target.Position);
                                break;
                            case ClassID.CDOTA_Unit_VisageFamiliar:
                                var stoneForm = unit.Spellbook.Spell1;
                                if (CanCast(unit, stoneForm) && !target.IsStunned() && unit.Distance2D(target) > 340f)
                                    unit.Follow(target);
                                else if (unit.Distance2D(target) <= 340f && Utils.SleepCheck("familiar_stoneform"))
                                {
                                    CastSpellsIfCanOrAttack(stoneForm, unit, target, true, 0, Vector3.Zero, 340f);
                                    Utils.Sleep(1000 + Game.Ping, "familiar_stoneform");
                                }
                                else
                                    unit.Attack(target);
                                break;
                            case ClassID.CDOTA_Unit_Brewmaster_PrimalStorm:
                                var dispelMagic = unit.Spellbook.Spell1;
                                var drunkenHaze = unit.Spellbook.Spell4;
                                if (CanCast(unit, dispelMagic))
                                    CastSpellsIfCanOrAttack(dispelMagic, unit, target, false, 2, target.Position);
                                else if (CanCast(unit, drunkenHaze))
                                    CastSpellsIfCanOrAttack(drunkenHaze, unit, target, false, 1, Vector3.Zero);
                                else
                                    unit.Attack(target);
                                break;
                            case ClassID.CDOTA_Unit_Brewmaster_PrimalEarth:
                                var hurlBoulder = unit.Spellbook.Spell1;
                                var earthThunderClap = unit.Spellbook.Spell4;
                                if (CanCast(unit, hurlBoulder))
                                    CastSpellsIfCanOrAttack(hurlBoulder, unit, target, true, 1, Vector3.Zero);
                                else if (CanCast(unit, earthThunderClap))
                                    CastSpellsIfCanOrAttack(earthThunderClap, unit, target, true, 0, Vector3.Zero, 400f);
                                else
                                    unit.Attack(target);
                                break;
                            case ClassID.CDOTA_BaseNPC_Creep:
                                switch (unit.Name)
                                {
                                    default:
                                        if (unit.Name.Contains("npc_dota_necronomicon_archer"))
                                        {
                                            var manaBurn = unit.Spellbook.Spell1;
                                            CastSpellsIfCanOrAttack(manaBurn, unit, target, false, 1, Vector3.Zero);
                                        }
                                        else unit.Attack(target);
                                        break;
                                }
                                break;
                            case ClassID.CDOTA_BaseNPC_Creep_Neutral:
                                switch (unit.Name)
                                {
                                    case "npc_dota_neutral_harpy_storm":
                                        var chainLightning = unit.Spellbook.Spell1;
                                        CastSpellsIfCanOrAttack(chainLightning, unit, target, false, 1, Vector3.Zero);
                                        break;
                                    case "npc_dota_neutral_big_thunder_lizard":
                                        var slam = unit.Spellbook.Spell1;
                                        var frenzy = unit.Spellbook.Spell2;
                                        if (CanCast(unit, frenzy)) frenzy.UseAbility();
                                        CastSpellsIfCanOrAttack(slam, unit, target, true, 0, Vector3.Zero, 250f);
                                        break;
                                    case "npc_dota_neutral_polar_furbolg_ursa_warrior":
                                        var thunderClap = unit.Spellbook.Spell1;
                                        CastSpellsIfCanOrAttack(thunderClap, unit, target, false, 1, Vector3.Zero, 300f);
                                        break;
                                    case "npc_dota_neutral_forest_troll_high_priest":
                                        var heal = unit.Spellbook.Spell1;
                                        if (MyHero.Health < MyHero.MaximumHealth && PriestsHealHero.isActive)
                                        {
                                            if (CanCast(unit, heal))
                                                heal.UseAbility(MyHero);
                                        }
                                        else if (PriestsHealAlliesCreeps.isActive)
                                        {
                                            foreach (Unit uni in controllable_units)
                                            {
                                                if (uni.Health < uni.MaximumHealth)
                                                    if (CanCast(unit, heal))
                                                        heal.UseAbility(uni);
                                            }
                                        }
                                        break;
                                    case "npc_dota_neutral_dark_troll_warlord":
                                        var ensnare = unit.Spellbook.Spell1;
                                        CastSpellsIfCanOrAttack(ensnare, unit, target, true, 1, Vector3.Zero);
                                        break;
                                    case "npc_dota_neutral_centaur_khan":
                                        var warStomp = unit.Spellbook.Spell1;
                                        CastSpellsIfCanOrAttack(warStomp, unit, target, true, 0, Vector3.Zero, 250f);
                                        break;
                                    case "npc_dota_neutral_satyr_hellcaller":
                                        var shockwave = unit.Spellbook.Spell1;
                                        CastSpellsIfCanOrAttack(shockwave, unit, target, false, 2, target.Position);
                                        break;
                                    default: // Just attack
                                        unit.Attack(target);
                                        break;
                                }
                                break;
                            default: // Just attack
                                unit.Attack(target);
                                break;
                        }
                    }
                }
                else SetTargetString("None");
                //
                Utils.Sleep(200, "delay");
            }
        }
        #endregion

        #region Methods
        private static void CastSpellsIfCanOrAttack(Ability ability, Unit source,  Hero target, bool cStun, byte targetType, Vector3 targetPos, float range = 0f)
        {
            if (range == 0f)
            {
                if ((cStun ? !target.IsStunned() : true) && CanCast(source, ability))
                { 
                    switch(targetType) {
                        case 0:
                            ability.UseAbility();
                            break;
                        case 1:
                            ability.UseAbility(target);
                            break;
                        case 2:
                            ability.UseAbility(targetPos);
                            break;
                    }
                    Utils.Sleep(ability.ChannelTime + Game.Ping, ability.Handle.ToString());
                }
                else if (Utils.SleepCheck(ability.Handle.ToString())) source.Attack(target);
            }
            else
            {
                if ((cStun ? !target.IsStunned() : true) && CanCast(source, ability) && source.Distance2D(target) <= range) { ability.UseAbility(); Utils.Sleep(ability.ChannelTime + Game.Ping, ability.Handle.ToString()); }
                else if (Utils.SleepCheck(ability.Handle.ToString())) source.Attack(target);
            }
        }
        private static bool CanCast(Unit source, Ability ability)
        {
            return ((Math.Round(source.Mana) - ability.ManaCost >= 0) && ability.Cooldown == 0f && !source.IsStunned());
        }
        private static void SetTargetString(string target)
        {
            Target.Content = "Current Target: " + target;
        }
        public static Hero GetClosestToMouseTarget(Hero source, float range = 1000)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes =
                ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x => x.IsValid &&
                        x.Team != source.Team && !x.IsIllusion && x.IsAlive && x.IsVisible
                        && x.Distance2D(mousePosition) <= range);
            if (enemyHeroes == null) return null;
            Hero closestHero = null;
            foreach (var enemyHero in enemyHeroes)
            {
                if (closestHero == null || closestHero.Distance2D(mousePosition) > enemyHero.Distance2D(mousePosition))
                {
                    closestHero = enemyHero;
                }
            }
            return closestHero;
        }
        #endregion
    }
}
