using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct Pos
{
	public Pos(int y, int x) { Y = y; X = x; }
	public int Y;
	public int X;
}

public struct PQNode : IComparable<PQNode>
{
	public int F;
	public int G;
	public int Y;
	public int X;

	public int CompareTo(PQNode other)
	{
		if (F == other.F)
			return 0;
		return F < other.F ? 1 : -1;
	}
}

public class MapManager
{
	public Grid CurrentGrid { get; private set; }
    public int CurrentMapId { get; set; }
    public int MinX { get; set; }
	public int MaxX { get; set; }
	public int MinY { get; set; }
	public int MaxY { get; set; }

	public int SizeX { get { return MaxX - MinX + 1; } }
	public int SizeY { get { return MaxY - MinY + 1; } }

	bool[,] _collision;
    private Dictionary<Vector3Int, GameObject> _portalDict = new Dictionary<Vector3Int, GameObject>(); // 문 객체들을 저장할 Dictionary
	private Dictionary<Vector3Int, GameObject> _questDict = new Dictionary<Vector3Int, GameObject>(); // 문 객체들을 저장할 Dictionary
    private Dictionary<int, Vector2Int> _cameraDict = new Dictionary<int, Vector2Int>(); // 카메라 포인트를 저장할 Dictionary
    private Dictionary<Vector2Int, ChestController> _chestDict = new Dictionary<Vector2Int, ChestController>(); // 상자 위치를 저장할 Dictionary
    private Dictionary<int , InteractionController> _interactionDict = new Dictionary< int, InteractionController>(); // 상호작용 위치를 저장할 Dictionary
    public bool CanGo(Vector3Int cellPos)
	{
        Vector3Int adjustedPos = new Vector3Int(cellPos.x + 1, cellPos.y + 1, 0);

        if (adjustedPos.x < MinX || adjustedPos.x > MaxX)
			return false;
		if (adjustedPos.y < MinY || adjustedPos.y > MaxY)
			return false;

		int x = adjustedPos.x - MinX;
		int y = MaxY - adjustedPos.y;
		return !_collision[y, x];
	}
    public int GetNextMap(int portalId)
    {
        Managers.Data.MapDict.TryGetValue(CurrentMapId, out MapData mapData);
        if (mapData == null)
            return 0;
        foreach (var portal in mapData.portals)
        {
           if(portal.id == portalId)
                return portal.mapId;
        }
        return 0;
    }
    public GameObject IsPlayerAtPortal(Vector3Int cellPos)
    {
        // 디버깅을 위해 좌표 출력
        //Debug.Log($"Checking door at cell position: {cellPos}");

        Vector3Int adjustedPos = new Vector3Int(cellPos.x + 1, cellPos.y + 1, 0);
        if(_portalDict.TryGetValue(adjustedPos, out var result))
			return result;
		return null;		
    }

    public GameObject GetPortalById(int id)
    {
        foreach(var portal in _portalDict)
        {
            if (int.Parse(portal.Value.name.Split("_")[1]) == id)
                return portal.Value;
        }
        return null;
    }

	public void RemovePortalByName(string portalName)
	{
		foreach(var portal in _portalDict)
        {
            if (portal.Value.name == portalName)
            {
                _portalDict.Remove(portal.Key);
                break;
            }
        }
    }

	public GameObject IsPlayerAtQuest(Vector3Int cellPos)
	{
		Vector3Int adjustedPos = new Vector3Int(cellPos.x + 1, cellPos.y + 1, 0);
		if (_questDict.TryGetValue(adjustedPos, out var result))
        {
            return result;
        }
		return null;
    }
    public ChestController GetChest(Vector2Int cellPos)
    {
        if (_chestDict.TryGetValue(cellPos, out var result))
        {
            return result;
        }
        else return null;
    }

    public InteractionController GetInteractionById(int templateId)
    {
        _interactionDict.TryGetValue(templateId, out InteractionController ic);
        return ic;        
    }

    public InteractionController GetInteraction(Vector2Int cellPos)
    {
        foreach (var interaction in _interactionDict.Values)
        {
            foreach (var pos in interaction.CellPoses)
            {
                if (pos == cellPos)
                    return interaction;
            }
        }
        return null;
    }

    public ChestController GetChestById(int id)
    {
        foreach (var chest in _chestDict.Values)
        {
            if (chest.ChestId == id)
                return chest;
        }
        return null;
    }

    public void SetChests(List<int> chestIds)
    {
        foreach (var chest in _chestDict.Values)
        {
            if (!chestIds.Contains(chest.ChestId))
                chest.gameObject.SetActive(false);
        }
    }

    public void SetInteractions(List<int> interactionIds)
    {
        foreach (var interaction in _interactionDict.Values)
        {
            if (interactionIds.Contains(interaction.TemplateId))
                interaction.DeactivateInteraction();
        }
    }

