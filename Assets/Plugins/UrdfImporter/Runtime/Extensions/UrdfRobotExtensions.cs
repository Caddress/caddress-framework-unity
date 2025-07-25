﻿/*
© Siemens AG, 2018
Author: Suzannah Smith (suzannah.smith@siemens.com)
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Unity.Robotics.UrdfImporter
{
    public static class UrdfRobotExtensions
    {
        static string collisionObjectName = "Collisions";
        public static ImportSettings importsettings;

        public static void Create()
        {
            CreateTag();
            GameObject robotGameObject = new GameObject("Robot");

            robotGameObject.AddComponent<UrdfRobot>();

            UrdfPlugins.Create(robotGameObject.transform);

            UrdfLink urdfLink = UrdfLinkExtensions.Create(robotGameObject.transform).GetComponent<UrdfLink>();
            urdfLink.name = "base_link";
            urdfLink.IsBaseLink = true;
        }

        #region Import

        // Note: The Create() function is now broken into multiple pipeline stages to facilitate having
        // both a non-blocking coroutine Create function as well as a blocking synchronous one needed for stable runtime import.

        // Is used to pass data and parameters between stages of the import pipeline.
        private class ImportPipelineData
        {
            public ImportSettings settings;
            public bool wasRuntimeMode;
            public bool loadStatus;
            public bool forceRuntimeMode;
            public Robot robot;
            public GameObject robotGameObject;
            public Stack<Tuple<Link, Transform, Joint>> importStack;
        }

        // Initializes import pipeline and reads the urdf file.
        private static ImportPipelineData ImportPipelineInit(string filename, ImportSettings settings, bool loadStatus, bool forceRuntimeMode)
        {
            ImportPipelineData im = new ImportPipelineData();
            im.settings = settings;
            im.loadStatus = loadStatus;
            im.wasRuntimeMode = RuntimeUrdf.IsRuntimeMode();
            im.forceRuntimeMode = forceRuntimeMode;

            if (forceRuntimeMode)
            {
                RuntimeUrdf.SetRuntimeMode(true);
            }

            im.robot = new Robot(filename);

            if (!UrdfAssetPathHandler.IsValidAssetPath(im.robot.filename))
            {
                Debug.LogError("URDF file and resources must be placed in project folder:" +
                    $"\n{Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length)}");
                if (forceRuntimeMode)
                { // set runtime mode back to what it was
                    RuntimeUrdf.SetRuntimeMode(im.wasRuntimeMode);
                }
                return null;
            }
            return im;
        }

        // Creates the robot game object.
        private static void ImportPipelineCreateObject(ImportPipelineData im)
        {
            im.robotGameObject = new GameObject(im.robot.name);

            importsettings = im.settings;
            im.settings.totalLinks = im.robot.links.Count;

            CreateTag();

            im.robotGameObject.AddComponent<UrdfRobot>();

            im.robotGameObject.GetComponent<UrdfRobot>().SetAxis(im.settings.chosenAxis);

            UrdfAssetPathHandler.SetPackageRoot(Path.GetDirectoryName(im.robot.filename));
            UrdfMaterial.InitializeRobotMaterials(im.robot);
            UrdfPlugins.Create(im.robotGameObject.transform, im.robot.plugins);
        }

        // Creates the stack of robot joints. Should be called iteratively until false is returned.
        private static bool ProcessJointStack(ImportPipelineData im)
        {
            if (im.importStack == null)
            {
                im.importStack = new Stack<Tuple<Link, Transform, Joint>>();
                im.importStack.Push(new Tuple<Link, Transform, Joint>(im.robot.root, im.robotGameObject.transform, null));
            }

            if (im.importStack.Count != 0)
            {
                Tuple<Link, Transform, Joint> currentLink = im.importStack.Pop();
                GameObject importedLink = UrdfLinkExtensions.Create(currentLink.Item2, currentLink.Item1, currentLink.Item3);
                im.settings.linksLoaded++;
                foreach (Joint childJoint in currentLink.Item1.joints)
                {
                    Link child = childJoint.ChildLink;
                    im.importStack.Push(new Tuple<Link, Transform, Joint>(child, importedLink.transform, childJoint));
                }
                return true;
            }
            return false;
        }

        // Post creation stuff: add to parent, fix axis and add collision exceptions.
        private static void ImportPipelinePostCreate(ImportPipelineData im)
        {
#if UNITY_EDITOR
            GameObjectUtility.SetParentAndAlign(im.robotGameObject, Selection.activeObject as GameObject);
            Undo.RegisterCreatedObjectUndo(im.robotGameObject, "Create " + im.robotGameObject.name);
            Selection.activeObject = im.robotGameObject;
#endif

            CorrectAxis(im.robotGameObject);
            CreateCollisionExceptions(im.robot, im.robotGameObject);

            if (im.forceRuntimeMode)
            { // set runtime mode back to what it was
                RuntimeUrdf.SetRuntimeMode(im.wasRuntimeMode);
            }
        }

        /// <summary>
        /// Coroutine to create a Robot game object from the urdf file
        /// </summary>
        /// <param name="filename">URDF filename</param>
        /// <param name="settings">Import Settings</param>
        /// <param name="loadStatus">If true, will show the progress of import step by step</param>
        /// <param name="forceRuntimeMode">
        /// When true, runs the runtime loading mode even in Editor. When false, uses the default behavior,
        /// i.e. runtime will be enabled in standalone build and disable when running in editor.
        /// In runtime mode, the Controller component of the robot will be added but not activated automatically and has to be enabled manually.
        /// This is to allow initializing the controller values (stiffness, damping, etc.) before the controller.Start() is called
        /// </param>
        /// <returns></returns>
        public static IEnumerator<GameObject> Create(string filename, ImportSettings settings, bool loadStatus = false, bool forceRuntimeMode = false)
        {
            UrdfGeometryCollision.BeginNewUrdfImport();
            ImportPipelineData im = ImportPipelineInit(filename, settings, loadStatus, forceRuntimeMode);
            if (im == null)
            {
                yield break;
            }

            ImportPipelineCreateObject(im);

            while (ProcessJointStack(im))
            {
                if (loadStatus)
                {
                    yield return null;
                }
            }

            ImportPipelinePostCreate(im);
            yield return im.robotGameObject;
        }

        /// <summary>
        /// Create a Robot gameobject from filename in runtime.
        /// It is a synchronous (blocking) function and only returns after the gameobject has been created.
        /// </summary>
        /// <param name="filename">URDF filename</param>
        /// <param name="settings">Import Settings</param>
        /// <returns> Robot game object</returns>
        public static GameObject CreateRuntime(string filename, ImportSettings settings, bool bThrowException = false)
        {
            ImportPipelineData im = null;
            try
            {
                im = ImportPipelineInit(filename, settings, false, true);
            }
            catch (Exception e)
            {
                throw new Exception("URDF文件不合法!");
            }
            if (im == null)
            {
                throw new Exception("URDF文件不合法!");
            }
            try
            {
                ImportPipelineCreateObject(im);
                while (ProcessJointStack(im))
                {// process the stack until finished.
                }
                ImportPipelinePostCreate(im);
            }
            catch (Exception e)
            {
                throw new Exception("URDF模型解析错误!");
            }
            if (im.robotGameObject == null)
            {
                throw new Exception("URDF模型解析错误!");
            }
            else
            {
                var mfs = im.robotGameObject.GetComponentsInChildren<MeshFilter>();
                if (mfs == null || mfs.Length == 0)
                {
                    throw new Exception("URDF模型网格解析生成失败!");
                }
                var mrs = im.robotGameObject.GetComponentsInChildren<MeshRenderer>();
                if (mrs == null || mrs.Length == 0)
                {
                    throw new Exception("URDF模型材质解析生成失败!");
                }
            }

            return im.robotGameObject;
        }

        public static void CorrectAxis(GameObject robot)
        {
            //Debug.Log("hit");
            UrdfRobot robotScript = robot.GetComponent<UrdfRobot>();
            if (robotScript == null)
            {
                Debug.LogError("Robot has no UrdfRobot component attached. Abandon correcting axis");
                return;
            }

            if (robotScript.CheckOrientation())
            {
                return;
            }
            Quaternion correctYtoZ = Quaternion.Euler(-90, 0, 90);
            Quaternion correctZtoY = Quaternion.Inverse((correctYtoZ));
            Quaternion correction = new Quaternion();

            if (robotScript.chosenAxis == ImportSettings.axisType.zAxis)
            {
                correction = correctYtoZ;
            }
            else
            {
                correction = correctZtoY;
            }

            UrdfVisual[] visualMeshList = robot.GetComponentsInChildren<UrdfVisual>();
            UrdfCollision[] collisionMeshList = robot.GetComponentsInChildren<UrdfCollision>();
            foreach (UrdfVisual visual in visualMeshList)
            {
                visual.transform.localRotation = visual.transform.localRotation * correction;
            }

            foreach (UrdfCollision collision in collisionMeshList)
            {
                if (collision.geometryType == GeometryTypes.Mesh)
                {
                    collision.transform.localRotation = collision.transform.localRotation * correction;
                }
            }
            robotScript.SetOrientation();
        }

        private static void CreateCollisionExceptions(Robot robot, GameObject robotGameObject)
        {
            List<CollisionIgnore> CollisionList = new List<CollisionIgnore>();
            if (robot.ignoreCollisionPair.Count > 0)
            {
                foreach (Tuple<string, string> ignoreCollision in robot.ignoreCollisionPair)
                {
                    Transform collisionObject1 = GameObject.Find(ignoreCollision.Item1).transform.Find(collisionObjectName);
                    Transform collisionObject2 = GameObject.Find(ignoreCollision.Item2).transform.Find(collisionObjectName);

                    CollisionList.Add(new CollisionIgnore(collisionObject1, collisionObject2));
                }
            }
            robotGameObject.GetComponent<UrdfRobot>().collisionExceptions = CollisionList;
        }

        #endregion

        #region Export

        public static void ExportRobotToUrdf(this UrdfRobot urdfRobot, string exportRootFolder, string exportDestination = "")
        {
#if UNITY_EDITOR
            UrdfExportPathHandler.SetExportPath(exportRootFolder, exportDestination);

            urdfRobot.FilePath = Path.Combine(UrdfExportPathHandler.GetExportDestination(), urdfRobot.name + ".urdf");

            Robot robot = urdfRobot.ExportRobotData();
            if (robot == null) return;

            robot.WriteToUrdf();

            Debug.Log(robot.name + " was exported to " + UrdfExportPathHandler.GetExportDestination());

            UrdfMaterial.Materials.Clear();
            UrdfExportPathHandler.Clear();
            AssetDatabase.Refresh();
#else
            Debug.LogError("URDF Export is only available in Editor.");
#endif
        }

        private static Robot ExportRobotData(this UrdfRobot urdfRobot)
        {
#if UNITY_EDITOR
            Robot robot = new Robot(urdfRobot.FilePath, urdfRobot.gameObject.name);

            List<string> linkNames = new List<string>();

            foreach (UrdfLink urdfLink in urdfRobot.GetComponentsInChildren<UrdfLink>())
            {
                //Link export
                if (linkNames.Contains(urdfLink.name))
                {
                    EditorUtility.DisplayDialog("URDF Export Error",
                        "URDF export failed. There are several links with the name " +
                        urdfLink.name + ". Make sure all link names are unique before exporting this robot.",
                        "Ok");
                    return null;
                }
                robot.links.Add(urdfLink.ExportLinkData());
                linkNames.Add(urdfLink.name);
            }

            robot.materials = UrdfMaterial.Materials.Values.ToList();
            robot.plugins = urdfRobot.GetComponentInChildren<UrdfPlugins>().ExportPluginsData();

            return robot;
#else
            Debug.LogError("URDF Export is only available in Editor.");
            return null;
#endif
        }

        #endregion

        public static void CreateTag()
        {
#if UNITY_EDITOR
            if (RuntimeUrdf.IsRuntimeMode())
            {
                // This is to make the behavior consistent with Runtime mode
                // as tags cannot be created in a Standalone build.
                return;
            }

            // Open tag manager
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            tagManager.ApplyModifiedProperties();
#endif
        }
    }
}
