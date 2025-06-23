using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LitJson;
using Caddress.Tools;

#if UNITY_STANDALONE
public class OBJLoader {
    public static bool splitByMaterial = false;

    public static JsonData GetTextureInfo(string fn) {
        JsonData jd = new JsonData();
        if (Directory.Exists(fn)) {
            FileInfo mtlFileInfo = new FileInfo(fn);
            string mtlFileDirectory = mtlFileInfo.Directory.FullName + Path.DirectorySeparatorChar;
            foreach (string ln in File.ReadAllLines(fn, Encoding.GetEncoding("GB2312"))) {
                string l = ln.Trim().Replace("  ", " ");
                string[] cmps = l.Split(' ');
                string data = l.Remove(0, l.IndexOf(' ') + 1);
                string jkey, jvalue;
                if (cmps[0].ToLower() == "map_kd") {
                    //TEXTURE
                    string textureName = "";
                    string fpth = GetImagePath(data, mtlFileDirectory, out textureName);
                    if (fpth != null && File.Exists(fpth)) {
                        var texture = TextureLoader.LoadTexture(fpth);
                        jkey = textureName;
                        if (HasChinese(textureName)) {
                            texture.name = StringToUnicode(textureName);
                        }
                        else {
                            texture.name = textureName;
                        }
                        jvalue = texture.name;
                        jd[jkey] = jvalue;
                    }
                }
                else if (cmps[0].ToLower() == "map_bump") {
                    string textureName = "";
                    string fpth = GetImagePath(data, mtlFileDirectory, out textureName);
                    if (fpth != null && File.Exists(fpth)) {
                        var texture = TextureLoader.LoadTexture(fpth);
                        jkey = textureName;
                        if (HasChinese(textureName)) {
                            texture.name = StringToUnicode(textureName);
                        }
                        else
                            texture.name = textureName;

                        jvalue = texture.name;
                        jd[jkey] = jvalue; ;

                    }
                }
                else if (cmps[0].ToLower() == "map_ks") {
                    string textureName = "";
                    string fpth = GetImagePath(data, mtlFileDirectory, out textureName);
                    if (fpth != null && File.Exists(fpth)) {
                        var texture = TextureLoader.LoadTexture(fpth);
                        jkey = textureName;
                        if (HasChinese(textureName)) {
                            texture.name = StringToUnicode(textureName);
                        }
                        else {
                            texture.name = textureName;
                        }
                        jvalue = texture.name;
                        jd[jkey] = jvalue; ;

                    }
                }
            }
        }
        jd[""] = "";
        return jd;
    }

