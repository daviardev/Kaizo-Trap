using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {
    #region Layers
    private Dictionary<string, int> layerMap = new Dictionary<string, int>();

    private void Awake() {
        // map name the layers to yours index
        layerMap.Add("Shell", LayerMask.NameToLayer("Shell"));
        layerMap.Add("Ground", LayerMask.NameToLayer("Ground"));
        layerMap.Add("Player", LayerMask.NameToLayer("Player"));
        layerMap.Add("GrabShell", LayerMask.NameToLayer("GrabShell"));
    }
    #endregion

    private void Start() {
        #region Ignore collisions
        Physics2D.IgnoreLayerCollision(layerMap["Player"], layerMap["Shell"], true);
        Physics2D.IgnoreLayerCollision(layerMap["GrabShell"], layerMap["Shell"], true);
        Physics2D.IgnoreLayerCollision(layerMap["GrabShell"], layerMap["Ground"], true);
        #endregion
    }
}