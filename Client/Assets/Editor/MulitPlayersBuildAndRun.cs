using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class MulitPlayersBuildAndRun
{
    [MenuItem("Tools/Run MultiPlayer/2 Players")]
    static void PerformWin64Build2()
    {
        PerformWin64Build(2);
    }

    [MenuItem("Tools/Run MultiPlayer/3 Players")]
    static void PerformWin64Build3()
    {
        PerformWin64Build(3);
    }

    [MenuItem("Tools/Run MultiPlayer/4 Players")]
    static void PerformWin64Build4()
    {
        PerformWin64Build(4);
    }

    static void PerformWin64Build(int playerCount)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(
        BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);

        // 로그인 씬을 첫 번째로 설정
        string[] scenes = GetScenePaths();
        Array.Sort(scenes, (x, y) => x.Contains("Login") ? -1 : y.Contains("Login") ? 1 : 0);

        for (int i = 1; i <= playerCount; i++)
        {
            BuildPipeline.BuildPlayer(
                scenes,
                "Builds/Win64/" + GetProjectName() + i.ToString() + "/" + GetProjectName() + i.ToString() + ".exe",
                BuildTarget.StandaloneWindows,
                BuildOptions.AutoRunPlayer);
        }
    }

    static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    static string[] GetScenePaths()
    {

       string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }
}

