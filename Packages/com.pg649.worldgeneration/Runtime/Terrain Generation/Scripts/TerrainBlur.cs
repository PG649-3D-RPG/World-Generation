using System;
using System.Threading;
using System.Threading.Tasks;

public class TerrainBlur {

    public static float[,] BlurCPU(float[,] heights, int passes) {
        int size = heights.GetLength(0);
        float[] heightOne = new float[size * size];
        float[] heightTwo = new float[size * size];
        // linearize result
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                heightOne[i * size + j] = heights[i, j];
            }
        }

        ParallelLoopResult tmp;
        for (int n = 0; n < passes; n++) {
            if (n % 2 == 0) {
                tmp = Parallel.For(0, size * size, i => {
                    int row = i / size; // integer division => row
                    int col = i % size; // modulo => column
                    int sum_elements = 0;
                    float sum = 0;

                    if (row > 0 && col > 0) {
                        sum += heightOne[(row - 1) * size + (col - 1)];
                        sum_elements++;
                    }
                    if (col > 0) {
                        sum += heightOne[(row) * size + (col - 1)];
                        sum_elements++;
                    }
                    if (row < (size - 1) && col > 0) {
                        sum += heightOne[(row + 1) * size + (col - 1)];
                        sum_elements++;
                    }
                    if (row > 0) {
                        sum += heightOne[(row - 1) * size + (col)];
                        sum_elements++;
                    }
                    if (row < (size - 1)) {
                        sum += heightOne[(row + 1) * size + (col)];
                        sum_elements++;
                    }
                    if (row > 0 && col < (size - 1)) {
                        sum += heightOne[(row - 1) * size + (col + 1)];
                        sum_elements++;
                    }
                    if (col < (size - 1)) {
                        sum += heightOne[(row) * size + (col + 1)];
                        sum_elements++;
                    }
                    if (row < (size - 1) && col < (size - 1)) {
                        sum += heightOne[(row + 1) * size + (col + 1)];
                        sum_elements++;
                    }
                    heightTwo[row * size + col] = heightOne[row * size + col] + (((sum / sum_elements) - heightOne[row * size + col]) / 2);
                });
            }
            else {
                tmp = Parallel.For(0, size * size, i => {
                    int row = i / size; // integer division => row
                    int col = i % size; // modulo => column
                    int sum_elements = 0;
                    float sum = 0;

                    if (row > 0 && col > 0) {
                        sum += heightTwo[(row - 1) * size + (col - 1)];
                        sum_elements++;
                    }
                    if (col > 0) {
                        sum += heightTwo[(row) * size + (col - 1)];
                        sum_elements++;
                    }
                    if (row < (size - 1) && col > 0) {
                        sum += heightTwo[(row + 1) * size + (col - 1)];
                        sum_elements++;
                    }
                    if (row > 0) {
                        sum += heightTwo[(row - 1) * size + (col)];
                        sum_elements++;
                    }
                    if (row < (size - 1)) {
                        sum += heightTwo[(row + 1) * size + (col)];
                        sum_elements++;
                    }
                    if (row > 0 && col < (size - 1)) {
                        sum += heightTwo[(row - 1) * size + (col + 1)];
                        sum_elements++;
                    }
                    if (col < (size - 1)) {
                        sum += heightTwo[(row) * size + (col + 1)];
                        sum_elements++;
                    }
                    if (row < (size - 1) && col < (size - 1)) {
                        sum += heightTwo[(row + 1) * size + (col + 1)];
                        sum_elements++;
                    }
                    heightOne[row * size + col] = heightTwo[row * size + col] + (((sum / sum_elements) - heightTwo[row * size + col]) / 2);
                });
            }
        }

        // expand result
        float[,] result = new float[size, size];
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                result[i, j] = passes % 2 == 0 ? heightOne[i * size + j] : heightTwo[i * size + j];
            }
        }
        return result;
    }
}
