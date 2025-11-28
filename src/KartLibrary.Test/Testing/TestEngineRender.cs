using System.Diagnostics;
using System.Numerics;
using System.Text;
using KartLibrary.File;
using KartLibrary.Game.Engine.Track;
using KartLibrary.IO;
using eP.Command;
using ImGuiNET;
using KartCity.Common.IO.SmartStream;
using KartLibrary.Engine.Relements;
using KartLibrary.Engine.Render;
using KartLibrary.Engine.Render.Renderables;
using KartLibrary.Game.Engine;
using KartLibrary.Game.Engine.Render;
using KartLibrary.Game.Engine.Render.Store;
using KartLibrary.Game.Engine.Tontrollers;
using KartLibrary.Record;
using KartLibrary.Tests.Engine;
using KartRiderLibrary.Tests;
using SixLabors.ImageSharp;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Vortice.Direct3D11;

namespace KartLibrary.Tests.Testing;

public class TestEngineRender: TestKartStorageSystem
{
    private bool _trackListInitialized = false;
    private Dictionary<string, string> _trackList = new Dictionary<string, string>();
    private TestingWindow _testingWindow;
    
    public TestEngineRender()
    {
        KartObjectManager.Initialize();
    }

    [Command("testRenderLogo", "")]
    private CommandExecuteResult testRenderLogo(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        string logoPath = "zeta/cn/logo/intro";
        string logoFile = "bgIntro.1s";
        KartStorageFile? logoModelFile = StorageSystem.GetFile($"{logoPath}/{logoFile}");
        if (logoModelFile is null)
        {
            return new CommandExecuteResult(ResultType.Failure, "");
        }

        BinaryReader logoFileReader = new BinaryReader(logoModelFile.CreateStream());
        Relement logoRelement = logoFileReader.ReadKartObject<Relement>(new(BufferMode.ForRead));

        _testingWindow?.Dispose();
        
        _testingWindow = new TestingWindow();
        _testingWindow.SceneContext.SceneCamera.CameraPosition = new Vector3(117.06405f, 537.35504f, 26.849836f);
        _testingWindow.SceneContext.SceneCamera.CameraDirection = new Vector3(0, -1, 0);
        _testingWindow.SceneContext.SceneCamera.CameraUp = new Vector3(0, 0, 1);
        _testingWindow.SceneContext.SceneCamera.FieldOfView = 87.468068157908946871103409086704f / 180f * MathF.PI;
        _testingWindow.SceneContext.SceneCamera.Far = 1000;
        _testingWindow.SceneContext.SceneCamera.Near = 1f;
        
        _testingWindow.CreateWindow();
        
        CommandList initCommandList = _testingWindow.GraphicsDevice.ResourceFactory.CreateCommandList();
        DeviceObjectCache objectCache = new DeviceObjectCache();
        initCommandList.Begin();

        KartTextureResourceStore textureResourceStore = new KartTextureResourceStore(StorageSystem, _testingWindow.GraphicsDevice);
        textureResourceStore.LoadGeneralResource(logoPath);

        objectCache.PushTextureResourceStore(textureResourceStore);

        initCommandList.End();
        _testingWindow.GraphicsDevice.SubmitCommands(initCommandList);
        initCommandList.Dispose();

        _testingWindow.BaseScene.AddRelement(logoRelement, objectCache);
        _testingWindow.InteractiveCamera.IsEnabled = false;

        
        _testingWindow.SceneContext.SceneCamera.CameraPosition = new Vector3(0, -593.755f, 0);
        _testingWindow.SceneContext.SceneCamera.CameraDirection = new Vector3(0, -1, 0);
        _testingWindow.SceneContext.SceneCamera.FieldOfView = 161.7574f/ 2 * MathF.PI / 180;
        _testingWindow.SceneContext.SceneCamera.Far = 1000000;
        _testingWindow.SceneContext.SceneCamera.Near = 31;
        while (_testingWindow.WindowExists)
        {
            _testingWindow.Update();
        }
        
        return new CommandExecuteResult(ResultType.Success, "");
    }
    
