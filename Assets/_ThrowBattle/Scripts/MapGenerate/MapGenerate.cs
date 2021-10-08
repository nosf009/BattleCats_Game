using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace _ThrowBattle
{
    public enum CreateSegmentMode
    {
        Flat,
        NormalLower,
        MuchLower,
        NormalHigher,
        MuchHigher
    }

    public static class MyVector3Extension
    {
        public static Vector3[] toVector3Array(this Vector2[] v2)
        {
            return System.Array.ConvertAll<Vector2, Vector3>(v2, getV2fromV3);
        }

        public static Vector3 getV2fromV3(Vector2 v2)
        {
            return new Vector3(v2.x, v2.y, -1);
        }
    }


    public class MapGenerate : MonoBehaviour
    {
        public MapType[] mapTypes;

        public float segmentWidth;
        public float lineHeight;
        public float segmentHeigh;
        public float mapWidth;
        public float slopesWidth;

        public int limitSameSegment = 2;

        [HideInInspector]
        public List<Vector3> vertices = new List<Vector3>();
        [HideInInspector]
        public List<Vector2> surfaceVertices = new List<Vector2>();
        [HideInInspector]
        public List<Vector3> normals = new List<Vector3>();
        [HideInInspector]
        public List<Vector2> uvs = new List<Vector2>();
        [HideInInspector]
        public List<int> triangle = new List<int>();
        [HideInInspector]
        public List<int> randomSegmentRecord = new List<int>();
        List<GameObject> oldBackgrounds = new List<GameObject>();

        private float originalX = 0;

        private int currentVerticesIndex = 0;
        private int sameSegmentModeCount = 0;
        private int currentRandomRecord = 0;

        private bool isLeftUV = false;

        private MeshFilter mapFilter;
        private MeshRenderer mapRenderer;
        private LineRenderer lineRender;

        public GameObject mapLimitLeft;
        public GameObject mapLimitRight;
        public GameObject mapLimitTop;

        private Vector2 firstPosition;
        private Vector2 currentPosition;
        private float currentDistance = 0;
        public float lowerDistance = 20;
        [HideInInspector]
        public float maxY = 0;
        [HideInInspector]
        public float originalSegmenHeight;
        [HideInInspector]
        public int maptypeIndex = 0;

        private void OnEnable()
        {
#if EASY_MOBILE_PRO
            MultiplayerRealtimeManager.OnLeaveRoom += OnPlayerLeft;
#endif
        }

        private void OnDisable()
        {
#if EASY_MOBILE_PRO
            MultiplayerRealtimeManager.OnLeaveRoom -= OnPlayerLeft;
#endif
        }

        // Use this for initialization
        void Start()
        {
            originalSegmenHeight = segmentHeigh;
            lineRender = transform.GetChild(0).GetComponent<LineRenderer>();
            originalX = -mapWidth / 2;
            mapRenderer = GetComponent<MeshRenderer>();
            mapFilter = GetComponent<MeshFilter>();
            InitialSetting();
        }

        //add first vertex
        private void InitialSetting()
        {
            if (mapLimitTop)
            {
                mapLimitTop.SetActive(false);
            }
            currentVerticesIndex = 0;
            sameSegmentModeCount = 0;
            currentRandomRecord = 0;
            firstPosition = new Vector2(originalX, 0);
            currentPosition = firstPosition;
            AddLeftQuadVertices();
            MappingLeftUVs();
        }

        public void EnableTopCollider()
        {
            if (mapLimitTop)
            {
                mapLimitTop.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        //Clear all map when other player left the game in multiplayer mode
        void OnPlayerLeft()
        {
            GameManager.Instance.playerController.generateMapComplete = false;
            vertices.Clear();
            triangle.Clear();
            normals.Clear();
            uvs.Clear();
            surfaceVertices.Clear();
            randomSegmentRecord.Clear();
            mapFilter.mesh = null;
            Destroy(GetComponent<EdgeCollider2D>());
            lineRender.positionCount = 0;
            lineRender.gameObject.transform.localPosition = Vector3.zero;
            for (int i = 0; i < oldBackgrounds.Count; i++)
            {
                Destroy(oldBackgrounds[i]);
            }
            oldBackgrounds.Clear();
            Camera.main.GetComponent<Parallaxing>().backgrounds.Clear();
            InitialSetting();
        }

        public void GenerateMap(bool isSender = true)
        {
            maptypeIndex = UnityEngine.Random.Range(0, mapTypes.Length);
            bool isCreateFlat = true;
            while (Vector2.Distance(new Vector2(originalX, 0), new Vector2(currentPosition.x, 0)) < mapWidth)
            {
                if (isCreateFlat)
                {
                    AddRightQuadVertices();

                    if (currentVerticesIndex == 0)
                        currentVerticesIndex = 3;
                    else
                        currentVerticesIndex += 2;
                }
                else
                {
                    CreateSegmentMode segmentMode = new CreateSegmentMode();
                    if (GameManager.Instance.IsMultiplayerMode())
                        MultiPlayerGenerateHandling(ref segmentMode, isSender);
                    else
                        RandomSegmentHeightHandling(ref segmentMode);

                    AddRightQuadVertices(segmentMode);
                    currentVerticesIndex += 2;
                }
                //Add left triangle
                CreateLeftTriangle();
                //Add right triangle
                CreateRightTriangle();
                //Mapping uv for each segment
                if (!isLeftUV)
                    MappingRightUVs();
                else
                    MappingLeftUVs();

                isLeftUV = !isLeftUV;

                isCreateFlat = !isCreateFlat;
                firstPosition = currentPosition;
                currentDistance = Vector2.Distance(new Vector2(originalX, 0), new Vector2(currentPosition.x, 0));
            }
            SetUVs();
            CreateMap(isSender);
        }

        void SetUVs()
        {
            float currentYDistance = Vector2.Distance(new Vector2(0, -lowerDistance), new Vector2(0, maxY));
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 currentUV = uvs[i];
                currentUV.x = Vector2.Distance(new Vector2(originalX, 0), new Vector2(vertices[i].x, 0)) / currentDistance;
                currentUV.y = Vector2.Distance(new Vector2(0, -lowerDistance), new Vector2(0, vertices[i].y)) / currentYDistance;
                uvs[i] = currentUV;
            }
        }

        void MultiPlayerGenerateHandling(ref CreateSegmentMode segmentMode, bool isSender)
        {
            if (isSender)
            {
                RandomSegmentHeightHandling(ref segmentMode);
                if (segmentMode == CreateSegmentMode.NormalHigher)
                    randomSegmentRecord.Add(1);
                else if (segmentMode == CreateSegmentMode.MuchHigher)
                    randomSegmentRecord.Add(2);
                else if (segmentMode == CreateSegmentMode.NormalLower)
                    randomSegmentRecord.Add(3);
                else if (segmentMode == CreateSegmentMode.MuchLower)
                    randomSegmentRecord.Add(4);
            }
            else
            {
                if (randomSegmentRecord[currentRandomRecord] == 1)
                    segmentMode = CreateSegmentMode.NormalHigher;
                else if (randomSegmentRecord[currentRandomRecord] == 2)
                    segmentMode = CreateSegmentMode.MuchHigher;
                else if (randomSegmentRecord[currentRandomRecord] == 3)
                    segmentMode = CreateSegmentMode.NormalLower;
                else if (randomSegmentRecord[currentRandomRecord] == 4)
                    segmentMode = CreateSegmentMode.MuchLower;
                currentRandomRecord++;
            }
        }

        void RandomSegmentHeightHandling(ref CreateSegmentMode segmentMode)
        {
            int rand = UnityEngine.Random.Range(0, 2);
            if (sameSegmentModeCount < -limitSameSegment)
                rand = 0;
            else if (sameSegmentModeCount > limitSameSegment)
                rand = 1;
            //Random rand=0 then create higher segment
            if (rand == 0)
            {
                if (sameSegmentModeCount < 0)
                    sameSegmentModeCount = 0;

                sameSegmentModeCount++;
                //Random randomHigherMode to make normal higher or much higher
                int randomHigherMode = UnityEngine.Random.Range(0, 3);
                segmentMode = (randomHigherMode == 1) ? CreateSegmentMode.MuchHigher : CreateSegmentMode.NormalHigher;
            }
            //Random rand=1 then create lower segment
            if (rand == 1)
            {
                if (sameSegmentModeCount > 0)
                    sameSegmentModeCount = 0;

                sameSegmentModeCount--;
                int randomLowerMode = UnityEngine.Random.Range(0, 3);
                segmentMode = (randomLowerMode == 1) ? CreateSegmentMode.MuchLower : CreateSegmentMode.NormalLower;
            }
        }

        void CreateMap(bool isSender = false)
        {
            //Create new mesh by vertices,triangle,normals,uvs list
            Mesh newMesh = new Mesh();
            newMesh.vertices = vertices.ToArray();
            newMesh.triangles = triangle.ToArray();
            newMesh.normals = normals.ToArray();
            newMesh.uv = uvs.ToArray();

            //Setting for line render on the surface of the mesh
            lineRender.positionCount = surfaceVertices.Count;
            Vector2[] surfaceVerticesArray = surfaceVertices.ToArray();
            Vector3[] array = surfaceVerticesArray.toVector3Array();
            lineRender.SetPositions(array);
            lineRender.material = mapTypes[maptypeIndex].terrainBorderMaterial;
            transform.GetChild(0).localPosition -= new Vector3(0, lineRender.startWidth / 4, 0);

            //Assign new mesh to gameObject
            mapFilter.mesh = newMesh;
            mapRenderer.material = mapTypes[maptypeIndex].terrainMaterial;

            //Add collider on the surface of the mesh
            EdgeCollider2D collider = gameObject.AddComponent<EdgeCollider2D>();
            collider.points = surfaceVertices.ToArray();

            RaycastHit2D hit = Physics2D.Raycast(GameManager.Instance.playerController.leftPosition.position, Vector2.down, LayerMask.GetMask("Plane"));
            Camera.main.GetComponent<Parallaxing>().backgrounds.Clear();
            oldBackgrounds.Clear();
            Camera.main.GetComponent<Parallaxing>().backgrounds = new List<Transform>(mapTypes[maptypeIndex].backgrounds.Length);
            if (hit.collider != null)
            {
                for (int i = 0; i < mapTypes[maptypeIndex].backgrounds.Length; i++)
                {
                    GameObject background = Instantiate(mapTypes[maptypeIndex].backgrounds[i]);
                    oldBackgrounds.Add(background);
                    Vector3 position = background.transform.position;
                    position.y = hit.point.y + 5;
                    background.transform.position = position;
                    Camera.main.GetComponent<Parallaxing>().backgrounds.Add(background.transform);
                }
            }
            //Check if this is multiplayer mode then decide to send map's data to other player 
            //or send start game signal
            if (GameManager.Instance.IsMultiplayerMode())
            {
                if (isSender)
                    SendMapData();
                else
                {
                    byte[] startGameSignal = { 2 };
                    byte[] thisPlayerCharacterIndex = { 9, (byte)CharacterManager.Instance.CurrentCharacterIndex };
                    GameManager.Instance.playerController.SendDataToOtherPlayer(thisPlayerCharacterIndex);
                    GameManager.Instance.playerController.SendDataToOtherPlayer(startGameSignal);
                    GameManager.Instance.StartGame();
                }
            }
            Camera.main.GetComponent<Parallaxing>().InitialSetting();
        }

        //Send Map's data to other player by converting randomSegmentRecord to byte array
        //With first byte of this data is the rendomSegmentRecord's length
        public void SendMapData()
        {
            byte[] dataArray = new byte[randomSegmentRecord.Count + 1];
            dataArray[0] = 6;
            for (int i = 1; i < dataArray.Length; i++)
            {
                dataArray[i] = (byte)randomSegmentRecord[i - 1];
            }
            byte[] thisPlayerCharacterIndex = { 9, (byte)CharacterManager.Instance.CurrentCharacterIndex };
            GameManager.Instance.playerController.SendDataToOtherPlayer(thisPlayerCharacterIndex);
            GameManager.Instance.playerController.SendDataToOtherPlayer(dataArray);
            GameManager.Instance.playerController.StartWaitOtherGenerateMap();
        }

        //Handle received map's data by convert byte array to int value then add it to randomSegmentRecord list
        public void ByteArrayToMapData(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                randomSegmentRecord.Add(data[i]);
            }
            GenerateMap(false);
        }

        //Add two vertices on the left of the quad with upper position is firstPosition
        //Then create lower position with the same x axis with upper position
        void AddLeftQuadVertices()
        {
            Vector2 lowerLeftPosition = firstPosition;
            lowerLeftPosition.y = -lowerDistance;

            if (firstPosition.y > maxY)
                maxY = firstPosition.y;
            surfaceVertices.Add(firstPosition);

            vertices.Add(lowerLeftPosition);
            vertices.Add(firstPosition);

            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
        }

        //Add two vertices on the right of the quad with upper position is currentPosition handling from HandleSegmentMode function
        //Then create lower position with the same x axis with upper position
        void AddRightQuadVertices(CreateSegmentMode segmentMode = CreateSegmentMode.Flat)
        {
            currentPosition = firstPosition;

            HandleSegmentMode(segmentMode);

            Vector2 lowerRightPosition = currentPosition;
            lowerRightPosition.y = -lowerDistance;

            surfaceVertices.Add(currentPosition);

            if (currentPosition.y > maxY)
                maxY = currentPosition.y;
            vertices.Add(lowerRightPosition);
            vertices.Add(currentPosition);

            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
        }

        void HandleSegmentMode(CreateSegmentMode segmentMode)
        {
            if (segmentMode == CreateSegmentMode.Flat)
                currentPosition.x = firstPosition.x + segmentWidth;
            else
                currentPosition.x = firstPosition.x + slopesWidth;
            if (segmentMode == CreateSegmentMode.NormalLower)
            {
                currentPosition.y -= segmentHeigh;
            }
            else if (segmentMode == CreateSegmentMode.MuchLower)
            {
                currentPosition.y -= segmentHeigh * 2;
                currentPosition.x += slopesWidth;
            }
            else if (segmentMode == CreateSegmentMode.NormalHigher)
            {
                currentPosition.y += segmentHeigh;
            }
            else if (segmentMode == CreateSegmentMode.MuchHigher)
            {
                currentPosition.y += segmentHeigh * 2;
                currentPosition.x += slopesWidth;
            }
        }

        //Mapping UV for two left vertices of the quad
        void MappingLeftUVs()
        {
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
        }

        //Mapping UV for two right vertices of the quad
        void MappingRightUVs()
        {
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
        }

        void CreateLeftTriangle()
        {
            triangle.Add(currentVerticesIndex - 3);
            triangle.Add(currentVerticesIndex - 2);
            triangle.Add(currentVerticesIndex - 1);
        }

        void CreateRightTriangle()
        {
            triangle.Add(currentVerticesIndex - 2);
            triangle.Add(currentVerticesIndex);
            triangle.Add(currentVerticesIndex - 1);
        }
    }
}
