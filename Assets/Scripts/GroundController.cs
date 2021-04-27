using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class CitizenParams {
    public string name;
    public int age, gender, happiness, delay, at;
    public int havecar, budget;
    public HouseParams home, job, shop, leisure;
    public Vector2Int navto;
    public float walkspeed, carspeed;
    public char motion;
}
public class HouseParams {
    public string name;
    public int type, maxcitizens;
    public Vector3 viewcenter;
    public Vector2Int main;
    public Vector3 Entrance;
    public List<Vector2Int> buildingtiles;
    public List<CitizenParams> citizens;
}
public class ServiceCar {
    public string name;
    public int type, state;
    public GameObject body;
    public List<Vector3> NavList;
}
public class Service {
    public string name;
    public int type, carsamount;
    public Vector3 viewcenter;
    public Vector2Int main;
    public Vector3 Entrance;
    public List<Vector2Int> buildingtiles;
}
public struct StartBuilding {
    public int status, type;
    public float maxgridsize;
    public GameObject gridself;
    public HouseParams info;
    public Vector3 buildingscales;
}
public class GroundController : MonoBehaviour {
    public CameraController CamContr;
    public Transform PathVisualiserParent, CitizenVisualiserParent, CameraTransform;
    public int TilesX = 80, TilesY = 80, MaxCitizens;
    public float DetailDistance;
    const int tX = 80, tY = 80;
    public GameObject[,] Tiles = new GameObject[tX, tY];
    public Vector2Int[,] Types = new Vector2Int[tX, tY];
    public CeilSignsData[,] SignsData = new CeilSignsData[tX, tY];
    public HouseParams[,] HouseInfo = new HouseParams[tX, tY];
    public GameObject[] RoadPrefabs, PavPrefabs, TreePrefabs, HousePrefabs, MarkerPrefabs, CarPrefabs, CitizenPrefabs, ServicePrefabs, ServiceCarPrefabs;
    public Material[] CarMaterial;

    public List<Vector2Int> RoadConnectsToTypes;
    public GameObject[,] Roads = new GameObject[6, 1];
    public GameObject[] Roadzer, Roadend, Roadstr, Roadtrn, Roadtcr, Roadxcr;
    public List<Vector2Int> PavConnectsToTypes;
    public GameObject[,] Pavs = new GameObject[6, 2];
    public GameObject[] Pavzer, Pavend, Pavstr, Pavtrn, Pavtcr, Pavxcr;

    public Material TrafficLightMaterial; //materials for signs
    public Material[] SignMaterial; //trafficlight(1)
    public GameObject[] TrafficLightMesh;
    public GameObject[] SignMesh;