    public Vector2Int GetCameraPosition(int id)
    {
        return _cameraDict[id];
    }

    public void LoadMap(int mapId)
	{
		DestroyMap();
        ClearAllDict();
        string mapName = "Map_" + mapId.ToString("000");
		GameObject go = Managers.Resource.Instantiate($"Map/{mapName}");
		go.name = "Map";

		GameObject collision = Util.FindChild(go, "Tilemap_Collision", true);
		if (collision != null)
			collision.SetActive(false);

		CurrentGrid = go.GetComponent<Grid>();

		// Collision 관련 파일
		TextAsset txt = Managers.Resource.Load<TextAsset>($"Map/{mapName}");
		StringReader reader = new StringReader(txt.text);

		MinX = int.Parse(reader.ReadLine());
		MaxX = int.Parse(reader.ReadLine());
		MinY = int.Parse(reader.ReadLine());
		MaxY = int.Parse(reader.ReadLine());

		int xCount = MaxX - MinX + 1;
		int yCount = MaxY - MinY + 1;
		_collision = new bool[yCount, xCount];

		for (int y = 0; y < yCount; y++)
		{
			string line = reader.ReadLine();
			for (int x = 0; x < xCount; x++)
			{
				_collision[y, x] = (line[x] == '1' ? true : false);
			}
		}
        CurrentMapId = mapId;
        FindPortals();
        FindQuests();
        FindCameraPoints();
        FindChests();
        FindInteractions();
    }

	public void DestroyMap()
	{
		GameObject map = GameObject.Find("Map");
		if (map != null)
		{
			GameObject.Destroy(map);
			CurrentGrid = null;
        }
	}

