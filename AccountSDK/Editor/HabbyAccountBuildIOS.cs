using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.Callbacks;
using System.Xml;

using Habby;

using UnityEngine;
using UnityEditor.Build.Reporting;
using System.Diagnostics;
using System.Threading;

#if UNITY_IPHONE
using UnityEditor.iOS.Xcode.Extensions;
using UnityEditor.iOS.Xcode;
#endif
public class HabbyAccountBuildIOS : Editor
{
#if UNITY_IPHONE
    [PostProcessBuild(998)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        UnityEngine.Debug.Log("Start change HabbyAccount Setting " + path);
        var pathBase = path;
        string projPath = PBXProject.GetPBXProjectPath(path);

        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);
        var frameTarget = proj.GetUnityFrameworkTargetGuid();
        proj.AddHeadersBuildPhase(frameTarget);

        AddProjectSetting(proj, buildTarget, path);

        proj.WriteToFile(projPath);
        
        UnityEngine.Debug.Log("Ene change HabbyAccount Setting");
    }

    static void AddFileToPublicHeader(PBXProject proj,string filePath)
    {
        var frameTarget = proj.GetUnityFrameworkTargetGuid();
        var resGUID = proj.FindFileGuidByProjectPath(filePath);
        if(string.IsNullOrEmpty(resGUID))
        {
            UnityEngine.Debug.LogErrorFormat("Cant found Header=> guid:{0}, filepath:{1}", resGUID, filePath);
        }
        else
        {
            UnityEngine.Debug.LogFormat("Add Public Header=> guid:{0}, filepath:{1}", resGUID, filePath);
            proj.AddPublicHeaderToBuild(frameTarget, resGUID);
        }
    }

    static void AddIMInclude(BuildTarget buildTarget, string filePath,string tagStr,string includeStr)
    {
        string tfileStr = File.ReadAllText(filePath);
        int tindex = tfileStr.IndexOf(tagStr) + tagStr.Length;
        tfileStr = tfileStr.Insert(tindex, includeStr);
        File.WriteAllText(filePath, tfileStr);
    }

    static void AddFileToEmbedFrameworks(PBXProject proj, string targetGuid, string framework)
    {
        string fileGuid = proj.AddFile(framework, "Frameworks/" + framework, UnityEditor.iOS.Xcode.PBXSourceTree.Sdk);
        PBXProjectExtensions.AddFileToEmbedFrameworks(proj, targetGuid, fileGuid);
    }

    static void AddProjectSetting(PBXProject proj,BuildTarget buildTarget, string path)
    {
        string targetGuid = proj.GetUnityMainTargetGuid();
        proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        proj.SetBuildProperty(proj.ProjectGuid(), "ENABLE_BITCODE", "NO");

        //proj.AddFrameworkToProject(targetGuid, "GameKit.framework", true);

        string frameGuid = proj.GetUnityFrameworkTargetGuid();
        proj.AddFrameworkToProject(frameGuid, "GameKit.framework", true);
        //GameKit.framework
    }
    
#endif


}
