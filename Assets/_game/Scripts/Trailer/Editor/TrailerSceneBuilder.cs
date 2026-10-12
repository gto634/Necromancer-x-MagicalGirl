using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;
using UnityEngine.UI;

[Serializable]
public class TrailerShotDefinition
{
    public string Name, ScenePath;
    public float Duration = 12f;
    public Vector3 CameraStart, CameraEnd, TargetStart, TargetEnd;
    public Vector3 MGStart, MGEnd, NECStart, NECEnd;
    public float FovStart = 45f, FovEnd = 45f, Orbit;
    public TrailerActionKind Action;
    public bool Heroes, Army;
}

public static class TrailerSceneBuilder
{
    const string Folder = "Assets/_game/Scenes/Trailer";
    const string Home = Folder + "/Trailer_Prison.unity";
    const string Dungeon = Folder + "/Trailer_Dungeon.unity";
    const string Arena = Folder + "/Trailer_Arena.unity";
    public static TrailerShotDefinition[] Shots => CreateShots();
    static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);

    static TrailerShotDefinition Shot(string name, string scene, Vector3 from, Vector3 to, Vector3 target, float fov = 45f)
        => new TrailerShotDefinition { Name = name, ScenePath = scene, CameraStart = from, CameraEnd = to,
            TargetStart = target, TargetEnd = target, FovStart = fov, FovEnd = fov,
            MGStart = V(4, 0, 0), MGEnd = V(4, 0, 0), NECStart = V(-4, 0, 0), NECEnd = V(-4, 0, 0) };

    static TrailerShotDefinition[] CreateShots()
    {
        var list = new List<TrailerShotDefinition>();
        list.Add(Shot("Prison_Revelation", Home, V(.2f,1.1f,-10),V(.2f,.7f,-8),V(.3f,-.2f,-.7f),48));
        list.Add(Shot("Necromancer_Portrait", Home,V(-1.9f,.7f,-4.7f),V(.2f,.5f,-4.6f),V(-.43f,-.15f,-.43f),37));
        list.Add(Shot("MagicalGirl_Portrait", Home,V(2.7f,.65f,-5.6f),V(.7f,.45f,-5.3f),V(1.02f,-.1f,-1.3f),37));
        list.Add(Shot("Prison_Duo", Home,V(-.8f,1.3f,-9),V(1.2f,1.1f,-8),V(.3f,-.1f,-.8f),43));
        list.Add(Shot("Donjon_Cellules",Dungeon,V(-12,9,52),V(0,8,50),V(-2,0,42),48));
        list.Add(Shot("Donjon_Couloir",Dungeon,V(-12,7,30),V(-8,7,30),V(-9,.4f,21),54));
        list.Add(Shot("Donjon_Armurerie",Dungeon,V(-9,13,-4),V(-21,12,-8),V(-16,-.4f,-16),52));
        list.Add(Shot("Donjon_Grimoire",Dungeon,V(-27.1f,3.8f,-8.8f),V(-29.7f,3.2f,-9.7f),V(-28.4f,.5f,-11.6f),38));
        list.Add(Shot("Donjon_Salle_Combat",Dungeon,V(3,10,-22),V(8,9,-18),V(5.6f,0,-14),50));
        var walk = Shot("Duo_Exploration",Dungeon,V(-10,8,32),V(-9,8,29),V(-9,.3f,25),49);
        walk.Heroes=true; walk.MGStart=V(-7.5f,-1.15f,27); walk.MGEnd=V(-7.5f,-1.15f,24); walk.NECStart=V(-10.5f,-1.15f,27); walk.NECEnd=V(-10.5f,-1.15f,24);
        walk.TargetStart=V(-9,.2f,27); walk.TargetEnd=V(-9,.2f,24); list.Add(walk);
        var wide=Shot("Arene_Grue",Arena,V(0,24,-15),V(0,13,-9),V(0,.5f,0),54); wide.Heroes=true; list.Add(wide);
        var army=Shot("Armees_Face_A_Face",Arena,V(-8,8,-8),V(7,8,-7),V(0,1,0),54); army.Heroes=true; army.Army=true; list.Add(army);
        var mg1=Shot("Coeurs_Salve",Arena,V(4,7,-6),V(2,6,-5),V(0,1,0),54);
        mg1.Heroes=true; mg1.Action=TrailerActionKind.MGVolley; mg1.MGStart=mg1.MGEnd=V(-4,0,0); mg1.NECStart=mg1.NECEnd=V(9,0,7); list.Add(mg1);
        var mg2=Shot("Coeurs_Rotation",Arena,V(0,10,-8),V(3,9,-6),V(0,1,0),56);
        mg2.Heroes=true; mg2.Action=TrailerActionKind.MGOrbit; mg2.MGStart=V(-2,0,0); mg2.MGEnd=V(2,0,0); mg2.NECStart=mg2.NECEnd=V(9,0,7); list.Add(mg2);
        var nec1=Shot("Necromancer_Boomerang",Arena,V(2,6,-7),V(4,5,-5),V(0,1,0),54);
        nec1.Heroes=true; nec1.Action=TrailerActionKind.NECBoomerang; nec1.NECStart=nec1.NECEnd=V(-4,0,0); nec1.MGStart=nec1.MGEnd=V(9,0,7); list.Add(nec1);
        var nec2=Shot("Necromancer_Teleportation",Arena,V(0,8,-8),V(3,7,-6),V(0,1,0),55);
        nec2.Heroes=true; nec2.Action=TrailerActionKind.NECTeleport; nec2.NECStart=V(-5,0,0); nec2.NECEnd=V(4,0,0); nec2.MGStart=nec2.MGEnd=V(9,0,7); list.Add(nec2);
        var action=Shot("Duo_Competences",Arena,V(0,14,-8),V(0,11,-6),V(0,1,0),58);
        action.Heroes=true; action.Action=TrailerActionKind.CooperativeVolley; action.MGStart=action.MGEnd=V(-4,0,-2); action.NECStart=action.NECEnd=V(-4,0,3); list.Add(action);
        var end=Shot("Arene_Orbite_Finale",Arena,V(0,11,-10),V(0,11,-10),V(0,1,0),54);
        end.Heroes=true; end.Army=true; end.Orbit=65f; list.Add(end);
        list.Add(Shot("Logo_Final",Home,V(.2f,1.1f,-10),V(.2f,.8f,-9),V(.3f,-.2f,-.7f),48));
        return list.ToArray();
    }

    [MenuItem("Tools/Trailer/1 - Prepare capture scenes")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Leave Play before preparing capture scenes.");
        Directory.CreateDirectory(TrailerCaptureEditor.Work);
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        MakeScene("Assets/_game/Scenes/Final/HomeScreen.unity",Home,false);
        MakeScene("Assets/_game/Scenes/Final/Levels/Level1/Level1.unity",Dungeon,true);
        MakeScene("Assets/_game/Scenes/Final/Levels/Level2/Level2.unity",Arena,true);
        for (int i=0;i<Shots.Length;i++)
        {
            ConfigureShot(i);
            var director=UnityEngine.Object.FindFirstObjectByType<TrailerShotDirector>();
            director.ResetShot();
            foreach (Animator animator in Components<Animator>(SceneManager.GetActiveScene())) if (animator.isActiveAndEnabled) animator.Update(0f);
            PositionCamera(director, Shots[i]);
            TrailerCaptureEditor.Capture(Camera.main,"preview-"+(i+1).ToString("00")+"-"+Shots[i].Name+".png");
        }
        ConfigureShot(10);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        File.WriteAllText(TrailerCaptureEditor.Work+"/shots.csv", "Index;Name;Seconds;Scene\n" + string.Join("\n",Shots.Select((s,i)=>(i+1)+";"+s.Name+";"+s.Duration+";"+s.ScenePath)));
        File.WriteAllText(TrailerCaptureEditor.Work+"/built.txt",DateTime.Now.ToString("O"));
        AssetDatabase.SaveAssets();
    }

    static IEnumerable<T> Components<T>(Scene scene) where T:Component
        => scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<T>(true));

    static void MakeScene(string source,string target,bool spawnHeroes)
    {
        Scene original=EditorSceneManager.OpenScene(source,OpenSceneMode.Single);
        if (!EditorSceneManager.SaveScene(original,target,true)) throw new IOException("Cannot create "+target);
        Scene scene=EditorSceneManager.OpenScene(target,OpenSceneMode.Single);
        // Only these dedicated copies are stripped; source levels and prefabs remain untouched.
        foreach (Canvas canvas in Components<Canvas>(scene).ToArray()) UnityEngine.Object.DestroyImmediate(canvas.gameObject);
        foreach (MonoBehaviour script in Components<MonoBehaviour>(scene).OrderBy(s=>s!=null && (s.GetType().Name=="Variables" || s.GetType().Name=="DialogActivator")?1:0).ToArray())
        {
            if (script == null) continue;
            string ns=script.GetType().Namespace??"";
            if (script.GetType().Assembly.GetName().Name=="Assembly-CSharp" && script.GetType().Name!="LightFlicker"
                || ns.StartsWith("Unity.VisualScripting") || ns.StartsWith("Unity.Cinemachine")
                || ns.StartsWith("ALTUSHKA.EasyNodeDialogSystem")
                || script is UnityEngine.InputSystem.PlayerInputManager || script is UnityEngine.InputSystem.PlayerInput)
                UnityEngine.Object.DestroyImmediate(script);
        }
        foreach (Camera camera in Components<Camera>(scene).ToArray()) if(camera!=null)UnityEngine.Object.DestroyImmediate(camera.gameObject);
        foreach (AudioListener listener in Components<AudioListener>(scene).ToArray()) UnityEngine.Object.DestroyImmediate(listener);
        foreach (PlayableDirector timeline in Components<PlayableDirector>(scene).ToArray()) UnityEngine.Object.DestroyImmediate(timeline);
        foreach (NavMeshAgent agent in Components<NavMeshAgent>(scene)) agent.enabled=false;
        foreach (Collider collider in Components<Collider>(scene)) if (collider.isTrigger) collider.enabled=false;
        if(target==Home)
        {
            foreach(Renderer renderer in Components<Renderer>(scene))
                if(Mathf.Abs(renderer.bounds.center.x)<1f && renderer.bounds.center.z<0 && renderer.bounds.size.y>4 && renderer.bounds.size.x<2)
                    renderer.enabled=false;
            AddFillLight(V(-2,2,-4),V(0,-.4f,-1),new Color(.65f,.76f,1),1.1f);
            AddFillLight(V(3,2,-4),V(1,-.4f,-1),new Color(1,.7f,.82f),.9f);
            CreateLogo();
        }
        if (spawnHeroes)
        {
            CreateActor("MG","Assets/_game/Prefabs/Players/Coop/MG/MG.prefab");
            CreateActor("NEC","Assets/_game/Prefabs/Players/Coop/NEC/NEC.prefab");
            if(target==Arena) CreateArmy();
        }
        var cameraObject=new GameObject("Trailer Main Camera");
        cameraObject.tag="MainCamera";
        var main=cameraObject.AddComponent<Camera>();
        main.nearClipPlane=.08f; main.farClipPlane=180f; main.useOcclusionCulling=false; main.allowHDR=true;
        main.clearFlags=CameraClearFlags.Skybox;
        var urp=cameraObject.AddComponent<UniversalAdditionalCameraData>(); urp.renderPostProcessing=true;
        urp.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        var brain=cameraObject.AddComponent<CinemachineBrain>();
        brain.DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut,0);
        var virtualObject=new GameObject("Trailer Cinemachine Travelling");
        var vcam=virtualObject.AddComponent<CinemachineCamera>(); vcam.Priority=100;
        var director=new GameObject("Trailer Shot Director").AddComponent<TrailerShotDirector>(); director.Camera=vcam;
        EditorSceneManager.SaveScene(scene,target);
    }

    static void CreateLogo()
    {
        Sprite sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_game/Art/Ui/ToonKit_v2/title_necromancer_magical_girl.png");
        if(sprite==null)throw new InvalidOperationException("The game logo sprite is missing.");
        var canvas=new GameObject("Trailer Logo",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
        canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.GetComponent<Canvas>().sortingOrder=100;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);
        var image=new GameObject("Game logo",typeof(RectTransform),typeof(Image));image.transform.SetParent(canvas.transform,false);
        var rect=image.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.85f);rect.sizeDelta=new Vector2(1480,220);
        image.GetComponent<Image>().sprite=sprite;image.GetComponent<Image>().preserveAspect=true;image.GetComponent<Image>().raycastTarget=false;
        canvas.SetActive(false);
    }

    static void AddFillLight(Vector3 position,Vector3 target,Color color,float intensity)
    {
        var light=new GameObject("Trailer portrait fill").AddComponent<Light>();
        light.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position));
        light.type=LightType.Spot;light.color=color;light.intensity=intensity;light.range=18;light.spotAngle=75;
        light.shadows=LightShadows.None;
    }

    static void CreateActor(string name,string prefab)
    {
        GameObject actor=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefab));
        PrefabUtility.UnpackPrefabInstance(actor,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        actor.name="Trailer Actor "+name;
        var removed=new HashSet<string>{"PlayerCameraSetup","MagicalGirlBase","NecromantianBase","Respawn","PlayerCommander","CommandMode","PlayerDeathFeedback","SharedPlayerCamera","LockCameraY"};
        foreach(MonoBehaviour component in actor.GetComponentsInChildren<MonoBehaviour>(true).ToArray())
        {
            if(component==null)continue;
            string type=component.GetType().Name;
            string ns=component.GetType().Namespace??"";
            if(type=="Brain_MG" || type=="Brain_NEC") component.enabled=false;
            else if(removed.Contains(type) || ns.StartsWith("Unity.Cinemachine") || component is UnityEngine.InputSystem.PlayerInput)
                UnityEngine.Object.DestroyImmediate(component);
        }
        foreach(Camera camera in actor.GetComponentsInChildren<Camera>(true)) if(camera!=null)UnityEngine.Object.DestroyImmediate(camera.gameObject);
        foreach(AudioListener listener in actor.GetComponentsInChildren<AudioListener>(true)) UnityEngine.Object.DestroyImmediate(listener);
        foreach(Rigidbody body in actor.GetComponentsInChildren<Rigidbody>(true)){body.isKinematic=true;body.useGravity=false;}
        foreach(KnockbackSystem knockback in actor.GetComponentsInChildren<KnockbackSystem>(true)) knockback.enabled=false;
        foreach(LookSystem look in actor.GetComponentsInChildren<LookSystem>(true)){look.UseMouseAim=false;look.LookDirection=Vector2.zero;}
        var attack2=actor.GetComponentInChildren<Attack2_MG>(true);
        var feedback=actor.GetComponentInChildren<Feedback_MG>(true);
        if(attack2!=null && feedback!=null)
        {
            var source=new SerializedObject(attack2);var output=new SerializedObject(feedback);
            output.FindProperty("_shootSystem2Follow").objectReferenceValue=source.FindProperty("_shootSystemFollow").objectReferenceValue;
            output.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    static void CreateArmy()
    {
        var army=new GameObject("Trailer Armies");
        string[] paths={"Assets/_game/Prefabs/Enemy/necromancerAllyModel/AllyNEC.prefab","Assets/_game/Prefabs/Enemy/MAgicalGirlAllyModel/AllyMG.prefab"};
        for(int side=0;side<2;side++)for(int i=0;i<6;i++)
        {
            GameObject recruit=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(paths[side]));
            PrefabUtility.UnpackPrefabInstance(recruit,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            recruit.transform.SetParent(army.transform,true);
            recruit.name=(side==0?"NEC":"MG")+" recruit "+(i+1);
            foreach(MonoBehaviour script in recruit.GetComponentsInChildren<MonoBehaviour>(true).OrderBy(s=>s!=null && s.GetType().Name=="Variables"?1:0).ToArray()) if(script!=null)UnityEngine.Object.DestroyImmediate(script);
            foreach(NavMeshAgent agent in recruit.GetComponentsInChildren<NavMeshAgent>(true))agent.enabled=false;
            foreach(Collider collider in recruit.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            foreach(Rigidbody body in recruit.GetComponentsInChildren<Rigidbody>(true)){body.isKinematic=true;body.useGravity=false;}
            recruit.transform.position=V(side==0?-5:5,0,(i%3-1)*2.2f+(i/3)*.8f);
            recruit.transform.rotation=Quaternion.Euler(0,side==0?90:-90,0);
        }
    }

    public static void ConfigureShot(int index)
    {
        TrailerShotDefinition shot=Shots[index];
        if(SceneManager.GetActiveScene().path!=shot.ScenePath) EditorSceneManager.OpenScene(shot.ScenePath,OpenSceneMode.Single);
        var director=UnityEngine.Object.FindFirstObjectByType<TrailerShotDirector>();
        if(director==null)throw new InvalidOperationException("Prepare trailer scenes first.");
        director.ShotName=shot.Name; director.Duration=shot.Duration;
        director.CameraStart=shot.CameraStart;director.CameraEnd=shot.CameraEnd;
        director.TargetStart=shot.TargetStart;director.TargetEnd=shot.TargetEnd;
        director.FovStart=shot.FovStart;director.FovEnd=shot.FovEnd;
        director.CameraOrbitDegrees=shot.Orbit;director.CameraOrbitCenter=shot.TargetStart;
        director.ActionKind=shot.Action;director.CameraFollowSubject=false;
        director.MGStart=shot.MGStart;director.MGEnd=shot.MGEnd;
        director.NECStart=shot.NECStart;director.NECEnd=shot.NECEnd;
        director.MG=Components<Transform>(SceneManager.GetActiveScene()).FirstOrDefault(t=>t.name=="Trailer Actor MG");
        director.NEC=Components<Transform>(SceneManager.GetActiveScene()).FirstOrDefault(t=>t.name=="Trailer Actor NEC");
        bool onlyMG=shot.Action==TrailerActionKind.MGVolley || shot.Action==TrailerActionKind.MGOrbit;
        bool onlyNEC=shot.Action==TrailerActionKind.NECBoomerang || shot.Action==TrailerActionKind.NECTeleport;
        if(director.MG!=null){director.MG.gameObject.SetActive(shot.Heroes && !onlyNEC);director.MG.rotation=Quaternion.Euler(0,shot.ScenePath==Dungeon?180:270,0);}
        if(director.NEC!=null){director.NEC.gameObject.SetActive(shot.Heroes && !onlyMG);director.NEC.rotation=Quaternion.Euler(0,shot.ScenePath==Dungeon?180:90,0);}
        director.Army=Components<Transform>(SceneManager.GetActiveScene()).FirstOrDefault(t=>t.name=="Trailer Armies");
        if(director.Army!=null)director.Army.gameObject.SetActive(shot.Army);
        director.ArmyStart=Vector3.zero;director.ArmyEnd=Vector3.zero;
        if(shot.ScenePath==Home)
        {
            Transform rawMG=Components<Transform>(SceneManager.GetActiveScene()).FirstOrDefault(t=>t.parent==null && t.name=="MG");
            Transform rawNEC=Components<Transform>(SceneManager.GetActiveScene()).FirstOrDefault(t=>t.parent==null && t.name=="NEC");
            if(rawMG!=null)rawMG.gameObject.SetActive(shot.Name!="Necromancer_Portrait");
            if(rawNEC!=null)rawNEC.gameObject.SetActive(shot.Name!="MagicalGirl_Portrait");
            Transform logo=Components<Transform>(SceneManager.GetActiveScene()).FirstOrDefault(t=>t.name=="Trailer Logo");
            if(logo!=null)
            {
                var canvas=logo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=Camera.main;canvas.planeDistance=1f;
                logo.gameObject.SetActive(shot.Name=="Logo_Final");
            }
        }
        PositionCamera(director,shot);
    }

    static void PositionCamera(TrailerShotDirector director,TrailerShotDefinition shot)
    {
        director.Camera.transform.SetPositionAndRotation(shot.CameraStart,Quaternion.LookRotation(shot.TargetStart-shot.CameraStart));
        var lens=director.Camera.Lens;lens.FieldOfView=shot.FovStart;director.Camera.Lens=lens;
        Camera.main.transform.SetPositionAndRotation(shot.CameraStart,director.Camera.transform.rotation);Camera.main.fieldOfView=shot.FovStart;
    }
}
