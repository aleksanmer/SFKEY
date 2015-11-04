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
        private static EzElement UseAbilities;
        private static EzElement AttackTarget;

        private static Player MyPlayer;
        private static Hero MyHero;

        private static List<ClassID> SupportedUnits = new List<ClassID>();

        private static EzElement Target;
        private static EzElement Chasing;

        private static Vector3 LastMoving = Vector3.Zero;

        // Tusk
        private static EzElement MoveTuskSigil;
        // Visage
        private static EzElement UseStoneForm;
        // Brewmaster
        private static EzElement PrimalForms;
        private static EzElement UseHurlBoulder; // Earth
        private static EzElement UseThunderClap;
        private static EzElement UseDispelMagic; // Storm
        private static EzElement UseCyclone;
        private static EzElement UseDrunkenHaze;
        // Necronomicon
        private static EzElement UseArcherManaBurn;
        // Neutrals
        private static EzElement PriestsHealHero;
        private static EzElement PriestsHealAlliesCreeps;
        private static EzElement UseChainLightning;
        private static EzElement UseSlam;
        private static EzElement UseFrenzy;
        private static EzElement UseNThunderClap;
        private static EzElement UseHeal;
        private static EzElement UseEnsnare;
        private static EzElement UseWarStomp;
        private static EzElement UseShockWave;


        private static Hero TargetHero;

        private static uint ChasingKey;
        private static bool IsChasing;
        #endregion

        #region Init
        public static void Init()
        {
            Interface = new EzGUI(Drawing.Width - 350, 60, "Ez Unit Control");
            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Main", false));
            Enabled = new EzElement(ElementType.CHECKBOX, "Enabled", true);
            Interface.AddMainElement(Enabled);
            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Main options", false));
            UseAbilities = new EzElement(ElementType.CHECKBOX, "Use abilities", true);
            AttackTarget = new EzElement(ElementType.CHECKBOX, "Attack target (if can)", true);
            Interface.AddMainElement(AttackTarget);
            Interface.AddMainElement(UseAbilities);

            var Moving = new EzElement(ElementType.CATEGORY, "Moving", false);
            MoveTheSameWayAsHero = new EzElement(ElementType.CHECKBOX, "Move the same way as hero if no target", true);
            FollowIfNoTarget = new EzElement(ElementType.CHECKBOX, "Follow my hero if no target", false);
            Moving.AddElement(MoveTheSameWayAsHero);
            Moving.AddElement(FollowIfNoTarget);
            MoveTuskSigil = new EzElement(ElementType.CHECKBOX, "Move Tusk Sigil", true);
            Moving.AddElement(MoveTuskSigil);
            Interface.AddMainElement(Moving);

            var Abilities = new EzElement(ElementType.CATEGORY, "Abilities", false);

            var stoneForms = new EzElement(ElementType.CATEGORY, "StoneForms [Visage]", false);
            UseStoneForm = new EzElement(ElementType.CHECKBOX, "Use StoneForm", true);
            stoneForms.AddElement(UseStoneForm);
            Abilities.AddElement(stoneForms);

            var primalForms = new EzElement(ElementType.CATEGORY, "PrimalForms [Brewmaster]", false);
            PrimalForms = new EzElement(ElementType.CHECKBOX, "Use abilities", true);
            UseHurlBoulder = new EzElement(ElementType.CHECKBOX, "Use Hurl Boulder [EARTH]", true);
            UseThunderClap = new EzElement(ElementType.CHECKBOX, "Use Thunder Clap [EARTH]", true);
            UseDispelMagic = new EzElement(ElementType.CHECKBOX, "Use Dispel Magic [STORM]", true);
            UseCyclone = new EzElement(ElementType.CHECKBOX, "Use Cyclone [STORM]", true);
            UseDrunkenHaze = new EzElement(ElementType.CHECKBOX, "Use Drunken Haze [STORM]", true);
            primalForms.AddElement(PrimalForms);
            primalForms.AddElement(UseHurlBoulder);
            primalForms.AddElement(UseThunderClap);
            primalForms.AddElement(UseDispelMagic);
            primalForms.AddElement(UseCyclone);
            primalForms.AddElement(UseDrunkenHaze);
            Abilities.AddElement(primalForms);

            var neutrals = new EzElement(ElementType.CATEGORY, "Neutrals", false);
            UseHeal = new EzElement(ElementType.CHECKBOX, "Use Heal [Priest]", true);
            PriestsHealHero = new EzElement(ElementType.CHECKBOX, "Priests heal hero [Priest]", true);
            PriestsHealAlliesCreeps = new EzElement(ElementType.CHECKBOX, "Priests heal allied units (controllable) [Priest]", false);
            UseChainLightning = new EzElement(ElementType.CHECKBOX, "Use Chain Lightning [Harpy Storm]", true);
            UseSlam = new EzElement(ElementType.CHECKBOX, "Use Slam [Big Thunder Lizard]", true);
            UseFrenzy = new EzElement(ElementType.CHECKBOX, "Use Frenzy [Big Thunder Lizard]", true);
            UseNThunderClap = new EzElement(ElementType.CHECKBOX, "Use Thunder Clap [Ursa Warrior]", true);
            UseEnsnare = new EzElement(ElementType.CHECKBOX, "Use Ensnare [Troll Warlord]", true);
            UseWarStomp = new EzElement(ElementType.CHECKBOX, "Use War Stomp [Centaur Khan]", true);
            UseShockWave = new EzElement(ElementType.CHECKBOX, "Use ShockWave [Hellcaller]", true);
            neutrals.AddElement(UseHeal);
            neutrals.AddElement(PriestsHealHero);
            neutrals.AddElement(PriestsHealAlliesCreeps);
            neutrals.AddElement(UseChainLightning);
            neutrals.AddElement(UseSlam);
            neutrals.AddElement(UseFrenzy);
            neutrals.AddElement(UseNThunderClap);
            neutrals.AddElement(UseEnsnare);
            neutrals.AddElement(UseWarStomp);
            neutrals.AddElement(UseShockWave);
            Abilities.AddElement(neutrals);

            var necronomicon = new EzElement(ElementType.CATEGORY, "Necronomicon", false);
            UseArcherManaBurn = new EzElement(ElementType.CHECKBOX, "Use Mana Burn [ARCHER]", true);
            necronomicon.AddElement(UseArcherManaBurn);
            Abilities.AddElement(necronomicon);


            Interface.AddMainElement(Abilities);

            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Chasing", false));
            Chasing = new EzElement(ElementType.CHECKBOX, "Current ChasingKey: " + Utils.KeyToText(ChasingKey), false);
            Interface.AddMainElement(Chasing);

            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Info", false));
            Target = new EzElement(ElementType.TEXT, "Current Target: None", false);
            Interface.AddMainElement(Target);
            Interface.AddMainElement(new EzElement(ElementType.TEXT, "Version: 1.0.0.3", true));

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
            Game.OnWndProc += Game_OnWndProc;
        }
        #endregion

        #region Wnd
        static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsInGame && Enabled.isActive)
            {
                switch (args.Msg)
                {
                    case (uint)Utils.WindowsMessages.WM_KEYDOWN:
                        if (!Chasing.isActive) return;
                        ChasingKey = (uint)args.WParam;
                        SetChasingKeyString(ChasingKey);
                        Chasing.isActive = false;
                        break;
                }
            }
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
                if (Chasing.isActive) Chasing.Content = "Waiting for new key...";
                IsChasing = Game.IsKeyDown(ChasingKey);

                //

                var controllable_units = ObjectMgr.GetEntities<Unit>().Where(x => (SupportedUnits.Contains(x.ClassID) || x.IsIllusion) && x.IsControllable && x.IsAlive && x.Team == MyHero.Team);

                if (controllable_units == null) { SetTargetString("None"); return; }

                if (controllable_units != null)
                {
                    if (IsChasing && TargetHero != null && TargetHero.IsValid && TargetHero.IsAlive)
                        TargetHero = TargetHero;
                    else TargetHero = GetClosestToMouseTarget(MyHero, 800f);
                    var target = TargetHero;

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

                        if (!UseAbilities.isActive)
                        {
                            AttackIfCan(unit, target);
                            continue;
                        }

                        switch (unit.ClassID)
                        {
                            case ClassID.CDOTA_BaseNPC_Tusk_Sigil:
                                if (MoveTuskSigil.isActive)
                                    unit.Move(target.Position);
                                break;
                            case ClassID.CDOTA_Unit_VisageFamiliar:
                                var stoneForm = unit.Spellbook.Spell1;
                                if ((UseStoneForm.isActive && CanCast(unit, stoneForm)) && !target.IsStunned() && unit.Distance2D(target) > 340f)
                                    unit.Follow(target);
                                else if ((UseStoneForm.isActive && CanCast(unit, stoneForm)) && unit.Distance2D(target) <= 340f && Utils.SleepCheck("familiar_stoneform"))
                                {
                                    CastSpellsIfCanOrAttack(stoneForm, unit, target, true, 0, Vector3.Zero, 340f);
                                    Utils.Sleep(1000 + Game.Ping, "familiar_stoneform");
                                }
                                else
                                    AttackIfCan(unit, target);
                                break;
                            case ClassID.CDOTA_Unit_Brewmaster_PrimalStorm:
                                if (!PrimalForms.isActive) AttackIfCan(unit, target);
                                var dispelMagic = unit.Spellbook.Spell1;
                                var cyclone = unit.Spellbook.Spell2;
                                var drunkenHaze = unit.Spellbook.Spell4;
                                if (UseCyclone.isActive && CanCast(unit, cyclone))
                                    CastSpellsIfCanOrAttack(cyclone, unit, target, true, 1, Vector3.Zero);
                                else if (UseDispelMagic.isActive && CanCast(unit, dispelMagic))
                                    CastSpellsIfCanOrAttack(dispelMagic, unit, target, false, 2, target.Position);
                                else if (UseDrunkenHaze.isActive && CanCast(unit, drunkenHaze))
                                    CastSpellsIfCanOrAttack(drunkenHaze, unit, target, false, 1, Vector3.Zero);
                                else
                                    AttackIfCan(unit, target);
                                break;
                            case ClassID.CDOTA_Unit_Brewmaster_PrimalEarth:
                                if (!PrimalForms.isActive) AttackIfCan(unit, target);
                                var hurlBoulder = unit.Spellbook.Spell1;
                                var earthThunderClap = unit.Spellbook.Spell4;
                                if (UseHurlBoulder.isActive && CanCast(unit, hurlBoulder))
                                    CastSpellsIfCanOrAttack(hurlBoulder, unit, target, true, 1, Vector3.Zero);
                                else if (UseThunderClap.isActive && CanCast(unit, earthThunderClap))
                                    CastSpellsIfCanOrAttack(earthThunderClap, unit, target, true, 0, Vector3.Zero, 400f);
                                else
                                    AttackIfCan(unit, target);
                                break;
                            case ClassID.CDOTA_BaseNPC_Creep:
                                switch (unit.Name)
                                {
                                    default:
                                        if (unit.Name.Contains("npc_dota_necronomicon_archer"))
                                        {
                                            var manaBurn = unit.Spellbook.Spell1;
                                            if (UseArcherManaBurn.isActive)
                                                CastSpellsIfCanOrAttack(manaBurn, unit, target, false, 1, Vector3.Zero);
                                            else
                                                AttackIfCan(unit, target);
                                        }
                                        else AttackIfCan(unit, target);
                                        break;
                                }
                                break;
                            case ClassID.CDOTA_BaseNPC_Creep_Neutral:
                                switch (unit.Name)
                                {
                                    case "npc_dota_neutral_harpy_storm":
                                        var chainLightning = unit.Spellbook.Spell1;
                                        if (UseChainLightning.isActive)
                                            CastSpellsIfCanOrAttack(chainLightning, unit, target, false, 1, Vector3.Zero);
                                        else
                                            AttackIfCan(unit, target);
                                        break;
                                    case "npc_dota_neutral_big_thunder_lizard":
                                        var slam = unit.Spellbook.Spell1;
                                        var frenzy = unit.Spellbook.Spell2;
                                        if (UseFrenzy.isActive && CanCast(unit, frenzy)) frenzy.UseAbility();
                                        if (UseSlam.isActive) CastSpellsIfCanOrAttack(slam, unit, target, true, 0, Vector3.Zero, 250f);
                                        else AttackIfCan(unit, target);
                                        break;
                                    case "npc_dota_neutral_polar_furbolg_ursa_warrior":
                                        var thunderClap = unit.Spellbook.Spell1;
                                        if (UseNThunderClap.isActive)
                                            CastSpellsIfCanOrAttack(thunderClap, unit, target, false, 1, Vector3.Zero, 300f);
                                        else AttackIfCan(unit, target);
                                        break;
                                    case "npc_dota_neutral_forest_troll_high_priest":
                                        var heal = unit.Spellbook.Spell1;
                                        if (UseHeal.isActive)
                                        {
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
                                            else AttackIfCan(unit, target);
                                        }
                                        else AttackIfCan(unit, target);
                                        break;
                                    case "npc_dota_neutral_dark_troll_warlord":
                                        var ensnare = unit.Spellbook.Spell1;
                                        if (UseEnsnare.isActive)
                                            CastSpellsIfCanOrAttack(ensnare, unit, target, true, 1, Vector3.Zero);
                                        else AttackIfCan(unit, target);
                                        break;
                                    case "npc_dota_neutral_centaur_khan":
                                        var warStomp = unit.Spellbook.Spell1;
                                        if (UseWarStomp.isActive)
                                            CastSpellsIfCanOrAttack(warStomp, unit, target, true, 0, Vector3.Zero, 250f);
                                        else AttackIfCan(unit, target);
                                        break;
                                    case "npc_dota_neutral_satyr_hellcaller":
                                        var shockwave = unit.Spellbook.Spell1;
                                        if (UseShockWave.isActive)
                                            CastSpellsIfCanOrAttack(shockwave, unit, target, false, 2, target.Position);
                                        else AttackIfCan(unit, target);
                                        break;
                                    default: 
                                        AttackIfCan(unit, target);
                                        break;
                                }
                                break;
                            default:
                                AttackIfCan(unit, target);
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
                if ((cStun ? !target.IsStunned() : true) && CanCast(source, ability) && !target.IsInvul())
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
                else if (Utils.SleepCheck(ability.Handle.ToString())) AttackIfCan(source, target);
            }
            else
            {
                if ((cStun ? !target.IsStunned() : true) && CanCast(source, ability) && !target.IsInvul() && source.Distance2D(target) <= range) { ability.UseAbility(); Utils.Sleep(ability.ChannelTime + Game.Ping, ability.Handle.ToString()); }
                else if (Utils.SleepCheck(ability.Handle.ToString())) AttackIfCan(source, target);
            }
        }
        private static void AttackIfCan(Unit source, Unit target)
        {
            if (source.CanAttack() && AttackTarget.isActive && !target.IsAttackImmune() && !target.IsInvul()) source.Attack(target);
        }
        private static bool CanCast(Unit source, Ability ability)
        {
            return ((Math.Round(source.Mana) - ability.ManaCost >= 0) && ability.Cooldown == 0f && ability.Level != 0 && !source.IsStunned());
        }
        private static void SetChasingKeyString(uint key)
        {
            Chasing.Content = "Current ChasingKey: " + Utils.KeyToText(key);
        }
        private static void SetTargetString(string target)
        {
            Target.Content = "Current Target ["+(IsChasing ? "Chasing" : "Without chase")+"]: "+ target;
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
