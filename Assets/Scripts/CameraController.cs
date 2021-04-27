using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class CameraController : MonoBehaviour {
    public GameObject CameraSelf, Selected, PauseMenu;
    public GameObject[] Menus;
    public Text[] InfoLabel;
    public InputField NameField;
    public Image SmileLabel, HappinessButton, NavigateButton, Unemployment, Wealth;
    public Sprite[] Smiles, NavSprite;
    public GroundController Ground;
    public float Sensivity = 0.0001f, Speed, TouchRotationAng = 0, TouchScaleAng = 0;
    public Text CitizensAmount;
    public Vector2Int BuildType;
    public List<StartBuilding> InBuilding = new List<StartBuilding>();

    float AngleHorizontal;// AngleVertical;
    //Vector3 Cpos = new Vector3(0, 4, -6);
    Vector3 ShowVector;
    Quaternion[] rot = new Quaternion[4];
    Vector3[] cords = new Vector3[4];
    bool CameraFollow = false, PosFollow = false, PauseButton = false;
    void Start() {
        rot[0] = Quaternion.Euler(0, 0, 0); rot[1] = Quaternion.Euler(0, 90, 0); rot[2] = Quaternion.Euler(0, 180, 0); rot[3] = Quaternion.Euler(0, 270, 0);
    }
    void Update() {
        //bool touchactivity = ProceedTouch();
        if (Input.GetMouseButton(1)) {
            AngleHorizontal += Input.GetAxis("Mouse X") * Sensivity;
            transform.rotation = Quaternion.AngleAxis(AngleHorizontal, Vector3.up);
        }
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 20f)) {
                if (hit.transform != null) {
                    int aX = Mathf.FloorToInt(hit.point.x), aY = Mathf.FloorToInt(hit.point.z);
                    if (aX < Ground.TilesX && aY < Ground.TilesY) {
                        GameObject current;
                        Vector2Int tsSurInfo = Vector2Int.zero;
                        if (BuildType.x == -1) { ClearCell(aX, aY); } 
                        else if (BuildType.x > 0) {
                            if (Ground.Tiles[aX, aY] == null || BuildType.x == 5 || BuildType.x == 6) {
                                switch (BuildType.x) {
                                    case 1: TryBuildWay(aX, aY, BuildType); return;
                                    case 2: TryBuildWay(aX, aY, BuildType); return;
                                    case 3: TryBuildTree(aX, aY, BuildType); return;
                                    case 4: TryBuildBuilding(aX, aY, BuildType); return;  
                                    case 5:
                                        switch (BuildType.y) {
                                            case 1: if (Ground.TargetA == null) { Ground.TargetA = Instantiate(Ground.MarkerPrefabs[1], Ground.transform); } Ground.TargetA.transform.position = new Vector3(aX + 0.5f, 0, aY + 0.5f); break;
                                            case 2: if (Ground.TargetB == null) { Ground.TargetB = Instantiate(Ground.MarkerPrefabs[2], Ground.transform); } Ground.TargetB.transform.position = new Vector3(aX + 0.5f, 0, aY + 0.5f); break;
                                        }
                                        if (Ground.TargetA != null && Ground.TargetB != null) { if (Ground.TargetA.transform.position == Ground.TargetB.transform.position) { Destroy(Ground.TargetA); Ground.TargetA = null; Destroy(Ground.TargetB); Ground.TargetB = null; foreach (Transform child in Ground.PathVisualiserParent) { Destroy(child.gameObject); } } }
                                        return;
                                    case 6: TryBuildMarker(aX, aY, BuildType); return;
                                    default: current = Instantiate(Ground.TreePrefabs[0], Ground.transform); break;
                                }
                            }
                        }
                        else if (BuildType.x == 0) {
                            string seltag = hit.collider.gameObject.tag;
                            if (seltag == "Pedestrian" || seltag == "Car") {
                                if (seltag == "Car") { Selected = hit.collider.transform.parent.gameObject; }
                                else { Selected = hit.collider.gameObject; }
                                UpdateCitizenInfo(Ground.Params[Ground.Citizens.FindIndex(d => d == Selected)]);
                                Ground.UpdateSitizenCount();
                                ToggleMenus(5);
                            }
                            else { Menus[5].SetActive(false); Selected = null; }
                        }
                    }
                }
            }
        }
        float mousedelta = Input.mouseScrollDelta.y;
        if (mousedelta != 0 && Vector3.Distance(transform.position, CameraSelf.transform.position) > mousedelta + 1) {
            Speed = Vector3.Distance(transform.position, CameraSelf.transform.position) / 35;
            float angl = Mathf.Atan2(CameraSelf.transform.localPosition.y, CameraSelf.transform.localPosition.z);
            CameraSelf.transform.localPosition -= new Vector3(0, mousedelta * Mathf.Sin(angl), mousedelta * Mathf.Cos(angl));
            //CameraSelf.transform.localRotation = new Quaternion(0, angl * Mathf.Rad2Deg, 0, 1);
        }
    }
    void FixedUpdate() {
        if (!EventSystem.current.IsPointerOverGameObject()) { //PC Controls
            if (Input.GetKey(KeyCode.W)) { transform.position += new Vector3(Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.y), 0, Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.y)) * Speed; if (CameraFollow || PosFollow) { CameraFollow = false; PosFollow = false; } }
            if (Input.GetKey(KeyCode.A)) { transform.position -= new Vector3(Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.y), 0, -Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.y)) * Speed; if (CameraFollow || PosFollow) { CameraFollow = false; PosFollow = false; } }
            if (Input.GetKey(KeyCode.S)) { transform.position -= new Vector3(Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.y), 0, Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.y)) * Speed; if (CameraFollow || PosFollow) { CameraFollow = false; PosFollow = false; } }
            if (Input.GetKey(KeyCode.D)) { transform.position += new Vector3(Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.y), 0, -Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.y)) * Speed; if (CameraFollow || PosFollow) { CameraFollow = false; PosFollow = false; } }
        }

        if (CameraFollow) { if (Selected != null) { transform.position = Vector3.MoveTowards(transform.position, Selected.transform.position, 2); } else { CameraFollow = false; } }
        else if (PosFollow) { transform.position = Vector3.MoveTowards(transform.position, ShowVector, 2); }

        if (Selected != null) { Ground.UpdateSitizenCount(); UpdateCitizenInfo(Ground.Params[Ground.Citizens.FindIndex(d => d == Selected)]); }
        if (InBuilding.Count > 0) { BuildBuildings(); }
        if (Input.GetKeyUp(KeyCode.Escape)) { PauseButtonToggle(); }
    }
    public Vector2 RotateVector(Vector2 a) { return Quaternion.Euler(0, 0, -transform.rotation.eulerAngles.y) * a; }
    /*
    public bool ProceedTouch() {
        int cnt = Input.touches.Length;

        if (cnt == 1) {
            Touch cur = Input.GetTouch(0);
            Vector2 dlp = RotateVector(cur.deltaPosition / 1000);
            if (EventSystem.current.IsPointerOverGameObject(cur.fingerId) || cur.position.y < 200) { return false; }

            if ((cur.phase == TouchPhase.Stationary) && (cur.deltaTime < 0.3f)) { return true; }
            else { transform.position -= new Vector3(dlp.x, 0, dlp.y) * Sensivity; if (CameraFollow || PosFollow) { CameraFollow = false; PosFollow = false; } } 
        }
        else if (cnt == 2) {
            if (Input.touches[0].phase == TouchPhase.Began || Input.touches[1].phase == TouchPhase.Began) { TouchRotationAng = Vector2.Angle(Input.touches[0].position, Input.touches[1].position); TouchScaleAng = Vector2.Distance(Input.touches[0].position, Input.touches[1].position); }
            else  if (Input.touches[0].phase == TouchPhase.Moved || Input.touches[1].phase == TouchPhase.Moved) {
                float npos = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                float mousedelta = ((npos - TouchScaleAng) / 512) * Sensivity;
                TouchScaleAng = npos;
                if (mousedelta != 0 && Vector3.Distance(transform.position, CameraSelf.transform.position) > mousedelta + 1) {
                    Speed = Vector3.Distance(transform.position, CameraSelf.transform.position) / 35;
                    float angl = Mathf.Atan2(CameraSelf.transform.localPosition.y, CameraSelf.transform.localPosition.z);
                    CameraSelf.transform.localPosition -= new Vector3(0, mousedelta * Mathf.Sin(angl), mousedelta * Mathf.Cos(angl));
                    //CameraSelf.transform.localRotation = new Quaternion(0, angl * Mathf.Rad2Deg, 0, 1);
                }

                npos = Vector2.Angle(Input.touches[0].position, Input.touches[1].position);
                mousedelta = npos - TouchRotationAng;
                TouchRotationAng = npos;
                
                AngleHorizontal += mousedelta;
                transform.rotation = Quaternion.AngleAxis(AngleHorizontal, Vector3.up);
            }
        }
        return false;
    }
    */
    public bool TryBuildWay(int aX, int aY, Vector2Int BuildType) {
        if (Ground.Tiles[aX, aY] == null) {
            GameObject current;
            Ground.Types[aX, aY] = BuildType;
            Vector2Int tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX, aY, DefineConntectList(Ground.Types[aX, aY].x))), vtype;
            switch (BuildType.x) {
                case 1:
                    current = DefineRoadMesh(tsSurInfo.x, aX, aY, rot[tsSurInfo.y]);
                    if (Ground.SignsData[aX, aY].type != -1 && Ground.Types[aX, aY].x == 1) {
                        Debug.Log("TrafficMark at: " + aX.ToString() + ' ' + aY.ToString());
                        GameObject NewTrafficMarker;
                        if (Ground.SignsData[aX, aY].type == 2) { NewTrafficMarker = Instantiate(Ground.TrafficLightMesh[tsSurInfo.x], current.transform); Debug.Log("Type: Traffic Light"); }
                        else { NewTrafficMarker = Instantiate(Ground.SignMesh[tsSurInfo.x], current.transform); Debug.Log("Type: Sign"); }

                        NewTrafficMarker.transform.localPosition = Vector3.zero;
                        for (int i = 0; i < NewTrafficMarker.transform.childCount; ++i) {
                            if (Ground.SignsData[aX, aY].type == 2) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.TrafficLightMaterial; }
                            else if (Ground.SignsData[aX, aY].type == 1) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[1]; }
                            else { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[0]; }
                        }
                    }

                    break;
                case 2: current = DefineRoadMesh(tsSurInfo.x, aX, aY, rot[tsSurInfo.y]); break;
                default: return false;
            }
            current.name = aX.ToString() + ' ' + aY.ToString();
            Ground.Tiles[aX, aY] = current;
            List<Vector2Int> ftype = DefineConntectList(BuildType.x);
            if (aY + 1 < Ground.TilesY) {
                vtype = Ground.Types[aX, aY + 1]; if (vtype.x == 1 || vtype.x == 2) {
                    tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX, aY + 1, DefineConntectList(vtype.x)));
                    Ground.Tiles[aX, aY + 1] = DefineRoadMesh(tsSurInfo.x, aX, aY + 1, rot[tsSurInfo.y]);

                    if (Ground.SignsData[aX, aY + 1].type != -1 && Ground.Types[aX, aY + 1].x == 1) {
                        Debug.Log("TrafficMark at: " + aX.ToString() + ' ' + (aY + 1).ToString());
                        GameObject NewTrafficMarker;
                        if (Ground.SignsData[aX, aY + 1].type == 2) { NewTrafficMarker = Instantiate(Ground.TrafficLightMesh[tsSurInfo.x], Ground.Tiles[aX, aY + 1].transform); Debug.Log("Type: Traffic Light"); }
                        else { NewTrafficMarker = Instantiate(Ground.SignMesh[tsSurInfo.x], Ground.Tiles[aX, aY + 1].transform); Debug.Log("Type: Sign"); }

                        NewTrafficMarker.transform.localPosition = Vector3.zero;
                        for (int i = 0; i < NewTrafficMarker.transform.childCount; ++i) {
                            if (Ground.SignsData[aX, aY + 1].type == 2) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.TrafficLightMaterial; }
                            else if (Ground.SignsData[aX, aY + 1].type == 1) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[1]; }
                            else { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[0]; }
                        }
                    }
                }
            }
            if (aX + 1 < Ground.TilesX) {
                vtype = Ground.Types[aX + 1, aY]; if (vtype.x == 1 || vtype.x == 2) {
                    tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX + 1, aY, DefineConntectList(vtype.x)));
                    Ground.Tiles[aX + 1, aY] = DefineRoadMesh(tsSurInfo.x, aX + 1, aY, rot[tsSurInfo.y]);

                    if (Ground.SignsData[aX + 1, aY].type != -1 && Ground.Types[aX + 1, aY].x == 1) {
                        Debug.Log("TrafficMark at: " + (aX + 1).ToString() + ' ' + aY.ToString());
                        GameObject NewTrafficMarker;
                        if (Ground.SignsData[aX + 1, aY].type == 2) { NewTrafficMarker = Instantiate(Ground.TrafficLightMesh[tsSurInfo.x], Ground.Tiles[aX + 1, aY].transform); Debug.Log("Type: Traffic Light"); }
                        else { NewTrafficMarker = Instantiate(Ground.SignMesh[tsSurInfo.x], Ground.Tiles[aX + 1, aY].transform); Debug.Log("Type: Sign"); }

                        NewTrafficMarker.transform.localPosition = Vector3.zero;
                        for (int i = 0; i < NewTrafficMarker.transform.childCount; ++i) {
                            if (Ground.SignsData[aX + 1, aY].type == 2) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.TrafficLightMaterial; }
                            else if (Ground.SignsData[aX + 1, aY].type == 1) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[1]; }
                            else { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[0]; }
                        }
                    }
                }
            }
            if (aY - 1 >= 0) {
                vtype = Ground.Types[aX, aY - 1]; if (vtype.x == 1 || vtype.x == 2) {
                    tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX, aY - 1, DefineConntectList(vtype.x)));
                    Ground.Tiles[aX, aY - 1] = DefineRoadMesh(tsSurInfo.x, aX, aY - 1, rot[tsSurInfo.y]);

                    if (Ground.SignsData[aX, aY - 1].type != -1 && Ground.Types[aX, aY - 1].x == 1) {
                        Debug.Log("TrafficMark at: " + aX.ToString() + ' ' + (aY - 1).ToString());
                        GameObject NewTrafficMarker;
                        if (Ground.SignsData[aX, aY - 1].type == 2) { NewTrafficMarker = Instantiate(Ground.TrafficLightMesh[tsSurInfo.x], Ground.Tiles[aX, aY - 1].transform); Debug.Log("Type: Traffic Light"); }
                        else { NewTrafficMarker = Instantiate(Ground.SignMesh[tsSurInfo.x], Ground.Tiles[aX, aY - 1].transform); Debug.Log("Type: Sign"); }

                        NewTrafficMarker.transform.localPosition = Vector3.zero;
                        for (int i = 0; i < NewTrafficMarker.transform.childCount; ++i) {
                            if (Ground.SignsData[aX, aY - 1].type == 2) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.TrafficLightMaterial; }
                            else if (Ground.SignsData[aX, aY - 1].type == 1) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[1]; }
                            else { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[0]; }
                        }
                    }
                }
            }
            if (aX - 1 >= 0) {
                vtype = Ground.Types[aX - 1, aY]; if (vtype.x == 1 || vtype.x == 2) {
                    tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX - 1, aY, DefineConntectList(vtype.x)));
                    Ground.Tiles[aX - 1, aY] = DefineRoadMesh(tsSurInfo.x, aX - 1, aY, rot[tsSurInfo.y]);

                    if (Ground.SignsData[aX - 1, aY].type != -1 && Ground.Types[aX - 1, aY].x == 1) {
                        Debug.Log("TrafficMark at: " + (aX - 1).ToString() + ' ' + aY.ToString());
                        GameObject NewTrafficMarker;
                        if (Ground.SignsData[aX - 1, aY].type == 2) { NewTrafficMarker = Instantiate(Ground.TrafficLightMesh[tsSurInfo.x], Ground.Tiles[aX - 1, aY].transform); Debug.Log("Type: Traffic Light"); }
                        else { NewTrafficMarker = Instantiate(Ground.SignMesh[tsSurInfo.x], Ground.Tiles[aX - 1, aY].transform); Debug.Log("Type: Sign"); }

                        NewTrafficMarker.transform.localPosition = Vector3.zero;
                        for (int i = 0; i < NewTrafficMarker.transform.childCount; ++i) {
                            if (Ground.SignsData[aX - 1, aY].type == 2) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.TrafficLightMaterial; }
                            else if (Ground.SignsData[aX - 1, aY].type == 1) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[1]; }
                            else { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[0]; }
                        }
                    }
                }
            }
            //tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX, aY, ftype));
            //current = DefineRoadMesh(tsSurInfo.x, BuildType.x, BuildType.y, rot[tsSurInfo.y]);
            //Ground.needtorecalcpath = true;
            return true;
        }
        return false;
    }
    public bool TryBuildBuilding(int aX, int aY, Vector2Int BuildType) {
        GameObject current;
        Vector2Int vtype;
        int Surround = Ground.CountSurroundings(aX, aY, Ground.PavConnectsToTypes), rotindex = -1;
        Vector3Int sizeparams = Vector3Int.FloorToInt(Ground.HousePrefabs[BuildType.y].transform.position);
        bool Placeable = false;
        int placeableindex = sizeparams.x * sizeparams.y, checkindex = 0;
        if ((Surround & 1) > 0) {
            for (int x = 0; x < sizeparams.y; ++x)
            {
                for (int y = 0; y > -sizeparams.x; --y)
                {
                    if (aX + x < Ground.TilesX && aY + y >= 0)
                    {
                        if (Ground.Tiles[aX + x, aY + y] == null) { ++checkindex; }
                        else { break; }
                    }
                }
            }
            if (checkindex == placeableindex) { rotindex = 1; Placeable = true; }
            checkindex = 0;
        }
        if ((Surround & 2) > 0) {
            for (int x = 0; x < sizeparams.x; ++x)
            {
                for (int y = 0; y < sizeparams.y; ++y)
                {
                    if (aX + x < Ground.TilesX && aY + y < Ground.TilesY)
                    {
                        if (Ground.Tiles[aX + x, aY + y] == null) { ++checkindex; }
                        else { break; }
                    }
                }
            }
            if (checkindex == placeableindex) { rotindex = 0; Placeable = true; }
            checkindex = 0;
        }
        if ((Surround & 4) > 0) {
            for (int x = 0; x > -sizeparams.y; --x)
            {
                for (int y = 0; y < sizeparams.x; ++y)
                {
                    if (aX + x >= 0 && aY + y < Ground.TilesY)
                    {
                        if (Ground.Tiles[aX + x, aY + y] == null) { ++checkindex; }
                        else { break; }
                    }
                }
            }
            if (checkindex == placeableindex) { rotindex = 3; Placeable = true; }
            checkindex = 0;
        }
        if (((Surround & 8) > 0)/* * && !Placeable*/) {
            for (int x = 0; x > -sizeparams.x; --x)
            {
                for (int y = 0; y > -sizeparams.y; --y)
                {
                    if (aX + x >= 0 && aY + y >= 0)
                    {
                        if (Ground.Tiles[aX + x, aY + y] == null) { ++checkindex; }
                        else { break; }
                    }
                }
            }
            if (checkindex == placeableindex) { rotindex = 2; Placeable = true; }
        }


        if (Placeable) {
            HouseParams NewHouse = new HouseParams();
            StartBuilding nb = new StartBuilding();

            vtype = new Vector2Int(aX, aY);
            NewHouse.type = BuildType.y;
            NewHouse.buildingtiles = new List<Vector2Int>();
            NewHouse.citizens = new List<CitizenParams>();
            NewHouse.maxcitizens = sizeparams.z;
            NewHouse.main = vtype;

            nb.info = NewHouse;
            nb.type = BuildType.y;
            nb.status = 0;
            nb.buildingscales = new Vector3(sizeparams.x, 0, sizeparams.y);
            nb.maxgridsize = 1.1f;

            switch (BuildType.y) {
                case 0: nb.maxgridsize = 1.1f; break;
                case 1: nb.maxgridsize = 2.3f; break;
                case 2: nb.maxgridsize = 1.2f; break;
                case 3: nb.maxgridsize = 4f; break;
                case 4: nb.maxgridsize = 1.5f; break;
                case 5: nb.maxgridsize = 3f; break;
                case 6: nb.maxgridsize = 5f; break;
                case 7: nb.maxgridsize = 0.5f; break;
            }

            switch (rotindex) {
                case 0:
                    for (int x = 0; x < sizeparams.x; ++x) {
                        for (int y = 0; y < sizeparams.y; ++y)
                        {
                            if (x == 0 && y == 0) { current = Instantiate(Ground.HousePrefabs[BuildType.y], Ground.transform); current.transform.rotation = rot[rotindex]; current.transform.position = new Vector3(aX + 0.5f, -0.01f, aY + 0.5f); Vector3 entpos = current.transform.GetChild(0).position; NewHouse.Entrance = new Vector3(entpos.x, entpos.z, 0); current.SetActive(false); }
                            else { current = Instantiate(Ground.NullObject, Ground.transform); current.transform.position = new Vector3(aX + x + 0.5f, 0.05f, aY + y + 0.5f); NewHouse.buildingtiles.Add(new Vector2Int(aX + x, aY + y)); current.SetActive(false); }
                            Ground.HouseInfo[aX + x, aY + y] = NewHouse; Ground.Tiles[aX + x, aY + y] = current; Ground.Types[aX + x, aY + y] = BuildType;
                        }
                    }
                    break;
                case 1:
                    for (int x = 0; x < sizeparams.y; ++x) {
                        for (int y = 0; y > -sizeparams.x; --y)
                        {
                            if (x == 0 && y == 0) { current = Instantiate(Ground.HousePrefabs[BuildType.y], Ground.transform); current.transform.rotation = rot[rotindex]; current.transform.position = new Vector3(aX + 0.5f, -0.01f, aY + 0.5f); Vector3 entpos = current.transform.GetChild(0).position; NewHouse.Entrance = new Vector3(entpos.x, entpos.z, 0); current.SetActive(false); }
                            else { current = Instantiate(Ground.NullObject, Ground.transform); current.transform.position = new Vector3(aX + x + 0.5f, 0.05f, aY + y + 0.5f); NewHouse.buildingtiles.Add(new Vector2Int(aX + x, aY + y)); current.SetActive(false); }
                            Ground.HouseInfo[aX + x, aY + y] = NewHouse; Ground.Tiles[aX + x, aY + y] = current; Ground.Types[aX + x, aY + y] = BuildType;
                        }
                    }
                    break;
                case 2:
                    for (int x = 0; x > -sizeparams.x; --x) {
                        for (int y = 0; y > -sizeparams.y; --y)
                        {
                            if (x == 0 && y == 0) { current = Instantiate(Ground.HousePrefabs[BuildType.y], Ground.transform); current.transform.rotation = rot[rotindex]; current.transform.position = new Vector3(aX + 0.5f, -0.01f, aY + 0.5f); Vector3 entpos = current.transform.GetChild(0).position; NewHouse.Entrance = new Vector3(entpos.x, entpos.z, 0); current.SetActive(false); }
                            else { current = Instantiate(Ground.NullObject, Ground.transform); current.transform.position = new Vector3(aX + x + 0.5f, 0.05f, aY + y + 0.5f); NewHouse.buildingtiles.Add(new Vector2Int(aX + x, aY + y)); current.SetActive(false); }
                            Ground.HouseInfo[aX + x, aY + y] = NewHouse; Ground.Tiles[aX + x, aY + y] = current; Ground.Types[aX + x, aY + y] = BuildType;
                        }
                    }
                    break;
                case 3:
                    for (int x = 0; x > -sizeparams.y; --x) {
                        for (int y = 0; y < sizeparams.x; ++y)
                        {
                            if (x == 0 && y == 0) { current = Instantiate(Ground.HousePrefabs[BuildType.y], Ground.transform); current.transform.rotation = rot[rotindex]; current.transform.position = new Vector3(aX + 0.5f, -0.01f, aY + 0.5f); Vector3 entpos = current.transform.GetChild(0).position; NewHouse.Entrance = new Vector3(entpos.x, entpos.z, 0); current.SetActive(false); }
                            else { current = Instantiate(Ground.NullObject, Ground.transform); current.transform.position = new Vector3(aX + x + 0.5f, 0.05f, aY + y + 0.5f); NewHouse.buildingtiles.Add(new Vector2Int(aX + x, aY + y)); current.SetActive(false); }
                            Ground.HouseInfo[aX + x, aY + y] = NewHouse; Ground.Tiles[aX + x, aY + y] = current; Ground.Types[aX + x, aY + y] = BuildType;
                        }
                    }
                    break;
            }
            NewHouse.viewcenter = Ground.Tiles[aX, aY].transform.GetChild(1).position;
            InBuilding.Add(nb);
        }
        return Placeable;
    }
    public bool TryBuildTree(int aX, int aY, Vector2Int BuildType) {
        if (Ground.Tiles[aX, aY] == null) {
            GameObject current = Instantiate(Ground.TreePrefabs[BuildType.y], Ground.transform);
            current.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            current.transform.position = new Vector3(aX + 0.5f, 0, aY + 0.5f);
            current.name = aX.ToString() + ' ' + aY.ToString();
            Ground.Tiles[aX, aY] = current; Ground.Types[aX, aY] = BuildType;
            return true;
        }
        return false;
    }
    public void BuildBuildings() {
        for (int i = 0; i < InBuilding.Count; ++i) {
            StartBuilding cur = InBuilding[i];
            if (Ground.Tiles[cur.info.main.x, cur.info.main.y] == null) { Destroy(cur.gridself); InBuilding.RemoveAt(i); --i; continue; }
            switch (cur.status) {
                case 0:
                    cur.gridself = Instantiate(Ground.BuildGrid, Ground.transform);
                    cur.gridself.transform.position = cur.info.viewcenter;
                    cur.gridself.transform.rotation = Ground.Tiles[cur.info.main.x, cur.info.main.y].transform.rotation;
                    cur.gridself.transform.localScale = cur.buildingscales;
                    cur.status = 1;
                    InBuilding[i] = cur;
                    break;
                case 1:
                    cur.gridself.transform.localScale += Vector3.up * 0.005f;
                    if (cur.gridself.transform.localScale.y >= cur.maxgridsize) { cur.status = 2; }
                    InBuilding[i] = cur;
                    break;
                case 2:
                    Ground.Tiles[cur.info.main.x, cur.info.main.y].SetActive(true);
                    for (int j = 0; j < cur.info.buildingtiles.Count; ++j) { Ground.Tiles[cur.info.buildingtiles[j].x, cur.info.buildingtiles[j].y].SetActive(true); }
                    cur.status = 3;
                    InBuilding[i] = cur;
                    break;
                case 3:
                    cur.gridself.transform.localScale += Vector3.down * 0.01f;
                    if (cur.gridself.transform.localScale.y <= 0) { cur.status = 4; }
                    InBuilding[i] = cur;
                    break;
                case 4:
                    Destroy(cur.gridself);
                    switch (cur.type) {
                        case 0: cur.info.name = Ground.LivingHouseNames[Random.Range(0, Ground.LivingHouseNames.Length)];Ground.Resident.Add(cur.info); Ground.MaxCitizens += cur.info.maxcitizens; break;
                        case 1: cur.info.name = Ground.CommercialNames[Random.Range(0, Ground.CommercialNames.Length)]; Ground.Commercial.Add(cur.info); break;
                        case 2: cur.info.name = Ground.IndustryNames[Random.Range(0, Ground.IndustryNames.Length)]; Ground.Industry.Add(cur.info); break;
                        case 3: cur.info.name = Ground.LivingHouseNames[Random.Range(0, Ground.LivingHouseNames.Length)]; Ground.Resident.Add(cur.info); Ground.MaxCitizens += cur.info.maxcitizens; break;
                        case 4: cur.info.name = Ground.CommercialNames[Random.Range(0, Ground.CommercialNames.Length)]; Ground.Commercial.Add(cur.info); break;
                        case 5: cur.info.name = Ground.IndustryNames[Random.Range(0, Ground.IndustryNames.Length)]; Ground.Industry.Add(cur.info); break;
                        case 6: cur.info.name = "Отель"; Ground.Commercial.Add(cur.info); break;
                        case 7: cur.info.name = "Парк"; Ground.Leisure.Add(cur.info); break;
                    }
                    InBuilding.RemoveAt(i); --i;
                    break;
            }
        }
    }
    public void TryBuildMarker(int aX, int aY, Vector2Int BuildType) {
        if (Ground.Types[aX, aY].x == 1) {
            
            Ground.SignsData[aX, aY].type = BuildType.y;
            if (BuildType.y == -1) { ClearCell(aX, aY); TryBuildWay(aX, aY, new Vector2Int(1, 0)); return; }

            Vector2Int tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX, aY, DefineConntectList(Ground.Types[aX, aY].x)));

            Debug.Log("TrafficMark at: " + aX.ToString() + ' ' + aY.ToString());
            GameObject NewTrafficMarker;
            if (Ground.SignsData[aX, aY].type == 2) { NewTrafficMarker = Instantiate(Ground.TrafficLightMesh[tsSurInfo.x], Ground.Tiles[aX, aY].transform); Debug.Log("Type: Traffic Light"); }
            else { NewTrafficMarker = Instantiate(Ground.SignMesh[tsSurInfo.x], Ground.Tiles[aX, aY].transform); Debug.Log("Type: Sign"); }

            NewTrafficMarker.transform.localPosition = Vector3.zero;
            for (int i = 0; i < NewTrafficMarker.transform.childCount; ++i) {
                if (Ground.SignsData[aX, aY].type == 2) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.TrafficLightMaterial; }
                else if (Ground.SignsData[aX, aY].type == 1) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[1]; }
                else { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[0]; }
            }
        }
    }

    public void ClearCell(int aX, int aY) {
        Vector2Int tsSurInfo = Vector2Int.zero;
        int ttype;
        if (Ground.Tiles[aX, aY] != null) {
            Destroy(Ground.Tiles[aX, aY]); Ground.Tiles[aX, aY] = null;
            if (Ground.Types[aX, aY].x == 4) {
                HouseParams toremove = Ground.HouseInfo[aX, aY];
                if (Ground.Resident.Contains(toremove)) { Ground.MaxCitizens -= toremove.citizens.Count; }

                switch (Ground.Types[aX, aY].y) {
                    case 0: Ground.Resident.Remove(toremove); for (int i = 0; i < toremove.citizens.Count; ++i) { toremove.citizens[i].home = null; } break;
                    case 1: Ground.Commercial.Remove(toremove); for (int i = 0; i < toremove.citizens.Count; ++i) { toremove.citizens[i].shop = null; } break;
                    case 2: Ground.Industry.Remove(toremove); for (int i = 0; i < toremove.citizens.Count; ++i) { toremove.citizens[i].job = null; } break;
                    case 3: Ground.Resident.Remove(toremove); for (int i = 0; i < toremove.citizens.Count; ++i) { toremove.citizens[i].home = null; } break;
                    case 4: Ground.Commercial.Remove(toremove); for (int i = 0; i < toremove.citizens.Count; ++i) { toremove.citizens[i].shop = null; } break;
                    case 5: Ground.Industry.Remove(toremove); for (int i = 0; i < toremove.citizens.Count; ++i) { toremove.citizens[i].job = null; } break;
                    case 6: Ground.Commercial.Remove(toremove); for (int i = 0; i < toremove.citizens.Count; ++i) { toremove.citizens[i].shop = null; } break;
                    case 7: Ground.Leisure.Remove(toremove); for (int i = 0; i < toremove.citizens.Count; ++i) { toremove.citizens[i].leisure = null; } break;
                }
                while (toremove.buildingtiles.Count > 0) {
                    Destroy(Ground.Tiles[toremove.buildingtiles[0].x, toremove.buildingtiles[0].y]);
                    Ground.Types[toremove.buildingtiles[0].x, toremove.buildingtiles[0].y] = Vector2Int.zero;
                    toremove.buildingtiles.RemoveAt(0);
                }
                Destroy(Ground.Tiles[toremove.main.x, toremove.main.y]);
                Ground.Types[toremove.main.x, toremove.main.y] = Vector2Int.zero;
                Ground.HouseInfo[aX, aY] = null;
                Ground.UpdateSitizenCount();
            }
            //else if (Ground.Types[aX, aY].x == 1 || Ground.Types[aX, aY].x == 2) { Ground.needtorecalcpath = true; }
            Ground.Types[aX, aY] = Vector2Int.zero;
        }

        if (aY + 1 < Ground.TilesY) {
            ttype = Ground.Types[aX, aY + 1].x;
            if (ttype == 1 || ttype == 2) {
                tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX, aY + 1, DefineConntectList(Ground.Types[aX, aY + 1].x)));
                Ground.Tiles[aX, aY + 1] = DefineRoadMesh(tsSurInfo.x, aX, aY + 1, rot[tsSurInfo.y]);
            }

            if (Ground.SignsData[aX, aY + 1].type != -1 && Ground.Types[aX, aY + 1].x == 1) {
                Debug.Log("TrafficMark at: " + aX.ToString() + ' ' + (aY + 1).ToString());
                GameObject NewTrafficMarker;
                if (Ground.SignsData[aX, aY + 1].type == 2) { NewTrafficMarker = Instantiate(Ground.TrafficLightMesh[tsSurInfo.x], Ground.Tiles[aX, aY + 1].transform); Debug.Log("Type: Traffic Light"); }
                else { NewTrafficMarker = Instantiate(Ground.SignMesh[tsSurInfo.x], Ground.Tiles[aX, aY + 1].transform); Debug.Log("Type: Sign"); }

                NewTrafficMarker.transform.localPosition = Vector3.zero;
                for (int i = 0; i < NewTrafficMarker.transform.childCount; ++i) {
                    if (Ground.SignsData[aX, aY + 1].type == 2) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.TrafficLightMaterial; }
                    else if (Ground.SignsData[aX, aY + 1].type == 1) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[1]; }
                    else { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[0]; }
                }
            }
        }
        if (aX + 1 < Ground.TilesX) {
            ttype = Ground.Types[aX + 1, aY].x;
            if (ttype == 1 || ttype == 2) {
                tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX + 1, aY, DefineConntectList(Ground.Types[aX + 1, aY].x)));
                Ground.Tiles[aX + 1, aY] = DefineRoadMesh(tsSurInfo.x, aX + 1, aY, rot[tsSurInfo.y]);
            }

            if (Ground.SignsData[aX + 1, aY].type != -1 && Ground.Types[aX + 1, aY].x == 1) {
                Debug.Log("TrafficMark at: " + (aX + 1).ToString() + ' ' + aY.ToString());
                GameObject NewTrafficMarker;
                if (Ground.SignsData[aX + 1, aY].type == 2) { NewTrafficMarker = Instantiate(Ground.TrafficLightMesh[tsSurInfo.x], Ground.Tiles[aX + 1, aY].transform); Debug.Log("Type: Traffic Light"); }
                else { NewTrafficMarker = Instantiate(Ground.SignMesh[tsSurInfo.x], Ground.Tiles[aX + 1, aY].transform); Debug.Log("Type: Sign"); }

                NewTrafficMarker.transform.localPosition = Vector3.zero;
                for (int i = 0; i < NewTrafficMarker.transform.childCount; ++i) {
                    if (Ground.SignsData[aX + 1, aY].type == 2) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.TrafficLightMaterial; }
                    else if (Ground.SignsData[aX + 1, aY].type == 1) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[1]; }
                    else { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[0]; }
                }
            }
        }
        if (aY - 1 >= 0) {
            ttype = Ground.Types[aX, aY - 1].x;
            if (ttype == 1 || ttype == 2) {
                tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX, aY - 1, DefineConntectList(Ground.Types[aX, aY - 1].x)));
                Ground.Tiles[aX, aY - 1] = DefineRoadMesh(tsSurInfo.x, aX, aY - 1, rot[tsSurInfo.y]);
            }

            if (Ground.SignsData[aX, aY - 1].type != -1 && Ground.Types[aX, aY - 1].x == 1) {
                Debug.Log("TrafficMark at: " + aX.ToString() + ' ' + (aY - 1).ToString());
                GameObject NewTrafficMarker;
                if (Ground.SignsData[aX, aY - 1].type == 2) { NewTrafficMarker = Instantiate(Ground.TrafficLightMesh[tsSurInfo.x], Ground.Tiles[aX, aY - 1].transform); Debug.Log("Type: Traffic Light"); }
                else { NewTrafficMarker = Instantiate(Ground.SignMesh[tsSurInfo.x], Ground.Tiles[aX, aY - 1].transform); Debug.Log("Type: Sign"); }

                NewTrafficMarker.transform.localPosition = Vector3.zero;
                for (int i = 0; i < NewTrafficMarker.transform.childCount; ++i) {
                    if (Ground.SignsData[aX, aY - 1].type == 2) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.TrafficLightMaterial; }
                    else if (Ground.SignsData[aX, aY - 1].type == 1) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[1]; }
                    else { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[0]; }
                }
            }
        }
        if (aX - 1 >= 0) {
            ttype = Ground.Types[aX - 1, aY].x;
            if (ttype == 1 || ttype == 2) {
                tsSurInfo = DefineRoadType(Ground.CountSurroundings(aX - 1, aY, DefineConntectList(Ground.Types[aX - 1, aY].x)));
                Ground.Tiles[aX - 1, aY] = DefineRoadMesh(tsSurInfo.x, aX - 1, aY, rot[tsSurInfo.y]);
            }

            if (Ground.SignsData[aX - 1, aY].type != -1 && Ground.Types[aX - 1, aY].x == 1) {
                Debug.Log("TrafficMark at: " + (aX - 1).ToString() + ' ' + aY.ToString());
                GameObject NewTrafficMarker;
                if (Ground.SignsData[aX - 1, aY].type == 2) { NewTrafficMarker = Instantiate(Ground.TrafficLightMesh[tsSurInfo.x], Ground.Tiles[aX - 1, aY].transform); Debug.Log("Type: Traffic Light"); }
                else { NewTrafficMarker = Instantiate(Ground.SignMesh[tsSurInfo.x], Ground.Tiles[aX - 1, aY].transform); Debug.Log("Type: Sign"); }

                NewTrafficMarker.transform.localPosition = Vector3.zero;
                for (int i = 0; i < NewTrafficMarker.transform.childCount; ++i) {
                    if (Ground.SignsData[aX - 1, aY].type == 2) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.TrafficLightMaterial; }
                    else if (Ground.SignsData[aX - 1, aY].type == 1) { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[1]; }
                    else { NewTrafficMarker.transform.GetChild(i).GetComponent<MeshRenderer>().material = Ground.SignMaterial[0]; }
                }
            }
        }
    }
    public Vector2Int DefineRoadType(int sTiles) {
        Vector2Int val = Vector2Int.zero;
        switch (sTiles) {
            case 0: val.x = 0; val.y = 0; break;
            case 1: val.x = 1; val.y = 3; break;
            case 2: val.x = 1; val.y = 2; break;
            case 3: val.x = 3; val.y = 3; break;
            case 4: val.x = 1; val.y = 1; break;
            case 5: val.x = 2; val.y = 1; break;
            case 6: val.x = 3; val.y = 2; break;
            case 7: val.x = 4; val.y = 1; break;
            case 8: val.x = 1; val.y = 0; break;
            case 9: val.x = 3; val.y = 0; break;
            case 10: val.x = 2; val.y = 0; break;
            case 11: val.x = 4; val.y = 2; break;
            case 12: val.x = 3; val.y = 1; break;
            case 13: val.x = 4; val.y = 3; break;
            case 14: val.x = 4; val.y = 0; break;
            case 15: val.x = 5; val.y = 0; break;
            default: break;
        }
        return val;
    }
    public List<Vector2Int> DefineConntectList(int type) {
        if (type == 1) { return new List<Vector2Int>(Ground.RoadConnectsToTypes); }
        if (type == 2) { return new List<Vector2Int>(Ground.PavConnectsToTypes); }
        return new List<Vector2Int>();
    }
    public GameObject DefineRoadMesh(int surroundings, int whereX, int whereY, Quaternion orientr) {
        if (Ground.Tiles[whereX, whereY] != null) { Destroy(Ground.Tiles[whereX, whereY]); }
        if (Ground.Types[whereX, whereY].x == 1) { return Instantiate(Ground.Roads[surroundings, Ground.Types[whereX, whereY].y], new Vector3(whereX + 0.5f, 0, whereY + 0.5f), orientr); }
        if (Ground.Types[whereX, whereY].x == 2) { return Instantiate(Ground.Pavs[surroundings, Ground.Types[whereX, whereY].y], new Vector3(whereX + 0.5f, 0, whereY + 0.5f), orientr); }
        return null;
    }
    public void OnBuildTypePressed(int BuildButtonType) { BuildType.x = BuildButtonType; }
    public void OnBuildSubTypePressed(int BuildButtonSubType) { BuildType.y = BuildButtonSubType; }
    public void ToggleMenus(int menucode) {
        BuildType = Vector2Int.zero;
        for (int i = 0; i < Menus.Length; ++i) { Menus[i].SetActive(false); }
        if (menucode >= 0) {
            if (Menus[menucode].activeSelf) { Menus[menucode].SetActive(false); } else { Menus[menucode].SetActive(true); }
        }
    }
    public Sprite DefineHappinessSprite(float value) {
        if (value > 80) { return Smiles[3]; }
        else if (value > 60 && value <= 80) { return Smiles[2]; }
        else if (value > 40 && value <= 60) { return Smiles[1]; }
        else { return Smiles[0]; }
    }
    public void UpdateSitizenHappiness() {
        float average = 0; int n = 0; float unemp = 0, wealthy = 0;
        if (Ground.Params.Count == 0) { HappinessButton.sprite = Smiles[0]; Unemployment.fillAmount = 1; Wealth.fillAmount = 0; return; }
        for (int i = 0; i < Ground.Params.Count; ++i) {
            CitizenParams cur = Ground.Params[i];
            cur.happiness = 0;
            if (cur.home != null) { cur.happiness += 20; }
            if (cur.shop != null) { cur.happiness += 10; }
            if (cur.job != null) { cur.happiness += 30; } else { unemp += 1f; }
            if (cur.leisure != null) { cur.happiness += 7; wealthy += 0.5f; }

            switch (cur.havecar) {
                case -1: cur.happiness += 5; break;
                case 0: cur.happiness += 10; break;
                case 1: cur.happiness += 15; break;
                case 2: cur.happiness += 13; break;
            }

            if (cur.budget > 30) { cur.happiness += 15; wealthy += 0.6f; }
            else if (cur.budget > 20) { cur.happiness += 10; wealthy += 0.5f; }
            else if (cur.budget > 10) { cur.happiness += 5; wealthy += 0.3f; }
            else if (cur.budget > 0 && cur.budget <= 10) { cur.happiness -= 3; }
            else { cur.happiness -= 15; }

            if (cur.happiness < 60) { cur.leisure = null; }
            average += cur.happiness;
            ++n;
        }
        HappinessButton.sprite = DefineHappinessSprite(average / n);
        Unemployment.fillAmount = ((unemp / n) + (Unemployment.fillAmount)) / 2f;
        Wealth.fillAmount = ((wealthy / n) + (Wealth.fillAmount)) / 2f;
    }
    public void UpdateCitizenInfo(CitizenParams cur) {
        //InfoLabel[0].text = cur.name;
        NameField.text = cur.name;
        InfoLabel[1].text = cur.age.ToString();
        switch (cur.gender) {
            case 0: InfoLabel[2].text = "Мужской"; break;
            case 1: InfoLabel[2].text = "Женский"; break;
        }
        if (Ground.HouseInfo[cur.navto.x, cur.navto.y] != null) { cords[3] = Ground.HouseInfo[cur.navto.x, cur.navto.y].viewcenter; InfoLabel[7].text = Ground.HouseInfo[cur.navto.x, cur.navto.y].name; }

        if (cur.home != null) { InfoLabel[3].text = cur.home.name; cords[0] = cur.home.viewcenter; if (cur.navto == cur.home.main) { InfoLabel[7].text = "Дом"; } } else { InfoLabel[3].text = "Бездомный"; }
        if (cur.shop != null) { InfoLabel[4].text = cur.shop.name; cords[1] = cur.shop.viewcenter; if (cur.navto == cur.shop.main) { InfoLabel[7].text = "Магазин"; } } else { InfoLabel[4].text = "В поиске"; }
        if (cur.job != null) { InfoLabel[5].text = cur.job.name; cords[2] = cur.job.viewcenter; if (cur.navto == cur.job.main) { InfoLabel[7].text = "Работа"; } } else { InfoLabel[5].text = "Безработный"; }

        switch (cur.motion) {
            case '0': InfoLabel[6].text = " Находится:"; break;
            case '1': InfoLabel[6].text = " Идет:"; break;
            case '2': InfoLabel[6].text = " Едет:"; break;
        }
        switch (cur.havecar) {
            case -1: InfoLabel[9].text = "Нет"; break;
            case 0: InfoLabel[9].text = "Седан"; break;
            case 1: InfoLabel[9].text = "Хэтчбэк"; break;
            case 2: InfoLabel[9].text = "Пикап"; break;
            default: InfoLabel[9].text = "Да"; break;
        }
        if (CameraFollow) { NavigateButton.sprite = NavSprite[1]; } else { NavigateButton.sprite = NavSprite[0]; }

        InfoLabel[10].text = cur.budget.ToString();
        InfoLabel[8].text = cur.happiness.ToString() + '%';
        SmileLabel.sprite = DefineHappinessSprite(cur.happiness);
    }
    public void NameEdit() {
        if (Selected != null) { Ground.Params[Ground.Citizens.FindIndex(d => d == Selected)].name = NameField.text; }
    }
    public void MoveToCord(int index) {
        if (index == 10) { CameraFollow = !CameraFollow; PosFollow = false; }
        else if (cords[index] != null) { ShowVector = cords[index]; CameraFollow = false; PosFollow = true; }
        
        if (Selected == null) { Menus[5].SetActive(false); }
    }
    public void PauseButtonToggle() {
        PauseButton = !PauseButton;
        PauseMenu.SetActive(PauseButton);
    }
    public void ToMenu() {
        SceneManager.LoadScene(0);
    }
}