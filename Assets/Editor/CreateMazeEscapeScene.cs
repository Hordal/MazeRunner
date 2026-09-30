using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreateMazeEscapeScene
{
    private const int Size = 17;
    private const float Cell = 4f;
    private const float Min = -34f;
    // 게임 카메라에서는 숨기고 지도 카메라에서만 보여 주는 전용 레이어다.
    private const int MapLayer = 30;
    private static Material mapWallMaterial;

    [MenuItem("MazeEscape/Create 3D Floor 1")]
    private static void CreateFloor1() => CreateScene(MazeFloorType.Floor1, 20260930);
    [MenuItem("MazeEscape/Create 3D Floor 2 (Trap)")]
    private static void CreateFloor2() => CreateScene(MazeFloorType.Floor2, 20261001);
    [MenuItem("MazeEscape/Create 3D Floor 3 (Timed)")]
    private static void CreateFloor3() => CreateScene(MazeFloorType.Floor3, 20261002);
    [MenuItem("MazeEscape/Create Lobby Scene")]
    private static void CreateLobby()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject camera = new GameObject("Main Camera"); camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10); Camera cam = camera.AddComponent<Camera>(); cam.backgroundColor = new Color(.02f, .03f, .07f); camera.AddComponent<AudioListener>();
        new GameObject("MazeLobbyMenu").AddComponent<MazeLobbyMenu>();
        const string path = "Assets/Scenes/MazeLobby.unity";
        EditorSceneManager.SaveScene(scene, path);
        EnsureBuildScene(path);
        EditorUtility.DisplayDialog("MazeEscape", "MazeLobby 생성 완료.", "확인");
    }

    [MenuItem("MazeEscape/Create Result Scene")]
    private static void CreateResultScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject camera = new GameObject("Main Camera"); camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10); Camera cam = camera.AddComponent<Camera>(); cam.backgroundColor = new Color(.03f, .05f, .1f); camera.AddComponent<AudioListener>();
        new GameObject("ResultSceneController").AddComponent<ResultSceneController>();
        const string path = "Assets/Scenes/ResultScene.unity";
        EditorSceneManager.SaveScene(scene, path);
        EnsureBuildScene(path);
        EditorUtility.DisplayDialog("MazeEscape", "ResultScene 생성 완료.", "확인");
    }

    private static void CreateScene(MazeFloorType type, int seed)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject managerObject = new GameObject("MazeGameManager");
        MazeGameManager manager = managerObject.AddComponent<MazeGameManager>();
        managerObject.AddComponent<MazeSettingsUI>();
        managerObject.AddComponent<MazeAudio>();
        manager.floorType = type;
        // 2층은 어둠 미로가 아니라 함정 미로다. 세 층 모두 길과 함정을 읽을 수 있게 기본 조명을 유지한다.
        RenderSettings.ambientLight = new Color(0.04f, 0.05f, 0.09f);
        CreateFloor();
        Transform mapDisplay = CreateMapDisplay();
        CreateMaze(seed, mapDisplay);
        GameObject player = CreatePlayer();
        manager.mapPlayerIcon = CreateMapPlayerIcon(player.transform);
        manager.mapCamera = CreateMapCamera();
        List<GameObject> mapKeyIcons = new List<GameObject>();
        List<GameObject> mapItemIcons = new List<GameObject>();

        if (type == MazeFloorType.Floor1)
        {
            List<GameObject> markers = new List<GameObject>();
            CreateKey(2, 14, markers, mapKeyIcons); CreateKey(12, 4, markers, mapKeyIcons); CreateKey(14, 13, markers, mapKeyIcons);
            manager.keyMarkers = markers.ToArray();
            manager.exitMarker = CreateExit(16, 16);
            manager.mapExitIcon = CreateMapIcon("Map Exit", Center(16, 16), new Color(.1f, 1f, .45f), 1.55f, true);
            // 지도 표시를 바로 확인할 수 있도록 시작 칸에 테스트용 상자를 하나 둔다.
            CreateChests(new[] { new Vector2Int(0, 0) }, mapItemIcons, true);
            CreateChests(new[] { new Vector2Int(2, 2), new Vector2Int(5, 10), new Vector2Int(9, 5), new Vector2Int(13, 10), new Vector2Int(8, 14), new Vector2Int(15, 6) }, mapItemIcons);
        }
        else
        {
            manager.exitMarker = CreateExit(16, 16);
            manager.mapExitIcon = CreateMapIcon("Map Exit", Center(16, 16), new Color(.1f, 1f, .45f), 1.55f, true);
            if (type == MazeFloorType.Floor3) CreateKey(8, 8, null, mapKeyIcons);
            CreateChests(type == MazeFloorType.Floor2
                ? new[] { new Vector2Int(4, 6), new Vector2Int(10, 12), new Vector2Int(15, 5) }
                : new[] { new Vector2Int(4, 12), new Vector2Int(10, 5), new Vector2Int(14, 10) }, mapItemIcons);
            if (type == MazeFloorType.Floor2) manager.trapMarkers = CreateFloor2Traps();
        }
        manager.mapKeyIcons = mapKeyIcons.ToArray();
        manager.mapItemIcons = mapItemIcons.ToArray();

        string name = type == MazeFloorType.Floor1 ? "Floor1_3D" : type == MazeFloorType.Floor2 ? "Floor2_3D" : "Floor3_3D";
        if (!AssetDatabase.IsValidFolder("Assets/Scenes/3D")) AssetDatabase.CreateFolder("Assets/Scenes", "3D");
        string path = "Assets/Scenes/3D/" + name + ".unity";
        EditorSceneManager.SaveScene(scene, path);
        EnsureBuildScene(path);
    }

    private static void EnsureBuildScene(string path)
    {
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool alreadyAdded = false;
        for (int i = 0; i < buildScenes.Count; i++)
        {
            if (buildScenes[i].path != path) continue;
            buildScenes[i] = new EditorBuildSettingsScene(path, true);
            alreadyAdded = true;
            break;
        }
        if (!alreadyAdded) buildScenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();
        if (path.Contains("Floor")) EditorUtility.DisplayDialog("MazeEscape", System.IO.Path.GetFileNameWithoutExtension(path) + " 생성 완료. Game 탭에서 Play를 누르세요.", "확인");
    }

    private static void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor"; floor.transform.position = new Vector3(0, -0.5f, 0); floor.transform.localScale = new Vector3(72f, 1, 72f);
        floor.GetComponent<Renderer>().sharedMaterial = MakeMaterial(new Color(0.07f, 0.09f, 0.12f));
    }

    private static void CreateMaze(int seed, Transform mapDisplay)
    {
        bool[,] horizontal = new bool[Size, Size + 1]; bool[,] vertical = new bool[Size + 1, Size]; bool[,] visited = new bool[Size, Size];
        for (int x = 0; x < Size; x++) for (int y = 0; y <= Size; y++) horizontal[x, y] = true;
        for (int x = 0; x <= Size; x++) for (int y = 0; y < Size; y++) vertical[x, y] = true;
        Random.InitState(seed); Stack<Vector2Int> stack = new Stack<Vector2Int>(); stack.Push(Vector2Int.zero); visited[0, 0] = true;
        Vector2Int[] dirs = { Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };
        while (stack.Count > 0)
        {
            Vector2Int cur = stack.Peek(); List<Vector2Int> options = new List<Vector2Int>();
            foreach (Vector2Int dir in dirs) { Vector2Int next = cur + dir; if (next.x >= 0 && next.x < Size && next.y >= 0 && next.y < Size && !visited[next.x, next.y]) options.Add(next); }
            if (options.Count == 0) { stack.Pop(); continue; }
            Vector2Int nextCell = options[Random.Range(0, options.Count)];
            if (nextCell.x < cur.x) vertical[cur.x, cur.y] = false; else if (nextCell.x > cur.x) vertical[cur.x + 1, cur.y] = false; else if (nextCell.y < cur.y) horizontal[cur.x, cur.y] = false; else horizontal[cur.x, cur.y + 1] = false;
            visited[nextCell.x, nextCell.y] = true; stack.Push(nextCell);
        }
        Material wall = MakeMaterial(new Color(0.12f, 0.17f, 0.22f));
        for (int x = 0; x < Size; x++) for (int y = 0; y <= Size; y++) if (horizontal[x, y])
        {
            Vector3 position = new Vector3(Min + (x + .5f) * Cell, 2, Min + y * Cell);
            Wall(position, new Vector3(Cell + .75f, 4, .75f), wall);
            CreateMapWall(mapDisplay, new Vector3(position.x, 10.2f, position.z), new Vector3(Cell + .3f, .12f, .42f));
        }
        for (int x = 0; x <= Size; x++) for (int y = 0; y < Size; y++) if (vertical[x, y])
        {
            Vector3 position = new Vector3(Min + x * Cell, 2, Min + (y + .5f) * Cell);
            Wall(position, new Vector3(.75f, 4, Cell + .75f), wall);
            CreateMapWall(mapDisplay, new Vector3(position.x, 10.2f, position.z), new Vector3(.42f, .12f, Cell + .3f));
        }
    }

    private static void Wall(Vector3 position, Vector3 scale, Material material)
    { GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "Wall"; wall.transform.position = position; wall.transform.localScale = scale; wall.GetComponent<Renderer>().sharedMaterial = material; }

    private static GameObject CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.position = Center(0, 0) + Vector3.up * 1.05f;
        CharacterController cc = player.AddComponent<CharacterController>(); cc.center = Vector3.up; cc.height = 2; cc.radius = .45f; FirstPersonController controller = player.AddComponent<FirstPersonController>();
        GameObject cam = new GameObject("Main Camera"); cam.tag = "MainCamera"; cam.transform.SetParent(player.transform); cam.transform.localPosition = new Vector3(0, 1.6f, 0); cam.AddComponent<Camera>(); cam.AddComponent<AudioListener>(); cam.GetComponent<Camera>().backgroundColor = new Color(.01f, .015f, .03f); cam.GetComponent<Camera>().nearClipPlane = .03f; cam.GetComponent<Camera>().cullingMask &= ~(1 << MapLayer);
        GameObject flashlight = new GameObject("Flashlight"); flashlight.transform.SetParent(cam.transform); flashlight.transform.localPosition = Vector3.zero; Light light = flashlight.AddComponent<Light>(); light.type = LightType.Spot; light.range = 18; light.spotAngle = 55; light.intensity = 6; light.color = new Color(.8f, .9f, 1);
        CreateRemyAvatar(player.transform, controller);
        return player;
    }

    private static void CreateRemyAvatar(Transform player, FirstPersonController controller)
    {
        const string modelPath = "Assets/Art/Characters/Remy/Remy.fbx";
        const string walkingPath = "Assets/Art/Characters/Remy/Remy_Walking.fbx";
        const string runningPath = "Assets/Art/Characters/Remy/Remy_Running.fbx";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(modelPath) == null) return;

        ConfigureRemyImports(modelPath, walkingPath, runningPath);
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        RuntimeAnimatorController animationController = GetOrCreateRemyController(walkingPath, runningPath);
        if (model == null || animationController == null) return;

        GameObject avatar = (GameObject)PrefabUtility.InstantiatePrefab(model);
        avatar.name = "Remy Avatar";
        avatar.transform.SetParent(player, false);
        // Mixamo Remy의 루트는 발이 아니라 골반 높이에 있다.
        // Player의 CharacterController 중심(1.05)에 맞춰야 발이 바닥에 닿는다.
        avatar.transform.localPosition = Vector3.zero;
        avatar.transform.localRotation = Quaternion.identity;
        Animator animator = avatar.GetComponent<Animator>();
        if (animator == null) animator = avatar.AddComponent<Animator>();
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.runtimeAnimatorController = animationController;
        PlayerAvatarVisual visual = avatar.AddComponent<PlayerAvatarVisual>();
        visual.controller = controller;
        visual.animator = animator;
    }

    private static void ConfigureRemyImports(string modelPath, string walkingPath, string runningPath)
    {
        ModelImporter characterImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (characterImporter == null) return;
        const string textureFolder = "Assets/Art/Characters/Remy/Textures";
        if (!AssetDatabase.IsValidFolder(textureFolder)) AssetDatabase.CreateFolder("Assets/Art/Characters/Remy", "Textures");
        // Mixamo FBX에 포함된 Remy 피부·의상 텍스처를 프로젝트 파일로 꺼낸다.
        characterImporter.ExtractTextures(textureFolder);
        characterImporter.materialLocation = ModelImporterMaterialLocation.External;
        if (characterImporter.animationType != ModelImporterAnimationType.Human || characterImporter.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
        {
            characterImporter.animationType = ModelImporterAnimationType.Human;
            characterImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            characterImporter.SaveAndReimport();
        }

        // Mixamo 애니메이션은 각 파일이 자기 뼈대를 가진 Humanoid 클립으로 가져와야
        // Remy/X Bot처럼 서로 다른 모델에도 안전하게 리타게팅할 수 있다.
        ConfigureAnimationImport(walkingPath);
        ConfigureAnimationImport(runningPath);
    }

    private static void ConfigureAnimationImport(string assetPath)
    {
        ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (importer == null) return;
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.sourceAvatar = null;
        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
        for (int i = 0; i < clips.Length; i++) { clips[i].loopTime = true; clips[i].loopPose = true; }
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    private static RuntimeAnimatorController GetOrCreateRemyController(string walkingPath, string runningPath)
    {
        const string controllerPath = "Assets/Art/Characters/Remy/RemyAnimator.controller";
        AnimationClip walk = FindAnimationClip(walkingPath);
        if (walk == null) return null;

        AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (existing != null)
        {
            // X Bot용 달리기 파일은 Remy의 팔·상체 리타게팅이 깨진다.
            // Run 상태도 정상인 걷기 클립으로 통일하고 속도는 런타임에서 높인다.
            foreach (ChildAnimatorState child in existing.layers[0].stateMachine.states)
            {
                if (child.state.name == "Walk" || child.state.name == "Run") child.state.motion = walk;
            }
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            return existing;
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Sprinting", AnimatorControllerParameterType.Bool);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState walkState = machine.AddState("Walk"); walkState.motion = walk;
        AnimatorState runState = machine.AddState("Run"); runState.motion = walk;
        machine.defaultState = walkState;
        AnimatorStateTransition toRun = walkState.AddTransition(runState); toRun.hasExitTime = false; toRun.duration = .12f; toRun.AddCondition(AnimatorConditionMode.If, 0f, "Sprinting");
        AnimatorStateTransition toWalk = runState.AddTransition(walkState); toWalk.hasExitTime = false; toWalk.duration = .12f; toWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sprinting");
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static AnimationClip FindAnimationClip(string assetPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (Object item in assets)
        {
            AnimationClip clip = item as AnimationClip;
            if (clip != null && !clip.name.StartsWith("__preview__")) return clip;
        }
        return null;
    }

    private static Transform CreateMapDisplay()
    {
        GameObject root = new GameObject("Map Display (Map Camera Only)");
        root.layer = MapLayer;
        mapWallMaterial = MakeMaterial(new Color(.07f, .16f, .28f));
        Material background = MakeMaterial(new Color(.77f, .88f, .95f));
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "Bright Map Background";
        board.transform.SetParent(root.transform, false);
        board.transform.position = new Vector3(0f, 10f, 0f);
        board.transform.localScale = new Vector3(70f, .1f, 70f);
        board.GetComponent<Collider>().enabled = false;
        board.GetComponent<Renderer>().sharedMaterial = background;
        SetMapLayer(board);
        return root.transform;
    }

    private static void CreateMapWall(Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Map Wall";
        wall.transform.SetParent(parent, false);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<Collider>().enabled = false;
        wall.GetComponent<Renderer>().sharedMaterial = mapWallMaterial;
        SetMapLayer(wall);
    }

    private static GameObject CreateMapIcon(string name, Vector3 position, Color color, float size, bool isExit = false)
    {
        GameObject icon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        icon.name = name;
        // 지도 배경과 벽보다 충분히 위에 띄워 항상 선명하게 보이게 한다.
        icon.transform.position = new Vector3(position.x, 12f, position.z);
        icon.transform.localScale = new Vector3(size, .18f, size);
        icon.GetComponent<Collider>().enabled = false;
        icon.GetComponent<Renderer>().sharedMaterial = MakeMaterial(color, true);
        if (isExit)
        {
            GameObject cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cross.name = "Exit Cross";
            cross.transform.SetParent(icon.transform, false);
            cross.transform.localPosition = new Vector3(0f, .65f, 0f);
            cross.transform.localScale = new Vector3(.28f, .5f, size * 1.1f);
            cross.GetComponent<Collider>().enabled = false;
            cross.GetComponent<Renderer>().sharedMaterial = MakeMaterial(Color.white, true);
        }
        SetMapLayer(icon);
        icon.SetActive(false);
        return icon;
    }

    private static GameObject CreateMapPlayerIcon(Transform player)
    {
        GameObject icon = CreateMapIcon("Map Player", Vector3.zero, new Color(.02f, 1f, 1f), 2.2f);
        icon.transform.SetParent(player, false);
        icon.transform.localPosition = new Vector3(0f, 10.95f, 0f);
        GameObject direction = GameObject.CreatePrimitive(PrimitiveType.Cube);
        direction.name = "Direction Arrow";
        direction.transform.SetParent(icon.transform, false);
        direction.transform.localPosition = new Vector3(0f, .7f, .75f);
        direction.transform.localScale = new Vector3(.42f, .32f, 1.35f);
        direction.GetComponent<Collider>().enabled = false;
        direction.GetComponent<Renderer>().sharedMaterial = MakeMaterial(Color.white, true);
        SetMapLayer(icon);
        return icon;
    }

    private static void SetMapLayer(GameObject target)
    {
        target.layer = MapLayer;
        foreach (Transform child in target.transform) SetMapLayer(child.gameObject);
    }

    private static Camera CreateMapCamera()
    {
        GameObject map = new GameObject("Map Camera"); map.transform.position = new Vector3(0, 70, 0); map.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera camera = map.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 38; camera.rect = new Rect(.55f, .47f, .43f, .48f); camera.depth = 2; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.77f, .88f, .95f); camera.cullingMask = 1 << MapLayer; return camera;
    }

    private static void CreateKey(int x, int y, List<GameObject> markers, List<GameObject> mapIcons)
    {
        Vector3 pos = ItemSpot(x, y);
        GameObject key = new GameObject("Key");
        key.transform.position = pos + Vector3.up * .65f;
        BoxCollider trigger = key.AddComponent<BoxCollider>(); trigger.isTrigger = true; trigger.size = new Vector3(1.05f, 1.15f, .5f);
        Rigidbody rb = key.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
        key.AddComponent<MazeKeyCollectible>();

        Material gold = MakeMaterial(new Color(1f, .63f, .06f), true);
        GameObject visual = new GameObject("Golden Key Visual"); visual.transform.SetParent(key.transform, false); visual.AddComponent<MazePropAnimation>();
        CreatePart("Ring", visual.transform, new Vector3(0f, .2f, 0f), new Vector3(.52f, .12f, .52f), PrimitiveType.Cylinder, gold);
        CreatePart("Ring center", visual.transform, new Vector3(0f, .27f, 0f), new Vector3(.27f, .03f, .27f), PrimitiveType.Cylinder, MakeMaterial(new Color(.08f, .06f, .02f)));
        CreatePart("Shaft", visual.transform, new Vector3(.52f, .2f, 0f), new Vector3(.7f, .16f, .18f), PrimitiveType.Cube, gold);
        CreatePart("Tooth A", visual.transform, new Vector3(.78f, .2f, .12f), new Vector3(.16f, .16f, .28f), PrimitiveType.Cube, gold);
        CreatePart("Tooth B", visual.transform, new Vector3(.99f, .2f, .12f), new Vector3(.14f, .16f, .22f), PrimitiveType.Cube, gold);
        if (markers != null) markers.Add(CreateMarker("Key Location Marker", pos, new Color(1, .7f, .08f)));
        if (mapIcons != null)
        {
            GameObject icon = CreateMapIcon("Map Key", pos, new Color(1f, .72f, .03f), 1.25f);
            icon.transform.SetParent(key.transform, true);
            mapIcons.Add(icon);
        }
    }

    private static GameObject CreateExit(int x, int y)
    {
        Vector3 pos = Center(x, y);
        GameObject exit = new GameObject("Exit Gate"); exit.transform.position = pos;
        BoxCollider trigger = exit.AddComponent<BoxCollider>(); trigger.isTrigger = true; trigger.center = new Vector3(0f, 1.35f, 0f); trigger.size = new Vector3(2.45f, 2.7f, .7f);
        Rigidbody rb = exit.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false; exit.AddComponent<MazeExit>();

        Material stone = MakeMaterial(new Color(.18f, .22f, .25f));
        Material portal = MakeMaterial(new Color(.08f, 1f, .48f), true);
        CreatePart("Left pillar", exit.transform, new Vector3(-1.18f, 1.35f, 0f), new Vector3(.42f, 2.7f, .6f), PrimitiveType.Cube, stone);
        CreatePart("Right pillar", exit.transform, new Vector3(1.18f, 1.35f, 0f), new Vector3(.42f, 2.7f, .6f), PrimitiveType.Cube, stone);
        CreatePart("Gate arch", exit.transform, new Vector3(0f, 2.55f, 0f), new Vector3(2.75f, .36f, .62f), PrimitiveType.Cube, stone);
        GameObject gateLight = CreatePart("Green portal", exit.transform, new Vector3(0f, 1.3f, .02f), new Vector3(1.85f, 2.25f, .08f), PrimitiveType.Cube, portal);
        MazePropAnimation animation = gateLight.AddComponent<MazePropAnimation>(); animation.rotate = false; animation.bob = true; animation.bobHeight = .06f; animation.bobSpeed = 1.4f;
        return CreateMarker("Exit Location Marker", pos, new Color(.1f, 1, .5f));
    }

    private static GameObject CreateMarker(string name, Vector3 position, Color color)
    { GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder); marker.name = name; marker.transform.position = position + Vector3.up * 25; marker.transform.localScale = new Vector3(.55f, 25, .55f); marker.GetComponent<Collider>().enabled = false; marker.GetComponent<Renderer>().sharedMaterial = MakeMaterial(color, true); marker.SetActive(false); return marker; }

    private static void CreateChests(Vector2Int[] cells, List<GameObject> mapIcons, bool guaranteedMapReveal = false)
    {
        foreach (Vector2Int cell in cells)
        {
            GameObject chest = new GameObject("Mystery Chest"); chest.transform.position = ItemSpot(cell.x, cell.y) + Vector3.up * .55f;
            BoxCollider trigger = chest.AddComponent<BoxCollider>(); trigger.isTrigger = true; trigger.size = new Vector3(1.45f, 1.2f, 1.1f);
            Rigidbody rb = chest.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false; RandomChest chestScript = chest.AddComponent<RandomChest>(); if (guaranteedMapReveal) chestScript.SetGuaranteedMapReveal();
            Material wood = MakeMaterial(new Color(.31f, .12f, .035f)); Material metal = MakeMaterial(new Color(.95f, .58f, .08f), true);
            CreatePart("Chest base", chest.transform, new Vector3(0f, -.05f, 0f), new Vector3(1.25f, .62f, .88f), PrimitiveType.Cube, wood);
            CreatePart("Chest lid", chest.transform, new Vector3(0f, .32f, -.04f), new Vector3(1.27f, .38f, .9f), PrimitiveType.Cube, wood);
            CreatePart("Chest band", chest.transform, new Vector3(0f, .18f, -.49f), new Vector3(.18f, .72f, .06f), PrimitiveType.Cube, metal);
            CreatePart("Chest lock", chest.transform, new Vector3(0f, .03f, -.52f), new Vector3(.26f, .25f, .07f), PrimitiveType.Cube, metal);
            if (mapIcons != null)
            {
                GameObject icon = CreateMapIcon("Map Chest", ItemSpot(cell.x, cell.y), new Color(1f, .3f, .04f), 1.15f);
                icon.transform.SetParent(chest.transform, true);
                mapIcons.Add(icon);
            }
        }
    }

    private static GameObject[] CreateFloor2Traps()
    {
        List<GameObject> markers = new List<GameObject>();
        CreateCollapseTrap(3, 3, false, markers);
        CreateCollapseTrap(6, 7, false, markers);
        CreateCollapseTrap(9, 8, false, markers);
        CreateCollapseTrap(12, 11, false, markers);
        CreateCollapseTrap(14, 14, false, markers);
        CreateCheckpoint(4, 4);
        CreateCheckpoint(8, 9);
        CreateCheckpoint(12, 12);
        return markers.ToArray();
    }

    private static void CreateCollapseTrap(int x, int y, bool unlocksExit, List<GameObject> markers)
    {
        Vector3 pos = Center(x, y);
        GameObject trap = new GameObject(unlocksExit ? "출구 앞 무너지는 바닥" : "무너지는 바닥"); trap.transform.position = pos;
        Material board = MakeMaterial(new Color(.47f, .15f, .035f)); Material crack = MakeMaterial(new Color(1f, .25f, .03f), true);
        for (int i = -2; i <= 2; i++) CreatePart("Cracked board", trap.transform, new Vector3(i * .59f, .035f, 0f), new Vector3(.5f, .08f, 3.1f), PrimitiveType.Cube, board);
        CreatePart("Warning crack", trap.transform, new Vector3(0f, .085f, 0f), new Vector3(.12f, .025f, 2.5f), PrimitiveType.Cube, crack);
        BoxCollider trigger = trap.AddComponent<BoxCollider>(); trigger.isTrigger = true; trigger.size = new Vector3(3.15f, 2.2f, 3.15f); trigger.center = new Vector3(0f, 1.1f, 0f);
        Rigidbody body = trap.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false; trap.AddComponent<CollapseTrap>().SetUnlocksExit(unlocksExit);
        markers.Add(CreateMarker("함정 위치 표시", pos, new Color(1f, .35f, .05f)));
    }

    private static void CreateCheckpoint(int x, int y)
    {
        GameObject checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        checkpoint.name = "체크포인트"; checkpoint.transform.position = Center(x, y) + Vector3.up * .15f; checkpoint.transform.localScale = new Vector3(.75f, .15f, .75f);
        checkpoint.GetComponent<Renderer>().sharedMaterial = MakeMaterial(new Color(.1f, .6f, 1f), true);
        checkpoint.GetComponent<Collider>().isTrigger = true; Rigidbody body = checkpoint.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false; checkpoint.AddComponent<MazeCheckpoint>();
    }

    private static GameObject CreatePart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, PrimitiveType type, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name; part.transform.SetParent(parent, false); part.transform.localPosition = localPosition; part.transform.localScale = localScale;
        Collider collider = part.GetComponent<Collider>(); if (collider != null) Object.DestroyImmediate(collider);
        part.GetComponent<Renderer>().sharedMaterial = material;
        return part;
    }

    private static Vector3 Center(int x, int y) => new Vector3(Min + (x + .5f) * Cell, 0, Min + (y + .5f) * Cell);
    private static Vector3 ItemSpot(int x, int y) => Center(x, y) + new Vector3(x % 2 == 0 ? -.8f : .8f, 0, y % 2 == 0 ? -.8f : .8f);
    private static Material MakeMaterial(Color color, bool emission = false) { Material m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color = color; if (emission) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", color * 2); } return m; }
}
