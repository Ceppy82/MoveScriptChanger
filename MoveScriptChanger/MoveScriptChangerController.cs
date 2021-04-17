//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
//using IPA.Logging;
using IPA.Loader;
//using IPA.Utilities;
//using BS_Utils.Gameplay;
//using SongDataCore.BeatStar;
//using System.Data;
using System.IO;
using HarmonyLib;


namespace MoveScriptChanger
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class MoveScriptChangerController : MonoBehaviour
    {
        //Beginn initiating variables for MSC
        private string mapKey = "key not provided";
        private string mapHash = "mapHash not provided";
        private string newMapHash = "mapHash not provided";
        private string moveScriptChangerPath = Directory.GetCurrentDirectory() + @"\UserData\MoveScriptChanger";
        private int poolSize;
        private int oldPickedScript;
        private int pickedMoveScript;
        private bool cam2;
        private string cam2File = Directory.GetCurrentDirectory() + @"\UserData\Camera2\MovementScripts\changedByMSC.json";
        private string newMoveScript;

        //End initiating Variables for MSC
        
        public static MoveScriptChangerController Instance { get; private set; }
       
        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            if (Instance != null)
            {
                Logger.log?.Warn($"Instance of {this.GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            Instance = this;

            var cam2check = PluginManager.GetPlugin("Camera2");
            if (cam2check != null)
            {
                cam2 = true;
            }
            else
            {
                cam2 = false;
            }


            //Begin MSC checking folders and files
            if (!Directory.Exists(moveScriptChangerPath))
            {
                CreateMoveScriptChangerFiles();
            }
            //End MSC checking folders and files
                        
            ScanMoveScriptPool(); //MSC scan Pool
       

            BS_Utils.Utilities.BSEvents.gameSceneActive -= ChangeMovescript;
            BS_Utils.Utilities.BSEvents.gameSceneActive += ChangeMovescript;

        }
        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        /// 

        private void Start()
        {
        }

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        private void Update()
        {
        }

        /// <summary>
        /// Called every frame after every other enabled script's Update().
        /// </summary>
        private void LateUpdate()
        {


        }

        /// <summary>
        /// Called when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {
        }

        /// <summary>
        /// Called when the script becomes disabled or when it is being destroyed.
        /// </summary>
        private void OnDisable()
        {
            BS_Utils.Utilities.BSEvents.gameSceneActive -= ChangeMovescript;
        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Logger.log?.Debug($"{name}: OnDestroy()");
            Instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.
            BS_Utils.Utilities.BSEvents.gameSceneActive -= ChangeMovescript;
        }
        private void ChangeMovescript()
        {
            Logger.log?.Debug($"{name}: ChangeMovescript");

            LevelSelected();

            if (cam2)
            {
                File.Copy(newMoveScript, Directory.GetCurrentDirectory() + @"\UserData\Camera2\MovementScripts\changedByMSC.json", true);
                if (File.Exists(cam2File + ".cameraPlusFormat")) File.Delete(cam2File + ".cameraPlusFormat");
                AccessTools.TypeByName("Camera2.Managers.MovementScriptManager")?.GetMethod("LoadMovementScripts").Invoke(null, new object[] { true });
            }
            else
            {
                File.Copy(newMoveScript, moveScriptChangerPath + @"\changedByMSC.json", true);
            }
        }


        private void LevelSelected()
        {
            string levelID = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.level.levelID;
            if (newMapHash != levelID.Replace("custom_level_", "").ToLower())
            {
                newMapHash = levelID.Replace("custom_level_", "").ToLower();
            }

            if (mapHash != newMapHash)
            {
                if (SongDataCore.Plugin.Songs.IsDataAvailable() && SongDataCore.Plugin.Songs.Data.Songs.ContainsKey(newMapHash))
                {
                    mapKey = SongDataCore.Plugin.Songs.Data.Songs[newMapHash].key;
                }
                else
                {
                    mapKey = "notProvided";
                }
                mapHash = newMapHash;

                SelectMoveScript(); //MSC calling "choose between random or defined MoveScript"
            }



        }

        private void SelectMoveScript()
        {
            if (Directory.GetFiles(moveScriptChangerPath + @"\Pool", "*" + "keyForMSC*" + mapKey + "*.json", SearchOption.AllDirectories).Length > 0)
            {
                NonRandomScript();
            }
            else
            {
                RandomScript();

            }
        }

        private void CreateMoveScriptChangerFiles()
        {
            Directory.CreateDirectory(moveScriptChangerPath + @"\Pool\Random");
            File.WriteAllText(moveScriptChangerPath + @"\changedByMSC.json", Resource.changedByMSC);
            if (cam2) File.WriteAllText(Directory.GetCurrentDirectory() + @"\UserData\Camera2\MovementScripts" + @"\changedByMSC.json", Resource.changedByMSC);
            File.WriteAllText(moveScriptChangerPath + @"\Pool\Lindsey_Stirling___Crystallize_keyForMSC_9336.json", Resource.Lindsey_Stirling___Crystallize_keyForMSC_9336);
            File.WriteAllText(moveScriptChangerPath + @"\Pool\Random\MoveScript1.json", Resource.MoveScript1);
            File.WriteAllText(moveScriptChangerPath + @"\Pool\Random\MoveScript2.json", Resource.MoveScript2);
        }

        private void ScanMoveScriptPool()
        {
            poolSize = Directory.GetFiles(moveScriptChangerPath + @"\Pool\Random", "*.json", SearchOption.AllDirectories).Length;
            if (poolSize == 0) CreateMoveScriptChangerFiles(); //MSC if no MoveScript found, create some
        }
       
        public void RandomScript()
        {
            oldPickedScript = pickedMoveScript;
            if (poolSize > 2)
            {
                while (oldPickedScript == pickedMoveScript) //MSC repeat till another MoveScript is selected
                {
                    var random = new System.Random();
                    int randomnumber = random.Next(poolSize - 1);
                    pickedMoveScript = randomnumber;
                }
            }
            else if(poolSize == 2 && oldPickedScript == 0)
            {
                pickedMoveScript = 1;                
            }
            else
            {
                pickedMoveScript = 0;
            }

            if (pickedMoveScript >= 0 & pickedMoveScript < poolSize) //MSC change MoveScript only if MoveScripts available
            {
                
                string[] filePaths = Directory.GetFiles(moveScriptChangerPath + @"\Pool\Random");
                newMoveScript = filePaths[pickedMoveScript];
            }
        }
    
        public void NonRandomScript()
        {
            string[] filePath = Directory.GetFiles(moveScriptChangerPath + @"\Pool", "*" + "keyForMSC*" + mapKey + "*.json", SearchOption.AllDirectories);
            int poolSize = filePath.Length;
            if (poolSize > 1)
            {
                //MSC more than 1 MoveScripts available for this map
                var random = new System.Random();
                int randomnumber = random.Next(poolSize);
                newMoveScript = filePath[randomnumber];

            }
            else
            {
                //MSC only 1 MoveScript available for this map
                newMoveScript = filePath[0];
            }
        }

        #endregion
    }


}