    public static Material[] LoadMTLFile(string fn) {
        Material currentMaterial = null;
        List<Material> matlList = new List<Material>();
        FileInfo mtlFileInfo = new FileInfo(fn);
        string baseFileName = Path.GetFileNameWithoutExtension(fn);
        string mtlFileDirectory = mtlFileInfo.Directory.FullName + Path.DirectorySeparatorChar;
        Encoding ft = FileEncoding.GetType(fn);
        string[] mtl = File.ReadAllLines(fn, ft);
        foreach (string ln in mtl) {
            string l = ln.Trim().Replace("  ", " ");
            string[] cmps = l.Split(' ');
            string data = l.Remove(0, l.IndexOf(' ') + 1);

            if (cmps[0] == "newmtl") {
                if (currentMaterial != null) {
                    matlList.Add(currentMaterial);
                }
                currentMaterial = new Material(Shader.Find("Standard"));
                currentMaterial.name = data;
            }
            else if (cmps[0].ToLower() == "map_kd") {
                string textureName = "";
                string fpth = null;
                if (cmps[1].Contains("\\")) {
                    fpth = GetImagePath(data, mtlFileDirectory, out textureName);
                }
                else {
                    fpth = mtlFileDirectory + cmps[1];
                    textureName = cmps[1];
                }
                if (fpth != null && File.Exists(fpth)) {
                    var texture = TextureLoader.LoadTexture(fpth);
                    if (HasChinese(textureName)) {
                        texture.name = StringToUnicode(textureName);
                    }
                    else {
                        texture.name = textureName;
                    }
                    Texture2D newTex = new Texture2D(texture.width, texture.height);
                    newTex.SetPixels(texture.GetPixels());
                    newTex.name = textureName;
                    newTex.Apply(true);
                    currentMaterial.SetTexture("_MainTex", newTex);

                    string ext = Path.GetExtension(fpth).ToLower();
                    if (ext == ".png" || ext == ".tga" || ext == ".dds") {
                        SetMaterialRenderingMode(currentMaterial, RenderingMode.Fade);
                    }
                }
            }
            else if (cmps[0] == "Kd") {
                currentMaterial.SetColor("_Color", ParseColorFromCMPS(cmps));
            }
            else if (cmps[0].ToLower() == "map_bump") {
                //TEXTURE
                string textureName = "";
                string fpth = GetImagePath(data, mtlFileDirectory, out textureName);
                if (fpth != null && File.Exists(fpth)) {
                    var texture = TextureLoader.LoadTexture(fpth);
                    if (HasChinese(textureName)) {
                        texture.name = StringToUnicode(textureName);
                    }
                    else
                        texture.name = textureName;


                    texture.SetPixels(texture.GetPixels(0, 0, texture.width, texture.height));
                    texture.Apply();

                    currentMaterial.EnableKeyword("_NORMALMAP");
                    currentMaterial.SetTexture("_BumpMap", texture);
                }
            }
            else if (cmps[0].ToLower() == "map_ks") {
                //TEXTURE
                string textureName = "";
                string fpth = GetImagePath(data, mtlFileDirectory, out textureName);
                if (fpth != null && File.Exists(fpth)) {
                    var texture = TextureLoader.LoadTexture(fpth);
                    if (HasChinese(textureName)) {
                        texture.name = StringToUnicode(textureName);
                    }
                    else
                        texture.name = textureName;


                    texture.SetPixels(texture.GetPixels(0, 0, texture.width, texture.height));
                    texture.Apply();

                    currentMaterial.EnableKeyword("_METALLICGLOSSMAP");
                    currentMaterial.SetTexture("_MetallicGlossMap", texture);
                }
            }
            else if (cmps[0] == "Ks") {
                currentMaterial.SetColor("_SpecColor", ParseColorFromCMPS(cmps));
            }
            else if (cmps[0] == "Ka") {
                currentMaterial.EnableKeyword("_EMISSION");
                currentMaterial.SetColor("_EmissionColor", ParseColorFromCMPS(cmps, 0.05f));
            }
            else if (cmps[0] == "d") {
                float visibility = float.Parse(cmps[1]);
                if (visibility < 1) {
                    Color temp = currentMaterial.color;

                    temp.a = visibility;
                    currentMaterial.SetColor("_Color", temp);
                    currentMaterial.SetFloat("_Mode", 3);
                    SetMaterialRenderingMode(currentMaterial, RenderingMode.Fade);
                }

            }
            else if (cmps[0] == "Ns") {
                float Ns = float.Parse(cmps[1]);
                Ns = (Ns / 1000);
                currentMaterial.SetFloat("_Glossiness", Ns);

            }
        }

        if (currentMaterial != null) {
            matlList.Add(currentMaterial);
        }
        return matlList.ToArray();
    }

