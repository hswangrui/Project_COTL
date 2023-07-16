using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class ResourcesBatchReplace : EditorWindow
{
    private static ResourcesBatchReplace _window;
    private List<Object> _sourceOld;
    private List<Object> _sourceNew;

    private List<string> _oldGuids;
    private List<string> _newGuids;

    private bool isContainScene = true;
    private bool isContainPrefab = true;
    private bool isContainMat = true;
    private bool isContainAsset = false;

    private List<string> withoutExtensions = new List<string>();

    [MenuItem("Tools/批量资源替换")]
    public static void BatchReplace()
    {
        _window = (ResourcesBatchReplace)EditorWindow.GetWindow(typeof(ResourcesBatchReplace), true, "批量资源替换");
        _window.Show();
    }

    void OnGUI()
    {
        GUILayout.Space(20);

        int count = EditorGUILayout.IntField("替换组数", Mathf.Max(1, _sourceOld != null ? _sourceOld.Count : 1));
        if (count != (_sourceOld != null ? _sourceOld.Count : 0))
        {
            _sourceOld = new List<Object>(count);
            _sourceNew = new List<Object>(count);
        }

        GUILayout.Space(20);

        for (int i = 0; i < count; i++)
        {
            GUILayout.Label(string.Format("第 {0} 组替换：", i + 1));

            if (_sourceOld.Count <= i)
            {
                _sourceOld.Add(null);
            }
            _sourceOld[i] = EditorGUILayout.ObjectField("旧的资源", _sourceOld[i], typeof(Object), true);

            if (_sourceNew.Count <= i)
            {
                _sourceNew.Add(null);
            }
            _sourceNew[i] = EditorGUILayout.ObjectField("新的资源", _sourceNew[i], typeof(Object), true);

            GUILayout.Space(10);
        }

        GUILayout.Space(20);
        GUILayout.Label("要在哪些类型中查找替换：");
        EditorGUILayout.BeginHorizontal();
        isContainScene = GUILayout.Toggle(isContainScene, ".unity");
        isContainPrefab = GUILayout.Toggle(isContainPrefab, ".prefab");
        isContainMat = GUILayout.Toggle(isContainMat, ".mat");
        isContainAsset = GUILayout.Toggle(isContainAsset, ".asset");
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);
        if (GUILayout.Button("开始替换!"))
        {
            if (!ValidateInputs())
            {
                Debug.LogError("输入有误，请检查！");
                ShowNotification(new GUIContent("输入有误，请检查！"));
                return;
            }

            StartBatchReplace();
        }
    }

    private bool ValidateInputs()
    {
        if (_sourceOld == null || _sourceOld.Count == 0 || _sourceNew == null || _sourceNew.Count == 0)
        {
            return false;
        }

        if (_sourceOld.Count != _sourceNew.Count)
        {
            return false;
        }

        foreach (Object obj in _sourceOld)
        {
            if (obj == null)
                return false;
        }

        foreach (Object obj in _sourceNew)
        {
            if (obj == null)
                return false;
        }

        if (!isContainScene && !isContainPrefab && !isContainMat && !isContainAsset)
        {
            return false;
        }

        return true;
    }

    private void StartBatchReplace()
    {
        _oldGuids = new List<string>(_sourceOld.Count);
        _newGuids = new List<string>(_sourceNew.Count);

        for (int i = 0; i < _sourceOld.Count; i++)
        {
            Object oldObj = _sourceOld[i];
            Object newObj = _sourceNew[i];

            string oldPath = AssetDatabase.GetAssetPath(oldObj);
            string newPath = AssetDatabase.GetAssetPath(newObj);

            _oldGuids.Add(AssetDatabase.AssetPathToGUID(oldPath));
            _newGuids.Add(AssetDatabase.AssetPathToGUID(newPath));
        }

        withoutExtensions = new List<string>();
        if (isContainScene)
        {
            withoutExtensions.Add(".unity");
        }
        if (isContainPrefab)
        {
            withoutExtensions.Add(".prefab");
        }
        if (isContainMat)
        {
            withoutExtensions.Add(".mat");
        }
        if (isContainAsset)
        {
            withoutExtensions.Add(".asset");
        }

        FindAndReplace();
    }

    private void FindAndReplace()
    {
        if (withoutExtensions == null || withoutExtensions.Count == 0)
        {
            withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
        }

        string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();

        if (files == null || files.Length == 0)
        {
            Debug.Log("没有找到筛选的引用");
            return;
        }

        int totalFileCount = files.Length;
        int processedFileCount = 0;
        int replacedFileCount = 0;

        foreach (string file in files)
        {
            string content = File.ReadAllText(file);

            for (int i = 0; i < _oldGuids.Count; i++)
            {
                string oldGuid = _oldGuids[i];
                string newGuid = _newGuids[i];

                if (Regex.IsMatch(content, oldGuid))
                {
                    content = content.Replace(oldGuid, newGuid);
                    replacedFileCount++;
                }
            }

            File.WriteAllText(file, content);
            processedFileCount++;

            EditorUtility.DisplayProgressBar("替换资源中", file, (float)processedFileCount / totalFileCount);
        }

        EditorUtility.ClearProgressBar();
        EditorApplication.update = null;

        AssetDatabase.Refresh();

        Debug.Log(string.Format("替换完成，共处理文件数：{0}，替换文件数：{1}", processedFileCount, replacedFileCount));
    }
}
