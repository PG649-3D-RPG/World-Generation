using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


public enum Gauss_SD {
    SD1, SD2, SD3
}

public static class TerrainShaderCPU {
    private const float Avg_3x3 = 1 / (float)9;
    private const float Gauss_3x3_sd1_diag = 0.07511361F;
    private const float Gauss_3x3_sd1_next = 0.1238414F;
    private const float Gauss_3x3_sd1_center = 0.20417996F;

    private const float Gauss_3x3_sd2_diag = 0.10186806F;
    private const float Gauss_3x3_sd2_next = 0.11543164F;
    private const float Gauss_3x3_sd2_center = 0.13080118F;

    private const float Gauss_3x3_sd3_diag = 0.1069973F;
    private const float Gauss_3x3_sd3_next = 0.11310982F;
    private const float Gauss_3x3_sd3_center = 0.11957153F;

    public static void ApplyPerlinNoiseMT(float[,] input, float maxAddedHeight = 1, float scale = .1f, Mask mask = null, bool invertMask = false, int fractalRuns = 1) {
        int dim0 = input.GetLength(0);
        int dim1 = input.GetLength(1);
        Parallel.For(0, dim0 * dim1, id => {
            int row = id / dim0;
            int col = id % dim1;
            if (mask == null || (mask[row, col] && !invertMask) || (invertMask && !mask[row, col])) {
                float amp = maxAddedHeight;
                // float s = scale;
                for (int k = 0; k < fractalRuns; k++) {
                    input[row, col] += Mathf.PerlinNoise(row * scale, col * scale) * amp;
                    amp *= 0.5f;
                    // s *= 2;
                }
            }
        });
    }

    public static void PowerMT(float[,] input, float power, Mask mask = null) {
        int dim0 = input.GetLength(0);
        int dim1 = input.GetLength(1);
        Parallel.For(0, dim0 * dim1, id => {
            int row = id / dim0;
            int col = id % dim1;
            if (mask == null || mask[row, col]) input[row, col] = Mathf.Pow(input[row, col], power);
        });
    }

    private static void GaussianBlurMT3x3Worker(int id, int _size_input, Gauss_SD sd, float[] _input, float[] _output) {
        int row = id / _size_input; // integer division => row
        int col = id % _size_input; // modulo => column

        float sum = 0;
        float diag, next, center;

        switch (sd) {
            case Gauss_SD.SD1:
                diag = Gauss_3x3_sd1_diag;
                next = Gauss_3x3_sd1_next;
                center = Gauss_3x3_sd2_center;
                break;
            case Gauss_SD.SD2:
                diag = Gauss_3x3_sd2_diag;
                next = Gauss_3x3_sd2_next;
                center = Gauss_3x3_sd2_center;
                break;
            case Gauss_SD.SD3:
                diag = Gauss_3x3_sd3_diag;
                next = Gauss_3x3_sd3_next;
                center = Gauss_3x3_sd3_center;
                break;
            default:
                diag = Gauss_3x3_sd1_diag;
                next = Gauss_3x3_sd1_next;
                center = Gauss_3x3_sd1_center;
                break;
        }

        sum += _input[(row - 1) * _size_input + (col - 1)] * diag;
        sum += _input[(row - 1) * _size_input + (col)] * next;
        sum += _input[(row - 1) * _size_input + (col + 1)] * diag;
        sum += _input[(row) * _size_input + (col - 1)] * next;
        sum += _input[(row) * _size_input + (col)] * center;
        sum += _input[(row) * _size_input + (col + 1)] * next;
        sum += _input[(row + 1) * _size_input + (col - 1)] * diag;
        sum += _input[(row + 1) * _size_input + (col)] * next;
        sum += _input[(row + 1) * _size_input + (col + 1)] * diag;
        _output[row * _size_input + col] = sum;
    }