    [Command("testRender", "")]
    private CommandExecuteResult testRender(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        string trackName = "god_R03";
        if (argumentQueue.Count > 0)
            trackName = argumentQueue.PopArgumentString();
        
        string trackTheme = "fengshen";
        string kartName = "cottonV1";
        string characterName = "bazzi";
        string[] themeNames = [trackTheme, "common", "sword", "god", "world"];
        // zeta\cn\ppl\ingame\fengshen\village
        string[] pplNames = ["fengshen"];
        
        KartStorageFile? trackModelFile = StorageSystem.GetFile($"track/{trackName}/track.1s") ?? 
                                          StorageSystem.GetFile($"track_/{trackName}/track.1s");
        KartStorageFile? skyDomeFile = StorageSystem.GetFile($"track/{trackName}/skydome.1s") ??
                                       StorageSystem.GetFile($"track_/{trackName}/skydome.1s");
        KartStorageFile? testKartFile = StorageSystem.GetFile($"kart_/{kartName}/model.1s");
        KartStorageFile? testKsvFile = StorageSystem.GetFolder($"track_/simulationGame/ksvTrackList/{trackName}")?.Files.OrderBy(x => Random.Shared.Next()).First();
        if (trackModelFile is null)
        {
            return new CommandExecuteResult(ResultType.Failure, "");
        }
        if (skyDomeFile is null)
        {
            return new CommandExecuteResult(ResultType.Failure, "");
        }
        if (testKartFile is null)
        {
            return new CommandExecuteResult(ResultType.Failure, "");
        }

        BinaryReader trackFileReader = new BinaryReader(trackModelFile.CreateStream());
        TrackContainer trackContainer = trackFileReader.ReadKartObject<TrackContainer>(new(BufferMode.ForRead));

        BinaryReader skyboxFileReader = new BinaryReader(skyDomeFile.CreateStream());
        Relement skyboxRelement = skyboxFileReader.ReadKartObject<Relement>(new(BufferMode.ForRead));

        BinaryReader kartFileReader = new BinaryReader(testKartFile.CreateStream());
        Relement kartRelement = kartFileReader.ReadKartObject<Relement>(new(BufferMode.ForRead));
        
        KSVInfo? ksvInfo = null;
        if (testKsvFile is not null)
        {
            BinaryReader ksvFileReader = new BinaryReader(testKsvFile.CreateStream());
            int ssLength = ksvFileReader.ReadInt32();
            ksvFileReader = new BinaryReader(ksvFileReader.ReadSmartStream(ssLength));
            ksvInfo = ksvFileReader.ReadKSVInfo();
        }
        _testingWindow?.Dispose();
        
        _testingWindow = new TestingWindow();
        _testingWindow.SceneContext.SceneCamera.CameraPosition = new Vector3(117.06405f, 537.35504f, 26.849836f);
        _testingWindow.SceneContext.SceneCamera.CameraDirection = new Vector3(0, -1, 0);
        _testingWindow.SceneContext.SceneCamera.CameraUp = new Vector3(0, 0, 1);
        _testingWindow.SceneContext.SceneCamera.FieldOfView = 87.468068157908946871103409086704f / 180f * MathF.PI;
        _testingWindow.SceneContext.SceneCamera.Far = 1000;
        _testingWindow.SceneContext.SceneCamera.Near = 1f;
        
        _testingWindow.CreateWindow();
        
        CommandList initCommandList = _testingWindow.GraphicsDevice.ResourceFactory.CreateCommandList();
        DeviceObjectCache _trackObjectCache = new DeviceObjectCache();
        DeviceObjectCache _kartObjectCache = new DeviceObjectCache();
        initCommandList.Begin();

        KartTextureResourceStore textureResourceStore = new KartTextureResourceStore(StorageSystem, _testingWindow.GraphicsDevice);
        textureResourceStore.LoadTrackResource(trackName);

        _trackObjectCache.PushTextureResourceStore(textureResourceStore);
        // KartStorageFolder? trackFolder = StorageSystem.GetFolder($"track/{trackName}") ?? StorageSystem.GetFolder($"track_/{trackName}");
        // foreach (KartStorageFile textureFile in trackFolder?.Files ?? [])
        // {
        //     string textureName = textureFile.NameWithoutExt;
        //     if (textureFile.Name.EndsWith(".png") || textureFile.Name.EndsWith(".dds") ||
        //         textureFile.Name.EndsWith(".tga"))
        //     {
        //         bool isDDs = textureFile.Name.ToLower().EndsWith(".dds");
        //         Texture createdTexture = TextureUtility.CreateTexture(_testingWindow.GraphicsDevice, initCommandList,
        //             textureFile.CreateStream(), isDDs);
        //         if (!_trackObjectCache.ContainsTexture(textureName))
        //             _trackObjectCache.AddTexture(textureName, createdTexture);
        //     }
        // }
        //
        // foreach (var themeName in themeNames)
        // {
        //     KartStorageFolder? themeFolder = StorageSystem.GetFolder($"theme/{themeName}/texture");
        //     foreach (KartStorageFile textureFile in themeFolder?.Files ?? [])
        //     {
        //         if (textureFile.Name.EndsWith(".png") || textureFile.Name.EndsWith(".dds") ||
        //             textureFile.Name.EndsWith(".tga"))
        //         {
        //             string textureName = textureFile.NameWithoutExt;
        //             bool isDDs = textureFile.Name.ToLower().EndsWith(".dds");
        //             Texture createdTexture = TextureUtility.CreateTexture(_testingWindow.GraphicsDevice, initCommandList, textureFile.CreateStream(), isDDs);
        //             if(!_trackObjectCache.ContainsTexture(textureName))
        //                 _trackObjectCache.AddTexture(textureName, createdTexture);    
        //         }
        //     }
        // }
        //
        // foreach (var pplName in pplNames)
        // {
        //     KartStorageFolder? themeFolder = StorageSystem.GetFolder($"zeta/cn/ppl/ingame/{pplName}/village");
        //     foreach (KartStorageFile textureFile in themeFolder?.Files ?? [])
        //     {
        //         string textureName = textureFile.NameWithoutExt;
        //         bool isDDs = textureFile.Name.ToLower().EndsWith(".dds");
        //         Texture createdTexture = TextureUtility.CreateTexture(_testingWindow.GraphicsDevice, initCommandList, textureFile.CreateStream(), isDDs);
        //         if(!_trackObjectCache.ContainsTexture(textureName))
        //             _trackObjectCache.AddTexture(textureName, createdTexture);
        //     }
        // }

        KartStorageFolder? kartFolder = StorageSystem.GetFolder($"kart/{kartName}") ?? StorageSystem.GetFolder($"kart_/{kartName}");
        foreach (KartStorageFile textureFile in kartFolder?.Files ?? [])
        {
            string textureName = textureFile.NameWithoutExt;
            if (textureFile.Name.EndsWith(".png") || textureFile.Name.EndsWith(".dds") ||
                textureFile.Name.EndsWith(".tga"))
            {
                bool isDDs = textureFile.Name.ToLower().EndsWith(".dds");
                Texture createdTexture = TextureUtility.CreateTexture(_testingWindow.GraphicsDevice, initCommandList,
                    textureFile.CreateStream(), isDDs);
                if (!_kartObjectCache.ContainsTexture(textureName))
                    _kartObjectCache.AddTexture(textureName, createdTexture);
            }
        }
        initCommandList.End();
        _testingWindow.GraphicsDevice.SubmitCommands(initCommandList);
        initCommandList.Dispose();

        DeviceObjectCache rectRenderableCache = new DeviceObjectCache();
        foreach (var item in trackContainer.TrackObjects)
        {
            if (item is ToRoad toRoad)
            {
                foreach (var item2 in toRoad.Unknown1)
                {
                    Vector4 redColor = new Vector4(0.75f, 0.25f, 0.0f, 0.3f);
                    Vector4 greenColor = new Vector4(0.0f, 1.0f, 0.0f, 0.5f);
                    Vector4 writeColor = new Vector4(1.0f, 1.0f, 1.0f, 0.3f);
                    Vector4 blueColor = new Vector4(0.0f, 0.0f, 1.0f, 0.5f);
                    Vector4 rectColor = item2.u1.ToLower() == "start" ? redColor : greenColor;
                    RectRenderable rectRenderable = new RectRenderable(
                        item2.u2[0],
                        item2.u2[1],
                        item2.u2[2],
                        item2.u2[3],
                        rectColor
                    );
                    // _testingWindow.BaseScene.AddRenderable(rectRenderable, rectRenderableCache);    
                    foreach (var item3 in item2.u2)
                    {
                        BallRenderable ballRenderable = new BallRenderable();
                        ballRenderable.UpdateModelMat(
                            Matrix4x4.CreateScale(1.9f) * Matrix4x4.CreateTranslation(item3)
                        );
                        // renderables.Add(ballRenderable);    
                    }
                }
            }
            else if (item is ToMovableObject movableObject)
            {
                trackContainer.TrackScene[0].Add(movableObject.Unknown5);
            }
        }
        
        _testingWindow.BaseScene.SetWorld(trackContainer.TrackScene, _trackObjectCache);
        _testingWindow.BaseScene.SetSkybox(skyboxRelement, _trackObjectCache);
        _testingWindow.BaseScene.AddRelement(kartRelement, _kartObjectCache);

        float startTime = _testingWindow.SceneContext.TimeSource.GetTimeStamp();
        var recordData = ksvInfo?.Records.First();

        _testingWindow.InteractiveCamera.IsEnabled = false;

        float delayTime = 400f;
        float fromTime = -1;
        float toTime = -1;
        Quaternion fromQuaternion = Quaternion.Identity;
        Quaternion toQuaternion = Quaternion.Identity;

        kartRelement.VisTontroller = new VisTontroller();
        kartRelement.VisTontroller.BoolKeyData = new NoEasingBoolKeyData()
        {
            new NoEasingBoolKeyframe()
            {
                Time = 0,
                Value = false
            },
            new NoEasingBoolKeyframe()
            {
                Time = 0,
                Value = false
            }
        };
        Relement? wheelRelement = kartRelement.FirstOrDefault(x => x.Name.Equals("wheel0"));
        if (wheelRelement is not null)
        {
            wheelRelement.PRSTontroller = new PRSTontroller();
            wheelRelement.PRSTontroller.RotateKeyData = new ThreeAxisRotateKeyData()
            {
                XAxisKeyData = new LinearFloatKeyData()
                {
                    new ()
                    {
                        Time = 0,
                        Value = 0
                    },
                    new ()
                    {
                        Time = 1000,
                        Value = 360
                    }
                }
            };
            wheelRelement.PRSTontroller._loopCount = 0;
            wheelRelement.PRSTontroller.RotateBeginTime = 0;
            wheelRelement.PRSTontroller.RotateEndTime = 100;
        }
        
        
        
        while (_testingWindow.WindowExists)
        {
            float time = (_testingWindow.SceneContext.TimeSource.GetTimeStamp());
            float t = fromTime == toTime ? 1 : Math.Clamp((time - fromTime) / (toTime - fromTime), 0, 1);
            Quaternion curQuaternion = Quaternion.Slerp(fromQuaternion, toQuaternion, t);
            var stamp = recordData?[time];
            
            Vector3 kartPos = new Vector3(stamp?.X ?? 0, stamp?.Y ?? 0, stamp?.Z ?? 0);
            Vector3 kartDir = Vector3.Transform(Vector3.UnitY, stamp?.Angle ?? Quaternion.Identity);
            Vector3 camDir = Vector3.Transform(Vector3.UnitY, curQuaternion);
            Vector3 kartUp = Vector3.Transform(Vector3.UnitZ, stamp?.Angle ?? Quaternion.Identity);
            Vector3 camUp = Vector3.Transform(Vector3.UnitZ, curQuaternion);
            kartRelement.Position = kartPos;
            kartRelement.Rotation = Matrix4x4.CreateFromQuaternion(stamp?.Angle ?? Quaternion.Identity);
            kartRelement.Scale = new Vector3(1);

            
            if (time != fromTime && testKsvFile is not null)
            {
                fromQuaternion = curQuaternion;
                toQuaternion = stamp?.Angle ?? Quaternion.Identity;
                fromTime = time;
                toTime = fromTime + delayTime;
            }
            
            if (!_testingWindow.InteractiveCamera.IsEnabled && testKsvFile is not null)
            {
                // Vector3 dir = Vector3.Transform(new Vector3(0, 1f, 1f) * 20f, delayStamp.Angle);
                // _testingWindow.SceneContext.SceneCamera.CameraPosition = kartPos + dir;
                // _testingWindow.SceneContext.SceneCamera.CameraDirection = dir;
                // _testingWindow.SceneContext.SceneCamera.CameraUp = kartDelayUp;

                _testingWindow.SceneContext.SceneCamera.CameraPosition = kartPos + camUp * 4.5f + camDir * 8f;
                _testingWindow.SceneContext.SceneCamera.CameraDirection = camDir;
                _testingWindow.SceneContext.SceneCamera.CameraUp = camUp;
                if (wheelRelement != null) wheelRelement.Rotation = Matrix4x4.CreateFromQuaternion(stamp?.Angle ?? Quaternion.Identity);
                if (wheelRelement != null) wheelRelement.Scale = new Vector3(3);
            }
            _testingWindow.Update();
        }
        
        return new CommandExecuteResult(ResultType.Success, "");
    }
    
