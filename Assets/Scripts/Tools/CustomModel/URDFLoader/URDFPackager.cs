using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using Caddress;

public class URDFPackager
{
    public bool bIsXML = false;
    public bool bIsURDF = false;
    public int meshCount = 0;
    public int meshFoundedCount = 0;
    public int meshCopiedCount = 0;
    public Dictionary<string, int> meshFounded = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> meshMissed = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    public string newUrdfPath = null;

    public void ProcessURDF(string inputUrdf, string outputRoot)
    {
        try
        {
            // 初始化路径
            inputUrdf = Path.GetFullPath(inputUrdf);
            string originalRoot = Path.GetDirectoryName(inputUrdf);
            int maxParentLevel = 8; // 最大上溯层级

            // 收集所有依赖项
            var dependencies = CollectDependencies(inputUrdf, originalRoot, maxParentLevel);
            if (dependencies == null || dependencies.Count == 0)
            {
                return;
            }
            // 复制文件并生成新URDF
            RepackageResources(inputUrdf, outputRoot, dependencies);

            Debug.Log("打包完成！");
        }
        catch (Exception e)
        {
            Debug.LogError($"处理失败: {e.Message}");
        }
    }

    Dictionary<string, string> CollectDependencies(string urdfPath, string originalRoot, int maxLevel)
    {
        var dependencyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var doc = new XmlDocument();
        doc.Load(urdfPath);
        bIsXML = true;
        bIsURDF = doc.SelectSingleNode("//robot") != null;
        if (!bIsURDF)
        {
            return null;
        }
        // 处理所有mesh节点
        foreach (XmlNode node in doc.SelectNodes("//mesh"))
        {
            var filenameAttr = node.Attributes["filename"];
            if (filenameAttr == null) continue;

            string originalPath = filenameAttr.Value;
            string foundPath = FindActualPath(originalPath, Path.GetDirectoryName(urdfPath), maxLevel);
            meshCount++;
            if (!string.IsNullOrEmpty(foundPath))
            {
                meshFoundedCount++;
                dependencyMap[originalPath] = foundPath;
                if (meshFounded.ContainsKey(originalPath))
                {
                    meshFounded[originalPath]++;
                }
                else
                {
                    meshFounded.Add(originalPath,1);
                }
            }
            else
            {
                Debug.LogError($"找不到资源文件: {originalPath}");
                if (meshMissed.ContainsKey(originalPath))
                {
                    meshMissed[originalPath]++;
                }
                else
                {
                    meshMissed.Add(originalPath, 1);
                }
            }
        }

        return dependencyMap;
    }

    string FindActualPath(string originalPath, string startDir, int maxLevel)
    {
        string currentDir = startDir;
        string normalizedPath = NormalizePath(originalPath);

        for (int i = 0; i < maxLevel; i++)
        {
            // 尝试直接路径
            string testPath = Path.Combine(currentDir, normalizedPath);
            if (File.Exists(testPath)) return testPath;

            // 尝试上溯父目录
            string parentDir = Directory.GetParent(currentDir)?.FullName;
            if (parentDir == null) break;

            // 构建上溯路径
            string relativePath = GetRelativePath(parentDir, currentDir);
            testPath = Path.Combine(parentDir, relativePath, normalizedPath);
            if (File.Exists(testPath)) return testPath;

            currentDir = parentDir;
        }
        return null;
    }

    void RepackageResources(string originalUrdf, string outputRoot, Dictionary<string, string> dependencies)
    {
        // 准备输出目录
        if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
        Directory.CreateDirectory(outputRoot);

        // 构建路径映射表
        var pathMapping = new Dictionary<string, string>();
        var dirs = new Dictionary<string,string>();
        foreach (var kvp in dependencies)
        {
            string newRelative = GetNewRelativePath(kvp.Value, outputRoot);
            pathMapping.Add(kvp.Key, newRelative);

            // 复制文件
            string destPath = Path.Combine(outputRoot, newRelative);
            //Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            //File.Copy(kvp.Value, destPath, true);
            var sourceDir = Path.GetDirectoryName(kvp.Value);
            var desDir = Path.GetDirectoryName(destPath);
            if (!dirs.ContainsKey(sourceDir))
            {
                dirs.Add(sourceDir, desDir);
            }
            meshCopiedCount++;
        }
        foreach (var dir in dirs)
        {
            Util.CopyFolder(dir.Key, dir.Value);
        }
        // 生成新的URDF文件
        GenerateNewURDF(originalUrdf, outputRoot, pathMapping);
    }

    void GenerateNewURDF(string originalPath, string outputRoot, Dictionary<string, string> pathMapping)
    {
        var doc = new XmlDocument();
        doc.Load(originalPath);

        // 更新所有路径引用
        foreach (XmlNode node in doc.SelectNodes("//mesh"))
        {
            var filenameAttr = node.Attributes["filename"];
            if (filenameAttr == null) continue;

            if (pathMapping.TryGetValue(filenameAttr.Value, out string newPath))
            {
                filenameAttr.Value = newPath.Replace('\\', '/');
            }
        }

        // 保存新URDF
        string urdfPath = Path.Combine(outputRoot, Path.GetFileName(originalPath));
        using (var writer = new StreamWriter(urdfPath, false, Encoding.UTF8))
        {
            doc.Save(writer);
        }
        newUrdfPath = urdfPath;
    }

    #region Utilities
    string NormalizePath(string path)
    {
        return path?
            .Replace("package://", "")
            .Replace("file://", "")
            .Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);
    }

    string GetRelativePath(string fromDir, string toPath)
    {
        Uri fromUri = new Uri(fromDir + Path.DirectorySeparatorChar);
        Uri toUri = new Uri(toPath);
        return Uri.UnescapeDataString(fromUri.MakeRelativeUri(toUri).ToString());
    }

    string GetNewRelativePath(string originalPath, string outputRoot)
    {
        // 保留原始路径的最后两部分
        string[] parts = originalPath.Split(Path.DirectorySeparatorChar);
        if (parts.Length < 2) return Path.GetFileName(originalPath);

        return Path.Combine(parts[parts.Length - 2], parts[parts.Length - 1]);
    }
    #endregion
}