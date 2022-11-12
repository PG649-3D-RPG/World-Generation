using System;
using UnityEngine;

public enum Gauss_SD {
    SD1, SD2, SD3
}

public static class TerrainShader {

    public static void GaussianBlurGPU3x3(ComputeShader computeShader, float[,] input, Mask mask, int passes, Gauss_SD std, bool invertMask = false, bool add = true) {
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

    public static void AverageFilterGPU3x3(ComputeShader computeShader, float[,] input, Mask mask, int passes, bool invertMask = false, bool add = true, bool multiplyFilter = false) {
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
}
