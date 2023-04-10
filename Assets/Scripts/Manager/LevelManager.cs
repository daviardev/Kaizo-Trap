using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {
   public enum Layers {
        player = 7,
        ground = 3,
        shell = 6,
        grabShell = 8
    }

    private int[,] LayerCollisionToIgnore = new int[,] {
        {(int) Layers.player, (int) Layers.shell},
        {(int) Layers.grabShell, (int) Layers.ground},
        {(int) Layers.grabShell, (int) Layers.shell}
    };

    void Start() {
        for (int i = 0; i < LayerCollisionToIgnore.GetLength(0); i++) {
            Physics2D.IgnoreLayerCollision(LayerCollisionToIgnore[i, 0], LayerCollisionToIgnore[i, 1], true);
        }
    }
}