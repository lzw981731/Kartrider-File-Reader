using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using eP.Command;
using eP.Testing;
using KartCity.Common.Engine;
using KartLibrary.Game.Engine;
using KartLibrary.Game.Engine.Tontrollers;
using KartLibrary.Record;

using V128d = System.Runtime.Intrinsics.Vector128<float>;

namespace KartLibrary.Tests.Testing;

public class TestPerformance: TestStage
{
    [Command("testMatrix")]
    private CommandExecuteResult commandTestMatrix(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        Matrix4x4 testMatrix = new Matrix4x4();
        DateTime dt = DateTime.Now;
        for (int i = 0; i < 30000000; i++)
        {
            Matrix4x4.CreateFromQuaternion(new Quaternion(0.35f, 9.2f, 39.8f, 82.1f));
        }
        TimeSpan ts = DateTime.Now - dt;
        commandConsole.WriteLine($"{ts.TotalMilliseconds:0.000} ms");
        return new CommandExecuteResult(ResultType.Success, "");
    }

    [Command("testTon")]
    private CommandExecuteResult commandTestTon(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        FloatTontroller testTontroller = new FloatTontroller();
        PRSTontroller testTontroller2 = new PRSTontroller();
        testTontroller2.PositionKeyData = new LinearVector3KeyData();
        testTontroller2.ScaleKeyData = new LinearVector3KeyData();
        testTontroller2.RotateKeyData = new LinearRotateKeyData();
        testTontroller.KeyData = new CubicFloatKeyData();
        for (int i = 0; i < 1200; i++)
        {
            ((CubicFloatKeyData)testTontroller.KeyData).Add(new CubicFloatKeyframe()
            {
                Time = i * 50,
                Value = 0.35f + (i * 0.353f),
                RightSlop = -0.36f,
                LeftSlop = 0.58f
            });
            ((LinearVector3KeyData)testTontroller2.PositionKeyData).Add(new LinearVector3Keyframe()
            {
                Time = i * 50,
                Value = new Vector3(0.35f + (i * 0.353f))
            });
            ((LinearVector3KeyData)testTontroller2.ScaleKeyData).Add(new LinearVector3Keyframe()
            {
                Time = i * 50,
                Value = new Vector3(0.35f + (i * 0.353f))
            });
            ((LinearRotateKeyData)testTontroller2.RotateKeyData).Add(new LinearRotateKeyframeData()
            {
                Time = i * 50,
                Value = new Quaternion(0.35f + (i * 0.353f), 0.35f + (i * 0.353f), 0.35f + (i * 0.353f), 0.35f + (i * 0.353f))
            });
        }
        DateTime dt = DateTime.Now;
        float acet = 1;
        for (int i = 0; i < 30000000; i++)
        {
            float time = ((i + 93851) * 405659) % (1200 * 50);
            acet = acet * testTontroller.GetValue(time);
        }
        TimeSpan ts = DateTime.Now - dt;
        commandConsole.WriteLine($"FloatTontroller: {ts.TotalMilliseconds:0.000} ms");
        
        dt = DateTime.Now;
        for (int i = 0; i < 30000000; i++)
        {
            float time = ((i + 93851) * 405659) % (1200 * 50);
            testTontroller2.GetPosition(time);
            testTontroller2.GetRotation(time);
            testTontroller2.GetScale(time);
        }
        ts = DateTime.Now - dt;
        commandConsole.WriteLine($"FloatTontroller: {ts.TotalMilliseconds:0.000} ms");
        
        return new CommandExecuteResult(ResultType.Success, "");
    }

    [Command("testDD")]
    private CommandExecuteResult commandTestDD(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        BoundingBox boundingBox = new BoundingBox()
        {
            MinPosition = new Vector3(0, 1, 2),
            MaxPosition = new Vector3(3, 4, 5),
        };
        Vortice.Mathematics.BoundingBox bb = new Vortice.Mathematics.BoundingBox();
        Matrix4x4 viewMat = Matrix4x4.Identity;
        DateTime dt = DateTime.Now;
        float acet = 1;
        for (int i = 0; i < 30000000; i++)
        {
            boundingBox.GetMinDistance(new Vector3(3, 9, 2), ref viewMat);
        }
        TimeSpan ts = DateTime.Now - dt;
        commandConsole.WriteLine($"FloatTontroller: {ts.TotalMilliseconds:0.000} ms");
        return new CommandExecuteResult(ResultType.Success, "");
    }
}