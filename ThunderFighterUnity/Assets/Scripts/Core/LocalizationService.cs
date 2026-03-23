using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderFighter.Core
{
    public enum GameLanguage
    {
        English = 0,
        ChineseSimplified = 1
    }

    public static class LocalizationService
    {
        private const string LanguageKey = "settings.language";

        private static readonly Dictionary<string, string> ZhMap = new Dictionary<string, string>
        {
            { "TACTICAL HANGAR  |  FLEET DEPLOYMENT TERMINAL", "\u6218\u672f\u673a\u5e93  |  \u8230\u961f\u90e8\u7f72\u7ec8\u7aef" },
            { "COMBAT DEPLOYMENT GRID", "\u4f5c\u6218\u90e8\u7f72\u7f51\u683c" },
            { "THUNDER FIGHTER", "\u96f7\u9706\u6218\u673a" },
            { "ENTER CAMPAIGN GRID", "\u8fdb\u5165\u6218\u5f79\u90e8\u7f72" },
            { "POWER DOWN", "\u9000\u51fa\u7cfb\u7edf" },
            { "RETURN TO HANGAR", "\u8fd4\u56de\u673a\u5e93" },
            { "TECH POINTS", "\u6280\u672f\u70b9\u6570" },
            { "POWER RATING", "\u6218\u529b\u8bc4\u7ea7" },
            { "MISSION BREAKDOWN", "\u4efb\u52a1\u7ed3\u7b97" },
            { "POST-MISSION NOTES", "\u6218\u540e\u7b80\u62a5" },
            { "UPGRADE BAY", "\u5347\u7ea7\u8231" },
            { "FIREPOWER", "\u706b\u529b" },
            { "ARMOR", "\u88c5\u7532" },
            { "REACTOR", "\u53cd\u5e94\u5806" },
            { "RETRY SORTIE", "\u91cd\u65b0\u51fa\u51fb" },
            { "NEXT CHAPTER", "\u4e0b\u4e00\u7ae0\u8282" },
            { "RETRY", "\u91cd\u8bd5" },
            { "DEPLOY", "\u90e8\u7f72" },
            { "SEALED", "\u5c01\u9501" },
            { "READY", "\u5c31\u7eea" },
            { "CLEARED", "\u5df2\u901a\u5173" },
            { "LOCKED", "\u672a\u89e3\u9501" },
            { "THREAT", "\u5a01\u80c1" },
            { "RECOMMENDED", "\u63a8\u8350\u6218\u529b" },
            { "HIGH SCORE", "\u6700\u9ad8\u5206" },
            { "NORMAL", "\u666e\u901a" },
            { "HARD", "\u56f0\u96be" },
            { "EXTREME", "\u6781\u9650" },
            { "CHAPTER 1", "\u7b2c\u4e00\u7ae0" },
            { "CHAPTER 2", "\u7b2c\u4e8c\u7ae0" },
            { "CHAPTER 3", "\u7b2c\u4e09\u7ae0" },
            { "ORBITAL INTERCEPT", "\u8f68\u9053\u62e6\u622a" },
            { "ASTEROID BREACH", "\u9668\u77f3\u5e26\u7a81\u5165" },
            { "DEEP SPACE FLAGSHIP", "\u6df1\u7a7a\u65d7\u8230\u6218" },
            { "Intercept hostile craft and clear the orbital corridor.", "\u62e6\u622a\u654c\u65b9\u6218\u673a\uff0c\u6e05\u7a7a\u8f68\u9053\u822a\u7ebf\u3002" },
            { "Punch through the asteroid belt and eliminate strike wings.", "\u7a81\u7834\u9668\u77f3\u5e26\uff0c\u6b7c\u706d\u654c\u65b9\u7a81\u51fb\u7f16\u961f\u3002" },
            { "Engage the flagship task force and neutralize the war core.", "\u8fce\u6218\u65d7\u8230\u7279\u9063\u8230\u961f\uff0c\u6467\u6bc1\u6218\u4e89\u6838\u5fc3\u3002" },
            { "NOVA ENERGY LOW", "\u65b0\u661f\u80fd\u91cf\u4e0d\u8db3" },
            { "OVERDRIVE ENERGY LOW", "\u8fc7\u8f7d\u80fd\u91cf\u4e0d\u8db3" },
            { "OVERDRIVE ONLINE", "\u8fc7\u8f7d\u5df2\u542f\u52a8" },
            { "PLASMA NOVA", "\u7b49\u79bb\u5b50\u65b0\u661f" },
            { "WARNING: BOSS APPROACH", "\u8b66\u544a\uff1aBoss \u63a5\u8fd1" },
            { "WARNING: COMMAND SHIP APPROACH", "\u8b66\u544a\uff1a\u6307\u6325\u8230\u63a5\u8fd1" },
            { "WARNING: ASSAULT CARRIER APPROACH", "\u8b66\u544a\uff1a\u7a81\u51fb\u6bcd\u8230\u63a5\u8fd1" },
            { "WARNING: FLAGSHIP CORE ONLINE", "\u8b66\u544a\uff1a\u65d7\u8230\u6838\u5fc3\u5df2\u4e0a\u7ebf" },
            { "ORBITAL DEFENSE SCRAMBLE", "\u8f68\u9053\u9632\u7ebf\u5df2\u8b66\u6212" },
            { "HOSTILE RING BROKEN", "\u654c\u65b9\u73af\u5f62\u9632\u7ebf\u5df2\u7a81\u7834" },
            { "ASTEROID BELT ENTRY", "\u5df2\u8fdb\u5165\u9668\u77f3\u5e26" },
            { "MULTI-VECTOR THREATS DETECTED", "\u68c0\u6d4b\u5230\u591a\u65b9\u5411\u5a01\u80c1" },
            { "DEEP SPACE TRANSMISSION JAMMED", "\u6df1\u7a7a\u901a\u4fe1\u53d7\u5230\u5e72\u6270" },
            { "ENEMY FLEET PRESSURE RISING", "\u654c\u8230\u961f\u538b\u5236\u5f3a\u5ea6\u4e0a\u5347" },
            { "BOSS SHIELD REBUILT", "Boss \u62a4\u76fe\u5df2\u91cd\u5efa" },
            { "CORE EXPOSED", "\u6838\u5fc3\u5df2\u66b4\u9732" },
            { "SHIELD BREAK - CORE EXPOSED", "\u62a4\u76fe\u7834\u88c2 - \u6838\u5fc3\u66b4\u9732" },
            { "LOCK-ON LASER", "\u9501\u5b9a\u6fc0\u5149" },
            { "CORE CHARGING", "\u6838\u5fc3\u84c4\u80fd\u4e2d" },
            { "BOSS ENGAGED", "Boss \u6218\u5f00\u59cb" },
            { "BOSS PHASE 2", "Boss \u7b2c\u4e8c\u9636\u6bb5" },
            { "BOSS PHASE 3", "Boss \u7b2c\u4e09\u9636\u6bb5" },
            { "PAUSED", "\u5df2\u6682\u505c" },
            { "COMBO", "\u8fde\u51fb" },
            { "MAX", "\u5df2\u6ee1\u7ea7" },
            { "UPLINK COMPLETE  RETURN TO DEPLOYMENT", "\u94fe\u8def\u540c\u6b65\u5b8c\u6210  \u8fd4\u56de\u90e8\u7f72" },
            { "RECALIBRATE LOADOUT AND RETRY", "\u91cd\u65b0\u6821\u51c6\u914d\u7f6e\u540e\u518d\u6b21\u51fa\u51fb" }
        };

        public static event Action<GameLanguage> OnLanguageChanged;

        public static GameLanguage CurrentLanguage => (GameLanguage)Mathf.Clamp(PlayerPrefs.GetInt(LanguageKey, (int)GameLanguage.ChineseSimplified), 0, 1);
        public static bool IsChinese => CurrentLanguage == GameLanguage.ChineseSimplified;

        public static void SetLanguage(GameLanguage language)
        {
            if (CurrentLanguage == language)
            {
                return;
            }

            PlayerPrefs.SetInt(LanguageKey, (int)language);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke(language);
        }

        public static void ToggleLanguage()
        {
            SetLanguage(IsChinese ? GameLanguage.English : GameLanguage.ChineseSimplified);
        }

        public static string Text(string english, string chinese)
        {
            return IsChinese ? chinese : english;
        }

        public static string TranslateLiteral(string text)
        {
            if (!IsChinese || string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (ZhMap.TryGetValue(text, out string translated))
            {
                return translated;
            }

            if (text.StartsWith("Score: "))
            {
                return "\u5206\u6570: " + text.Substring(7);
            }

            if (text.StartsWith("HP: "))
            {
                return "\u751f\u547d: " + text.Substring(4);
            }

            if (text.StartsWith("COMBO x"))
            {
                return "\u8fde\u51fb x" + text.Substring(7);
            }

            return text;
        }

        public static string LanguageButtonLabel()
        {
            return IsChinese ? "\u8bed\u8a00\uff1a\u4e2d\u6587" : "Language: EN";
        }
    }
}
