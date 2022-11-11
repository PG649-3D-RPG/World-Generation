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
        int shrinked_arr_size = side_length - radius;
        int flat_arr_size = shrinked_arr_size * shrinked_arr_size;

        float[] heightsFlat = new float[flat_arr_size];

        for (int i = radius; i < shrinked_arr_size; i++) {
            for (int j = radius; j < shrinked_arr_size; j++) {
                heightsFlat[i * shrinked_arr_size + j] = input[i, j];
            }
        }

        ComputeBuffer inputBuffer = new ComputeBuffer(flat_arr_size, sizeof(float));
        inputBuffer.SetData(heightsFlat, 0, 0, flat_arr_size);

        ComputeBuffer outputBuffer = new ComputeBuffer(flat_arr_size, sizeof(float));

        int kernelID = computeShader.FindKernel("GaussianBlur3x3SD1");
        computeShader.GetKernelThreadGroupSizes(kernelID, out var threadGroupSize, out _, out _);
        int threadGroups = Mathf.CeilToInt(flat_arr_size / threadGroupSize);

        for (int n = 0; n < passes; n++) {
            computeShader.SetInt("_size", side_length);
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
        for (int i = 1; i < shrinked_arr_size; i++) {
            for (int j = 1; j < shrinked_arr_size; j++) {
                input[i, j] = heightsFlat[i * shrinked_arr_size + j];
            }
        }

        inputBuffer.Release();
        outputBuffer.Release();
    }
}
