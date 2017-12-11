using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CellController : MonoBehaviour {
    public GameController GameController;
    public GameObject Text;
    private int _value;

    public int Value {
        set {
            if (value == _value || value == 0) return;
            _value = value;
            var text = Text.GetComponent<Text>();
            text.text = string.Format("{0}", value);
            if (value >= 1024) {
                text.fontSize = 8;
            }
            else if (value >= 128) {
                text.fontSize = 10;
            }
            else {
                text.fontSize = 14;
            }
            switch (value) {
                case 2:
                    text.color = new Color32(238, 228, 218, 255);
                    break;
                case 4:
                    text.color = new Color32(237, 224, 200, 255);
                    break;
                case 8:
                    text.color = new Color32(242, 177, 121, 255);
                    break;
                case 16:
                    text.color = new Color32(245, 149, 99, 255);
                    break;
                case 32:
                    text.color = new Color32(246, 124, 95, 255);
                    break;
                case 64:
                    text.color = new Color32(246, 94, 59, 255);
                    break;
                case 128:
                    text.color = new Color32(237, 207, 114, 255);
                    break;
                case 256:
                    text.color = new Color32(237, 204, 97, 255);
                    break;
                case 512:
                    text.color = new Color32(237, 200, 80, 255);
                    break;
                case 1024:
                    text.color = new Color32(237, 197, 63, 255);
                    break;
                default:
                    text.color = new Color32(237, 194, 46, 255);
                    break;
            }
        }
        get { return _value; }
    }


    public void DoMove(Vector2Int pos, int value, float time, bool toDestroy, IEnumerator counter) {
        var newPos = GameController.Logical2PhysicalPos(pos);
        if (toDestroy) {
            transform.DOMove(newPos, time).OnComplete(() => {
                gameObject.SetActive(false);
                GameController.CellCount--;
                if (!counter.MoveNext()) {
                    GameController.IsMoving = false;
                    GameController.PlaceNewCell(1);
                    GameController.CheckGameStatus();
                }
            });
        }
        else {
            transform.DOMove(newPos, time).OnComplete(() => {
                Value = value;
                if (!counter.MoveNext()) {
                    GameController.IsMoving = false;
                    GameController.PlaceNewCell(1);
                    GameController.CheckGameStatus();
                }
            });
        }
    }
}