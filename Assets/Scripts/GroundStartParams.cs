using System.Collections.Generic;
using UnityEngine;

public class GroundStartParams : MonoBehaviour {
    public Dictionary<int, Vector2Int> vertexNormal = new Dictionary<int, Vector2Int>();

    public GroundController Ground;
    public CameraController CamCtr;
    public Transform Test;
    const int Xsize = 80, Ysize = 80;
    
    public int squares;
    public string seed;
    public string subseed;

    public Vector2Int[] ClassicBuildings;
    public Vector2Int[] Parks;
    public Vector2Int ParkTreeType;

    public bool BuildRoad;
    public List<SquareType> AllSquares = new List<SquareType>();

    public bool BuildBuildings = false;
    public bool GrowTrees = false;

    public int TreePercentage;
    public int[,] Types = new int[Xsize, Ysize];
    public struct SquareType {
        public int lenth, width;
        public bool res, com, ind, hor, ver;
        public int typehor, typever;
    }
    public int DefineType(int index) {
        int result = 0;
        if (AllSquares[index].res) { result |= 1; }
        if (AllSquares[index].com) { result |= 2; }
        if (AllSquares[index].ind) { result |= 4; }

        return result;
    }
    public void DrawRectangle(Vector2Int start, Vector2Int end, Vector2Int buildtype, bool fill, int indexarray) {
        Vector2Int begin = Vector2Int.zero, offset = Vector2Int.zero;
        
        if (end.x > start.x) { begin.x = start.x; offset.x = end.x - start.x; }
        else if (end.x < start.x) { begin.x = end.x; offset.x = start.x - end.x; }
        else { begin.x = start.x; offset.x = 0; }

        if (end.y > start.y) { begin.y = start.y; offset.y = end.y - start.y; }
        else if (end.y < start.y) { begin.y = end.y; offset.y = start.y - end.y; }
        else { begin.y = start.y; offset.y = 0; }

        for (int x = begin.x; x <= begin.x + offset.x; ++x) { CamCtr.TryBuildWay(x, begin.y, buildtype); CamCtr.TryBuildWay(x, begin.y + offset.y, buildtype); }
        for (int y = begin.y; y <= begin.y + offset.y; ++y) { CamCtr.TryBuildWay(begin.x, y, buildtype); CamCtr.TryBuildWay(begin.x + offset.x, y, buildtype); }

        if (fill) { for (int x = begin.x + 1; x < begin.x + offset.x; ++x) { for (int y = begin.y + 1; y < begin.y + offset.y; ++y) { CamCtr.ClearCell(x, y); if (indexarray >= 0) { Types[x, y] = DefineType(indexarray); } } } }
    }
    public int PersudoRandom(int start, int end, int key) {
        int iterable = Mathf.Abs((seed.Length - end) & AllSquares.Count);
        if(iterable >= AllSquares.Count) { iterable >>= 1; }
        SquareType cur = AllSquares[iterable];
        iterable += ((cur.lenth + seed[iterable + 1]) * (cur.width + seed[(iterable >> 1) + end]) * (seed[iterable + 3] + seed[iterable + 4]));
        iterable += key * end - start;
        return iterable % (end + 1 - start) + start;
    }
    public int DefineBuildingCost(float centeraver, float centerdist, int type) {
        if (centerdist < centeraver / 2.2f) {
            if ((type & 0b1) > 0) { return 3; }
            else if ((type & 0b10) > 0) { return 6; }
            else { return 5; }
        }
        else if (centerdist > centeraver / 1.3f) {
            if ((type & 0b1) > 0) { return 1; }
            else if ((type & 0b10) > 0) { return 4; }
            else { return 2; }
        }
        else {
            if ((type & 0b1) > 0) { return 1; }
            else if ((type & 0b10) > 0) { return 0; }
            else { return 5; }
        }
    }
    private void Start() {
        seed = GameData.seed;
        if (GameData.gamevariant == 0) { BuildRoad = true; GrowTrees = true; while (squares > seed.Length) { seed += seed + subseed.ToUpper() + seed.Length.ToString(); seed += subseed; seed += seed.Length.ToString(); } } else { BuildRoad = false; GrowTrees = false; }
        if (GameData.gamevariant != 1) { GrowTrees = true; } else { GrowTrees = false; }

        for (int x = 0; x < Xsize; ++x) { for (int y = 0; y < Ysize; ++y) { Types[x, y] = -1; } }
    }
    private void LateUpdate() {

        GameObject current;
        
        if (BuildRoad) {
            seed += subseed;
            Debug.Log(seed);
            int previousA = 7, previousB = 8;
            for (int n = 0; n < seed.Length - 4; n += 4) {
                SquareType news = new SquareType();
                news.lenth = (seed[n + 2] | seed[n + 1] | 0b11) & 0xF;
                news.width = (seed[n] | seed[n + 2] | 0b11) & 0xF;

                news.res = ((seed[n + 1] * seed[n + 2] + seed[n]) % 16) > 12;
                news.com = ((seed[n] * seed[n + 3] + seed[n + 1]) % 14) > 8;
                news.ind = ((seed[n + 4] * seed[n] + seed[n + 3]) % 13) > 7;

                news.hor = ((seed[n + 2] & 0b1) == 1);
                news.ver = ((seed[n + 3] & 0b1) == 1);

                news.typehor = (seed[n + 1] >> 4);
                news.typever = (seed[n + 4] >> 4);
                AllSquares.Add(news);
            }
            float side = Mathf.Sqrt(Xsize / 1.3f); int turnside = 0;
            Vector2 MathCenter = Vector2.zero;
            for (int n = 0; n < AllSquares.Count; ++n) {
                if (turnside > side) { previousA = 10; previousB += 4; turnside = 0; }
                SquareType cur = AllSquares[n];
                Vector2Int offset = new Vector2Int(previousA, previousB + (((cur.lenth + seed[n + 1]) * (cur.width + seed[n + 2]) * (seed[n + 3] + seed[n + 4])) & 0b11));
                Vector2Int size = new Vector2Int(cur.lenth >> 1, cur.width >> 1);
                Vector2Int fillh, fillv;

                if (cur.typehor < 7) { fillh = new Vector2Int(2, 1); } else { fillh = Vector2Int.right; }
                if (cur.typever < 7) { fillv = new Vector2Int(2, 1); } else { fillv = Vector2Int.right; }

                //Debug.Log(typehor.ToString() + " " + typever.ToString());
                //Debug.Log(repeat);

                DrawRectangle(offset, offset + size, Vector2Int.right, true, n);
                
                if (cur.ver && cur.lenth > 4) { if (cur.typever > 4) { DrawRectangle(offset + new Vector2Int(Mathf.FloorToInt(cur.lenth / 2), 0), offset + new Vector2Int(Mathf.FloorToInt(cur.lenth / 2), cur.width), fillv, false, -1); } }
                if (cur.hor && cur.width > 4) { if (cur.typehor > 4) { DrawRectangle(offset + new Vector2Int(0, Mathf.FloorToInt(cur.width / 2)), offset + new Vector2Int(cur.lenth, Mathf.FloorToInt(cur.width / 2)), fillh, false, -1); } }

                MathCenter = (MathCenter + offset + new Vector2(cur.lenth / 2, cur.width / 2)) / 2.2f;
                previousA += - 1 + cur.lenth >> 1; ++turnside;
            }
            //Debug.Log("Math Center: " + MathCenter.y.ToString() + ", " + MathCenter.x.ToString());

            if (BuildBuildings) {
                for (int x = 0; x < Xsize; ++x) { for (int y = 0; y < Ysize; ++y) { if (Types[x, y] >= 0 && Ground.Tiles[x, y] == null) { 
                            if (Types[x, y] == 0) {
                                if (PersudoRandom(0, 5, x * y + 1) == 0) { CamCtr.TryBuildBuilding(x, y, Parks[UnityEngine.Random.Range(0, Parks.Length)]); }
                                //else { CamCtr.TryBuildTree(x, y, ParkTreeType); } 
                            } 
                            else {
                                if (PersudoRandom(0, 3, x * y + 1) > 1) { CamCtr.TryBuildBuilding(x, y, ClassicBuildings[DefineBuildingCost((MathCenter.x + MathCenter.y) / 2, Vector2.Distance(new Vector2(y, x), MathCenter), Types[x, y])] ); }
                                else if (PersudoRandom(0, 3, x / y + x * y + 1) < 2) { CamCtr.TryBuildTree(x, y, ParkTreeType); }
                            }
                        } 
                    } 
                }
                for (int x = 0; x < Xsize; ++x) { for (int y = 0; y < Ysize; ++y) { if (Types[x, y] >= 0 && Ground.Tiles[x, y] == null) {
                            if (Types[x, y] == 0) { CamCtr.TryBuildTree(x, y, ParkTreeType); }
                            else if (PersudoRandom(0, 3, x * y + 1) == 1) { CamCtr.TryBuildTree(x, y, ParkTreeType); }
                        }
                    }
                }
            }
        }
        
        if (GrowTrees) {
            for (int i = 0; i < Ground.TilesX; ++i) {
                for (int j = 0; j < Ground.TilesY; ++j) {
                    if (Ground.Tiles[i, j] == null && UnityEngine.Random.Range(0, 101) < (TreePercentage + UnityEngine.Random.Range(-50, 50))) {
                        Vector2Int RandTree = new Vector2Int(3, Ground.TreePrefabs.Length - 1);
                        current = Instantiate(Ground.TreePrefabs[UnityEngine.Random.Range(0, RandTree.y + 1)], Ground.transform);
                        current.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                        current.transform.position = new Vector3(i + 0.5f, 0, j + 0.5f);
                        current.name = i.ToString() + ' ' + j.ToString();
                        Ground.Tiles[i, j] = current; Ground.Types[i, j] = RandTree;
                    }
                }
            }
        }
        
        if (GameData.gamevariant == 1) {





            Debug.Log(GameData.JsonInfo.id);
            Debug.Log(GameData.JsonInfo.type);

            foreach (MapJSON.Map.Road.Vertex vertex in GameData.JsonInfo.map.road.vertexes) {
                vertexNormal.Add(vertex.id, new Vector2Int(vertex.position[0], vertex.position[1]));
            }




            
            foreach (int id in GameData.JsonInfo.map.trafficLights)
            {
                (int x, int y) = (vertexNormal[id].x,vertexNormal[id].y); //получение координат светофора по координатам соответствующей вершины дороги
           
                Ground.SignsData[x, y].type = 2;
            }

            foreach(MapJSON.Map.Sign sign in GameData.JsonInfo.map.signs) {
                (int x, int y) = (sign.position[0], sign.position[1]);


                if (sign.type == "SPEED_LIMIT")
                {
                    Ground.SignsData[x, y].type = 1;
                    Ground.SignsData[x, y].value = sign.value;
                }
                if (sign.type == "STOP")
                {
                    Ground.SignsData[x, y].type = 0;
                }
            }


            foreach (MapJSON.Map.Road.Edge edge in GameData.JsonInfo.map.road.edges)
            {

                Vector2Int from = vertexNormal[edge.from];
                Vector2Int to = vertexNormal[edge.to];

                DrawRectangle(from, to, new Vector2Int(1, 0), false, -1);
            }



            foreach (MapJSON.Map.Building building in GameData.JsonInfo.map.buildings) {
                Vector2Int BT = new Vector2Int(4, 0); ;
                if (building.type == "FACTORY") { BT = new Vector2Int(4, 0); }

                (int x0, int y0) = (building.size[0], building.size[1]);
                for (int x = 0; x < x0; x++) {
                    for (int y = 0; y < y0; y++) {
                        CamCtr.TryBuildBuilding(building.position[0] + x, building.position[1] + y, BT);
                    }
                }
            }


        }
        Destroy(gameObject);
    }
}