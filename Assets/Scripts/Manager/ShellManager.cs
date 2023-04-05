using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShellData {
    public int id;
    public bool isGrabbed;
    public GameObject shellObject;
}

public class PlayerInteractionsShell : MonoBehaviour {
    #region Variables for interaction with sell
    const float kickForce = 500f;

    GameObject shell;

    public Transform hand;
    public GameObject shellPrefab;

    private int nextId = 0;
    private List<ShellData> shellList = new List<ShellData>();
    #endregion

    private void SpawShell(Vector2 position) {
        GameObject newShellObject = Instantiate(shellPrefab, position, Quaternion.identity);
        ShellData newShellData = new ShellData {
            id = nextId++,
            isGrabbed = false,
            shellObject = newShellObject
        };
        shellList.Add(newShellData);
    }

    public void GrabShell(int id) {
        ShellData shellData = shellList.Find(s => s.id == id && !s.isGrabbed);
        if (shellData != null) {
            GameObject shellObject = shellData.shellObject;

            shellData.isGrabbed = true;

            shellObject.transform.parent = hand;
            shellObject.transform.localPosition = Vector3.zero;

            Rigidbody2D rbShell = shellObject.GetComponent<Rigidbody2D>();
            if (rbShell != null) {
                Object.Destroy(rbShell);
            }

            shellObject.layer = (int) LevelManager.Layers.grabShell;
        }
    }

    public void ReleaseShell(int id) {
        ShellData shellData = shellList.Find(s => s.id == id && s.isGrabbed);
        if (shellData != null) {
            GameObject shellObject = shellData.shellObject;

            shellData.isGrabbed = false;

            shellObject.transform.parent = null;

            Rigidbody2D rbShell = shellObject.AddComponent<Rigidbody2D>();
            rbShell.freezeRotation = true;
            rbShell.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rbShell.gravityScale = 3;

            KickShell(shellObject);

            shellObject.layer = (int) LevelManager.Layers.shell;
        }
    }

    public void KickShell(GameObject shellObject) {
        Rigidbody2D rbShell = shellObject.GetComponent<Rigidbody2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 scaleLocal = rb.transform.localScale;
        
        rbShell.AddForce(new Vector2(kickForce * scaleLocal.x, 0));
    }
}