
using System.Collections.Generic;
using System.Diagnostics;

namespace Caddress.Tools
{
    public class CustomModelExtension
    {
        /// <summary>
        /// 所支持的模型格式
        /// </summary>
        public static List<string> ModelExtList = new List<string>()
        {
            ".3D",
            ".3DS",
            ".3MF",
            ".AC",
            ".AC3D",
            ".ACC",
            ".AMF",
            ".AMJ",
            ".ASE",
            ".ASK",
            ".B3D",
            ".BLEND",
            ".BVH",
            ".COB",
            ".DAE",
            ".DXF",
            ".ENFF",
            ".FBX",// (FBX SDK 2012 fully supported, later versions are partially supported)
            ".GLTF",
            ".GLB",
            ".IFC",//IFC-STEP
            ".IRR",
            ".LWO",
            ".LWS",
            ".LXO",
            ".MD2",
            ".MD3",
            ".MD5",
            ".MDC",
            ".MDL",
            ".MESH",
            ".MOT",
            ".MS3D",
            ".NDO",
            ".NFF",
            ".OBJ",
            ".OFF",
            ".OGEX",
            ".PLY",
            ".PMX",
            ".PRJ",
            ".Q3O",
            ".Q3S",
            ".RAW",
            ".SCN",
            ".SIB",
            ".SMD",
            ".STL",
            ".URDF",
            ".TER",
            ".UC",
            ".VTA",
            ".X",
            ".X3D",
            ".XGL",
            ".ZGL"
        };

        public static string OpenFileFilterSelect()
        {
            string filter = "所有格式\0*.fbx;*.obj;*.3ds;*.stl;*.urdf;*.dae;*.zip;*.ply\0" +
                     "FBX (*.fbx)\0*.fbx\0OBJ (*.obj)\0*.obj\03DS (*.3ds)\0*.3ds\0STL (*.stl)\0*.stl\0URDF (*.urdf)\0*.urdf\0DAE (*.dae)\0*.dae\0ZIP(模型、材质、贴图等) (*.zip)\0*.zip\0PLY (*.ply)\0";


            return filter;
        }

        public static int GetFilterIndex(string ext)
        {
            if(!string.IsNullOrEmpty(ext))
                ext = ext.ToLower();

            int index = 0;
            switch (ext)
            {
                case ".fbx":
                    index = (int) FilterIndex.fbx;
                    break;
                case ".obj":
                    index = (int) FilterIndex.obj;
                    break;
                case ".3ds":
                    index = (int) FilterIndex.ds;
                    break;
                case ".stl":
                    index = (int) FilterIndex.stl;
                    break;
                case ".urdf":
                    index = (int) FilterIndex.urdf;
                    break;
                case ".dae":
                    index = (int) FilterIndex.dae;
                    break;
                case ".ply":
                    index = (int)FilterIndex.ply;
                    break;
                case ".zip":
                    index = (int) FilterIndex.zip;
                    break;
            }

            return index;
        }

        enum FilterIndex
        {
            fbx = 2,
            obj = 3,
            ds = 4,
            stl = 5,
            urdf = 6,
            dae = 7,
            zip = 8,
            ply = 9
        }
    }
}