    [Command("dumptrack", "Dump  track. dumptrack <track_name>")]
    private CommandExecuteResult commandDumpTrack(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        initTrackList();
        string trackName = argumentQueue.PopArgumentString();
        if(!_trackList.TryGetValue(trackName, out var trackPath))
            return new CommandExecuteResult(ResultType.Failure, $"Cannot found track: {trackName}.");
        KartStorageFile? trackModelFile = StorageSystem.GetFile($"{trackPath}/track.1s");
        if(trackModelFile is null)
            return new CommandExecuteResult(ResultType.Failure, $"Cannot load track: {trackName}.");
        using (Stream stream = trackModelFile.CreateStream())
        {
            BinaryReader modelReader = new BinaryReader(stream);
            TrackContainer trackContainer =
                modelReader.ReadKartObject<TrackContainer>(new(BufferMode.ForRead));
            string relementStr = "<?xml version='1.0' encoding='UTF-16'?>\r\n" + trackContainer.TrackScene.ToString();
            if (!Directory.Exists("tracks_info"))
                Directory.CreateDirectory("tracks_info");
            using (FileStream outStream =
                   new FileStream(Path.Combine("tracks_info", $"{trackName}.xml"), FileMode.Create))
            {
                outStream.Write(Encoding.Unicode.GetBytes(relementStr));
            }
        }
        return new CommandExecuteResult(ResultType.Success, "");
    }

