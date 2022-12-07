using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum Gauss_SD {
    SD1, SD2, SD3
}

public static class TerrainShader {

    public static void GaussianBlurGPU3x3(float[,] input, Mask mask, int passes, Gauss_SD std, bool invertMask = false, bool add = true) {
        ComputeShader computeShader = Resources.Load<ComputeShader>("ComputeShader/GaussianBlur");
        if (computeShader == null) throw new ArgumentNullException("Gaussian Shader is not set in settings object");

        int side_length = input.GetLength(0);

        // linearize heights and mask
        float[] heightsFlat = new float[side_length * side_length];
        int[] maskFlat = new int[side_length * side_length];

        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                maskFlat[i * side_length + j] = (mask == null || (mask[i, j] && !invertMask) || (invertMask && !mask[i, j])) ? 1 : 0;
                heightsFlat[i * side_length + j] = input[i, j];
            }
        }


        //TODO try mask instead of using int use a ByteArray and set those accordingly maybe
        //TODO improve speed by packing float values into float4 packages
        ComputeBuffer maskBuffer = new ComputeBuffer(side_length * side_length, sizeof(int));
        ComputeBuffer inputBuffer = new ComputeBuffer(side_length * side_length, sizeof(float));
        ComputeBuffer outputBuffer = new ComputeBuffer(side_length * side_length, sizeof(float));

        var kernelID = std switch {
            Gauss_SD.SD1 => add ? computeShader.FindKernel("GaussianBlurKeep3x3SD1") : computeShader.FindKernel("GaussianBlur3x3SD1"),
            Gauss_SD.SD2 => add ? computeShader.FindKernel("GaussianBlurKeep3x3SD2") : computeShader.FindKernel("GaussianBlur3x3SD2"),
            Gauss_SD.SD3 => add ? computeShader.FindKernel("GaussianBlurKeep3x3SD3") : computeShader.FindKernel("GaussianBlur3x3SD3"),
            _ => add ? computeShader.FindKernel("GaussianBlurKeep3x3SD1") : computeShader.FindKernel("GaussianBlur3x3SD1"),
        };

        computeShader.GetKernelThreadGroupSizes(kernelID, out var threadGroupSize, out _, out _);
        int threadGroups = Mathf.CeilToInt((side_length) * (side_length) / threadGroupSize);

        computeShader.SetInt("_size_safe", (side_length - 2));
        computeShader.SetInt("_size_input", side_length);
        computeShader.SetInt("_end_of_safe_zone", (side_length - 2) * (side_length - 2));
        computeShader.SetBuffer(kernelID, "_mask", maskBuffer);

        // populate buffers
        maskBuffer.SetData(maskFlat);
        inputBuffer.SetData(heightsFlat);
        outputBuffer.SetData(heightsFlat);

        for (int n = 0; n < passes; n++) {
            if (n % 2 == 0) { // alternate between runs
                // set input/output buffers accordingly
                computeShader.SetBuffer(kernelID, "_input", inputBuffer);
                computeShader.SetBuffer(kernelID, "_output", outputBuffer);
                computeShader.Dispatch(kernelID, threadGroups, 1, 1);
            } else {
                // set input/output buffers accordingly
                computeShader.SetBuffer(kernelID, "_input", outputBuffer);
                computeShader.SetBuffer(kernelID, "_output", inputBuffer);
                computeShader.Dispatch(kernelID, threadGroups, 1, 1);
            }
        }

        // get the data from the correct buffer
        if (passes % 2 == 1) outputBuffer.GetData(heightsFlat); else inputBuffer.GetData(heightsFlat);

        // expand result
        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                input[i, j] = heightsFlat[i * (side_length) + j];
            }
        }

        maskBuffer.Release();
        inputBuffer.Release();
        outputBuffer.Release();
    }

    public static void AverageFilterGPU3x3(float[,] input, Mask mask, int passes, bool invertMask = false, bool add = true, bool multiplyFilter = false) {
        ComputeShader computeShader = Resources.Load<ComputeShader>("ComputeShader/AverageFilter");
        if (computeShader == null) throw new ArgumentNullException("Average Shader is not set in settings object");

        int side_length = input.GetLength(0);

        // linearize heights and mask
        float[] heightsFlat = new float[side_length * side_length];
        int[] maskFlat = new int[side_length * side_length];

        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                maskFlat[i * side_length + j] = (mask == null || (mask[i, j] && !invertMask) || (invertMask && !mask[i, j])) ? 1 : 0;
                heightsFlat[i * side_length + j] = input[i, j];
            }
        }


        //TODO try mask instead of using int use a ByteArray and set those accordingly maybe
        //TODO improve speed by packing float values into float4 packages
        ComputeBuffer maskBuffer = new ComputeBuffer(side_length * side_length, sizeof(int));
        ComputeBuffer inputBuffer = new ComputeBuffer(side_length * side_length, sizeof(float));
        ComputeBuffer outputBuffer = new ComputeBuffer(side_length * side_length, sizeof(float));

        // // prepare buffer zeroing kernel
        // int zeroingKernel = computeShader.FindKernel("SetBufferZero");
        // computeShader.GetKernelThreadGroupSizes(zeroingKernel, out var zeroingThreadGroupSize, out _, out _);
        // int zeroingThreadGroups = Mathf.CeilToInt((side_length) * (side_length) / zeroingThreadGroupSize);

        int kernelID;
        if (add)
            kernelID = multiplyFilter ? computeShader.FindKernel("AverageFilterMultiplyKeep3x3") : computeShader.FindKernel("AverageFilterAddKeep3x3");
        else
            kernelID = multiplyFilter ? computeShader.FindKernel("AverageFilterMultiply3x3") : computeShader.FindKernel("AverageFilterAdd3x3");


        computeShader.GetKernelThreadGroupSizes(kernelID, out var threadGroupSize, out _, out _);
        int threadGroups = Mathf.CeilToInt((side_length) * (side_length) / threadGroupSize);

        computeShader.SetInt("_size_safe", (side_length - 2));
        computeShader.SetInt("_size_input", side_length);
        computeShader.SetInt("_end_of_safe_zone", (side_length - 2) * (side_length - 2));
        computeShader.SetBuffer(kernelID, "_mask", maskBuffer);

        // populate buffers
        maskBuffer.SetData(maskFlat);
        inputBuffer.SetData(heightsFlat);
        outputBuffer.SetData(heightsFlat);

        for (int n = 0; n < passes; n++) {
            if (n % 2 == 0) { // alternate between runs
                // zero current output buffer
                // computeShader.SetBuffer(zeroingKernel, "_zero_buffer", outputBuffer);
                // computeShader.Dispatch(zeroingKernel, zeroingThreadGroups, 1, 1);

                // set input/output buffers accordingly
                computeShader.SetBuffer(kernelID, "_input", inputBuffer);
                computeShader.SetBuffer(kernelID, "_output", outputBuffer);
                computeShader.Dispatch(kernelID, threadGroups, 1, 1);
            } else {
                // zero current output buffer
                // computeShader.SetBuffer(zeroingKernel, "_zero_buffer", inputBuffer);
                // computeShader.Dispatch(zeroingKernel, zeroingThreadGroups, 1, 1);

                // set input/output buffers accordingly
                computeShader.SetBuffer(kernelID, "_input", outputBuffer);
                computeShader.SetBuffer(kernelID, "_output", inputBuffer);
                computeShader.Dispatch(kernelID, threadGroups, 1, 1);
            }
        }

        // get the data from the correct buffer
        if (passes % 2 == 1) outputBuffer.GetData(heightsFlat); else inputBuffer.GetData(heightsFlat);

        // expand result
        for (int i = 0; i < side_length; i++) {
            for (int j = 0; j < side_length; j++) {
                input[i, j] = heightsFlat[i * (side_length) + j];
            }
        }

        maskBuffer.Release();
        inputBuffer.Release();
        outputBuffer.Release();
    }

    private static void AverageFilterMT3x3Worker(int id, int _size_input, float[] _input, float[] _output) {
        const float avg_3x3 = 1 / (float)9;
        int row = id / _size_input; // integer division => row
        int col = id % _size_input; // modulo => column

        float sum = 0;

        sum += _input[(row - 1) * _size_input + (col - 1)] * avg_3x3;
        sum += _input[(row - 1) * _size_input + (col)] * avg_3x3;
        sum += _input[(row - 1) * _size_input + (col + 1)] * avg_3x3;
        sum += _input[(row) * _size_input + (col - 1)] * avg_3x3;
        sum += _input[(row) * _size_input + (col)] * avg_3x3;
        sum += _input[(row) * _size_input + (col + 1)] * avg_3x3;
        sum += _input[(row + 1) * _size_input + (col - 1)] * avg_3x3;
        sum += _input[(row + 1) * _size_input + (col)] * avg_3x3;
        sum += _input[(row + 1) * _size_input + (col + 1)] * avg_3x3;
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
    }
    // private static void AverageFilterAVX3x3Worker(int4 id, int _size_input, float[] _input, float[] _output) {
    //     const float avg_3x3 = 1 / (float)9;
    //     int4 row = id / _size_input; // integer division => row
    //     int4 col = id % _size_input; // modulo => column

    //     float4 sum = 0;

    //     sum += _input[(row - 1) * _size_input + (col - 1)] * avg_3x3;
    //     sum += _input[(row - 1) * _size_input + (col)] * avg_3x3;
    //     sum += _input[(row - 1) * _size_input + (col + 1)] * avg_3x3;
    //     sum += _input[(row) * _size_input + (col - 1)] * avg_3x3;
    //     sum += _input[(row) * _size_input + (col)] * avg_3x3;
    //     sum += _input[(row) * _size_input + (col + 1)] * avg_3x3;
    //     sum += _input[(row + 1) * _size_input + (col - 1)] * avg_3x3;
    //     sum += _input[(row + 1) * _size_input + (col)] * avg_3x3;
    //     sum += _input[(row + 1) * _size_input + (col + 1)] * avg_3x3;
    //     _output[row * _size_input + col] = sum;
    // }

    // public static void AverageFilterAVX3x3(float[,] input, Mask mask, int passes, bool invertMask = false) {
    //     int side_length = input.GetLength(0);

    //     // linearize heights and mask
    //     float[] heightsFlat = new float[side_length * side_length];

    //     var workItems = new List<int>();

    //     for (int i = 0; i < side_length; i++) {
    //         for (int j = 0; j < side_length; j++) {
    //             heightsFlat[i * side_length + j] = input[i, j];
    //         }
    //     }
    //     // pack heights
    //     int heightsPackCount = Mathf.CeilToInt(side_length * side_length / 4f);
    //     var heightsFlatPacks = new float4[heightsPackCount];
    //     var heightsFlatPacks2 = new float4[heightsPackCount];
    //     for (int i = 0; i < heightsPackCount; i += 4) {
    //         var pack = new float4(heightsFlat[i], heightsFlat[i + 1], heightsFlat[i + 2], heightsFlat[i + 3]);
    //         heightsFlatPacks[i] = pack;
    //         heightsFlatPacks2[i] = pack;
    //     }

    //     // compute work items via mask
    //     for (int i = 1; i < side_length - 1; i++) {
    //         for (int j = 1; j < side_length - 1; j++) {
    //             if (mask == null || (mask[i, j] && !invertMask) || (invertMask && !mask[i, j])) workItems.Add(i * side_length + j);
    //         }
    //     }

    //     int workItemPackCount = Mathf.CeilToInt(workItems.Count / 4f);
    //     var workItemPacks = new int4[workItemPackCount];
    //     // pack work items into vector units
    //     for (int i = 0; i < workItemPackCount; i += 4) {
    //         int4 pack = new int4(workItems[i], workItems[i + 1], workItems[i + 2], workItems[i + 3]);
    //         workItemPacks[i] = pack;
    //     }

    //     // only process workitem indices -> multithread that work array
    //     for (int n = 0; n < passes; n++) {
    //         if (n % 2 == 0) {
    //             foreach (int4 id in workItemPacks) AverageFilterAVX3x3Worker(id, side_length, heightsFlat, heightsFlat2);
    //         } else {
    //             foreach (int4 id in workItemPacks) AverageFilterAVX3x3Worker(id, side_length, heightsFlat2, heightsFlat);
    //         }
    //     }

    //     // get the data from the correct buffer
    //     if (passes % 2 == 0) heightsFlat = heightsFlat2;

    //     // Debug.Log(heightsFlat[0]);
    //     // expand result
    //     for (int i = 0; i < side_length; i++) {
    //         for (int j = 0; j < side_length; j++) {
    //             input[i, j] = heightsFlat[i * (side_length) + j];
    //         }
    //     }
    // }
}
