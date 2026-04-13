using ThunderFighter.Spawning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThunderFighter.EditorSetup
{
    public static class RepairLevel01SpawnerBindings
    {
        [MenuItem("ThunderFighter/Repair Level_01 Spawner Bindings")]
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Level_01.unity", OpenSceneMode.Single);
            var spawner = Object.FindFirstObjectByType<EnemySpawner>();
            if (spawner == null)
            {
                Debug.LogError("[ThunderFighter] EnemySpawner not found in Level_01");
                return;
            }

            var wave = AssetDatabase.LoadAssetAtPath<WaveConfig>("Assets/Generated/Config/WaveConfig_Main.asset");
            var enemyBasic = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Generated/Prefabs/Enemy_Basic.prefab");

            var so = new SerializedObject(spawner);
            so.FindProperty("waveConfig").objectReferenceValue = wave;
            so.FindProperty("fallbackEnemyPrefab").objectReferenceValue = enemyBasic;
            so.FindProperty("fallbackSpawnInterval").floatValue = 1.1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(spawner);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AssetDatabase.SaveAssets();
            Debug.Log("[ThunderFighter] Level_01 spawner bindings repaired.");
        }
    }
}