    [Command("listtrack", "List all track")]
    private CommandExecuteResult commandListTrack(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        initTrackList();
        foreach(string trackName in _trackList.Keys)
            commandConsole.WriteLine($"{trackName}");
        return new CommandExecuteResult(ResultType.Success, "");
    }
    
    private void initTrackList()
    {
        if (_trackListInitialized)
            return;
        if (StorageSystem is null)
            throw new Exception();
        KartStorageFolder? oldTrackFolder = StorageSystem.GetFolder("track");
        KartStorageFolder? newTrackFolder = StorageSystem.GetFolder("track_");
        if (oldTrackFolder is not null)
        {
            foreach (KartStorageFolder childFolder in oldTrackFolder.Folders)
            {
                if (childFolder.Name.ToLower() != "common")
                {
                    _trackList.Add(childFolder.Name, childFolder.FullName);
                }
            }
        }

        if (newTrackFolder is not null)
        {
            foreach (KartStorageFolder childFolder in newTrackFolder.Folders)
            {
                if (childFolder.Name.ToLower() != "common")
                {
                    _trackList.Add(childFolder.Name, childFolder.FullName);
                }
            }
        }

        _trackListInitialized = true;
    }

    [Command("testLoadAll")]
    private CommandExecuteResult CommandTestLoadAllTrack(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        KartStorageFolder? trackRootFolder = StorageSystem.GetFolder($"track");
        KartStorageFolder? track5RootFolder = StorageSystem.GetFolder($"track_");
        if (trackRootFolder is not null)
        {
            foreach (var trackFolder in trackRootFolder.Folders)
            {
                KartStorageFile? trackFile = trackFolder.GetFile("track.1s");
                if (trackFile is not null)
                {
                    try
                    {
                        using var trackFileStream = trackFile.CreateStream();
                        BinaryReader trackFileReader = new BinaryReader(trackFileStream);
                        TrackContainer trackContainer =
                            trackFileReader.ReadKartObject<TrackContainer>(new KartObjectBuffer(BufferMode.ForRead));
                        
                        if(trackContainer.u1.Length > 0)
                            commandConsole.WriteLine($"{trackFile.Name}: {trackContainer.u1}");

                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        if (track5RootFolder is not null)
        {
            foreach (var trackFolder in track5RootFolder.Folders)
            {
                KartStorageFile? trackFile = trackFolder.GetFile("track.1s");
                if (trackFile is not null)
                {
                    try
                    {
                        using var trackFileStream = trackFile.CreateStream();
                        BinaryReader trackFileReader = new BinaryReader(trackFileStream);
                        TrackContainer trackContainer =
                            trackFileReader.ReadKartObject<TrackContainer>(new KartObjectBuffer(BufferMode.ForRead));
                            
                        if(trackContainer.u1.Length > 0)
                            commandConsole.WriteLine($"{trackFile.Name}: {trackContainer.u1}");

                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        return new CommandExecuteResult(ResultType.Success, "");
    }
    
}