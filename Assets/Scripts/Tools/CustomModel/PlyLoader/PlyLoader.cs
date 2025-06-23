using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class PlyLoader {
    enum FormatType {
        Unknown,
        Ascii,
        BinaryLittleEndian,
        BinaryBigEndian
    }

    class PlyProperty {
        public string name;
        public string dataType;
        public bool isList;
        public string listCountType;
        public string listElementType;
    }

    class PlyElement {
        public string name;
        public int count;
        public List<PlyProperty> properties = new List<PlyProperty>();
    }

    public static Mesh Load(string filePath) {
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var br = new BinaryReader(fs)) {
            List<PlyElement> elements = new List<PlyElement>();
            FormatType format = FormatType.Unknown;
            bool headerEnded = false;

            var headerLines = new List<string>();
            while (!headerEnded) {
                string line = ReadLine(br);
                if (line == null)
                    throw new Exception("Unexpected end of file while reading header");

                headerLines.Add(line);
                if (line.StartsWith("format ")) {
                    if (line.Contains("ascii"))
                        format = FormatType.Ascii;
                    else if (line.Contains("binary_little_endian"))
                        format = FormatType.BinaryLittleEndian;
                    else if (line.Contains("binary_big_endian"))
                        format = FormatType.BinaryBigEndian;
                }
                else if (line.StartsWith("element ")) {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 3)
                        throw new Exception("Invalid element header: " + line);
                    PlyElement elem = new PlyElement {
                        name = parts[1],
                        count = int.Parse(parts[2])
                    };
                    elements.Add(elem);
                }
                else if (line.StartsWith("property ")) {
                    if (elements.Count == 0)
                        throw new Exception("Property defined before element");

                    var elem = elements[elements.Count - 1];
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    PlyProperty prop = new PlyProperty();

                    if (parts[1] == "list") {
                        prop.isList = true;
                        prop.listCountType = parts[2];
                        prop.listElementType = parts[3];
                        prop.name = parts[4];
                    }
                    else {
                        prop.isList = false;
                        prop.dataType = parts[1];
                        prop.name = parts[2];
                    }
                    elem.properties.Add(prop);
                }
                else if (line.StartsWith("end_header")) {
                    headerEnded = true;
                }
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            foreach (var elem in elements) {
                if (elem.name == "vertex") {
                    for (int i = 0; i < elem.count; i++) {
                        if (format == FormatType.Ascii) {
                            string line = ReadLine(br);
                            if (line == null) throw new Exception("Unexpected EOF reading vertex ascii");

                            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            int idx = 0;

                            float x = 0, y = 0, z = 0;
                            float nx = 0, ny = 0, nz = 0;
                            float u = 0, v = 0;
                            float r = 1, g = 1, b = 1, a = 1;

                            foreach (var prop in elem.properties) {
                                string valStr = parts[idx++];
                                switch (prop.name) {
                                    case "x": x = float.Parse(valStr); break;
                                    case "y": y = float.Parse(valStr); break;
                                    case "z": z = float.Parse(valStr); break;
                                    case "nx": nx = float.Parse(valStr); break;
                                    case "ny": ny = float.Parse(valStr); break;
                                    case "nz": nz = float.Parse(valStr); break;
                                    case "u": u = float.Parse(valStr); break;
                                    case "v": v = float.Parse(valStr); break;
                                    case "s": u = float.Parse(valStr); break;
                                    case "t": v = float.Parse(valStr); break;
                                    case "red": r = int.Parse(valStr) / 255f; break;
                                    case "green": g = int.Parse(valStr) / 255f; break;
                                    case "blue": b = int.Parse(valStr) / 255f; break;
                                    case "alpha": a = int.Parse(valStr) / 255f; break;
                                    default: break;
                                }
                            }

                            vertices.Add(new Vector3(x, y, z));
                            if (elem.properties.Exists(p => p.name == "nx"))
                                normals.Add(new Vector3(nx, ny, nz));
                            if (elem.properties.Exists(p => p.name == "red"))
                                colors.Add(new Color(r, g, b, a));
                            if (elem.properties.Exists(p => p.name == "u") || elem.properties.Exists(p => p.name == "s"))
                                uvs.Add(new Vector2(u, v));
                        }
                        else {
                            long startPos = fs.Position;
                            float x = 0, y = 0, z = 0;
                            float nx = 0, ny = 0, nz = 0;
                            float u = 0, v = 0;
                            float r = 1, g = 1, b = 1, a = 1;

                            foreach (var prop in elem.properties) {
                                if (prop.isList)
                                    throw new Exception("List property not supported in vertex");

                                object val = ReadBinaryValue(br, prop.dataType, format);

                                switch (prop.name) {
                                    case "x": x = Convert.ToSingle(val); break;
                                    case "y": y = Convert.ToSingle(val); break;
                                    case "z": z = Convert.ToSingle(val); break;
                                    case "nx": nx = Convert.ToSingle(val); break;
                                    case "ny": ny = Convert.ToSingle(val); break;
                                    case "nz": nz = Convert.ToSingle(val); break;
                                    case "u": u = Convert.ToSingle(val); break;
                                    case "v": v = Convert.ToSingle(val); break;
                                    case "s": u = Convert.ToSingle(val); break;
                                    case "t": v = Convert.ToSingle(val); break;
                                    case "red": r = Convert.ToByte(val) / 255f; break;
                                    case "green": g = Convert.ToByte(val) / 255f; break;
                                    case "blue": b = Convert.ToByte(val) / 255f; break;
                                    case "alpha": a = Convert.ToByte(val) / 255f; break;
                                }
                            }

                            vertices.Add(new Vector3(x, y, z));
                            if (elem.properties.Exists(p => p.name == "nx"))
                                normals.Add(new Vector3(nx, ny, nz));
                            if (elem.properties.Exists(p => p.name == "red"))
                                colors.Add(new Color(r, g, b, a));
                            if (elem.properties.Exists(p => p.name == "u") || elem.properties.Exists(p => p.name == "s"))
                                uvs.Add(new Vector2(u, v));
                        }
                    }
                }
                else if (elem.name == "face") {
                    for (int i = 0; i < elem.count; i++) {
                        if (format == FormatType.Ascii) {
                            string line = ReadLine(br);
                            if (line == null) throw new Exception("Unexpected EOF reading face ascii");
                            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            int vertexCount = int.Parse(parts[0]);
                            if (vertexCount == 3) {
                                int idx0 = int.Parse(parts[1]);
                                int idx1 = int.Parse(parts[2]);
                                int idx2 = int.Parse(parts[3]);
                                triangles.Add(idx0);
                                triangles.Add(idx1);
                                triangles.Add(idx2);
                            }
                            else if (vertexCount == 4) {
                                int idx0 = int.Parse(parts[1]);
                                int idx1 = int.Parse(parts[2]);
                                int idx2 = int.Parse(parts[3]);
                                int idx3 = int.Parse(parts[4]);
                                triangles.Add(idx0);
                                triangles.Add(idx1);
                                triangles.Add(idx2);

                                triangles.Add(idx0);
                                triangles.Add(idx2);
                                triangles.Add(idx3);
                            }
                            else {
                                throw new Exception("Face vertex count > 4 not supported");
                            }
                        }
                        else {
                            var prop = elem.properties[0];
                            if (!prop.isList)
                                throw new Exception("Face element's first property should be list");

                            int listCount = ReadBinaryListCount(br, prop.listCountType, format);
                            if (listCount == 3) {
                                int idx0 = Convert.ToInt32(ReadBinaryValue(br, prop.listElementType, format));
                                int idx1 = Convert.ToInt32(ReadBinaryValue(br, prop.listElementType, format));
                                int idx2 = Convert.ToInt32(ReadBinaryValue(br, prop.listElementType, format));
                                triangles.Add(idx0);
                                triangles.Add(idx1);
                                triangles.Add(idx2);
                            }
                            else if (listCount == 4) {
                                int idx0 = Convert.ToInt32(ReadBinaryValue(br, prop.listElementType, format));
                                int idx1 = Convert.ToInt32(ReadBinaryValue(br, prop.listElementType, format));
                                int idx2 = Convert.ToInt32(ReadBinaryValue(br, prop.listElementType, format));
                                int idx3 = Convert.ToInt32(ReadBinaryValue(br, prop.listElementType, format));
                                triangles.Add(idx0);
                                triangles.Add(idx1);
                                triangles.Add(idx2);

                                triangles.Add(idx0);
                                triangles.Add(idx2);
                                triangles.Add(idx3);
                            }
                            else {
                                throw new Exception("Face vertex count > 4 not supported");
                            }
                        }
                    }
                }
                else {
                    if (format == FormatType.Ascii) {
                        for (int i = 0; i < elem.count; i++)
                            ReadLine(br);
                    }
                    else {
                        throw new Exception("Binary format with unknown element not supported");
                    }
                }
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = vertices.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;

            mesh.SetVertices(vertices);

            if (normals.Count == vertices.Count)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            if (uvs.Count == vertices.Count)
                mesh.SetUVs(0, uvs);

            if (colors.Count == vertices.Count)
                mesh.SetColors(colors);

            if (triangles.Count > 0)
                mesh.SetTriangles(triangles, 0);
            else
                mesh.SetIndices(CreateIndices(vertices.Count), MeshTopology.Points, 0);

            mesh.RecalculateBounds();

            return mesh;
        }
    }

    static string ReadLine(BinaryReader br) {
        List<byte> bytes = new List<byte>();
        while (true) {
            if (br.BaseStream.Position == br.BaseStream.Length)
                return null;

            byte b = br.ReadByte();
            if (b == '\n')
                break;
            if (b != '\r')
                bytes.Add(b);
        }
        return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
    }

    static object ReadBinaryValue(BinaryReader br, string dataType, FormatType format) {
        switch (dataType) {
            case "char":
            case "int8":
                return br.ReadSByte();
            case "uchar":
            case "uint8":
                return br.ReadByte();
            case "short":
            case "int16":
                return br.ReadInt16();
            case "ushort":
            case "uint16":
                return br.ReadUInt16();
            case "int":
            case "int32":
                return br.ReadInt32();
            case "uint":
            case "uint32":
                return br.ReadUInt32();
            case "float":
            case "float32":
                return br.ReadSingle();
            case "double":
            case "float64":
                return br.ReadDouble();
            default:
                throw new Exception("Unsupported data type " + dataType);
        }
    }

    static int ReadBinaryListCount(BinaryReader br, string countType, FormatType format) {
        switch (countType) {
            case "char":
            case "int8":
                return br.ReadSByte();
            case "uchar":
            case "uint8":
                return br.ReadByte();
            case "short":
            case "int16":
                return br.ReadInt16();
            case "ushort":
            case "uint16":
                return br.ReadUInt16();
            case "int":
            case "int32":
                return br.ReadInt32();
            case "uint":
            case "uint32":
                return (int)br.ReadUInt32();
            default:
                throw new Exception("Unsupported count type " + countType);
        }
    }

    static int[] CreateIndices(int vertexCount) {
        int[] indices = new int[vertexCount];
        for (int i = 0; i < vertexCount; i++)
            indices[i] = i;
        return indices;
    }
}
