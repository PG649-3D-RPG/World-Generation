using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


public class FastHeightmapTransformation {
    public float[,] orgHeights { get; }

    private NativeArray<float> heightsFlat0, heightsFlat1, currentInputArr, currentOutputArr;
    private readonly int side_length;
    private readonly bool useGPU;
    private readonly Dictionary<Mask, NativeArray<int>> MaskWorkItemCache;

    public void Destroy() {
        heightsFlat0.Dispose();
        heightsFlat1.Dispose();
        foreach (var arr in MaskWorkItemCache.Values) arr.Dispose();
    }

    private NativeArray<int> AddToMaskCache(Mask mask, bool invertMask) {
        var workItemList = new List<int>();
        // compute work items via mask
        //TODO safety bezel is only valid for AverageFIlter!!!
        for (int i = 1; i < side_length - 1; i++) {
            for (int j = 1; j < side_length - 1; j++) {
                if (mask == null || (mask[i, j] && !invertMask) || (invertMask && !mask[i, j])) workItemList.Add(i * side_length + j);
            }
        }
        NativeArray<int> workItemsArr = new NativeArray<int>(workItemList.Count, Allocator.TempJob);
        for (int i = 0; i < workItemList.Count; i++) {
            workItemsArr[i] = workItemList[i];
        }
        // MaskWorkItemCache.Add(mask, workItemsArr);
        MaskWorkItemCache[mask] = workItemsArr;
        return workItemsArr;
    }