    public void FindPortals()
    {
        GameObject[] portals = GameObject.FindGameObjectsWithTag("Portal");
        foreach (GameObject portal in portals)
        {
            Tilemap tilemap = portal.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                foreach (var pos in tilemap.cellBounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(pos))
                    {
                        Vector3Int cellPos = new Vector3Int(pos.x, pos.y, 0);
                        _portalDict[cellPos] = portal;
                    }
                }
            }
        }
    }
	private void FindQuests()
    {
        GameObject[] questObjects = GameObject.FindGameObjectsWithTag("Quest");
        foreach (GameObject questObject in questObjects)
        {
            // 퀘스트 오브젝트의 모든 타일을 _questDict에 추가
            Bounds bounds = questObject.GetComponent<Collider2D>().bounds;
            Vector3Int min = CurrentGrid.WorldToCell(bounds.min);
            Vector3Int max = CurrentGrid.WorldToCell(bounds.max);

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    _questDict[cellPos] = questObject;
                }
            }
        }
    }

    private void FindCameraPoints()
    {
        GameObject[] cameraObjects = GameObject.FindGameObjectsWithTag("Camera");
        foreach (GameObject cameraObject in cameraObjects)
        {
            int cameraId = int.Parse(cameraObject.name.Replace("Camera_", ""));
            Vector3Int cameraCellPos = CurrentGrid.WorldToCell(cameraObject.transform.position);
            _cameraDict[cameraId] = (Vector2Int)cameraCellPos;
        }
    }

    private void FindChests()
    {
        GameObject chestObject = GameObject.FindGameObjectWithTag("Chest");
        if (chestObject == null)
            return;

        ChestController[] chestControllers = chestObject.GetComponentsInChildren<ChestController>(true);

        foreach (ChestController chest in chestControllers)
        {
            string[] str = chest.gameObject.name.Split('_');
            string name = str[0];
            int templateId = str.Length > 1 ? int.Parse(str[1]) : 0;
            int chestId = str.Length > 2 ? int.Parse(str[2]) : 0;
            chest.SetChest(chestId, templateId);
            chest.CellPos = CurrentGrid.WorldToCell(chest.transform.position);
            Vector3Int chestPos = chest.CellPos;
            _chestDict[(Vector2Int)chestPos] = chest;
        }
    }

    private void FindInteractions()
    {
        GameObject[] interactionObjects = GameObject.FindGameObjectsWithTag("Interaction");
        foreach (GameObject interactionObject in interactionObjects)
        {
            Tilemap tilemap = interactionObject.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                foreach (var pos in tilemap.cellBounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(pos))
                    {
                        int interactionId = int.Parse(interactionObject.name.Split('_')[1]);
                        InteractionController ic = null;
                        _interactionDict.TryGetValue(interactionId, out ic);
                        if (ic == null)
                        {
                            ic = interactionObject.GetComponentInChildren<InteractionController>();
                            if (ic != null)
                            {
                                ic.SetInteraction(interactionId);
                                ic.CellPoses.Add(new Vector2Int(pos.x, pos.y));
                                _interactionDict[interactionId] = ic;
                            }                            
                        }
                        else
                        {
                            ic.CellPoses.Add(new Vector2Int(pos.x, pos.y));
                        }
                        Debug.Log($"Interaction Pos:{pos.x},{pos.y}");
                    }
                }
            }
        }
    }
    public void SetCollision(Vector2Int cellPos, bool collision)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX)
            return;
        if (cellPos.y < MinY || cellPos.y > MaxY)
            return;

        int x = cellPos.x - MinX;
        int y = MaxY - cellPos.y;
        _collision[y, x] = collision;
    }
    private void ClearAllDict()
    {
        _portalDict.Clear();
        _questDict.Clear();
        _cameraDict.Clear();
        _chestDict.Clear();
        _interactionDict.Clear();
    }
    public int GetPortalId(string name)
    {
		foreach (var mapData in Managers.Data.MapDict)
        {			
            if (mapData.Value.name == name)
            {
                return mapData.Value.id;
            }
        }
        return 0;
    }

    #region A* PathFinding

    // U D L R
    int[] _deltaY = new int[] { 1, -1, 0, 0 };
	int[] _deltaX = new int[] { 0, 0, -1, 1 };
	int[] _cost = new int[] { 10, 10, 10, 10 };

    public List<Vector2Int> FindPath(Vector3Int startCellPos, Vector3Int destCellPos, bool ignoreDestCollision = false)
    {
        List<Pos> path = new List<Pos>();

        // 점수 매기기
        // F = G + H
        // F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
        // G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
        // H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)

        // (y, x) 이미 방문했는지 여부 (방문 = closed 상태)
        bool[,] closed = new bool[SizeY, SizeX]; // CloseList

        // (y, x) 가는 길을 한 번이라도 발견했는지
        // 발견X => MaxValue
        // 발견O => F = G + H
        int[,] open = new int[SizeY, SizeX]; // OpenList
        for (int y = 0; y < SizeY; y++)
            for (int x = 0; x < SizeX; x++)
                open[y, x] = Int32.MaxValue;

        Pos[,] parent = new Pos[SizeY, SizeX];

        // 오픈리스트에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
        PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

        // CellPos -> ArrayPos
        Pos pos = Cell2Pos(startCellPos);
        Pos dest = Cell2Pos(destCellPos);

        // 시작점 발견 (예약 진행)
        open[pos.Y, pos.X] = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X));
        pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
        parent[pos.Y, pos.X] = new Pos(pos.Y, pos.X);

        while (pq.Count > 0)
        {
            // 제일 좋은 후보를 찾는다
            PQNode node = pq.Pop();
            // 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
            if (closed[node.Y, node.X])
                continue;

            // 방문한다
            closed[node.Y, node.X] = true;
            // 목적지 도착했으면 바로 종료
            if (node.Y == dest.Y && node.X == dest.X)
                break;

            // 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약(open)한다
            for (int i = 0; i < _deltaY.Length; i++)
            {
                Pos next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

                // 유효 범위를 벗어났으면 스킵
                // 벽으로 막혀서 갈 수 없으면 스킵
                if (!ignoreDestCollision || next.Y != dest.Y || next.X != dest.X)
                {
                    if (CanGo(Pos2Cell(next)) == false) // CellPos
                        continue;
                }

                // 이미 방문한 곳이면 스킵
                if (closed[next.Y, next.X])
                    continue;

                // 비용 계산
                int g = 0;// node.G + _cost[i];
                int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
                // 다른 경로에서 더 빠른 길 이미 찾았으면 스킵
                if (open[next.Y, next.X] < g + h)
                    continue;

                // 예약 진행
                open[dest.Y, dest.X] = g + h;
                pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });
                parent[next.Y, next.X] = new Pos(node.Y, node.X);
            }
        }

        return CalcCellPathFromParent(parent, dest).ConvertAll(pos => (Vector2Int)pos);
    }

    List<Vector2Int> CalcCellPathFromParent(Pos[,] parent, Pos dest)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        int y = dest.Y;
        int x = dest.X;
        while (parent[y, x].Y != y || parent[y, x].X != x)
        {
            cells.Add((Vector2Int)Pos2Cell(new Pos(y, x)));
            Pos pos = parent[y, x];
            y = pos.Y;
            x = pos.X;
        }
        cells.Add((Vector2Int)Pos2Cell(new Pos(y, x)));
        cells.Reverse();

        return cells;
    }

	Pos Cell2Pos(Vector3Int cell)
	{
		// CellPos -> ArrayPos
		return new Pos(MaxY - cell.y, cell.x - MinX);
	}

	Vector3Int Pos2Cell(Pos pos)
	{
		// ArrayPos -> CellPos
		return new Vector3Int(pos.X + MinX, MaxY - pos.Y, 0);
	}

	#endregion
}
