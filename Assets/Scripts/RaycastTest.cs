using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    public int rayCount = 10000;
    public bool useJob;

    private NativeArray<RaycastCommand> commands;
    private NativeArray<RaycastHit> results;


    private void Awake()
    {
        // Q: 왜 매 루프마다 dispose하는 Allocator.TempJob을 사용하지 않는가?
        // A: 큰 native container 할당하는 작업이 오래 걸림.
        commands = new NativeArray<RaycastCommand>(rayCount, Allocator.Persistent);
        results = new NativeArray<RaycastHit>(rayCount, Allocator.Persistent); // results의 크기는 RaycastCommand 최대 충돌 허용 횟수 * RaycastCommand 갯수와 같다.
    }

    private void OnDestroy()
    {
        commands.Dispose();
        results.Dispose();
    }

    private void FixedUpdate()
    {
        if (useJob)
        {
            QueryBuildJob queryBuildJob = new QueryBuildJob
            {
                origin = transform.position,
                rayCount = rayCount,
                commands = commands
            };

            // 난 innerLoopBatchCount가 정확히 어떻게 동작하는지는 모름
            JobHandle handle = queryBuildJob.Schedule(rayCount, rayCount / 8);
            handle = RaycastCommand.ScheduleBatch(commands, results, rayCount / 8, handle);

            handle.Complete();

            // do it
        }
        else
        {
            for (int i = 0; i < rayCount; i++)
            {
                float rotAngle = i / (float)rayCount * 360;
                Quaternion rot = Quaternion.AngleAxis(rotAngle, Vector3.forward);
                Vector3 dir = rot * Vector3.right;

                if (Physics.Raycast(transform.position, dir, out RaycastHit result, float.MaxValue))
                {
                    // do it
                }
            }
        }
    }
}

[BurstCompile]
public struct QueryBuildJob : IJobParallelFor
{
    [ReadOnly]
    public float3 origin;

    [ReadOnly]
    public int rayCount;

    [WriteOnly]
    public NativeArray<RaycastCommand> commands;


    public void Execute(int index)
    {
        float rotAngle = index / (float)rayCount * math.PI * 2;
        quaternion rotation = quaternion.AxisAngle(Vector3.forward, rotAngle);
        commands[index] = new RaycastCommand(origin, math.mul(rotation, Vector3.right));
    }
}