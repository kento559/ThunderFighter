using System.Linq;
using ThunderFighter.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThunderFighter.EditorSetup
{
    public static class ForceRebuildLevel01
    {
        [MenuItem("ThunderFighter/Force Rebuild Level_01 Camera+Player")]
        public static void Run()
        {
            const string scenePath = "Assets/Scenes/Level_01.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).ToArray())
            {
                Object.DestroyImmediate(cam.gameObject);
            }

            foreach (var p in Object.FindObjectsByType<ThunderFighter.Player.PlayerController>(FindObjectsSortMode.None).ToArray())
            {
                Object.DestroyImmediate(p.gameObject);
            }

            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);
            var camComp = cameraObj.AddComponent<Camera>();
            camComp.orthographic = true;
            camComp.orthographicSize = 5.2f;
            camComp.clearFlags = CameraClearFlags.SolidColor;
            camComp.backgroundColor = new Color(0.03f, 0.04f, 0.09f, 1f);
            camComp.nearClipPlane = 0.01f;
            camComp.farClipPlane = 100f;

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Generated/Prefabs/Player.prefab");
            GameObject player;
            if (playerPrefab != null)
            {
                player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            }
            else
            {
                player = new GameObject("Player");
                var sr = player.AddComponent<SpriteRenderer>();
                VisualDebugSprite.Ensure(sr);
                var rb = player.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                var col = player.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                player.AddComponent<ThunderFighter.Combat.HealthComponent>();
                player.AddComponent<ThunderFighter.InputSystem.KeyboardMouseInputProvider>();
                player.AddComponent<ThunderFighter.Combat.WeaponController>();
                player.AddComponent<ThunderFighter.Player.PlayerController>();
            }

            player.name = "Player";
            player.transform.position = new Vector3(0f, -3.7f, 0f);
            VisualDebugSprite.Ensure(player, new Color(0.35f, 0.9f, 1f, 1f), 20, 0.8f);

            if (player.GetComponent<ThunderFighter.Combat.DamageFeedback>() == null)
            {
                player.AddComponent<ThunderFighter.Combat.DamageFeedback>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[ThunderFighter] Forced rebuild complete: Level_01 camera + player");
        }
    }
}