    public float[,] GetHeightsArray() {
        float[,] output = new float[side_length, side_length];
        // expand result
        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                // get the data from the correct buffer
                output[i, j] = currentOutputArr[i * (side_length) + j];
            }
        }
        return output;
    }

    public FastHeightmapTransformation(float[,] heights) {
        side_length = heights.GetLength(0);
        heightsFlat0 = new NativeArray<float>(side_length * side_length, Allocator.TempJob);
        heightsFlat1 = new NativeArray<float>(side_length * side_length, Allocator.TempJob);
        orgHeights = heights;
        useGPU = SystemInfo.supportsComputeShaders && SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null && !Application.isBatchMode;
        currentInputArr = heightsFlat0;
        currentOutputArr = heightsFlat1;
        MaskWorkItemCache = new Dictionary<Mask, NativeArray<int>>();

        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                heightsFlat0[i * side_length + j] = heights[i, j];
                heightsFlat1[i * side_length + j] = heights[i, j];
            }
        }
    }


    public void AverageFilter3x3(Mask mask = null, int passes = 1, bool invertMask = false, bool add = true, bool multiplyFilter = false) {
        // System.Diagnostics.Stopwatch sw = new();
        // sw.Restart();
        if (passes == 0) return;
        NativeArray<int> workItems;
        if (MaskWorkItemCache.ContainsKey(mask)) workItems = MaskWorkItemCache[mask];
        else workItems = AddToMaskCache(mask, invertMask);

        // if (useGPU)
        //     AverageFilter3x3GPU(workItems, passes);
        // else
        AverageFilter3x3CPU(workItems, passes);
        // sw.Stop();
        // Debug.Log("Runtime AverageFilter FHT:\t " + sw.Elapsed);
        // sw.Reset();
    }

    private void AverageFilter3x3CPU(NativeArray<int> workItemsArr, int passes) {
        JobHandle jobHandle;
        TerrainShaderCPU.AverageFilterJob filterJob1 = new TerrainShaderCPU.AverageFilterJob() {
            _output = currentOutputArr,
            _input = currentInputArr,
            _work_items = workItemsArr,
            _size_input = side_length,
            _work_item_count = workItemsArr.Length
        };
        TerrainShaderCPU.AverageFilterJob filterJob2 = new TerrainShaderCPU.AverageFilterJob() {
            _output = currentInputArr,
            _input = currentOutputArr,
            _work_items = workItemsArr,
            _size_input = side_length,
            _work_item_count = workItemsArr.Length
        };


        // only process workitem indices -> multithread that work array
        for (int n = 0; n < passes; n++) {
            if (n % 2 == 0) {
                jobHandle = filterJob1.Schedule(workItemsArr.Length, 64);
                jobHandle.Complete();
            } else {
                jobHandle = filterJob2.Schedule(workItemsArr.Length, 64);
                jobHandle.Complete();
            }
        }
        // update current 
        if (passes % 2 == 0) {//TODO check if this is correct
            (currentOutputArr, currentInputArr) = (currentInputArr, currentOutputArr);
        }
        // else {
        //     var tmp = currentInputArr;
        //     currentInputArr = heightsFlat0;
        //     currentOutputArr = heightsFlat1;
        // }
        // workItemsArr.Dispose();
    }
    private void AverageFilter3x3GPU(List<int> workItems, int passes) {
    }

    public void PerlinNoise(float maxAddedHeight = 1, float scale = .1f, Mask mask = null, bool invertMask = false, int fractalRuns = 1) {
        // System.Diagnostics.Stopwatch sw = new();
        // sw.Restart();
        if (fractalRuns == 0) return;
        NativeArray<int> workItemsArr;
        if (MaskWorkItemCache.ContainsKey(mask)) workItemsArr = MaskWorkItemCache[mask];
        else workItemsArr = AddToMaskCache(mask, invertMask);


        JobHandle jobHandle;
        TerrainShaderCPU.PerlinNoiseJob pnJob = new TerrainShaderCPU.PerlinNoiseJob() {
            _output = currentOutputArr,
            _input = currentInputArr,
            _work_items = workItemsArr,
            _size_input = side_length,
            _work_item_count = workItemsArr.Length,
            _fractal_runs = fractalRuns,
            _max_added_height = maxAddedHeight,
            _scale = scale
        };
        jobHandle = pnJob.Schedule(workItemsArr.Length, 64);
        jobHandle.Complete();
        (currentOutputArr, currentInputArr) = (currentInputArr, currentOutputArr);


        // sw.Stop();
        // Debug.Log("Runtime PerlinNoise FHT:\t " + sw.Elapsed);
        // sw.Reset();
    }
    public void Power(float power, Mask mask, bool invertMask = false) {
        // System.Diagnostics.Stopwatch sw = new();
        // sw.Restart();
        NativeArray<int> workItemsArr;
        if (MaskWorkItemCache.ContainsKey(mask)) workItemsArr = MaskWorkItemCache[mask];
        else workItemsArr = AddToMaskCache(mask, invertMask);


        JobHandle jobHandle;
        TerrainShaderCPU.PowerJob pnJob = new TerrainShaderCPU.PowerJob() {
            _output = currentOutputArr,
            _input = currentInputArr,
            _work_items = workItemsArr,
            _work_item_count = workItemsArr.Length,
            _power = power
        };
        jobHandle = pnJob.Schedule(workItemsArr.Length, 64);
        jobHandle.Complete();
        (currentOutputArr, currentInputArr) = (currentInputArr, currentOutputArr);


        // sw.Stop();
        // Debug.Log("Runtime Power FHT:\t\t " + sw.Elapsed);
        // sw.Reset();
    }

    public void SetByMask(Mask mask, float trueValue, float falseValue = -1, bool invertMask = false) {
        // System.Diagnostics.Stopwatch sw = new();
        // sw.Restart();
        if (!MaskWorkItemCache.ContainsKey(mask)) AddToMaskCache(mask, invertMask);
        NativeArray<bool> maskArr = new NativeArray<bool>(side_length * side_length, Allocator.TempJob);
        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                maskArr[i * side_length + j] = mask[i, j];
            }
        }

        JobHandle jobHandle;
        TerrainShaderCPU.SetByMaskJob job = new TerrainShaderCPU.SetByMaskJob() {
            _output = currentOutputArr,
            _input = currentInputArr,
            _mask = maskArr,
            _work_item_count = maskArr.Length,
            _false_value = falseValue,
            _true_value = trueValue
        };
        jobHandle = job.Schedule(maskArr.Length, 64);
        jobHandle.Complete();
        (currentOutputArr, currentInputArr) = (currentInputArr, currentOutputArr);


        maskArr.Dispose();
        // sw.Stop();
        // Debug.Log("Runtime SetByMask FHT:\t " + sw.Elapsed);
        // sw.Reset();
    }

}
