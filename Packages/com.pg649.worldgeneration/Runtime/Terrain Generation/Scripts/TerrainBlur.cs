using System;
using System.Threading.Tasks;

public static class TerrainBlur {

    private static void NormalizeSquareMatrix(float[,] matrix) {
        int size = matrix.GetLength(0);
        float kernel_sum = 0f;
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                kernel_sum += matrix[i, j];
            }
        }

        if (kernel_sum != 0) {
            // normalize kernel
            for (int i = 0; i < size; i++) {
                for (int j = 0; j < size; j++) {
                    matrix[i, j] /= kernel_sum;
                }
            }
        }
    }

    public static float[,] GaussianKernel2D(int radius, float weight) {
        if (radius < 1) throw new ArgumentException("Dimension must be at least 1");
        int dim = 2 * radius + 1;
        float[,] kernel = new float[dim, dim];

        for (int x = 0; x < dim; x++) {
            for (int y = 0; y < dim; y++) {
                kernel[x, y] = (1 / (2 * MathF.PI * MathF.Pow(weight, 2))) * MathF.Exp(-(MathF.Pow(x - radius, 2) + MathF.Pow(y - radius, 2)) / (2 * MathF.Pow(weight, 2)));
            }
        }
        NormalizeSquareMatrix(kernel);

        return kernel;
    }

    private static float[] GaussianKernel2DFlat(int radius, float weight) {
        var kernel = GaussianKernel2D(radius, weight);
        var dim = kernel.GetLength(0);
        float[] kernelFlat = new float[dim * dim];
        for (int i = 0; i < dim; i++) {
            for (int j = 0; j < dim; j++) {
                kernelFlat[i * dim + j] = kernel[i, j];
            }
        }
        return kernelFlat;
    }

    public static float[,] GaussianBlurParCPU(float[,] heights, int radius, float weight) {
        int size = heights.GetLength(0);

        float[] blurred = new float[size * size];
        float[,] kernel = GaussianKernel2D(radius: radius, weight: weight);


        int dim = 2 * radius + 1;
        //fix indices
        ParallelLoopResult par_res = Parallel.For(0, size * size, i => {
            float sum = 0;
            int row = i / size; // integer division => row
            int col = i % size; // modulo => column

            for (int k0 = 0; k0 < dim; k0++) {
                for (int k1 = 0; k1 < dim; k1++) {
                    int curX = (row - radius + k0);
                    int curY = (col - radius + k1);
                    if (curX < 0 && curY < 0) sum += heights[0, 0] * kernel[k0, k1];
                    else if (curX >= size && curY >= size) sum += heights[size - 1, size - 1] * kernel[k0, k1];
                    else if (curX < 0 && curY >= size) sum += heights[0, size - 1] * kernel[k0, k1];
                    else if (curX >= size && curY < 0) sum += heights[size - 1, 0] * kernel[k0, k1];
                    else if (curX < 0) sum += heights[0, curY] * kernel[k0, k1];
                    else if (curY < 0) sum += heights[curX, 0] * kernel[k0, k1];
                    else if (curX >= size) sum += heights[size - 1, curY] * kernel[k0, k1];
                    else if (curY >= size) sum += heights[curX, size - 1] * kernel[k0, k1];
                    else sum += heights[curX, curY] * kernel[k0, k1];
                }
            }
            blurred[i] = sum;
        });

        // expand result
        float[,] result = new float[size, size];
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                result[i, j] = blurred[i * size + j];
            }
        }

        return result;
    }


    public static float[,] GaussianBlur(float[,] heights, int radius, float weight = 5.5f) {
        int size = heights.GetLength(0);

        float[,] blurred = new float[size, size];
        float[,] kernel = GaussianKernel2D(radius: radius, weight: weight);

        int dim = 2 * radius + 1;

        for (int i = radius; i < size - radius; i++) {
            for (int j = radius; j < size - radius; j++) {
                float sum = 0;
                for (int k0 = 0; k0 < dim; k0++) {
                    for (int k1 = 0; k1 < dim; k1++) {
                        sum += heights[i - radius + k0, j - radius + k1] * kernel[k0, k1];
                    }
                }
                blurred[i, j] = sum;
            }
        }

        // normalize array to 1
        float max_val = 0f;
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                if (blurred[i, j] > max_val) max_val = blurred[i, j];
            }
        }
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                blurred[i, j] /= max_val;
            }
        }

        return blurred;
    }
}

/** OLD CODE **
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
**/
