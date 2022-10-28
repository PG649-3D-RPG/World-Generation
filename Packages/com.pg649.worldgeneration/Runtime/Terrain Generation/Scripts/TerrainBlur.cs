using System;
using System.Threading;
using System.Threading.Tasks;

public class TerrainBlur {

    private static double[,] GaussianKernel2D2x3(int d) {
        double[,] kernel = new double[3, 3];

        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                kernel[x, y] = (1 / (2 * MathF.PI * Math.Pow(d, 2))) * MathF.Exp(-(MathF.Pow(x, 2) + MathF.Pow(y, 2)) / (2 * MathF.Pow(d, 2)));
            }
        }

        return kernel;
    }


    private static readonly Func<int, int, float[], int, Tuple<int, float>> blur = (int row, int col, float[] heights, int size) => {
        int sum_elements = 0;
        float sum = 0;

        if (row > 0 && col > 0) {
            sum += heights[(row - 1) * size + (col - 1)];
            sum_elements++;
        }
        if (col > 0) {
            sum += heights[(row) * size + (col - 1)];
            sum_elements++;
        }
        if (row < (size - 1) && col > 0) {
            sum += heights[(row + 1) * size + (col - 1)];
            sum_elements++;
        }
        if (row > 0) {
            sum += heights[(row - 1) * size + (col)];
            sum_elements++;
        }
        if (row < (size - 1)) {
            sum += heights[(row + 1) * size + (col)];
            sum_elements++;
        }
        if (row > 0 && col < (size - 1)) {
            sum += heights[(row - 1) * size + (col + 1)];
            sum_elements++;
        }
        if (col < (size - 1)) {
            sum += heights[(row) * size + (col + 1)];
            sum_elements++;
        }
        if (row < (size - 1) && col < (size - 1)) {
            sum += heights[(row + 1) * size + (col + 1)];
            sum_elements++;
        }
        // heightTwo[row * size + col] = heightOne[row * size + col] + (((sum / sum_elements) - heightOne[row * size + col]));

        return new(sum_elements, sum);
    };

    public static float[,] BlurCPU(float[,] heights, int passes, int radius) {
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
                    int telem;
                    float tsum;

                    for (int r = 0; r < radius; r++) {
                        //origin verschieben
                        if (row - r > 0 && col - r > 0) {
                            (telem, tsum) = blur(row - r, col - r, heightOne, size);
                            sum_elements += telem;
                            sum += tsum;
                        }

                        if (row - r > 0) {
                            (telem, tsum) = blur(row - r, col, heightOne, size);
                            sum_elements += telem;
                            sum += tsum;
                        }

                        if (col - r > 0) {
                            (telem, tsum) = blur(row, col - r, heightOne, size);
                            sum_elements += telem;
                            sum += tsum;
                        }

                        (telem, tsum) = blur(row, col, heightOne, size);
                        sum_elements += telem;
                        sum += tsum;
                    }
                    // if (row > 0 && col > 0) {
                    //     sum += heightOne[(row - 1) * size + (col - 1)];
                    //     sum_elements++;
                    // }
                    // if (col > 0) {
                    //     sum += heightOne[(row) * size + (col - 1)];
                    //     sum_elements++;
                    // }
                    // if (row < (size - 1) && col > 0) {
                    //     sum += heightOne[(row + 1) * size + (col - 1)];
                    //     sum_elements++;
                    // }
                    // if (row > 0) {
                    //     sum += heightOne[(row - 1) * size + (col)];
                    //     sum_elements++;
                    // }
                    // if (row < (size - 1)) {
                    //     sum += heightOne[(row + 1) * size + (col)];
                    //     sum_elements++;
                    // }
                    // if (row > 0 && col < (size - 1)) {
                    //     sum += heightOne[(row - 1) * size + (col + 1)];
                    //     sum_elements++;
                    // }
                    // if (col < (size - 1)) {
                    //     sum += heightOne[(row) * size + (col + 1)];
                    //     sum_elements++;
                    // }
                    // if (row < (size - 1) && col < (size - 1)) {
                    //     sum += heightOne[(row + 1) * size + (col + 1)];
                    //     sum_elements++;
                    // }
                    heightTwo[row * size + col] = heightOne[row * size + col] + (((sum / sum_elements) - heightOne[row * size + col]));
                });
            }
            else {
                tmp = Parallel.For(0, size * size, i => {
                    int row = i / size; // integer division => row
                    int col = i % size; // modulo => column
                    int sum_elements = 0;
                    float sum = 0;

                    var (telem, tsum) = blur(row, col, heightTwo, size);
                    sum_elements = telem;
                    sum = tsum;

                    // if (row > 0 && col > 0) {
                    //     sum += heightTwo[(row - 1) * size + (col - 1)];
                    //     sum_elements++;
                    // }
                    // if (col > 0) {
                    //     sum += heightTwo[(row) * size + (col - 1)];
                    //     sum_elements++;
                    // }
                    // if (row < (size - 1) && col > 0) {
                    //     sum += heightTwo[(row + 1) * size + (col - 1)];
                    //     sum_elements++;
                    // }
                    // if (row > 0) {
                    //     sum += heightTwo[(row - 1) * size + (col)];
                    //     sum_elements++;
                    // }
                    // if (row < (size - 1)) {
                    //     sum += heightTwo[(row + 1) * size + (col)];
                    //     sum_elements++;
                    // }
                    // if (row > 0 && col < (size - 1)) {
                    //     sum += heightTwo[(row - 1) * size + (col + 1)];
                    //     sum_elements++;
                    // }
                    // if (col < (size - 1)) {
                    //     sum += heightTwo[(row) * size + (col + 1)];
                    //     sum_elements++;
                    // }
                    // if (row < (size - 1) && col < (size - 1)) {
                    //     sum += heightTwo[(row + 1) * size + (col + 1)];
                    //     sum_elements++;
                    // }

                    heightOne[row * size + col] = heightTwo[row * size + col] + (((sum / sum_elements) - heightTwo[row * size + col]));
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



//     if (row > (0 + r) && col > (0 + r)) {
//         sum += heightOne[(row - (1 + r)) * size + (col - (1 + r))];
//         sum_elements++;
//     }
//     if (col > (0 + r)) {
//         sum += heightOne[(row + (0 + r)) * size + (col - (1 + r))];
//         sum_elements++;
//     }
//     if (row < (size - (1 + r)) && col > (0 + r)) {
//         sum += heightOne[(row + (1 + r)) * size + (col - (1 + r))];
//         sum_elements++;
//     }
//     if (row > (0 + r)) {
//         sum += heightOne[(row - (1 + r)) * size + (col + (0 + r))];
//         sum_elements++;
//     }
//     if (row < (size - (1 + r))) {
//         sum += heightOne[(row + (1 + r)) * size + (col + (0 + r))];
//         sum_elements++;
//     }
//     if (row > (0 + r) && col < (size - (1 + r))) {
//         sum += heightOne[(row - (1 + r)) * size + (col + (1 + r))];
//         sum_elements++;
//     }
//     if (col < (size - (1 + r))) {
//         sum += heightOne[(row + (0 + r)) * size + (col + (1 + r))];
//         sum_elements++;
//     }
//     if (row < (size - (1 + r)) && col < (size - (1 + r))) {
//         sum += heightOne[(row + (1 + r)) * size + (col + (1 + r))];
//         sum_elements++;
//     }
// }