    public static GameObject LoadOBJFile(string fn) {
        if (!File.Exists(fn)) {
            Debug.LogError("文件不存在:" + fn);
            return null;
        }
        string meshName = Path.GetFileNameWithoutExtension(fn);
        bool hasNormals = false;
        //OBJ LISTS
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        //UMESH LISTS
        List<Vector3> uvertices = new List<Vector3>();
        List<Vector3> unormals = new List<Vector3>();
        List<Vector2> uuvs = new List<Vector2>();
        //MESH CONSTRUCTION
        List<string> materialNames = new List<string>();
        List<string> objectNames = new List<string>();
        Dictionary<string, int> hashtable = new Dictionary<string, int>();
        List<OBJFace> faceList = new List<OBJFace>();
        string cmaterial = "";
        string cmesh = "default";
        Material[] materialCache = null;
        List<Material[]> materialCacheList = new List<Material[]>();
        FileInfo OBJFileInfo = new FileInfo(fn);
        Encoding ft = FileEncoding.GetType(fn);
        string[] objAllLine = File.ReadAllLines(fn, ft);//Encoding.GetEncoding("GB2312")
        foreach (string ln in objAllLine) {
            if (ln.Length > 0 && ln[0] != '#') {
                string l = ln.Trim().Replace("  ", " ");
                string[] cmps = l.Split(' ');
                string data = l.Remove(0, l.IndexOf(' ') + 1);
                string lineTitle = cmps[0];
                if (lineTitle == "mtllib") {
                    string fileName = Path.GetFileName(fn);
                    string mtlName = cmps[1].Replace(".mtl", "");
                    string mtlPath = fn.Replace(fileName, mtlName + ".mtl");
                    if (File.Exists(mtlPath)) {
                        materialCache = LoadMTLFile(mtlPath);
                        materialCacheList.Add(materialCache);
                    }
                }
                else if ((lineTitle == "g" || lineTitle == "o") && splitByMaterial == false) {
                    cmesh = data;
                    if (!objectNames.Contains(cmesh)) {
                        objectNames.Add(cmesh);
                    }
                }
                else if (lineTitle == "usemtl") {
                    cmaterial = data;
                    if (!materialNames.Contains(cmaterial)) {
                        materialNames.Add(cmaterial);
                    }

                    if (splitByMaterial) {
                        if (!objectNames.Contains(cmaterial)) {
                            objectNames.Add(cmaterial);
                        }
                    }
                }
                else if (lineTitle == "v") {
                    //VERTEX
                    vertices.Add(ParseVectorFromCMPS(cmps));
                }
                else if (lineTitle == "vn") {
                    //VERTEX NORMAL
                    normals.Add(ParseVectorFromCMPS(cmps));
                }
                else if (lineTitle == "vt") {
                    //VERTEX UV
                    uvs.Add(ParseVectorFromCMPS(cmps));
                }
                else if (lineTitle == "f") {
                    int[] indexes = new int[cmps.Length - 1];
                    for (int i = 1; i < cmps.Length; i++) {
                        string felement = cmps[i];
                        int vertexIndex = -1;
                        int normalIndex = -1;
                        int uvIndex = -1;
                        if (felement.Contains("//")) {
                            //doubleslash, no UVS.
                            string[] elementComps = felement.Split('/');
                            var one = int.Parse(elementComps[0]);
                            if (one < 0)
                                one += (vertices.Count + 1);
                            var three = int.Parse(elementComps[2]);
                            if (three < 0)
                                three += (normals.Count + 1);
                            vertexIndex = one - 1;
                            normalIndex = three - 1;
                        }
                        else if (felement.Count(x => x == '/') == 2) {
                            //contains everything
                            string[] elementComps = felement.Split('/');
                            var one = int.Parse(elementComps[0]);
                            if (one < 0)
                                one += (vertices.Count + 1);
                            var two = int.Parse(elementComps[1]);
                            if (two < 0)
                                two += (uvs.Count + 1);
                            var three = int.Parse(elementComps[2]);
                            if (three < 0)
                                three += (normals.Count + 1);
                            vertexIndex = one - 1;
                            uvIndex = two - 1;
                            normalIndex = three - 1;
                        }
                        else if (!felement.Contains("/")) {
                            //just vertex inedx
                            var one = int.Parse(felement);
                            if (one < 0)
                                one += (vertices.Count + 1);
                            vertexIndex = one - 1;
                        }
                        else {
                            //vertex and uv
                            string[] elementComps = felement.Split('/');
                            var one = int.Parse(elementComps[0]);
                            if (one < 0)
                                one += (vertices.Count + 1);
                            var two = int.Parse(elementComps[1]);
                            if (two < 0)
                                two += (uvs.Count + 1);
                            vertexIndex = one - 1;
                            uvIndex = two - 1;
                        }
                        string hashEntry = vertexIndex + "|" + normalIndex + "|" + uvIndex;
                        if (hashtable.ContainsKey(hashEntry)) {
                            indexes[i - 1] = hashtable[hashEntry];
                        }
                        else {
                            //create a new hash entry
                            indexes[i - 1] = hashtable.Count;
                            hashtable[hashEntry] = hashtable.Count;
                            uvertices.Add(vertices[vertexIndex]);
                            if (normalIndex < 0 || (normalIndex > (normals.Count - 1))) {
                                unormals.Add(Vector3.zero);
                            }
                            else {
                                hasNormals = true;
                                unormals.Add(normals[normalIndex]);
                            }
                            if (uvIndex < 0 || (uvIndex > (uvs.Count - 1))) {
                                uuvs.Add(Vector2.zero);
                            }
                            else {
                                uuvs.Add(uvs[uvIndex]);
                            }

                        }
                    }
                    if (indexes.Length < 5 && indexes.Length >= 3) {
                        OBJFace f1 = new OBJFace();
                        f1.materialName = cmaterial;
                        f1.indexes = new int[] { indexes[0], indexes[1], indexes[2] };
                        f1.meshName = (splitByMaterial) ? cmaterial : cmesh;
                        faceList.Add(f1);
                        if (indexes.Length > 3) {

                            OBJFace f2 = new OBJFace();
                            f2.materialName = cmaterial;
                            f2.meshName = (splitByMaterial) ? cmaterial : cmesh;
                            f2.indexes = new int[] { indexes[2], indexes[3], indexes[0] };
                            faceList.Add(f2);
                        }
                    }
                }
            }
        }
        if (objectNames.Count == 0)
            objectNames.Add("default");

        //build objects
        GameObject parentObject = new GameObject(meshName);
        GameObject body = new GameObject("Body");
        foreach (string obj in objectNames) {
            GameObject subObject = new GameObject(obj);
            subObject.transform.localScale = new Vector3(1, 1, 1);
            subObject.transform.parent = body.transform;
            body.transform.parent = parentObject.transform;
            //Create mesh
            Mesh m = new Mesh();
            m.name = obj;
            //LISTS FOR REORDERING
            List<Vector3> processedVertices = new List<Vector3>();
            List<Vector3> processedNormals = new List<Vector3>();
            List<Vector2> processedUVs = new List<Vector2>();
            List<int[]> processedIndexes = new List<int[]>();
            Dictionary<int, int> remapTable = new Dictionary<int, int>();
            //POPULATE MESH
            List<string> meshMaterialNames = new List<string>();

            OBJFace[] ofaces = faceList.Where(x => x.meshName == obj).ToArray();
            // Debug.Log("materialCount:  "+ materialNames.Count);
            foreach (string mn in materialNames) {
                OBJFace[] faces = ofaces.Where(x => x.materialName == mn).ToArray();
                if (faces.Length > 0) {
                    int[] indexes = new int[0];
                    foreach (OBJFace f in faces) {
                        int l = indexes.Length;
                        Array.Resize(ref indexes, l + f.indexes.Length);
                        int[] temp = f.indexes.Reverse().ToArray();
                        Array.Copy(temp, 0, indexes, l, f.indexes.Length);
                    }
                    meshMaterialNames.Add(mn);
                    if (m.subMeshCount != meshMaterialNames.Count)
                        m.subMeshCount = meshMaterialNames.Count;

                    for (int i = 0; i < indexes.Length; i++) {
                        int idx = indexes[i];
                        //build remap table
                        if (remapTable.ContainsKey(idx)) {
                            //ezpz
                            indexes[i] = remapTable[idx];
                        }
                        else {
                            processedVertices.Add(uvertices[idx]);
                            processedNormals.Add(unormals[idx]);
                            processedUVs.Add(uuvs[idx]);
                            remapTable[idx] = processedVertices.Count - 1;
                            indexes[i] = remapTable[idx];
                        }
                    }
                    processedIndexes.Add(indexes);
                }
                else {

                }
            }
            m.vertices = processedVertices.ToArray();
            m.normals = processedNormals.ToArray();
            m.uv = processedUVs.ToArray();

            for (int i = 0; i < processedIndexes.Count; i++) {
                m.SetTriangles(processedIndexes[i], i);
            }

            if (!hasNormals) {
                m.RecalculateNormals();
            }
            m.RecalculateBounds();
            m.Optimize();

            MeshFilter mf = subObject.AddComponent<MeshFilter>();
            MeshRenderer mr = subObject.AddComponent<MeshRenderer>();

            Material[] processedMaterials = new Material[meshMaterialNames.Count];
            for (int i = 0; i < meshMaterialNames.Count; i++) {
                if (materialCacheList.Count == 0)//materialCache == null
                {
                    processedMaterials[i] = new Material(Shader.Find("Standard"));// (Specular setup)
                }
                else {
                    Material mfn = null;
                    for (int j = 0; j < materialCacheList.Count; j++) {
                        for (int k = 0; k < materialCacheList[j].Length; k++) {
                            //Debug.Log("*******:  " + materialCacheList[j][k]);
                            if (materialCacheList[j][k].name == meshMaterialNames[i]) {
                                mfn = materialCacheList[j][k];
                            }
                        }
                    }
                    if (mfn == null) {
                        processedMaterials[i] = new Material(Shader.Find("Standard"));// (Specular setup)
                    }
                    else {
                        processedMaterials[i] = mfn;
                    }
                }
                var reflection = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                if (processedMaterials[i].GetTexture("_MetallicGlossMap") != null)
                    reflection = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
                if (processedMaterials[i].HasProperty("_SpecColor")) {
                    Color specC = processedMaterials[i].GetColor("_SpecColor");
                    if (specC != Color.black)
                        reflection = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
                }
                mr.reflectionProbeUsage = reflection;
                processedMaterials[i].name = meshMaterialNames[i];
            }
            mr.materials = processedMaterials;
            mf.mesh = m;
        }

        AutoAddBoxCollider(parentObject.transform);
        return parentObject;
    }

