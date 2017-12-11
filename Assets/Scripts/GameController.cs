using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public int BoardSize = 4;
    public float BoardWidth = 10f;
    public float GapPercentage = .2f;
    public float AnimTime = .3f;
    public int CellCount;
    public GameObject FailText;
    public bool IsMoving;

    private GameObject _board;
    private float _gridWidth;
    private float _gapWidth;
    private CellController[,] _cellBoard;
    private bool _moved;
    private bool _failed;

    private List<CellController> _cellPool;

    // speed up
    private int _cellTotalCount;

    private class CellState {
        public Vector2Int Pos;
        public bool ToDestroy;
        public int Value;
    }


    // Use this for initialization
    void Start() {
        // speed up
        _cellTotalCount = BoardSize * BoardSize;

        // state
        _gridWidth = BoardWidth / ((1 + GapPercentage) * BoardSize + GapPercentage);
        _gapWidth = GapPercentage * _gridWidth;
        _cellPool = new List<CellController>();
        _cellBoard = new CellController[BoardSize, BoardSize];
        CreateBoard();
        CreateBoardGrid();
        PlaceNewCell(2);
    }

    // Update is called once per frame
    void Update() {
        if (_failed && Input.anyKey) {
            SceneManager.LoadScene("main");
            return;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow)) {
            if (IsMoving) return;
            IsMoving = true;
            OnSwipe(Vector2Int.up);
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow)) {
            if (IsMoving) return;
            IsMoving = true;
            OnSwipe(Vector2Int.down);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
            if (IsMoving) return;
            IsMoving = true;
            OnSwipe(Vector2Int.left);
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow)) {
            if (IsMoving) return;
            IsMoving = true;
            OnSwipe(Vector2Int.right);
        }
    }

    private void OnSwipe(Vector2Int direction) {
        _moved = false;
        var newCellStates = new Dictionary<CellController, CellState>();
        for (var i = 0; i < BoardSize; i++) {
            for (var j = 0; j < BoardSize; j++) {
                var curCell = _cellBoard[i, j];
                if (curCell == null) continue;
                _do_move_cell(new Vector2Int(i, j), direction, newCellStates);
            }
        }
        if (!_moved) {
            IsMoving = false;
            return;
        }
        var counter = Counter(newCellStates.Count - 1);
        foreach (var kv in newCellStates) {
            var cellController = kv.Key;
            var cellState = kv.Value;
            cellController.DoMove(cellState.Pos, cellState.Value, AnimTime, cellState.ToDestroy, counter);
        }
    }

    private Vector2Int _do_move_cell(
        Vector2Int pos,
        Vector2Int direction,
        Dictionary<CellController, CellState> newCellStates) {
        var curCell = _cellBoard[pos.x, pos.y];
        if (newCellStates.ContainsKey(curCell)) {
            return pos;
        }
        var placed = false;
        var newPos = new Vector2Int(-1, -1);
        var tPos = pos + direction;

        var tPosPlaced = false;
        while (true) {
            if (!_is_in_board(tPos)) {
                tPos -= direction;
                break;
            }
            if (_cellBoard[tPos.x, tPos.y] != null) {
                tPosPlaced = true;
                break;
            }
            tPos += direction;
        }

        if (tPosPlaced) {
            var siblingPos = _do_move_cell(tPos, direction, newCellStates);
            var atCell = _cellBoard[siblingPos.x, siblingPos.y];
            if (newCellStates[atCell].Value == 0) {
                if (curCell.Value == atCell.Value) {
                    _moved = true;
                    var curState = new CellState();
                    newCellStates[curCell] = curState;
                    curState.Pos = siblingPos;
                    curState.ToDestroy = true;
                    var atState = newCellStates[atCell];
                    atState.Value = atCell.Value * 2;
                    _cellBoard[pos.x, pos.y] = null;
                    _cellPool.Add(curCell);
                    newPos = siblingPos;
                    placed = true;
                }
            }
            if (!placed) {
                newPos = siblingPos - direction;
            }
        }
        else {
            newPos = tPos;
        }

        if (placed || newCellStates.ContainsKey(curCell)) return newPos;
        if (pos != newPos) {
            _moved = true;
        }
        var state = new CellState();
        newCellStates[curCell] = state;
        state.Pos = newPos;
        _cellBoard[pos.x, pos.y] = null;
        _cellBoard[newPos.x, newPos.y] = curCell;
        return newPos;
    }

    private bool _is_in_board(Vector2Int pos) {
        return pos.x >= 0 && pos.x < BoardSize && pos.y >= 0 && pos.y < BoardSize;
    }

    private static IEnumerator Counter(int count) {
        for (var i = 0; i < count; i++) {
            yield return i;
        }
    }

    private void CreateBoard() {
        _board = new GameObject("Board");
        var spriteRenderer = _board.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>("2048");
        spriteRenderer.color = new Color32(187, 173, 160, 255);
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteRenderer.size = new Vector2(BoardWidth, BoardWidth);
    }

    private void CreateBoardGrid() {
        for (var i = 0; i < BoardSize; i++) {
            for (var j = 0; j < BoardSize; j++) {
                var grid = new GameObject("Grid");

                var spriteRenderer = grid.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = Resources.Load<Sprite>("2048");
                spriteRenderer.color = new Color32(204, 192, 179, 255);
                spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                spriteRenderer.size = new Vector2(_gridWidth, _gridWidth);
                spriteRenderer.sortingOrder = 1;

                grid.transform.parent = _board.transform;
                grid.transform.position = Logical2PhysicalPos(i, j);
            }
        }
    }

    private Vector2 Logical2PhysicalPos(int x, int y) {
        var px = x * (_gridWidth + _gapWidth) - BoardWidth / 2 + _gridWidth / 2 + _gapWidth;
        var py = y * (_gridWidth + _gapWidth) - BoardWidth / 2 + _gridWidth / 2 + _gapWidth;
        return new Vector2(px, py);
    }

    public Vector2 Logical2PhysicalPos(Vector2Int pos) {
        return Logical2PhysicalPos(pos.x, pos.y);
    }

    public void PlaceNewCell(int count) {
        if (CellCount == _cellTotalCount) return;
        var nullCellPosList = new List<Vector2Int>(_cellTotalCount - CellCount);
        for (var i = 0; i < BoardSize; i++) {
            for (var j = 0; j < BoardSize; j++) {
                if (_cellBoard[i, j] == null) {
                    nullCellPosList.Add(new Vector2Int(i, j));
                }
            }
        }
        for (var i = 0; i < count; i++) {
            var index = Random.Range(0, nullCellPosList.Count);
            var pos = nullCellPosList[index];
            nullCellPosList.RemoveAt(index);
            var cellController = CreateCellAtPos(pos, 2);
            _cellBoard[pos.x, pos.y] = cellController;
            if (++CellCount == _cellTotalCount) return;
        }
    }

    private CellController CreateCellAtPos(Vector2Int pos, int value) {
        var x = pos.x;
        var y = pos.y;
        GameObject cell;
        CellController script;
        if (_cellPool.Count != 0) {
            script = _cellPool[0];
            cell = script.gameObject;
            _cellPool.RemoveAt(0);
            cell.transform.position = Logical2PhysicalPos(x, y);
            cell.SetActive(true);
        }
        else {
            cell = new GameObject("Cell");
            var spriteRenderer = cell.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>("2048");
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(_gridWidth, _gridWidth);
            spriteRenderer.sortingLayerName = "Cell";
            spriteRenderer.color = Color.white;

            cell.transform.parent = _board.transform;
            cell.transform.position = Logical2PhysicalPos(x, y);

            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.transform.SetParent(cell.transform, false);
            canvas.transform.localScale = new Vector3(_gridWidth / 100, _gridWidth / 100);
            canvas.sortingLayerName = "Cell";
            canvas.sortingOrder = 1;
            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 10;

            var textObj = new GameObject("Text");
            var text = textObj.AddComponent<Text>();
            text.transform.SetParent(canvas.transform, false);
            text.transform.localScale = new Vector3(100f / 19, 100f / 19);
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.Load<Font>("HelveticaNeue");
            text.fontStyle = FontStyle.Bold;

            script = cell.AddComponent<CellController>();
            script.Text = textObj;
            script.GameController = this;
        }
        script.Value = value;
        return cell.GetComponent<CellController>();
    }

    public void CheckGameStatus() {
        for (var i = 0; i < BoardSize; i++) {
            for (var j = 0; j < BoardSize; j++) {
                var curCell = _cellBoard[i, j];
                if (curCell == null) return;
                if (_is_in_board(new Vector2Int(i, j - 1)) && _cellBoard[i, j - 1] != null) {
                    if (_cellBoard[i, j - 1].Value == curCell.Value) {
                        return;
                    }
                }
                if (_is_in_board(new Vector2Int(i, j + 1)) && _cellBoard[i, j + 1] != null) {
                    if (_cellBoard[i, j + 1].Value == curCell.Value) {
                        return;
                    }
                }
                if (_is_in_board(new Vector2Int(i - 1, j)) && _cellBoard[i - 1, j] != null) {
                    if (_cellBoard[i - 1, j].Value == curCell.Value) {
                        return;
                    }
                }
                if (_is_in_board(new Vector2Int(i + 1, j)) && _cellBoard[i + 1, j] != null) {
                    if (_cellBoard[i + 1, j].Value == curCell.Value) {
                        return;
                    }
                }
            }
        }
        YouFail();
    }

    private void YouFail() {
        _failed = true;
        FailText.SetActive(true);
    }
}