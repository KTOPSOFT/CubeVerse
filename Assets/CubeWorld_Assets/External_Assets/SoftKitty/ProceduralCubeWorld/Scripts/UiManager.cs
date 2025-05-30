using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace SoftKitty.PCW.Demo
{
    public class UiManager : MonoBehaviour
    {
        public static UiManager instance;
        public Text PlayModeText;
        public GameObject [] Hints;
        public GameObject[] StateLabel;
        public GameObject PreviewRoot;
        public Transform PreviewModelRoot;
        List<GameObject> PreviewCubes = new List<GameObject>();
        public static int previewType = 0;
        public static int previewHeight = 1;
        public static int previewUid = 0;
        public GameObject SavePanel;
        public InputField SavePath;
        public Text SaveMsgText;
        public Text SpawnAiText;
        public Image [] SpawnAiMarks;
        public RectTransform ProgressBarRoot;
        public RectTransform ProgressBar;
        List<GameObject> AliveAiObjs = new List<GameObject>();
        bool viewCustomCubes = true;
        public bool isOpen = false;
        public int CustomCubeLoadIndex = 0;
         
        void Start()
        {
            instance = this;
        }

        public void ShowProgess(float _progress)
        {
            ProgressBarRoot.position = TransferPos(Input.mousePosition, ProgressBarRoot.parent.GetComponent<RectTransform>());
            ProgressBar.sizeDelta = new Vector2(150F * _progress,20F);
            ProgressBarRoot.gameObject.SetActive(true);
        }


        public void HideProgress()
        {
            ProgressBarRoot.gameObject.SetActive(false);
        }

        public Vector3 TransferPos(Vector3 _pos, RectTransform _parentTransform)//Transfrom mouse position to rect position.
        {
            Vector2 localPosition = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentTransform,
                _pos,
                GetComponentInChildren<Camera>(),
                out localPosition);
            return _parentTransform.TransformPoint(localPosition);
        }

        public void DoSpawnAi()
        {
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/Button"), Camera.main.transform.position);
            if (AliveAiObjs.Count <= 0)
            {
                SpawnAiText.text = "KILL AI";
                foreach (Image obj in SpawnAiMarks) obj.color = Color.yellow;
                int _count = Random.Range(1,10);
                for (int i=0;i< _count;i++) {
                    Vector3 _dir = new Vector3(Random.Range(-1F,1F),0F, Random.Range(-1F, 1F)).normalized;
                    Vector3 _pos = BlockGenerator.instance.Player.transform.position + _dir * Random.Range(5F, 10F);
                    _pos.y = BlockGenerator.instance.GetSurfaceHeightByPosition(_pos)+1F;
                    GameObject newEnemy = Instantiate(Resources.Load<GameObject>("ProceduralCubeWorld/Enemy"), _pos, Quaternion.identity);
                    newEnemy.SetActive(true);
                    AliveAiObjs.Add(newEnemy);
                }
            }
            else
            {
                foreach (GameObject obj in AliveAiObjs) {
                    Destroy(obj);
                }
                AliveAiObjs.Clear();
                SpawnAiText.text = "SPAWN AI";
                foreach (Image obj in SpawnAiMarks) obj.color = Color.gray*0.3F;

            }
        }

        public void DoSave()
        {
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/Button"), Camera.main.transform.position);
            SaveMsgText.gameObject.SetActive(false);
            string _path = SavePath.text.Replace(@"\", "/");
            string _filename= Path.GetFileName(_path);
            string _dir = _path.Replace(@"/"+_filename,"");
            Debug.Log(_dir);
            if (_filename==null || _filename.Length<1 || !_filename.Contains(".")) {
                SaveMsgText.text = "Invalid file name!";
            }
            else if (!Directory.Exists(_dir))
            {
                SaveMsgText.text = "Directory not exist!";
            }
            else
            {
                //BlockGenerator.instance.SaveWorld(_path);
                SaveMsgText.text = "Successfully saved!";
            }
            SaveMsgText.gameObject.SetActive(true);
        }

        public void DoLoad()
        {
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/Button"), Camera.main.transform.position);
            SaveMsgText.gameObject.SetActive(false);
            string _path = SavePath.text.Replace(@"\", "/");
            if (!File.Exists(_path))
            {
                SaveMsgText.text = "File not exist!";
                SaveMsgText.gameObject.SetActive(true);
            }
            else
            {
                //BlockGenerator.instance.LoadSavedWorld(_path);
                ToggleSavePanel();
            }
           
        }

        public void SwitchPlayMode()
        {
            if (BuildControl.PlayMode == BuildControl.PlayModes.VitualCube)
            {
                BuildControl.PlayMode = BuildControl.PlayModes.TapOrHold;
                PlayModeText.text = "Build Control #2";
            }
            else
            {
                BuildControl.PlayMode = BuildControl.PlayModes.VitualCube;
                PlayModeText.text = "Build Control #1";
            }
        }

        public void ToggleSavePanel()
        {
            SaveMsgText.text = "";
            SavePanel.SetActive(!SavePanel.activeSelf);
            if (SavePanel.activeSelf)
            {
                BuildControl.BuildMode = false;
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/MenuActive"), Camera.main.transform.position);
            }
            else
            {
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/Deactive"), Camera.main.transform.position);
            }
        }

        public void DoTeleport()
        {
            foreach (SkinnedMeshRenderer obj in BlockGenerator.instance.Player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                GameObject newObj = new GameObject();
                newObj.AddComponent<DestroyLater>();
                newObj.transform.position = obj.transform.position;
                newObj.transform.rotation = obj.transform.rotation;
                Mesh newMesh = new Mesh();
                obj.BakeMesh(newMesh);
                newObj.AddComponent<MeshFilter>().mesh = newMesh;
                Material newMat = Resources.Load<Material>("ProceduralCubeWorld/TeleportMat");
                newObj.AddComponent<MeshRenderer>();
                Material[] newMats = new Material[3];
                for (int i = 0; i < 3; i++)
                {
                    newMats[i] = newMat;
                }
                newObj.GetComponent<MeshRenderer>().sharedMaterials = newMats;
                newObj.GetComponent<MeshRenderer>().materials = newMats;
               
            }
            Instantiate(Resources.Load<GameObject>("ProceduralCubeWorld/Teleport"),BlockGenerator.instance.PlayerPos,Quaternion.identity);
            BlockGenerator.instance.Teleport(new Vector3(Random.Range(-1000,1000), 1F, Random.Range(-1000, 1000)));
        }

       
        void Update()
        {
            if (!PreviewRoot.activeSelf && BuildControl.BuildMode && !BuildControl.DeleteMode && !isOpen) 
            {
                LoadPreviewCubes();
            }

            if (SavePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape)) ToggleSavePanel();

            if (Input.GetKeyDown(KeyCode.Q)) {
                PreviewOffset(-1, CustomCubeLoadIndex);
                LoadPreviewCubes(-1 , CustomCubeLoadIndex);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                PreviewOffset(1, CustomCubeLoadIndex);
                LoadPreviewCubes(1 , CustomCubeLoadIndex);
            }

           
            Hints[0].SetActive(BuildControl.BuildMode && !BuildControl.DeleteMode && BuildControl.PlayMode == BuildControl.PlayModes.VitualCube);
            Hints[1].SetActive(BuildControl.BuildMode && BuildControl.DeleteMode);
            Hints[2].SetActive(BuildControl.BuildMode && BuildControl.PlayMode == BuildControl.PlayModes.VitualCube);
            Hints[3].SetActive(BuildControl.BuildMode && BuildControl.PlayMode == BuildControl.PlayModes.VitualCube);
            Hints[4].SetActive(BuildControl.BuildMode && !BuildControl.DeleteMode);
            Hints[5].SetActive(BuildControl.BuildMode && !BuildControl.DeleteMode);
            Hints[6].SetActive(BuildControl.BuildMode && BuildControl.PlayMode == BuildControl.PlayModes.TapOrHold);
            Hints[7].SetActive(BuildControl.BuildMode && BuildControl.PlayMode == BuildControl.PlayModes.TapOrHold);
            StateLabel[0].SetActive(BuildControl.BuildMode && !BuildControl.DeleteMode && BuildControl.PlayMode == BuildControl.PlayModes.VitualCube);
            StateLabel[1].SetActive(BuildControl.BuildMode && BuildControl.DeleteMode && BuildControl.PlayMode == BuildControl.PlayModes.VitualCube);
            StateLabel[2].SetActive(BuildControl.BuildMode);
            if (PreviewRoot.activeSelf != (BuildControl.BuildMode && !BuildControl.DeleteMode))
            {
                if (BuildControl.BuildMode) isOpen = true;
                if (!BuildControl.BuildMode) isOpen = false;
                PreviewRoot.SetActive(BuildControl.BuildMode && !BuildControl.DeleteMode);
            }

                if (PreviewRoot.activeSelf)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (PreviewCubes.Count > i && PreviewCubes[i] != null)
                    {
                        PreviewCubes[i].transform.localPosition = Vector3.Lerp(PreviewCubes[i].transform.localPosition, new Vector3(-1.4F + i * 1.4F, i == 1 ? 0F : 0.25F, i == 1 ? 0F : 1F),Time.deltaTime*10F);
                    }
                }
            }
        }

        void LoadPreviewCubes(int _offset=0 , int CustomType = 0)
        {
            ClearPreviewCubes();
            PreviewOffset(-1 , CustomCubeLoadIndex);
            PreviewOffset(-1 , CustomCubeLoadIndex);
            for (int i = 0; i < 3; i++)
            {
                PreviewOffset(1, CustomCubeLoadIndex);
                BlockInfo _info = BlockPool.instance.CreateCustomBlock(BlockPool.BlockType.Custom, previewHeight, PreviewModelRoot, 0, Random.Range(0, 100), Vector3.zero, previewUid, CustomType);
                Debug.Log(_info.gameObject.name + PreviewRoot.name);
                _info.transform.SetParent(PreviewModelRoot);

                int _posID = i + _offset;
                if (_posID > 2)
                {
                    _posID = 0;
                }
                else if (_posID < 0)
                {
                    _posID = 2;
                }
                _info.transform.localPosition = new Vector3(-1.4F+ _posID * 1.4F, _posID == 1?0F:0.25F, _posID == 1?0F:1F);
                _info.transform.localScale = Vector3.one * (i == 1 ? 1F : 0.75F);
                _info.transform.localEulerAngles = new Vector3(-45F, -26F, 18F);
                _info.gameObject.layer = 5;
                _info.gameObject.SetActive(true);
                PreviewCubes.Add(_info.gameObject);
            }
            PreviewOffset(-1 , CustomCubeLoadIndex);
        }

        void PreviewOffset(int _offset , int CustomType)
        {
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/switch"), Camera.main.transform.position);
            if (viewCustomCubes)
            {
                previewUid += _offset;
                if (previewUid > BlockPool.instance.mCustomCubePrefabs.Count-1)
                {
                    previewUid = 0;
                    previewType = 0;
                    previewHeight = 0;
                    viewCustomCubes = false;
                }
                else if (previewUid <0)
                {
                    previewUid = 0;
                    previewType = 3;
                    previewHeight = 4;
                    viewCustomCubes = false;
                }
            }
            else
            {
                previewUid = 0;
                if (previewHeight + _offset > 4)
                {
                    if (previewType < 3)
                    {
                        previewType++;
                    }
                    else
                    {
                        previewType = 4;
                        viewCustomCubes = true;
                        previewUid = 0;
                    }
                    previewHeight = 0;
                }
                else if (previewHeight + _offset < 0)
                {
                    if (previewType > 0)
                    {
                        previewType--;
                    }
                    else
                    {
                        previewType = 4;
                        viewCustomCubes = true;
                        previewUid = BlockPool.instance.mCustomCubePrefabs.Count-1;
                    }
                    previewHeight = 4;
                }
                else
                {
                    previewHeight += _offset;
                }
            }
        }

        void ClearPreviewCubes()
        {
            foreach (GameObject obj in PreviewCubes) {
                if(obj)Destroy(obj);
            }
            PreviewCubes.Clear();
        }
    }
}