    public static void AutoAddBoxCollider(Transform parent) {
        Vector3 postion = parent.position;
        Quaternion rotation = parent.rotation;
        Vector3 scale = parent.localScale;
        parent.position = Vector3.zero;
        parent.rotation = Quaternion.Euler(Vector3.zero);
        parent.localScale = Vector3.one;

        Vector3 center = Vector3.zero;
        Renderer[] renders = parent.GetComponentsInChildren<Renderer>();
        foreach (Renderer child in renders) {
            center += child.bounds.center;
        }
        center /= renders.Length;
        Bounds bounds = new Bounds(center, Vector3.zero);
        foreach (Renderer child in renders) {
            bounds.Encapsulate(child.bounds);
        }
        BoxCollider boxCollider = parent.gameObject.AddComponent<BoxCollider>();
        boxCollider.center = bounds.center - parent.position;
        boxCollider.size = bounds.size;

        parent.position = postion;
        parent.rotation = rotation;
        parent.localScale = scale;
    }

    public static Vector3 ParseVectorFromCMPS(string[] cmps) {
        float x = float.Parse(cmps[1]);
        float y = float.Parse(cmps[2]);
        if (cmps.Length == 4) {
            float z = float.Parse(cmps[3]);
            return new Vector3(-x, y, z);
        }
        return new Vector2(x, y);
    }

