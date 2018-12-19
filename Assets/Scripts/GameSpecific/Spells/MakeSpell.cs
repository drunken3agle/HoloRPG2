﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeSpell {

/*	[MenuItem("BNJMO/Create Projectile")]
    public static void CreateAsset()
    {
        ProjectileObject asset = ScriptableObject.CreateInstance<ProjectileObject>();
        AssetDatabase.CreateAsset (asset, "Assets/StaticAssets/Prefabs/Projectiles/NewProjectileObject.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }*/

    public static void InstantiateObj(string tag)
    {   
        GameObject.Instantiate(Resources.Load(tag), Camera.main.transform.position, Quaternion.LookRotation(CameraHelper.Stats.camLookDir, Vector3.up));
    }

    public static void InstantiateObj(string tag, float speed)
    {   
        GameObject obj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(tag), Camera.main.transform.position, Quaternion.LookRotation(CameraHelper.Stats.camLookDir, Vector3.up));
        ISpell proj = obj.GetComponent<ISpell>();
        proj.Speed = speed;
    }

    public static void InstantiateObj(string tag, float speed, float lifeDuration)
    {   
        GameObject obj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(tag), Camera.main.transform.position, Quaternion.LookRotation(CameraHelper.Stats.camLookDir, Vector3.up));
        ISpell proj = obj.GetComponent<ISpell>();
        proj.Speed = speed;
        proj.LifeDuration = lifeDuration;
    }

    public static void InstantiateObj(string tag, Vector3 position)
    {
        GameObject obj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(tag), position, Quaternion.LookRotation(CameraHelper.Stats.camLookDir, Vector3.up));
    }

    public static void InstantiateObj(string tag, Vector3 position, Vector3 direction)
    {
        GameObject obj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(tag), position, Quaternion.LookRotation(direction, Vector3.up));
    }

    
}
