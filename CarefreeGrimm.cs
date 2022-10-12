using HutongGames.PlayMaker;
using Modding;
using UnityEngine;
using SFCore;

namespace CarefreeGrimm
{
    public class CarefreeGrimm : Mod, ILocalSettings<Settings>
    {
        public CarefreeGrimm() : base("Carefree Grimm") { }

        public override string GetVersion() => "1.0.0.2";

        public int CharmId;

        Settings settings;

        public override void Initialize()
        {
            //Create charm with empty sprite
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);            
            CharmId = CharmHelper.AddSprites(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f)))[0];

            //Hooks
            On.CharmIconList.GetSprite += CharmIconHook;
            On.PlayMakerFSM.OnEnable += FsmHook;
            ModHooks.GetPlayerIntHook += GetIntHook;
            ModHooks.SetPlayerBoolHook += SetBoolHook;
            ModHooks.GetPlayerBoolHook += GetBoolHook;
            ModHooks.LanguageGetHook += LanguageHook;
            ModHooks.CharmUpdateHook += CharmUpdateHook;
        }

        Sprite CharmIconHook(On.CharmIconList.orig_GetSprite orig, CharmIconList self, int id)
        {
            if (id == CharmId)
            {
                if (PlayerData.instance.GetInt("grimmChildLevel") == 5)
                {
                    return self.grimmchildLevel4;
                }
                return self.nymmCharm;
            }
            return orig(self, id);
        }

        void FsmHook(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.FsmName.Equals("Spawn Grimmchild"))
            {
                FsmState spawnState = self.Fsm.GetState("Normal Spawn");
                spawnState.AddTransition("CHECK", "Spawn");
                spawnState.AddTransition("CHECK", "Init");
                spawnState.AddAction(fsm =>
                {
                    if ((PlayerData.instance.GetBool($"equippedCharm_{((CarefreeGrimm)ModHooks.GetMod("Carefree Grimm")).CharmId}") && PlayerData.instance.grimmChildLevel == 5) || (PlayerData.instance.GetBool($"equippedCharm_40") && PlayerData.instance.grimmChildLevel == 4))
                    {
                        fsm.Event("CHECK");
                    }
                });
                FsmState charmState = self.Fsm.GetState("Charms Allowed?");
                charmState.AddTransition("CHECK", "Spawn");
                charmState.AddTransition("CHECK", "Init");
                charmState.AddAction(fsm =>
                {
                    if ((PlayerData.instance.GetBool($"equippedCharm_{((CarefreeGrimm)ModHooks.GetMod("Carefree Grimm")).CharmId}") && PlayerData.instance.grimmChildLevel == 5) || (PlayerData.instance.GetBool($"equippedCharm_40") && PlayerData.instance.grimmChildLevel == 4))
                    {
                        fsm.Event("CHECK");
                    }
                });
            }
            else if (self.FsmName.Equals("Control") && self.gameObject.name.Contains("Grimmchild"))
            {
                FsmState init = self.Fsm.GetState("Init");
                init.ReplaceAction(_ =>
                {
                    if (PlayerData.instance.GetBool($"equippedCharm_{((CarefreeGrimm)ModHooks.GetMod("Carefree Grimm")).CharmId}") && PlayerData.instance.grimmChildLevel == 5)
                    {
                        self.FsmVariables.GetFsmInt("Grimm Level").Value = 4;
                    }
                    else
                    {
                        self.FsmVariables.GetFsmInt("Grimm Level").Value =
                            PlayerData.instance.GetInt("grimmChildLevel");
                    }
                }, 3);
            }
        }

        int GetIntHook(string target, int val)
        {
            if (target.Equals($"charmCost_{CharmId}"))
            {
                if (PlayerData.instance.GetInt("grimmChildLevel") == 5)
                {
                    return 2;
                }
                return 3;
            }
            return val;
        }

        bool SetBoolHook(string target, bool val)
        {
            if (target.Equals($"equippedCharm_{CharmId}"))
            {
                settings.Equipped = val;
            }
            return val;
        }

        bool GetBoolHook(string target, bool val)
        {
            if (target.Equals($"equippedCharm_{CharmId}") || target.Equals("equippedCharm_40_N"))
            {
                return settings.Equipped;
            }

            if(target.Equals($"gotCharm_{CharmId}"))
            {
                return PlayerData.instance.GetInt("grimmChildLevel") > 3;
            }

            if (target.Equals($"newCharm_{CharmId}"))
            {
                return false;
            }
            return val;
        }

        string LanguageHook(string target, string target2, string val)
        {
            if (target2.Equals("UI"))
            {
                if (target.Equals($"CHARM_NAME_{CharmId}"))
                {
                    if (PlayerData.instance.GetInt("grimmChildLevel") == 4)
                    {
                        return Language.Language.Get("CHARM_NAME_40_N", "UI");
                    }
                    return Language.Language.Get("CHARM_NAME_40", "UI");
                }

                if (target.Equals($"CHARM_DESC_{CharmId}"))
                {
                    if (PlayerData.instance.GetInt("grimmChildLevel") == 4)
                    {
                        return Language.Language.Get("CHARM_DESC_40_N", "UI");
                    }
                    return Language.Language.Get("CHARM_DESC_40_F", "UI");
                }
            }
            return val;
        }

        void CharmUpdateHook(PlayerData data, HeroController hero)
        {
            if (data.GetInt("grimmChildLevel") == 4)
            {
                hero.carefreeShieldEquipped = settings.Equipped;
            }

        }

        public void OnLoadLocal(Settings s)
        {
            settings = s;
        }

        public Settings OnSaveLocal()
        {
            return settings;
        }
    }
}