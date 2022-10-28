using System;
using System.Threading;
using System.Threading.Tasks;

public class TerrainBlur {

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

    private static float[,] GaussianKernel2D(int radius, float weigth = 5.5f) {
        if (radius < 1) throw new ArgumentException("Dimension must be at least 1");
        int size = 2 * radius + 1;
        float[,] kernel = new float[size, size];

        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                kernel[x, y] = (1 / (2 * MathF.PI * MathF.Pow(weigth, 2))) * MathF.Exp(-(MathF.Pow(x - radius, 2) + MathF.Pow(y - radius, 2)) / (2 * MathF.Pow(weigth, 2)));
            }
        }
        NormalizeSquareMatrix(kernel);

        return kernel;
    }

    public static float[,] ApplyBlur(float[,] heights, int radius) {
        int size = heights.GetLength(0);

        float[,] blurred = new float[size, size];
        float[,] kernel = GaussianKernel2D(radius: radius);
        // float[,] kernel = { { 0.0585498f, 0.0965324f, 0.0585498f }, { 0.0965324f, 0.159155f, 0.0965324f }, { 0.0585498f, 0.0965324f, 0.0585498f } };

        // float kernel_sum = 0f;
        // for (int i = 0; i < 3; i++) {
        //     for (int j = 0; j < 3; j++) {
        //         kernel_sum += kernel[i, j];
        //     }
        // }

        // // normalize kernel
        // for (int i = 0; i < 3; i++) {
        //     for (int j = 0; j < 3; j++) {
        //         kernel[i, j] /= kernel_sum;
        //     }
        // }

        // set all to mean
        // for (int x = 1; x < size - 1; x++) {
        //     for (int y = 1; y < size - 1; y++) {
        //         float sum = 0;
        //         sum += heights[x - 1, y - 1] * kernel[0, 0];
        //         sum += heights[x, y - 1] * kernel[1, 0];
        //         sum += heights[x + 1, y - 1] * kernel[2, 0];
        //         sum += heights[x - 1, y] * kernel[0, 1];
        //         sum += heights[x, y] * kernel[1, 1];
        //         sum += heights[x + 1, y] * kernel[2, 1];
        //         sum += heights[x - 1, y + 1] * kernel[0, 2];
        //         sum += heights[x, y + 1] * kernel[1, 2];
        //         sum += heights[x + 1, y - 1] * kernel[2, 2];
        //         blurred[x, y] = sum;
        //     }
        // }

        for (int x = 1 + radius; x < size - (1 + radius); x++) {
            for (int y = 1 + radius; y < size - (1 + radius); y++) {
                float sum = 0;
                for (int r = 0; r < radius; r++) {
                    sum += heights[x - (1 + r), y - (1 + r)] * kernel[0 + r, 0 + r];
                    sum += heights[x + r, y - (1 + r)] * kernel[1 + r, 0 + r];
                    sum += heights[x + 1 + r, y - (1 + r)] * kernel[2 + r, 0 + r];
                    sum += heights[x - (1 + r), y + r] * kernel[0 + r, 1 + r];
                    sum += heights[x + r, y + r] * kernel[1 + r, 1 + r];
                    sum += heights[x + (1 + r), y + r] * kernel[2 + r, 1 + r];
                    sum += heights[x - (1 + r), y + 1 + r] * kernel[0 + r, 2 + r];
                    sum += heights[x + r, y + 1 + r] * kernel[1 + r, 2 + r];
                    sum += heights[x + 1 + r, y + 1 + r] * kernel[2 + r, 2 + r];
                }
                blurred[x, y] = sum;
            }
        }
        //normalize array to 1
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

    public static float[,] ApplyBlurPar(float[,] heights, int radius) {
        //TODO implement
        int size = heights.GetLength(0);

        float[,] blurred = new float[size, size];
        float[,] kernel = GaussianKernel2D(radius: radius);

        //linearize kernel and heights

        for (int x = 1 + radius; x < size - (1 + radius); x++) {
            for (int y = 1 + radius; y < size - (1 + radius); y++) {
                float sum = 0;
                for (int r = 0; r < radius; r++) {
                    sum += heights[x - (1 + r), y - (1 + r)] * kernel[0 + r, 0 + r];
                    sum += heights[x + r, y - (1 + r)] * kernel[1 + r, 0 + r];
                    sum += heights[x + 1 + r, y - (1 + r)] * kernel[2 + r, 0 + r];
                    sum += heights[x - (1 + r), y + r] * kernel[0 + r, 1 + r];
                    sum += heights[x + r, y + r] * kernel[1 + r, 1 + r];
                    sum += heights[x + (1 + r), y + r] * kernel[2 + r, 1 + r];
                    sum += heights[x - (1 + r), y + 1 + r] * kernel[0 + r, 2 + r];
                    sum += heights[x + r, y + 1 + r] * kernel[1 + r, 2 + r];
                    sum += heights[x + 1 + r, y + 1 + r] * kernel[2 + r, 2 + r];
                }
                blurred[x, y] = sum;
            }
        }
        //normalize array to 1
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
