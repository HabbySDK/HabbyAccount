#if UNITY_ANDROID
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HabbySDK.HabbyAccount
{
    public class HabbyAccountBuildAndroid : IPreprocessBuildWithReport,IPostprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("account build android start.");
            
            //CopyGMSSetting();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log("account build android end");
        }

        void CopyGMSSetting()
        {
            var tgmsPath = Path.Combine(Application.dataPath,
                "HabbySDK/HabbyAccount/ThirdPartyLib/GMS/");
            if (!Directory.Exists(tgmsPath))
            {
                return;
            }

            var tfilestr = "Plugins/Android/GooglePlayGamesManifest.androidlib";
            var tpath = Path.Combine(Application.dataPath,
                tgmsPath, tfilestr);

            var ttarPath = Path.Combine(Application.dataPath, tfilestr);

            var tmanifest = Path.Combine(tpath, "AndroidManifest.xml");
            var ttarmanifest = Path.Combine(ttarPath, "AndroidManifest.xml");

            var tproject = Path.Combine(tpath, "project.properties");
            var ttarproject = Path.Combine(ttarPath, "project.properties");

            if (!Directory.Exists(ttarPath))
            {
                Directory.CreateDirectory(ttarPath);
            }

            File.Copy(tmanifest, ttarmanifest, true);
            File.Copy(tproject, ttarproject, true);
        }
    }
}
#endif