    public static Color ParseColorFromCMPS(string[] cmps, float scalar = 1.0f) {
        float Kr = float.Parse(cmps[1]) * scalar;
        float Kg = float.Parse(cmps[2]) * scalar;
        float Kb = float.Parse(cmps[3]) * scalar;
        return new Color(Kr, Kg, Kb);
    }

    static string GetImagePath(string path, string basePath, out string fileName) {
        string[] strs = path.Split('\\');
        fileName = "";
        string p = null;
        if (strs.Length > 0) {
            string name = strs[strs.Length - 1];
            fileName = name;
            if (!File.Exists(basePath + name))
                p = CopyFileToTemp(basePath, name);
            else
                p = basePath + name;
            return p;
        }
        return null;
    }

    static string CopyFileToTemp(string url, string fileName) {
        string ext = fileName.Substring(fileName.LastIndexOf("."));
        var files = Directory.GetFiles(url, "*" + ext, SearchOption.AllDirectories);
        foreach (var file in files) {
            if (file.ToLower().Contains(fileName.ToLower())) {
                return file;
            }
        }
        return null;
    }

    public static string StringToUnicode(string value) {
        byte[] bytes = Encoding.Unicode.GetBytes(value);
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i += 2) {
            stringBuilder.AppendFormat("u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
        }
        return stringBuilder.ToString();
    }
    public static bool HasChinese(string text) {
        bool result = false;
        for (int i = 0; i < text.Length; i++)
            if ((int)text[i] > 127)
                result = true;
        return result;
    }

    public static void SetMaterialRenderingMode(Material material, RenderingMode renderingMode) {
        switch (renderingMode) {
            case RenderingMode.Opaque:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case RenderingMode.Cutout:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
                break;
            case RenderingMode.Fade:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case RenderingMode.Transparent:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }

    public static bool CheckModelVerticesCount(string fn) {
        string[] objAllLine = File.ReadAllLines(fn, Encoding.GetEncoding("GB2312"));
        int verticeCount = 0;
        foreach (string ln in objAllLine) {
            if (ln.Length > 0 && ln[0] == 'v') {
                ++verticeCount;
            }
        }
        if (verticeCount >= 65000) {
            Debug.LogError("导入失败！单个模型顶点不能超过65000个！");
            return false;
        }
        return true;
    }

    public enum RenderingMode {
        Opaque,
        Cutout,
        Fade,
        Transparent,
    }

    struct OBJFace {
        public string materialName;
        public string meshName;
        public int[] indexes;
    }
}
#endif