using UnityEngine;
using System.IO;
using LitJson;
using Caddress.Common;
using UnityEditor;
using System.Collections.Generic;

namespace Caddress.Tools {
    public static class CustomModelLoader {
        public enum LoadType {
            Trilib,
            Urdf,
            Ply
        }

        private static float _openPathInterval = 0f;
        private static string _modelIndexPath = Application.streamingAssetsPath + "/Bundles/CustomModel/";
        private static ModelLoader _customModel = null;
        public static InfoCache cache { get; private set; }

        public static void AddModelFileByFile(string ext) {
            OpenFileName openFileName = GetOpenFileName();
            openFileName.filter = CustomModelExtension.OpenFileFilterSelect();
            openFileName.filterIndex = CustomModelExtension.GetFilterIndex(ext);

            if (Time.time - _openPathInterval <= 0.1f)
                return;
            if (LocalDialog.GetOpenFileName(openFileName)) {
                string path = openFileName.file;
                if (!string.IsNullOrEmpty(path)) {
                    ext = Path.GetExtension(path).ToLower();
                    if (ext == ".zip") {
                        AddModelZip(path, ext, openFileName.fileTitle);
                    }
                    else {
                        AddModelFile(path, ext, openFileName.fileTitle);
                    }
                }
            }
            _openPathInterval = Time.time;
        }

        private static void AddModelZip(string path, string ext, string fileTitle) {
            string guid = Util.GetNewModelID();
            string extractPath = _modelIndexPath + guid;
            if (fileTitle.Contains(" ") || fileTitle.Contains("\t") || fileTitle.Contains("\n")) {
                Debug.LogError("导入失败！模型名称不能含有特殊字符: " + fileTitle);
                return;
            }
            if (string.IsNullOrEmpty(path)) {
                Debug.LogError("导入失败！模型路径为空！");
                return;
            }
            var options = new Ionic.Zip.ReadOptions();
            options.Encoding = System.Text.Encoding.GetEncoding("GB2312");
            var zipFile = Ionic.Zip.ZipFile.Read(path, options);
            string maxPath = "";
            string mtlPath = "";
            bool isBreak = false;
            foreach (var item in zipFile) {
                if (!isBreak) {
                    foreach (string modelExt in CustomModelExtension.ModelExtList) {
                        if (item.FileName.ToLower().Contains(modelExt.ToLower())) {
                            fileTitle = item.FileName;
                            ext = modelExt;
                            isBreak = true;
                            break;
                        }
                    }
                }
                if (item.FileName.ToLower().Contains(".max"))
                    maxPath = extractPath + "/" + item.FileName;
                if (item.FileName.ToLower().Contains(".mtl"))
                    mtlPath = extractPath + "/" + item.FileName;
                item.Extract(extractPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
            }
            if (ext == ".zip") {
                Debug.LogError("导入失败！未找到可以加载的模型文件。");
                return;
            }
            string modelPath = extractPath + "/" + fileTitle;
            CreateObject(fileTitle, modelPath, extractPath, ext, guid, maxPath, mtlPath);
        }

        private static void AddModelFile(string path, string ext, string fileTitle) {
            string guid = Util.GetNewModelID();
            string extractPath = _modelIndexPath + guid;

            if (ext.ToLower() == ".urdf") {
                var tempDir = Path.Combine(Application.streamingAssetsPath, "TEMP_URDF");
                var pkger = new URDFPackager();
                pkger.ProcessURDF(path, tempDir);
                if (!pkger.bIsXML || !pkger.bIsURDF) {
                    Debug.LogError($"导入失败！URDF文件格式不合法，请检查。");
                    return;
                }
                if (string.IsNullOrEmpty(pkger.newUrdfPath)) {
                    Debug.LogError($"URDF文件解析出错。");
                    if (Directory.Exists(extractPath) && Directory.GetFiles(extractPath).Length == 0) {
                        System.IO.Directory.Delete(extractPath, true);
                    }
                    return;
                }
                path = pkger.newUrdfPath;
            }

            CreateObject(fileTitle, path, extractPath, ext, guid, "", "");
        }

        private static void CreateObject(string fileTitle, string path, string extractPath, string ext, string guid, string maxPath, string mtlPath) {
            string modelPath = extractPath + "/" + fileTitle;
            if (string.IsNullOrEmpty(modelPath)) {
                System.IO.Directory.Delete(extractPath, true);
                Debug.LogError("导入失败！未找到<color=red>\" + ext + \"</color>模型文件，请检查是否存在与模型包同名的模型文件。");
                return;
            }
            else {
                var obj = FindOrCreateCustomModel(guid, path).gameObject;
                if (obj != null) {
                    PrefabExporter.SaveModelAsPrefab(obj, fileTitle);
                }
            }
        }

        public static Transform FindOrCreateCustomModel(string id, string path) {
            Transform model = null;
            string _p = path.Replace("\\", "/");
            var sourceModelPath = _p.Substring(0, _p.LastIndexOf("/"));
            string ext = path.Substring(path.LastIndexOf("."));
            string filePath = $"{Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"))}/ModelTemp";
            if (Directory.Exists(filePath)) {
                Directory.Delete(filePath, true);
            }
            if (FileEncoding.HasChinese(path) && ext.ToLower() != ".obj") {
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                string p = filePath + "CustomModel" + ext;
                File.Copy(path, p, true);
                path = p;
            }
            GameObject gb = null;
            if (ext.ToLower() == ".obj") {
                gb = OBJLoader.LoadOBJFile(path);
            }
            else if (ext.ToLower() == ".urdf") {
                gb = LoadingModel(path, LoadType.Urdf).GetModel();
            }
            else if (ext.ToLower() == ".ply") {
                gb = LoadingModel(path, LoadType.Ply).GetModel();
            }
            else {
                gb = LoadingModel(path, LoadType.Trilib).GetModel();
            }

            gb.SetActive(true);
            model = gb.transform;
            foreach (Transform item in model) {
                item.localScale = Vector3.one;
            }

            Util.ResetModelCenter(model);
            return model;
        }

        public static ModelLoader LoadingModel(string file, LoadType type) {
            switch (type) {
                case LoadType.Trilib:
                    _customModel = new ModelLoadByTriLib(file);
                    _customModel.ModelLoading();
                    cache = _customModel.Cache();
                    break;
                case LoadType.Urdf:
                    _customModel = new URDFModelLoader(file);
                    _customModel.ModelLoading();
                    cache = _customModel.Cache();
                    break;
                case LoadType.Ply:
                    _customModel = new PlyModelLoader(file);
                    _customModel.ModelLoading();
                    cache = _customModel.Cache();
                    break;
            }

            return _customModel;
        }

        private static OpenFileName GetOpenFileName() {
            OpenFileName openFileName = new OpenFileName();
            openFileName.structSize = System.Runtime.InteropServices.Marshal.SizeOf(openFileName);
            openFileName.filter = "模型压缩文件(*.zip)\0*.zip\0";
            openFileName.file = new string(new char[256]);
            openFileName.maxFile = openFileName.file.Length;
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.initialDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            openFileName.title = "导入模型";
            openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

            return openFileName;
        }
    }

}