    public static void GaussianBlurMT3x3(float[,] input, Mask mask, int passes, Gauss_SD std, bool invertMask = false) {
        int side_length = input.GetLength(0);

        // linearize heights and mask
        float[] heightsFlat = new float[side_length * side_length];
        float[] heightsFlat2 = new float[side_length * side_length];

        var workItems = new List<int>();

        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                heightsFlat[i * side_length + j] = input[i, j];
                heightsFlat2[i * side_length + j] = input[i, j];
            }
        }
        // compute work items via mask
        for (int i = 1; i < side_length - 1; i++) {
            for (int j = 1; j < side_length - 1; j++) {
                if (mask == null || (mask[i, j] && !invertMask) || (invertMask && !mask[i, j])) workItems.Add(i * side_length + j);
            }
        }

        var workItemsArr = workItems.ToArray();

        // only process workitem indices -> multithread that work array
        for (int n = 0; n < passes; n++) {
            if (n % 2 == 0) {
                Parallel.For(0, workItemsArr.Length, i => { GaussianBlurMT3x3Worker(workItemsArr[i], side_length, std, heightsFlat, heightsFlat2); });
            } else {
                Parallel.For(0, workItemsArr.Length, i => { GaussianBlurMT3x3Worker(workItemsArr[i], side_length, std, heightsFlat2, heightsFlat); });
            }
        }

        // get the data from the correct buffer
        if (passes % 2 == 0) heightsFlat = heightsFlat2;

        // expand result
        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                input[i, j] = heightsFlat[i * (side_length) + j];
            }
        }
    }
    /*
    private static void AverageFilterMT3x3Worker(int id, int _size_input, float[] _input, float[] _output) {
        int row = id / _size_input; // integer division => row
        int col = id % _size_input; // modulo => column

        float sum = 0;

        sum += _input[(row - 1) * _size_input + (col - 1)] * Avg_3x3;
        sum += _input[(row - 1) * _size_input + (col)] * Avg_3x3;
        sum += _input[(row - 1) * _size_input + (col + 1)] * Avg_3x3;
        sum += _input[(row) * _size_input + (col - 1)] * Avg_3x3;
        sum += _input[(row) * _size_input + (col)] * Avg_3x3;
        sum += _input[(row) * _size_input + (col + 1)] * Avg_3x3;
        sum += _input[(row + 1) * _size_input + (col - 1)] * Avg_3x3;
        sum += _input[(row + 1) * _size_input + (col)] * Avg_3x3;
        sum += _input[(row + 1) * _size_input + (col + 1)] * Avg_3x3;
        _output[row * _size_input + col] = sum;
    }

    public static void AverageFilterMT3x3(float[,] input, Mask mask, int passes, bool invertMask = false) {
        int side_length = input.GetLength(0);

        // linearize heights and mask
        float[] heightsFlat = new float[side_length * side_length];
        float[] heightsFlat2 = new float[side_length * side_length];

        var workItems = new List<int>();

        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                heightsFlat[i * side_length + j] = input[i, j];
                heightsFlat2[i * side_length + j] = input[i, j];
            }
        }
        // compute work items via mask
        for (int i = 1; i < side_length - 1; i++) {
            for (int j = 1; j < side_length - 1; j++) {
                if (mask == null || (mask[i, j] && !invertMask) || (invertMask && !mask[i, j])) workItems.Add(i * side_length + j);
            }
        }

        var workItemsArr = workItems.ToArray();

        // only process workitem indices -> multithread that work array
        for (int n = 0; n < passes; n++) {
            if (n % 2 == 0) {
                Parallel.For(0, workItemsArr.Length, i => { AverageFilterMT3x3Worker(workItemsArr[i], side_length, heightsFlat, heightsFlat2); });
            } else {
                Parallel.For(0, workItemsArr.Length, i => { AverageFilterMT3x3Worker(workItemsArr[i], side_length, heightsFlat2, heightsFlat); });
            }
        }

        // get the data from the correct buffer
        if (passes % 2 == 0) heightsFlat = heightsFlat2;

        // expand result
        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                input[i, j] = heightsFlat[i * (side_length) + j];
            }
        }
    }*/
    public static void AverageFilterMT3x3Burst(float[,] input, Mask mask, int passes, bool invertMask = false) {
        //System.Diagnostics.Stopwatch sw = new();
        //sw.Restart();
        int side_length = input.GetLength(0);

        // linearize heights and mask
        NativeArray<float> heightsFlat = new NativeArray<float>(side_length * side_length, Allocator.TempJob);
        NativeArray<float> heightsFlat2 = new NativeArray<float>(side_length * side_length, Allocator.TempJob);

        var workItems = new List<int>();

        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                heightsFlat[i * side_length + j] = input[i, j];
                heightsFlat2[i * side_length + j] = input[i, j];
            }
        }
        // compute work items via mask
        for (int i = 1; i < side_length - 1; i++) {
            for (int j = 1; j < side_length - 1; j++) {
                if (mask == null || (mask[i, j] && !invertMask) || (invertMask && !mask[i, j])) workItems.Add(i * side_length + j);
            }
        }

        NativeArray<int> workItemsArr = new NativeArray<int>(workItems.Count, Allocator.TempJob);
        for (int i = 0; i < workItems.Count; i++) {
            workItemsArr[i] = workItems[i];
        }

        JobHandle jobHandle;
        AverageFilterJob filterJob1 = new AverageFilterJob() {
            _output = heightsFlat2,
            _input = heightsFlat,
            _work_items = workItemsArr,
            _size_input = side_length,
            _work_item_count = workItemsArr.Length
        };
        AverageFilterJob filterJob2 = new AverageFilterJob() {
            _output = heightsFlat,
            _input = heightsFlat2,
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

        // expand result
        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                // get the data from the correct buffer
                if (passes % 2 == 0) input[i, j] = heightsFlat2[i * (side_length) + j];
                else input[i, j] = heightsFlat[i * (side_length) + j];
            }
        }

        heightsFlat.Dispose();
        heightsFlat2.Dispose();
        workItemsArr.Dispose();

        //sw.Stop();
        //Debug.Log("Runtime AverageFilter Burst:\t " + sw.Elapsed);
        //sw.Reset();
    }

    [BurstCompile]
    private struct AverageFilterJob : IJobParallelFor {

        [NativeDisableParallelForRestriction, WriteOnly]
        public NativeArray<float> _output;

        [ReadOnly]
        public NativeArray<float> _input;

        [ReadOnly]
        public NativeArray<int> _work_items;

        [ReadOnly]
        public int _size_input;
        [ReadOnly]
        public int _work_item_count;

        public void Execute(int i) {
            if (i >= _work_item_count) return;
            int item = _work_items[i];

            int row = item / _size_input; // integer division => row
            int col = item % _size_input; // modulo => column

            float sum = 0;

            sum += _input[(row - 1) * _size_input + (col - 1)] * Avg_3x3;
            sum += _input[(row - 1) * _size_input + (col)] * Avg_3x3;
            sum += _input[(row - 1) * _size_input + (col + 1)] * Avg_3x3;
            sum += _input[(row) * _size_input + (col - 1)] * Avg_3x3;
            sum += _input[(row) * _size_input + (col)] * Avg_3x3;
            sum += _input[(row) * _size_input + (col + 1)] * Avg_3x3;
            sum += _input[(row + 1) * _size_input + (col - 1)] * Avg_3x3;
            sum += _input[(row + 1) * _size_input + (col)] * Avg_3x3;
            sum += _input[(row + 1) * _size_input + (col + 1)] * Avg_3x3;
            _output[row * _size_input + col] = sum;
        }

    }

}
