#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FPSRetroKit.Editor
{
    public static class WoodenStickSetupEditor
    {
        private const string PlayerPrefabPath = "Assets/Retro FPS Kit/Prefabs/Player.prefab";
        private const string StickPrefabPath = "Assets/Retro FPS Kit/Prefabs/WoodenStickHand.prefab";

        [MenuItem("Retro FPS Kit/Setup Wooden Stick Starting Weapon (All Levels + Player Prefab)")]
        public static void SetupAllLevelsAndPlayerPrefab()
        {
            SetupPlayerPrefab();

            string[] scenePaths =
            {
                "Assets/Retro FPS Kit/Scenes/level1.unity",
                "Assets/Retro FPS Kit/Scenes/AnotherLevel.unity",
                "Assets/Retro FPS Kit/Scenes/NewLevelExample.unity"
            };

            string originalScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;

            foreach (string scenePath in scenePaths)
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
                SetupCurrentScene();
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                Debug.Log($"Wooden Stick 시작 무기 설정 완료: {scenePath}");
            }

            if (!string.IsNullOrEmpty(originalScene))
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(originalScene);

            AssetDatabase.SaveAssets();
            Debug.Log("Player.prefab 및 모든 레벨 씬에 Wooden Stick 시작 무기 설정이 완료되었습니다.");
        }

        [MenuItem("Retro FPS Kit/Setup Wooden Stick Starting Weapon (Player Prefab)")]
        public static void SetupPlayerPrefab()
        {
            GameObject stickPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StickPrefabPath);
            if (stickPrefab == null)
            {
                Debug.LogError("WoodenStickHand.prefab 을 찾을 수 없습니다: " + StickPrefabPath);
                return;
            }

            GameObject playerRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (!TrySetupWeapons(playerRoot.transform, stickPrefab))
            {
                PrefabUtility.UnloadPrefabContents(playerRoot);
                return;
            }

            PrefabUtility.SaveAsPrefabAsset(playerRoot, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(playerRoot);
            AssetDatabase.SaveAssets();
            Debug.Log("Player.prefab 에 Wooden Stick 시작 무기 설정이 완료되었습니다.");
        }

        [MenuItem("Retro FPS Kit/Setup Wooden Stick Starting Weapon (Current Scene)")]
        public static void SetupCurrentScene()
        {
            GameObject stickPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StickPrefabPath);
            if (stickPrefab == null)
            {
                Debug.LogError("WoodenStickHand.prefab 을 찾을 수 없습니다: " + StickPrefabPath);
                return;
            }

            WeaponSwitch[] switches = Object.FindObjectsOfType<WeaponSwitch>(true);
            if (switches.Length == 0)
            {
                Debug.LogWarning("현재 씬에 WeaponSwitch 가 없습니다.");
                return;
            }

            foreach (WeaponSwitch weaponSwitch in switches)
            {
                if (TrySetupWeapons(weaponSwitch.transform, stickPrefab))
                    EditorUtility.SetDirty(weaponSwitch.gameObject);
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log($"현재 씬의 WeaponSwitch {switches.Length}개에 Wooden Stick 시작 무기 설정이 완료되었습니다.");
        }

        private static bool TrySetupWeapons(Transform weaponsRoot, GameObject stickPrefab)
        {
            if (weaponsRoot == null)
            {
                Debug.LogError("Weapons Transform 을 찾을 수 없습니다.");
                return false;
            }

            WeaponSwitch weaponSwitch = weaponsRoot.GetComponent<WeaponSwitch>();
            if (weaponSwitch == null)
            {
                Debug.LogError($"{weaponsRoot.name} 에 WeaponSwitch 가 없습니다.");
                return false;
            }

            Transform stickTransform = weaponsRoot.Find("WoodenStickHand");
            GameObject stickHand;

            if (stickTransform != null)
            {
                stickHand = stickTransform.gameObject;
            }
            else
            {
                stickHand = (GameObject)PrefabUtility.InstantiatePrefab(stickPrefab, weaponsRoot);
                stickHand.name = "WoodenStickHand";
            }

            stickHand.transform.SetAsFirstSibling();
            stickHand.SetActive(false);

            Text ammoText = FindAmmoText(weaponsRoot.root);
            WoodenStickWeapon stickWeapon = stickHand.GetComponent<WoodenStickWeapon>();
            if (stickWeapon != null && ammoText != null)
            {
                SerializedObject stickSerialized = new SerializedObject(stickWeapon);
                stickSerialized.FindProperty("weaponText").objectReferenceValue = ammoText;
                stickSerialized.ApplyModifiedPropertiesWithoutUndo();
            }

            SerializedObject switchSerialized = new SerializedObject(weaponSwitch);
            SerializedProperty weaponsProperty = switchSerialized.FindProperty("weapons");
            SerializedProperty initialWeaponProperty = switchSerialized.FindProperty("initialWeapon");

            List<Transform> weaponList = new List<Transform> { stickHand.transform };
            for (int i = 0; i < weaponsProperty.arraySize; i++)
            {
                Transform weapon = weaponsProperty.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                if (weapon != null && weapon != stickHand.transform)
                    weaponList.Add(weapon);
            }

            weaponsProperty.arraySize = weaponList.Count;
            for (int i = 0; i < weaponList.Count; i++)
                weaponsProperty.GetArrayElementAtIndex(i).objectReferenceValue = weaponList[i];

            initialWeaponProperty.intValue = 0;
            switchSerialized.ApplyModifiedPropertiesWithoutUndo();

            return true;
        }

        private static Text FindAmmoText(Transform playerRoot)
        {
            foreach (Text text in playerRoot.GetComponentsInChildren<Text>(true))
            {
                if (text.gameObject.name == "AmmoText")
                    return text;
            }

            return null;
        }
    }
}
#endif
