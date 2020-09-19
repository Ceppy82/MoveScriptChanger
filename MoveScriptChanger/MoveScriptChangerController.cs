using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using IPA.Logging;
using BS_Utils.Gameplay;
using SongDataCore.BeatStar;
using System.Data;
using System.IO;


namespace MoveScriptChanger
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class MoveScriptChangerController : MonoBehaviour
    {
        private string mapKey = "key not provided";
        private string mapHash = "mapID not provided";
        private string newMapHash = "mapID not provided";
        private string moveScriptChangerPath = Directory.GetCurrentDirectory() + @"\UserData\MoveScriptChanger";
        private int poolSize;
        private int oldPickedScript;
        private int pickedMoveScript;
        private string pickedMoveScriptName;

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
            Logger.log?.Debug($"{name}: Awake()");
        }
        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {
            CheckMoveScriptChangerFolder();

        }

        private void ScanMoveScriptPool()
        {
                

            

            poolSize = Directory.GetFiles(moveScriptChangerPath + @"\Pool\Random", "*.json", SearchOption.AllDirectories).Length;
           

        }

        private void RandomScript()
        {

            oldPickedScript = pickedMoveScript;

            while (oldPickedScript == pickedMoveScript)
            {
                var random = new System.Random();
                int randomnumber = random.Next(poolSize);
                pickedMoveScript = randomnumber;
            }



            string[] filePaths = Directory.GetFiles((moveScriptChangerPath + @"\Pool\Random"));
            //filePaths.ToList().ForEach(i => Console.WriteLine(i.ToString()));
            pickedMoveScriptName = filePaths[pickedMoveScript];
            System.IO.File.Copy(pickedMoveScriptName, moveScriptChangerPath + @"\changedByMSC.json", true);
        }

        private void NonRandomScript()
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
            BS_Utils.Utilities.BSEvents.levelSelected += LevelSelected;
            
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
                SelectMoveScript();
                
            }


        }
        
        private void CreateMoveScriptChangerFolder()
        {
            
            Directory.CreateDirectory(moveScriptChangerPath + @"\Pool\Random");
            

        }

        private void CheckMoveScriptChangerFolder()

        {
            if(!Directory.Exists(moveScriptChangerPath))
            {

                CreateMoveScriptChangerFolder();
            }
            ScanMoveScriptPool();
        }




        private void SelectMoveScript()
        {

            RandomScript();


        }
        private void LevelSelected(LevelCollectionViewController arg1, IPreviewBeatmapLevel arg2)
        {
                        
                if (newMapHash != arg2.levelID.Replace("custom_level_", "").ToLower())
            
            {
                newMapHash = arg2.levelID.Replace("custom_level_", "").ToLower();
            }

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

        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Logger.log?.Debug($"{name}: OnDestroy()");
            Instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.

        }
        #endregion
    }
}