    public GameObject TargetA, TargetB, NullObject, BuildGrid, FirePrefab;
    public List<Vector2Int> AllowedTiles;
    public List<Vector4> RoadOffset, PavOffset;
    public List<List<Vector4>> Offsets = new List<List<Vector4>>();
    List<List <Vector3> > Navigations = new List<List<Vector3>>();
    List<int> InterruptDelay = new List<int>();
    public List<GameObject> Citizens = new List<GameObject>();
    public List<CitizenParams> Params = new List<CitizenParams>();
    public LayerMask CitizensLayer;
    public string[] NamesMale, NamesFemale, SurMale, SurFemale;
    public string[] LivingHouseNames, CommercialNames, IndustryNames;
    public AudioSource ASMain, ASSecond;
    public Vector3 ThreePointBeizer(Vector3 Begin, Vector3 Middle, Vector3 End, float parameter) {
        Vector2 pA = new Vector2(Begin.x, Begin.y), pB = new Vector2(Middle.x, Middle.y),pC = new Vector2(End.x, End.y);
        Vector2 ans = Square(1 - parameter) * pA + 2 * (1 - parameter) * parameter * pB + Square(parameter) * pC;
        return new Vector3(ans.x, ans.y, Middle.z);
    }
    public List<Vector3> BeizerPoints(Vector3 Begin, Vector3 Middle, Vector3 End, float amount) {
        List<Vector3> points = new List<Vector3>();
        for (int am = 0; am < amount; ++am) { points.Add(ThreePointBeizer(Begin, Middle, End, am / amount)); }
        return points;
    }
    public float Square(float a) { return a * a; }
    public List<HouseParams> Resident = new List<HouseParams>(), Commercial = new List<HouseParams>(), Industry = new List<HouseParams>(), Leisure = new List<HouseParams>();
    public List<Service> CityServises = new List<Service>();
    public List<Vector2Int> NeedService = new List<Vector2Int>();
    public List<Vector3> FindPath(int CurX, int CurY, int destX, int destY, List<Vector2Int> TileType, int Type) {
        bool[,] nvisited = new bool[tX, tY];
        bool addcheck = false;
        for (int i = 0; i < tX; ++i) { for (int j = 0; j < tY; ++j) { nvisited[i, j] = true; } }
        nvisited[CurX, CurY] = false;
        Queue< List<Vector3> > Perspective = new Queue<List<Vector3>> { };
        Vector3 ToAdd; Vector4 ToShift;

        HouseParams checkfrom = HouseInfo[CurX, CurY];
        if (checkfrom != null) { CurX = Mathf.FloorToInt(checkfrom.Entrance.x); CurY = Mathf.FloorToInt(checkfrom.Entrance.y); addcheck = true; }
        //if (HouseInfo[destX, destY] != null) { destX = Mathf.FloorToInt(HouseInfo[destX, destY].Entrance.x); destY = Mathf.FloorToInt(HouseInfo[destX, destY].Entrance.y); }

        if (CurX + 1 < tX) { if (TileType.Contains(Types[CurX + 1, CurY])) { 
                List<Vector3> Current = new List<Vector3> { };
                ToAdd = new Vector3(CurX + 1, CurY, 1); ToShift = Offsets[Types[CurX + 1, CurY].x - 1][Types[CurX + 1, CurY].y];
                switch (Type) {
                    case 0: ToAdd.x += ToShift.y; ToAdd.y += 1 - ToShift.x; break;
                    case 1: ToAdd.x += ToShift.w; ToAdd.y += 1 - ToShift.z; break;
                }
                Current.Add(ToAdd);
                Perspective.Enqueue(Current); nvisited[CurX + 1, CurY] = false; } 
        }
        if (CurY + 1 < tY) { if (TileType.Contains(Types[CurX, CurY + 1])) { 
                List<Vector3> Current = new List<Vector3> { };
                ToAdd = new Vector3(CurX, CurY + 1, 1); ToShift = Offsets[Types[CurX, CurY + 1].x - 1][Types[CurX, CurY + 1].y];
                switch (Type) {
                    case 0: ToAdd.x += ToShift.x; ToAdd.y += ToShift.y; break;
                    case 1: ToAdd.x += ToShift.z; ToAdd.y += ToShift.w; break;
                }
                Current.Add(ToAdd); 
                Perspective.Enqueue(Current); nvisited[CurX, CurY + 1] = false; } 
        } 
        if (CurX - 1 >= 0) { if (TileType.Contains(Types[CurX - 1, CurY])) { 
                List<Vector3> Current = new List<Vector3> { };
                ToAdd = new Vector3(CurX - 1, CurY, 1); ToShift = Offsets[Types[CurX - 1, CurY].x - 1][Types[CurX - 1, CurY].y];
                switch (Type) {
                    case 0: ToAdd.x += 1 - ToShift.y; ToAdd.y += ToShift.x; break;
                    case 1: ToAdd.x += 1 - ToShift.w; ToAdd.y += ToShift.z; break;
                }
                Current.Add(ToAdd); 
                Perspective.Enqueue(Current); nvisited[CurX - 1, CurY] = false; } }
        if (CurY - 1 >= 0) { if (TileType.Contains(Types[CurX, CurY - 1])) { 
                List<Vector3> Current = new List<Vector3> { };
                ToAdd = new Vector3(CurX, CurY - 1, 1); ToShift = Offsets[Types[CurX, CurY - 1].x - 1][Types[CurX, CurY - 1].y];
                switch (Type) {
                    case 0: ToAdd.x += 1 - ToShift.x; ToAdd.y += 1 - ToShift.y; break;
                    case 1: ToAdd.x += 1 - ToShift.z; ToAdd.y += 1 - ToShift.w; break;
                }
                Current.Add(ToAdd); 
                Perspective.Enqueue(Current); nvisited[CurX, CurY - 1] = false; } 
        }
        while (Perspective.Count > 0) {
            int len = Perspective.Peek().Count - 1;
            int CX = Mathf.FloorToInt(Perspective.Peek()[len].x), CY = Mathf.FloorToInt(Perspective.Peek()[len].y), DS = Mathf.FloorToInt(Perspective.Peek()[len].z + 1);

            if (CX + 1 < tX) {
                if (CX + 1 == destX && CY == destY) { if (addcheck) { Perspective.Peek().Insert(0, checkfrom.Entrance); } if (HouseInfo[CX + 1, CY] != null) { Perspective.Peek().Add(HouseInfo[CX + 1, CY].Entrance + Vector3.forward * DS); Perspective.Peek().Add(new Vector3(HouseInfo[CX + 1, CY].viewcenter.x, HouseInfo[CX + 1, CY].viewcenter.z, DS + 1)); } else { Perspective.Peek().Add(new Vector3(CX + 1, CY, DS + 1)); } return Perspective.Peek(); }
                if (nvisited[CX + 1, CY] && TileType.Contains(Types[CX + 1, CY])) {
                    List<Vector3> Current = new List<Vector3>(Perspective.Peek()); nvisited[CX + 1, CY] = false;
                    ToAdd = new Vector3(CX + 1, CY, DS); ToShift = Offsets[Types[CX + 1, CY].x - 1][Types[CX + 1, CY].y];
                    switch (Type) {
                        case 0: ToAdd.x += ToShift.y; ToAdd.y += 1 - ToShift.x; break;
                        case 1: ToAdd.x += ToShift.w; ToAdd.y += 1 - ToShift.z; break;
                    }
                    Vector3 Old = Current[Current.Count - 1];
                    Vector3 interpolate = new Vector3(Old.x, ToAdd.y, DS);
                    if (Vector3.Distance(Old, interpolate) != 1f) { Current.AddRange(BeizerPoints(Old, interpolate, ToAdd, 6)); ToAdd += Vector3.forward; }

                    Current.Add(ToAdd);
                    Perspective.Enqueue(Current);
                }
            }
            if (CY + 1 < tY) {
                if (CX == destX && CY + 1 == destY) { if (addcheck) { Perspective.Peek().Insert(0, checkfrom.Entrance); } if (HouseInfo[CX, CY + 1] != null) { Perspective.Peek().Add(HouseInfo[CX, CY + 1].Entrance + Vector3.forward * DS); Perspective.Peek().Add(new Vector3(HouseInfo[CX, CY + 1].viewcenter.x, HouseInfo[CX, CY + 1].viewcenter.z, DS + 1)); } else { Perspective.Peek().Add(new Vector3(CX, CY + 1, DS + 1)); } return Perspective.Peek(); }
                if (nvisited[CX, CY + 1] && TileType.Contains(Types[CX, CY + 1])) {
                    List<Vector3> Current = new List<Vector3>(Perspective.Peek()); nvisited[CX, CY + 1] = false;
                    ToAdd = new Vector3(CX, CY + 1, DS); ToShift = Offsets[Types[CX, CY + 1].x - 1][Types[CX, CY + 1].y];
                    switch (Type) {
                        case 0: ToAdd.x += ToShift.x; ToAdd.y += ToShift.y; break;
                        case 1: ToAdd.x += ToShift.z; ToAdd.y += ToShift.w; break;
                    }
                    Vector3 Old = Current[Current.Count - 1];
                    Vector3 interpolate = new Vector3(ToAdd.x, Old.y, DS);
                    if (Vector3.Distance(Old, interpolate) != 1f) { Current.AddRange(BeizerPoints(Old, interpolate, ToAdd, 6)); ToAdd += Vector3.forward; }

                    Current.Add(ToAdd);
                    Perspective.Enqueue(Current);
                }
            }
            if (CX - 1 >= 0) {
                if (CX - 1 == destX && CY == destY) { if (addcheck) { Perspective.Peek().Insert(0, checkfrom.Entrance); } if (HouseInfo[CX - 1, CY] != null) { Perspective.Peek().Add(HouseInfo[CX - 1, CY].Entrance + Vector3.forward * DS); Perspective.Peek().Add(new Vector3(HouseInfo[CX - 1, CY].viewcenter.x, HouseInfo[CX - 1, CY].viewcenter.z, DS + 1)); } else { Perspective.Peek().Add(new Vector3(CX - 1, CY, DS + 1)); } return Perspective.Peek(); }
                if (nvisited[CX - 1, CY] && TileType.Contains(Types[CX - 1, CY])) {
                    List<Vector3> Current = new List<Vector3>(Perspective.Peek()); nvisited[CX - 1, CY] = false;
                    ToAdd = new Vector3(CX - 1, CY, DS); ToShift = Offsets[Types[CX - 1, CY].x - 1][Types[CX - 1, CY].y];
                    switch (Type) {
                        case 0: ToAdd.x += 1 - ToShift.y; ToAdd.y += ToShift.x; break;
                        case 1: ToAdd.x += 1 - ToShift.w; ToAdd.y += ToShift.z; break;
                    }
                    Vector3 Old = Current[Current.Count - 1];
                    Vector3 interpolate = new Vector3(Old.x, ToAdd.y, DS);
                    if (Vector3.Distance(Old, interpolate) != 1f) { Current.AddRange(BeizerPoints(Old, interpolate, ToAdd, 6)); ToAdd += Vector3.forward; }

                    Current.Add(ToAdd);
                    Perspective.Enqueue(Current);
                }
            }
            if (CY - 1 >= 0) {
                if (CX == destX && CY - 1 == destY) { if (addcheck) { Perspective.Peek().Insert(0, checkfrom.Entrance); } if (HouseInfo[CX, CY - 1] != null) { Perspective.Peek().Add(HouseInfo[CX, CY - 1].Entrance + Vector3.forward * DS); Perspective.Peek().Add(new Vector3(HouseInfo[CX, CY - 1].viewcenter.x, HouseInfo[CX, CY - 1].viewcenter.z, DS)); } else { Perspective.Peek().Add(new Vector3(CX, CY - 1, DS + 1)); } return Perspective.Peek(); }
                if (nvisited[CX, CY - 1] && TileType.Contains(Types[CX, CY - 1])) {
                    List<Vector3> Current = new List<Vector3>(Perspective.Peek()); nvisited[CX, CY - 1] = false;
                    ToAdd = new Vector3(CX, CY - 1, DS); ToShift = Offsets[Types[CX, CY - 1].x - 1][Types[CX, CY - 1].y];
                    switch (Type) {
                        case 0: ToAdd.x += 1 - ToShift.x; ToAdd.y += 1 - ToShift.y; break;
                        case 1: ToAdd.x += 1 - ToShift.z; ToAdd.y += 1 - ToShift.w; break;
                    }
                    Vector3 Old = Current[Current.Count - 1];
                    Vector3 interpolate = new Vector3(ToAdd.x, Old.y, DS);
                    if (Vector3.Distance(Old, interpolate) != 1f) { Current.AddRange(BeizerPoints(Old, interpolate, ToAdd, 6)); ToAdd += Vector3.forward; }

                    Current.Add(ToAdd);
                    Perspective.Enqueue(Current);
                }
            }
            Perspective.Dequeue();
        }
        return null;
    }
    void Start() {
        int i;
        for (i = 0; i < tX; ++i) {
            for (int j = 0; j < tY; ++j) {
                Tiles[i, j] = null;
                HouseInfo[i, j] = null;
                Types[i, j] = Vector2Int.zero;
                SignsData[i, j] = new CeilSignsData();
            }
        }
        for (i = 0; i < Roadzer.Length; ++i) { Roads[0, i] = Roadzer[i]; Roads[1, i] = Roadend[i]; Roads[2, i] = Roadstr[i]; Roads[3, i] = Roadtrn[i]; Roads[4, i] = Roadtcr[i]; Roads[5, i] = Roadxcr[i]; }
        for (i = 0; i < Pavzer.Length; ++i) { Pavs[0, i] = Pavzer[i]; Pavs[1, i] = Pavend[i]; Pavs[2, i] = Pavstr[i]; Pavs[3, i] = Pavtrn[i]; Pavs[4, i] = Pavtcr[i]; Pavs[5, i] = Pavxcr[i]; }
        Offsets.Add(RoadOffset); Offsets.Add(PavOffset);
        TargetA = null;
        TargetB = null;
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.R) && TargetA != null && TargetB != null) {
            foreach (Transform child in PathVisualiserParent) { Destroy(child.gameObject); }
            List<Vector3> My = FindPath(Mathf.FloorToInt(TargetA.transform.position.x), Mathf.FloorToInt(TargetA.transform.position.z), Mathf.FloorToInt(TargetB.transform.position.x), Mathf.FloorToInt(TargetB.transform.position.z), AllowedTiles, 1);
            if (My == null) { /*Debug.Log("Path dont Exist!");*/ }
            else { HighlightPath(My); }
        }
    }
    void HighlightPath(List<Vector3> Array) {
        foreach (Transform child in PathVisualiserParent) { Destroy(child.gameObject); }
        GameObject CJ = Instantiate(MarkerPrefabs[0]);
        CJ.transform.position = new Vector3(Mathf.FloorToInt(TargetA.transform.position.x) + 0.5f, 0, Mathf.FloorToInt(TargetA.transform.position.z) + 0.5f);
        CJ.transform.parent = PathVisualiserParent;
        CJ.GetComponent<TextMesh>().text = "0";
        for (int i = 0; i < Array.Count; ++i) {
            CJ = Instantiate(MarkerPrefabs[0]);
            CJ.transform.parent = PathVisualiserParent;
            CJ.transform.position = new Vector3(Array[i].x, 0, Array[i].y);
            CJ.GetComponent<TextMesh>().text = Array[i].z.ToString();
        }
    }
    public void UpdateSitizenCount() {
        CamContr.CitizensAmount.text = CitizenVisualiserParent.childCount.ToString();
        CamContr.UpdateSitizenHappiness();

        if (CamContr.Speed > 0.8 || MaxCitizens <= 3) {

            if (MaxCitizens > 3) { if (ASMain.volume > 0.2f) { ASMain.volume -= 0.005f; } }
            else if (ASMain.volume > 0.02f) { ASMain.volume -= 0.005f; }
            else { if (ASMain.isPlaying) { ASMain.Stop(); } }

            if (ASSecond.volume < GameData.SoundLevel) { ASSecond.volume += 0.005f; }
        }
        else if (MaxCitizens > 3) { 
            if (!ASMain.isPlaying) { ASMain.Play(); }
            if (ASMain.volume < GameData.SoundLevel) { ASMain.volume += 0.005f; }
            if (ASSecond.volume > 0.02f) { ASSecond.volume -= 0.005f; }
        }
    }
    public int CountSurroundings(int Xpos, int Ypos, List<Vector2Int> type) {
        int Surs = 0;
        if (Ypos + 1 < TilesY) { if (type.Contains(Types[Xpos, Ypos + 1])) { Surs += 8; } }
        if (Xpos + 1 < TilesX) { if (type.Contains(Types[Xpos + 1, Ypos])) { Surs += 4; } }
        if (Ypos - 1 >= 0) { if (type.Contains(Types[Xpos, Ypos - 1])) { Surs += 2; } }
        if (Xpos - 1 >= 0) { if (type.Contains(Types[Xpos - 1, Ypos])) { Surs += 1; } }
        return Surs;
    }
    public void RemoveCitizen(int i) {
        if (Industry.Contains(Params[i].job)) { HouseInfo[Params[i].job.main.x, Params[i].job.main.y].citizens.Remove(Params[i]); }
        if (Resident.Contains(Params[i].home)) { HouseInfo[Params[i].home.main.x, Params[i].home.main.y].citizens.Remove(Params[i]); }
        Destroy(Citizens[i]); Citizens.RemoveAt(i); Navigations.RemoveAt(i); Params.RemoveAt(i);
    }
    public int RecalcPathForCitizen(int i) {
        if (Navigations[i].Count > 2) {
            if (Params[i].motion == '2') { Navigations[i] = FindPath(Mathf.FloorToInt(Citizens[i].transform.position.x), Mathf.FloorToInt(Citizens[i].transform.position.z), Mathf.FloorToInt(Navigations[i][Navigations[i].Count - 1].x), Mathf.FloorToInt(Navigations[i][Navigations[i].Count - 1].y), RoadConnectsToTypes, 0); }
            else { Navigations[i] = FindPath(Mathf.FloorToInt(Citizens[i].transform.position.x), Mathf.FloorToInt(Citizens[i].transform.position.z), Mathf.FloorToInt(Navigations[i][Navigations[i].Count - 1].x), Mathf.FloorToInt(Navigations[i][Navigations[i].Count - 1].y), PavConnectsToTypes, 1); }
            if (Navigations[i] == null) { RemoveCitizen(i); --i; }
        }
        return i;
    }
    public CitizenParams CreateCitizen() {
        CitizenParams a = new CitizenParams();
        HouseParams house;
        a.age = Random.Range(0, 100);
        a.gender = Random.Range(0, CitizenPrefabs.Length);
        a.delay = Random.Range(100, 600);
        switch (a.gender) {
            case 0: a.name = SurMale[Random.Range(0, SurMale.Length)] + ' ' + NamesMale[Random.Range(0, NamesMale.Length)]; break;
            case 1: a.name = SurFemale[Random.Range(0, SurFemale.Length)] + ' ' + NamesFemale[Random.Range(0, NamesFemale.Length)]; break;
            default: a.name = "My citizen"; break;
        }
        a.havecar = Random.Range(-1, CarPrefabs.Length);
        a.home = a.shop = a.job = null;
        a.budget = Random.Range(20, 35);
        for (int i = 0; i < Resident.Count; ++i) { house = Resident[i]; if (house.citizens.Count < house.maxcitizens) { a.home = Resident[i]; house.citizens.Add(a); break; } }
        a.happiness = Random.Range(0, 100);
        a.walkspeed = Random.Range(0.4f, 0.6f);
        a.carspeed = Random.Range(0.9f, 1.1f);
        a.motion = '0';
        a.at = '0';
        return a;
    }
    private void FixedUpdate() {
        if(Params.Count < MaxCitizens) {
            if(Resident.Count > 0 && CitizenVisualiserParent.childCount < MaxCitizens) {
                CitizenParams NewAddCitizen = CreateCitizen(); GameObject NewCitizen = Instantiate(CitizenPrefabs[NewAddCitizen.gender], CitizenVisualiserParent);
                if (NewAddCitizen.home != null) {
                    NewCitizen.transform.position = new Vector3(NewAddCitizen.home.main.x + 0.5f, 0, NewAddCitizen.home.main.y + 0.5f);
                    NewCitizen.GetComponent<NavMeshAgent>().speed = NewAddCitizen.walkspeed;
                    Navigations.Add(new List<Vector3>()); InterruptDelay.Add(0); Params.Add(NewAddCitizen);
                    if (NewAddCitizen.havecar >= 0) { GameObject car = Instantiate(CarPrefabs[NewAddCitizen.havecar], NewCitizen.transform); car.transform.GetChild(0).GetComponent<MeshRenderer>().material = CarMaterial[Random.Range(0, CarMaterial.Length)]; car.transform.GetChild(1).GetComponent<MeshRenderer>().material = car.transform.GetChild(0).GetComponent<MeshRenderer>().material; car.SetActive(false); }
                    Citizens.Add(NewCitizen);
                    UpdateSitizenCount();
                } else { Destroy(NewCitizen); }
            }
        }
        for (int i = 0; i < Citizens.Count; ++i) {
            if (!Resident.Contains(Params[i].home)) { RemoveCitizen(i); --i; continue; }
            bool calcmove = true;
            if (Navigations[i].Count > 0) {
                RaycastHit Checkcollision;
                Vector3 Ppos = Citizens[i].transform.position;
                Vector3 Tpos = new Vector3(Navigations[i][0].x, 0, Navigations[i][0].y);
                if (Physics.Raycast(Ppos, Tpos - Ppos, out Checkcollision, 1f, CitizensLayer.value)) {
                    if ((Checkcollision.transform.gameObject.tag == "Car" || Params[i].motion == '2') && Checkcollision.distance < 0.5f) { ++InterruptDelay[i]; if (InterruptDelay[i] > 0 && InterruptDelay[i] < 500) { calcmove = false; } else if (InterruptDelay[i] > 500) { InterruptDelay[i] = -1000; } } }
                Debug.DrawRay(Ppos, Tpos - Ppos, Color.yellow);
            }
            if (calcmove) {
                CitizenParams cur = Params[i];
                HouseParams temphouse;
                Vector3 Ppos = Citizens[i].transform.position;
                if (Navigations[i].Count > 0) {
                    Vector3 Tpos = new Vector3(Navigations[i][0].x, 0, Navigations[i][0].y);
                    Citizens[i].GetComponent<NavMeshAgent>().destination = Tpos;
                    if (Tiles[Mathf.FloorToInt(Navigations[i][0].x), Mathf.FloorToInt(Navigations[i][0].y)] == null && Navigations[i][0].z > 1) { i = RecalcPathForCitizen(i); }
                    else {
                        if (Vector3.Distance(CameraTransform.position, Ppos) < DetailDistance) { if (cur.motion == '1') { Animation anim = Citizens[i].GetComponentInChildren<Animation>(); if (!anim.isPlaying) { anim.Play(); } } }
                        if (Vector3.Distance(Ppos, Tpos) < 0.1f) { Navigations[i].RemoveAt(0); }
                        if (InterruptDelay[i] > 0) { --InterruptDelay[i]; }
                    }
                } else {
                    if (cur.delay > 0) { Citizens[i].SetActive(false); cur.motion = '0'; --cur.delay;
                        if (cur.delay == 0) { Citizens[i].SetActive(true); } InterruptDelay[i] = 0; }
                    else {
                        bool skip = false;
                        switch (cur.at) {
                            case '3': //When leisure ends:
                                if (cur.home != null) { cur.navto = cur.home.main; cur.at = '0'; }
                                else { RemoveCitizen(i); --i; continue; }
                                break;

                            case '2': //When shopping ends:
                                cur.at = '3';
                                if (cur.shop != null) { cur.budget -= Random.Range(1, 7); }
                                if (cur.happiness < 63) { skip = true; break; }
                                if (cur.leisure != null) {
                                    if (Random.Range(0, 2) == 0) {
                                        cur.navto = cur.leisure.main;
                                    }
                                    else { cur.navto = Leisure[Random.Range(0, Leisure.Count)].main; }
                                }
                                else {
                                    if (Leisure.Count > 0) {
                                        cur.navto = Leisure[Random.Range(0, Leisure.Count)].main; cur.leisure = Leisure[Random.Range(0, Leisure.Count)];
                                        Leisure[Random.Range(0, Leisure.Count)].citizens.Add(cur); cur.navto = cur.leisure.main; break;
                                    }
                                    skip = true;
                                }
                                break;

                            case '1': //When job ends:
                                cur.at = '2';
                                if (cur.job != null) { cur.budget += Random.Range(3, 12); }
                                if (cur.shop != null) { 
                                    if (Random.Range(0, 2) == 0) { 
                                        cur.navto = cur.shop.main;
                                    } 
                                    else { cur.navto = Commercial[Random.Range(0, Commercial.Count)].main; }
                                }
                                else {
                                    if (Commercial.Count > 0) {
                                        cur.navto = Commercial[Random.Range(0, Commercial.Count)].main; cur.shop = Commercial[Random.Range(0, Commercial.Count)];
                                        Commercial[Random.Range(0, Commercial.Count)].citizens.Add(cur); cur.navto = cur.shop.main; break;
                                    }
                                    skip = true;
                                }
                                break;
                            
                            case '0': //When home ends:
                                cur.at = '1';
                                if (cur.job != null) { 
                                    cur.navto = cur.job.main; cur.budget -= Random.Range(1, 6);
                                    if (cur.havecar >= 0) { cur.budget -= Random.Range(1, 4); }
                                } 
                                else { 
                                    if (Industry.Count > 0) { 
                                        int t = Random.Range(0, Industry.Count); temphouse = HouseInfo[Industry[t].main.x, Industry[t].main.y]; 
                                        if (temphouse.citizens.Count < temphouse.maxcitizens) { 
                                            cur.job = temphouse; temphouse.citizens.Add(cur); cur.navto = cur.job.main; break;
                                        }
                                    }
                                    skip = true;
                                } break;
                        }
                        cur.delay = Random.Range(400, 700);
                        if (skip) { continue; }
                        List<Vector3> pavpath = FindPath(Mathf.FloorToInt(Ppos.x), Mathf.FloorToInt(Ppos.z), cur.navto.x, cur.navto.y, PavConnectsToTypes, 1);
                        
                        if (pavpath == null) { RemoveCitizen(i); --i; continue; }
                        else {
                            List<Vector3> carpath = FindPath(Mathf.FloorToInt(Ppos.x), Mathf.FloorToInt(Ppos.z), cur.navto.x, cur.navto.y, RoadConnectsToTypes, 0);
                            Vector3 OldPos = Citizens[i].transform.position;
                            if (cur.at != '3' && cur.havecar >= 0 && pavpath[pavpath.Count - 1].z > 17 && carpath != null) { Navigations[i] = carpath; if (cur.motion != '2') { Citizens[i].transform.GetChild(1).gameObject.SetActive(true); Citizens[i].transform.GetChild(0).gameObject.SetActive(false); Citizens[i].GetComponent<NavMeshAgent>().speed = cur.carspeed; cur.motion = '2'; } }
                            else { Navigations[i] = pavpath; if (cur.motion != '1') { if (Citizens[i].transform.childCount == 2) { Citizens[i].transform.GetChild(1).gameObject.SetActive(false); Citizens[i].transform.GetChild(0).gameObject.SetActive(true); Citizens[i].GetComponent<NavMeshAgent>().speed = cur.walkspeed; } cur.motion = '1'; } }
                            Citizens[i].transform.position = OldPos;
                        }
                    }
                }
            }
        }
        UpdateSitizenCount();
    }
}

public class CeilSignsData{
    public int timeLight = 0;
    public Phase phase;

    public int type = -1;

    public int value;

    public enum Phase {
        Red,
        Green,
        Yellow
    }
    public enum Type {
            NONETYPE = -1,
            STOP = 0,
            SPEED_LIMIT = 1,
            TRAFFICLIGHT = 2
    }
}