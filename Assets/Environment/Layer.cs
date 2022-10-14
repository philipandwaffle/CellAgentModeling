using System;
using UnityEngine;

namespace Assets.Environment {
    public class Layer {
        public float[,] values;
        float[,] m;
        public int w, h;
        public int mW, mH;
        public int tW, tH;

        public Layer(int w, int h, float[,] m, int value = 0) {
            InitVariables(w, h, m);

            // init layer with default value
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) { 
                    values[i, j] = value;
                }
            }
            LoopMatrix();
        }
        public Layer(int w, int h, float[,] m, Func<int, int, float> gen) {
            InitVariables(w, h, m);

            // init layer with default value
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    float f = gen(i, j);
                    //Debug.Log(f);
                    values[(mW / 2) + i, (mH / 2) + j] = f;
                }
            }
            LoopMatrix();
        }

        public void InitVariables(int w, int h, float[,] m) {
            this.m = m;

            this.w = w;
            this.h = h;

            mW = m.GetLength(0);
            mH = m.GetLength(1);

            tW = w + 2*(mW/2);
            tH = h + 2*(mH/2);

            values = new float[tW,tH];
        }

        public void LoopMatrix() {
            for (int i = 0; i < mH / 2; i++) {
                // top, top into bottom
                int tTB = (tH - (mH / 2) - 1) - i;
                // bottom, top into bottom
                int bTB = (mH / 2) - 1 - i;

                // top, bottom into top
                int tBT = (tH - (mH / 2)) + i;
                // bottom, bottom into top
                int bBT = (mW / 2) + i;
                
                for (int x = 0; x < tW; x++) {
                    // top row into bottom
                    values[x, bTB] = values[x, tTB];

                    // bottom row into top
                    values[x, tBT] = values[x, bBT];
                }
            }

            for (int i = 0; i < mW / 2; i++) {
                // right, right into left
                int rRL = (tW - (mW / 2) - 1) - i;
                // left, right into left
                int lRL = (mW / 2) - 1 - i;

                // right, left into right
                int rLR = (tH - (mW / 2)) + i;
                // left, left into right
                int lLR = (mW / 2) + i;
                
                for (int y = 0; y < tW; y++) {
                    // right colunn into left
                    values[lRL, y] = values[rRL, y];

                    // left column into right
                    values[rLR, y] = values[lLR, y];
                }
            }
        }

        public float[,] Convolute() {            
            float[,] newValues = new float[w, h];

            for (int y = mH / 2; y < h + (mH / 2); y++) {
                for (int x = mW / 2; x < w + (mW / 2); x++) {
                    float total = 0f;
                    for (int mY = 0; mY < mH; mY++) {
                        for (int mX = 0; mX < mW; mX++) {
                            int vX = x + mX - (mH / 2);
                            int vY = y + mY - (mW / 2);
                            //Debug.Log("values: (" + vX + "," + vY + ")");
                            total += values[vX, vY] * m[mX, mY];
                        }
                    }
                    bool alive = values[x, y] == 1;

                    if (alive && total < 2) {
                        total = 0f;
                    }else if(alive && total > 3) {
                        total = 0f;
                    } else if (alive && (total == 2 || total == 3)){
                        total = 1f;
                    } else if (!alive && total == 3){
                        total = 1;
                    } else {
                        total = alive ? 1 : 0;
                        //Debug.Log("dfsdf");
                    }
                    //Debug.Log("calculating for: (" + x + "," + y + ")"); 
                    newValues[x - (mW / 2), y - (mH / 2)] = total;
                }
            }
            SetNewValues(newValues);
            return newValues;
        }

        private void SetNewValues(float[,] newValues) {
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    values[(mW / 2) + x, (mH / 2) + y] = newValues[x,y];
                }
            }
            LoopMatrix();
        }

        public override string ToString() {
            string s = "";
            for (int j = 0; j < values.GetLength(1); j++) {
                for (int i = 0; i < values.GetLength(0); i++) {
                    s += values[i, j].ToString() + ", ";
                }
                s += "\n";
            }
            return s;
        }
    }
}
