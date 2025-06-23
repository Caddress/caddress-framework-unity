#pragma warning disable 649
using UnityEngine;
using System.IO;
using System.Linq;
namespace TriLib
{
    namespace Samples
    {
        /// <summary>
        /// Represents a sample that lets user load assets from "persistentDataPath"
        /// </summary>
        public class PersistentDataPathLoadSample : MonoBehaviour
		{
			/// <summary>
			/// Reference to available assets.
			/// </summary>
			private string[] _files;

			/// <summary>
			/// Reference to latest loaded <see cref="UnityEngine.GameObject"/>.
			/// </summary>
			private GameObject _loadedGameObject;

			/// <summary>
			/// Stores a reference to all assets supported by TriLib located at "persistentDataPath".
			/// </summary>
			private void Start ()
			{
				var filter = AssetLoaderBase.GetSupportedFileExtensions();
				_files = Directory.GetFiles(Application.persistentDataPath, "*.*").Where(x => filter.Contains("*" + FileUtils.GetFileExtension(x) + ";")).ToArray();

            }

			/// <summary>
			/// Displays the available assets and let user load them by clicking.
			/// </summary>
			private void OnGUI ()
			{
				#if UNITY_IOS && !USE_IOS_FILES_SHARING
				GUILayout.Label ("Please tick \"iOS File Sharing Enabled\" on TriLib menu to use iTunes File Sharing");
				#endif
				GUILayout.Label ("Listing assets located at:");
				GUILayout.TextField(Application.persistentDataPath);
				foreach (var file in _files)
                {
					if (GUILayout.Button(FileUtils.GetShortFilename(file), GUILayout.Width(Screen.width * 0.25f))) {
						using (var assetLoader = new AssetLoader ())
                        {
                            Debug.Log("assetLoader: " + assetLoader);
							if (_loadedGameObject != null) {
								Destroy (_loadedGameObject);
							}

                            string _file = file.Replace("\\", "/");
                            Debug.Log(_file);

							GameObject rootNode = new GameObject("RootNode");
							_loadedGameObject = assetLoader.LoadFromFile(_file, null,gameObject);
                            _loadedGameObject.transform.SetParent(rootNode.transform);
							rootNode.transform.SetParent(transform);

							if (_loadedGameObject != null) {
								Camera.main.FitToBounds(_loadedGameObject.transform, 3f);
							}
						}
					}
				}
			}
		}
	}
}
#pragma warning restore 649