using System;
using UnityEngine;

public static class TerrainShader {

    public static float[] GaussianBlurGPU(ComputeShader computeShader, int sidelength, float[] input) {
        //input has size*size elements
        if (computeShader == null) throw new ArgumentNullException("Gaussian Blur Shader is not set in settings object");

        // int groups = Mathf.CeilToInt(sidelength / 8f); // for a 8x8 2D workgroup

        ComputeBuffer inputBuffer = new ComputeBuffer(input.GetLength(0), sizeof(float));
        inputBuffer.SetData(input, 0, 0, input.GetLength(0));

        // float[] flatKernel = GaussianKernel2DFlat(radius, weight);
        // ComputeBuffer kernelBuffer = new ComputeBuffer(flatKernel.GetLength(0), sizeof(float));
        // kernelBuffer.SetData(input, 0, 0, flatKernel.GetLength(0));

        ComputeBuffer outputBuffer = new ComputeBuffer(input.GetLength(0), sizeof(float));
        float[] output = new float[input.GetLength(0)];

        int kernelID = computeShader.FindKernel("CSMain");
        computeShader.SetInt("_size", sidelength);
        // computeShader.SetBuffer(kernelID, "_kernel", kernelBuffer);
        computeShader.SetBuffer(kernelID, "_input", inputBuffer);
        computeShader.SetBuffer(kernelID, "_output", outputBuffer);


        computeShader.GetKernelThreadGroupSizes(kernelID, out var threadGroupSize, out _, out _);
        int threadGroups = (int)((input.GetLength(0) + (threadGroupSize - 1)) / threadGroupSize);
        computeShader.Dispatch(kernelID, threadGroups, 1, 1);

        outputBuffer.GetData(output);
        inputBuffer.Release();
        // kernelBuffer.Release();
        outputBuffer.Release();
        return output;
    }

    public static void GaussianBlurGPU3x3SD1(ComputeShader computeShader, float[,] input, int passes) {
        if (computeShader == null) throw new ArgumentNullException("Gaussian Blur Shader is not set in settings object");

        int side_length = input.GetLength(0);

        // exclude borders and linearize
        int radius = Mathf.FloorToInt(3 / 2);
        int shrinked_side_length = side_length - radius;
        int flat_arr_size = shrinked_side_length * shrinked_side_length;

        float[] heightsFlat = new float[flat_arr_size];

        for (int i = radius; i < shrinked_side_length; i++) {
            for (int j = radius; j < shrinked_side_length; j++) {
                heightsFlat[i * shrinked_side_length + j] = input[i, j];
            }
        }

        ComputeBuffer inputBuffer = new ComputeBuffer(flat_arr_size, sizeof(float));
        inputBuffer.SetData(heightsFlat, 0, 0, flat_arr_size);

        ComputeBuffer outputBuffer = new ComputeBuffer(flat_arr_size, sizeof(float));

        int kernelID = computeShader.FindKernel("GaussianBlur3x3SD1");
        computeShader.GetKernelThreadGroupSizes(kernelID, out var threadGroupSize, out _, out _);
        int threadGroups = Mathf.CeilToInt(flat_arr_size / threadGroupSize);

        for (int n = 0; n < passes; n++) {
            computeShader.SetInt("_size", shrinked_side_length);
            // alternate between runs
            if (n % 2 == 0) {
                computeShader.SetBuffer(kernelID, "_input", inputBuffer);
                computeShader.SetBuffer(kernelID, "_output", outputBuffer);

                computeShader.Dispatch(kernelID, threadGroups, 1, 1);
            } else {
                computeShader.SetBuffer(kernelID, "_input", outputBuffer);
                computeShader.SetBuffer(kernelID, "_output", inputBuffer);

                computeShader.Dispatch(kernelID, threadGroups, 1, 1);
            }
        }
        // get the data from the correct buffer
        if (passes % 2 == 1) outputBuffer.GetData(heightsFlat); else inputBuffer.GetData(heightsFlat);

        // expand result
        for (int i = 1; i < shrinked_side_length; i++) {
            for (int j = 1; j < shrinked_side_length; j++) {
                input[i, j] = heightsFlat[i * shrinked_side_length + j];
            }
        }

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
                maskFlat[i * side_length + j] = mask[i, j] ? 1 : 0;
                heightsFlat[i * side_length + j] = input[i, j];
            }
        }


        //TODO try mask instead of using int use a ByteArray and set those accordingly maybe
        //TODO improve speed by packing float values into float4 packages
        ComputeBuffer maskBuffer = new ComputeBuffer(side_length * side_length, sizeof(int));
        maskBuffer.SetData(maskFlat);

        ComputeBuffer inputBuffer = new ComputeBuffer(side_length * side_length, sizeof(float));

        ComputeBuffer outputBuffer = new ComputeBuffer(side_length * side_length, sizeof(float));

        // prepare buffer zeroing kernel
        int zeroingKernel = computeShader.FindKernel("SetBufferZero");
        computeShader.GetKernelThreadGroupSizes(zeroingKernel, out var zeroingThreadGroupSize, out _, out _);
        int zeroingThreadGroups = Mathf.CeilToInt((side_length) * (side_length) / zeroingThreadGroupSize);

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

        // populate input
        inputBuffer.SetData(heightsFlat, 0, 0, side_length * side_length);

        for (int n = 0; n < passes; n++) {
            if (n % 2 == 0) { // alternate between runs
                // zero current output buffer
                computeShader.SetBuffer(zeroingKernel, "_zero_buffer", outputBuffer);
                computeShader.Dispatch(zeroingKernel, zeroingThreadGroups, 1, 1);

                // set input/output buffers accordingly
                computeShader.SetBuffer(kernelID, "_input", inputBuffer);
                computeShader.SetBuffer(kernelID, "_output", outputBuffer);
                computeShader.Dispatch(kernelID, threadGroups, 1, 1);
            } else {
                // zero current output buffer
                computeShader.SetBuffer(zeroingKernel, "_zero_buffer", inputBuffer);
                computeShader.Dispatch(zeroingKernel, zeroingThreadGroups, 1, 1);

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
