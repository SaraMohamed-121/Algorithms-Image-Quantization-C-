using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;

///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }

    public struct Nodes_clusters
    {
        public int node { get; set; }
        public double Distance { get; set; }

    }
    public struct colorindecesmatrix
    {
        public int nodeindex1 { get; set; }
        public int nodeindex2 { get; set; }


    }
    public class NodeOfcolors
    {
        public int from_Color, to_Color;
        public double ElcideanDistance;
        public bool visted;
        public NodeOfcolors(int from_Color, int to_Color, double ElcideanDistance)
        {
            this.from_Color = from_Color;
            this.to_Color = to_Color;
            this.ElcideanDistance = ElcideanDistance;
            visted = false;
        }
    }
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>

    public class ImageOperations
    {


        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>

        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }

        // function 1
        public static List<RGBPixel> ListOfDC;
        public static List<int> listOfIndex;
        public static int FindDistinctColorsANDList(RGBPixel[,] ImageMatrix)
        {
            //function O(D^2) , D->number of Distinct Colors (hight*width) 
            listOfIndex = new List<int>();
            ListOfDC = new List<RGBPixel>();
            int R, G, B;
            //using hash set to add a uniqe item only
            HashSet<int> Set = new HashSet<int>();
            HashSet<RGBPixel> Setij = new HashSet<RGBPixel>();
            //colors in each image have different color channels, Red, Green, and Blue.
            //Each channel has a range from 0 to 255 ,So we need 8+8+8=24 bits to represent 1 pixel color
            //digital color image (24 bits per pixel) representative set of colors.
            for (int j = 0; j < GetHeight(ImageMatrix); j++)  //O(N)
            {
                for (int i = 0; i < GetWidth(ImageMatrix); i++)  //O(N)
                {
                    R = ImageMatrix[j, i].red;    //O(1)
                    G = ImageMatrix[j, i].green;  //O(1)
                    B = ImageMatrix[j, i].blue;   //O(1)
                    Set.Add(R + (G << 8) + (B << 16));  //O(1) add to list distinct colors using shift 8bits to store(R,G,B)index.
                    Setij.Add(ImageMatrix[j, i]); //O(1)
                }
            }
            listOfIndex = Set.ToList();  //O(1) num of distinct colors
            ListOfDC = Setij.ToList();   //O(1)
            return listOfIndex.Count();  //O(1)
        }
       
        public static double CalculateElcideanDistance(int V1, int V2)
        {
            //function O(1) calculate distance between two node of colors 
            return Math.Sqrt((ListOfDC[V1].red - ListOfDC[V2].red) * (ListOfDC[V1].red - ListOfDC[V2].red)
                + (ListOfDC[V1].green - ListOfDC[V2].green) * (ListOfDC[V1].green - ListOfDC[V2].green)
                + (ListOfDC[V1].blue - ListOfDC[V2].blue) * (ListOfDC[V1].blue - ListOfDC[V2].blue));
        }

        // function 2
        public static double SumOfCost; //O(1)
        public static List<NodeOfcolors> ListOfMST; //O(1)
        public static double ElcideanDistance; //O(1)
        public static bool[] visited; //O(1)
        public static int vertex2, vertex1; //O(1)
        public static void MST()
        {
            //function O(N)+O(N^2)+O(N)=O(N^2)
            double min = double.MaxValue;  //O(1)
            visited = new bool[ListOfDC.Count()]; //O(1)

            ListOfMST = new List<NodeOfcolors>(ListOfDC.Count()); //O(1) //save the connected tree in this list of nodes
            ListOfMST.Add(new NodeOfcolors(-1, 0, 0)); //O(1)
            for (int i = 1; i < ListOfDC.Count(); i++) //O(N)->N:count of distinced colors 
                                                       //calculate the weight between 0 node and each node and get the minimum weights on edges
            {
                ElcideanDistance = CalculateElcideanDistance(0, i);  //O(1)
                if (ElcideanDistance < min)   //O(1)
                {
                    min = ElcideanDistance;   //O(1)
                    vertex1 = i;    //O(1)
                }
                ListOfMST.Add(new NodeOfcolors(0, i, ElcideanDistance));  //O(1)
            }
            visited[0] = true;  //O(1) //set 0 node as visited
            visited[vertex1] = true;  //O(1)

            for (int count = 1; count < ListOfDC.Count(); count++)  //O(N)->N:count of distinced colors 
            {
                //this loop check that every node is visited and make the min value max value at each loop
                min = double.MaxValue;  //O(1)
                vertex2 = vertex1;     //O(1)
                for (int i = 1; i < ListOfDC.Count(); i++)  //O(N)->N:count of distinced colors 
                {
                    //this loop check the visited nodes and calcualate the weight between vertcies and update the min
                    if (visited[i] == false)  //O(1)
                    {
                        ElcideanDistance = CalculateElcideanDistance(vertex2, i);  //O(1)
                        if (ElcideanDistance < ListOfMST[i].ElcideanDistance)
                        {
                            ListOfMST[i].ElcideanDistance = ElcideanDistance;  //O(1)
                            ListOfMST[i].from_Color = vertex2;  //O(1)
                        }

                        if (ListOfMST[i].ElcideanDistance < min)
                        {
                            min = ListOfMST[i].ElcideanDistance;    //O(1)
                            vertex1 = ListOfMST[i].to_Color;   //O(1)
                        }
                    }
                }
                visited[vertex1] = true;  //O(1)
            }
            SumOfCost = 0;  //O(1)
            for (int i = 0; i < ListOfDC.Count(); i++)  //O(D)->D:count of distinced colors //this loop calculate the total weight of the tree
            {
                SumOfCost += ListOfMST[i].ElcideanDistance;  //O(1)
            }
            for (int i = 0; i < ListOfMST.Count(); i++)
            {
                Console.WriteLine(ListOfMST[i].from_Color);
                Console.WriteLine(ListOfMST[i].to_Color);
                Console.WriteLine(ListOfMST[i].ElcideanDistance);
            }
        }


        //function 3
        public static List<KeyValuePair<int, int>> DistictColorsPixels_indeces;//O(1)
        public static List<int>[] DistictColorsPixels;//O(1)
        public static Dictionary<int, List<Nodes_clusters>> Adjasent_list = new Dictionary<int, List<Nodes_clusters>>();//O(1)
        //function 3
        public static void Editmsp(List<NodeOfcolors> list_Msp, int number_of_clusters)
        {   //function O(D * K)
            int loopcount = 0;//O(1)
            while (loopcount < number_of_clusters - 1)//O(K)
            {//from_Color, to_Color
                int c = 0;//O(1)
                int maxind = 0;//O(1)
                double max_distance = 0;//O(1)

                foreach (var item in list_Msp)//O(D)
                {


                    if (item.ElcideanDistance > max_distance)//O(1)
                    {
                        max_distance = item.ElcideanDistance;//O(1)
                        maxind = c;//O(1)
                    }
                    c++;//O(1)
                }
                list_Msp[maxind].ElcideanDistance = 0;//O(1)
                loopcount++;//O(1)
            }

        }
        public static void Removing_repeats(ref HashSet<int> visited, int currentvertex, ref HashSet<int> cluster_try)
        {

            // to Add node for the hash set
            cluster_try.Add(currentvertex);//O(1)
            visited.Add(currentvertex);//O(1)

            foreach (var neighbour in Adjasent_list[currentvertex])//O(v) v are current neighbour vertises in each node parent O(D) maxmum
            {
                if (!visited.Contains(neighbour.node))//O(n) count elements in list visited 
                    Removing_repeats(ref visited, neighbour.node, ref cluster_try);
            }

        }
        public static List<HashSet<int>> FindTheClustersForDistictColor(List<NodeOfcolors> list_Msp, int numberofclusters)
        {
            //function O(K*E)
            List<HashSet<int>> ClustersofColors = new List<HashSet<int>>();//O(1)

            // list menna msp          
            int numditictcolor = list_Msp.Count;//O(1)
            List<Nodes_clusters> Extention_adjlist = new List<Nodes_clusters>(numditictcolor);//O(1)

            // to delete k-1 edges
            Editmsp(list_Msp, numberofclusters);//O(D*K)
            // Fill The adgacent List for distict colors
            foreach (var item in list_Msp)//O(D)
            {

                if (item.ElcideanDistance != 0)//O(1)
                {

                    if (Adjasent_list.ContainsKey(item.from_Color))//O(v) v are current vertises in adjasent list O(D) maxmum
                    {
                        Nodes_clusters obj1 = new Nodes_clusters { node = item.to_Color, Distance = item.ElcideanDistance };//O(1)
                        Adjasent_list[item.from_Color].Add(obj1);//O(1)
                    }
                    else
                    {
                        List<Nodes_clusters> list = new List<Nodes_clusters>();//O(1)
                        Nodes_clusters obj2 = new Nodes_clusters { node = item.to_Color, Distance = item.ElcideanDistance };//O(1)
                        list.Add(obj2);//O(1)
                        Adjasent_list.Add(item.from_Color, list);//O(1)
                    }
                    if (Adjasent_list.ContainsKey(item.to_Color))//O(v) v are current vertises in adjasent list O(D) maxmum
                    {

                        Nodes_clusters obj3 = new Nodes_clusters { node = item.from_Color, Distance = item.ElcideanDistance };//O(1)
                        Adjasent_list[item.to_Color].Add(obj3);//O(1)
                    }
                    else
                    {
                        List<Nodes_clusters> list = new List<Nodes_clusters>();//O(1)
                        Nodes_clusters obj4 = new Nodes_clusters { node = item.from_Color, Distance = item.ElcideanDistance };//O(1)
                        list.Add(obj4);//O(1)
                        Adjasent_list.Add(item.to_Color, list);//O(1)
                    }

                }
                else
                {
                    if (!Adjasent_list.ContainsKey(item.from_Color))//O(v) v are current vertises in adjasent list O(D) maxmum
                    {
                        List<Nodes_clusters> list = new List<Nodes_clusters>();//O(1)
                        Adjasent_list.Add(item.from_Color, list);//O(1)
                    }
                    if (!Adjasent_list.ContainsKey(item.to_Color))//O(v) v are current vertises in adjasent list O(D) maxmum
                    {
                        List<Nodes_clusters> list = new List<Nodes_clusters>();//O(1)
                        Adjasent_list.Add(item.to_Color, list);//O(1)
                    }




                }
            }
            // to delete repeats nodes from adgasent list
            HashSet<int> visitedNodes = new HashSet<int>();//O(1)
            foreach (var vertex in Adjasent_list)//O(v) v are vetices
            {
                if (!visitedNodes.Contains(vertex.Key))//O(v) v are vetices in list visited node
                {
                    HashSet<int> set = new HashSet<int>();//O(1)
                    Removing_repeats(ref visitedNodes, vertex.Key, ref set);//O(D)
                    ClustersofColors.Add(set);//O(1)

                }
            }
            Console.WriteLine("done");//O(1)
            return ClustersofColors;//O(1)

        }



        // function 4
        public static Dictionary<int, int> FindTherepresentiveColorForeachcluster(List<HashSet<int>> ClustersofColors, int number_distenctcolor)//O(D)=O(K*N) D is ditinct color For every Cluster
        { //O(D)
            int avrred = 0, avrgreen = 0, avrblue = 0;//O(1)
            int counterrr;//O(1)
            int finalcolor;//O(1)
            int[] collection_colorredsum = new int[number_distenctcolor];//O(1)
            int[] collection_colorgreensum = new int[number_distenctcolor];//O(1)
            int[] collection_colorbluesum = new int[number_distenctcolor];//O(1) 
            Dictionary<int, int> colorandreprsentivecolor = new Dictionary<int, int>();//O(1)

            // to calc centroide color for each cluster

            foreach (var item1 in ClustersofColors)
            {//O(K)
             //  HashSet<int> hass = item1;//O(1)
                counterrr = 0;//O(1)
                avrred = 0;//O(1)
                avrgreen = 0;//O(1)
                avrblue = 0;//O(1)
                finalcolor = 0;//O(1)
                foreach (var item2 in item1)
                {//O(n) n is ditinct color For every Cluster
                    collection_colorredsum[counterrr] = (byte)(item2);//O(1)
                    collection_colorgreensum[counterrr] = (byte)(item2 << 8);//O(1)
                    collection_colorbluesum[counterrr] = (byte)(item2 << 16);//O(1)
                    counterrr = counterrr + 1;//O(1)
                }//R + (G << 8) + (B << 16)
                for (int c = 0; c < counterrr; c++) //O(D) D is ditinct color For every Cluster
                {
                    avrred += collection_colorredsum[counterrr];//O(1) 
                    avrgreen += collection_colorgreensum[counterrr];//O(1) 
                    avrblue += collection_colorbluesum[counterrr];//O(1) 


                }
                avrred = avrred / counterrr;//O(1) 
                avrgreen = avrgreen / counterrr;//O(1) 
                avrblue = avrblue / counterrr;//O(1) 
                finalcolor = (avrred) + (avrgreen << 8) + (avrblue << 16);//O(1) 
                foreach (var item2 in item1)//O(D) D is ditinct color For every Cluster
                {
                    colorandreprsentivecolor.Add(item2, finalcolor);//O(1) 
                }

            }
            Console.WriteLine("done");//O(1) 
            return colorandreprsentivecolor;//O(1) 


        }
        // function 5
        public static RGBPixel[,] QuantizationTheImage(RGBPixel[,] Matrixforimagepath, Dictionary<int, int> colorandreprsentivecolor)
        {
            int color = 0;//O(1) 
            int counter_rows = GetHeight(Matrixforimagepath);//O(1) 
            int counter_columns = GetWidth(Matrixforimagepath);//O(1) 
            Dictionary<int, colorindecesmatrix> listcolors = new Dictionary<int, colorindecesmatrix>(counter_rows * counter_columns);//O(1) 
            int counter_loop1 = 0;//O(1) 
            int counter_loop2 = 0;//O(1) 
            // to pring representive color in each cluster
            colorindecesmatrix struc = new colorindecesmatrix();//O(1) 
            while (counter_loop1 < counter_rows)
            {//O👎

                while (counter_loop2 < counter_columns)//O👎
                {
                    int red = Matrixforimagepath[counter_loop1, counter_loop2].red;//O(1)
                    int blue = Matrixforimagepath[counter_loop1, counter_loop2].blue;//O(1)
                    int green = Matrixforimagepath[counter_loop1, counter_loop2].green;//O(1)
                    color = (red << 16) + (green << 8) + blue;//O(1)
                    struc.nodeindex1 = counter_loop1;//O(1)
                    struc.nodeindex2 = counter_loop2;//O(1)
                    if (!listcolors.ContainsKey(color))
                    {
                        listcolors.Add(color, struc);//O(1)
                    }
                    counter_loop2 = counter_loop2 + 1;//O(1)
                }
                counter_loop1 = counter_loop1 + 1;//O(1)

            }
            foreach (var item in listcolors)//O(N^2)
            {
                int value = colorandreprsentivecolor[item.Key];//O(1)
                Matrixforimagepath[item.Value.nodeindex1, item.Value.nodeindex2].red = (byte)(value);//O(1)
                Matrixforimagepath[item.Value.nodeindex1, item.Value.nodeindex2].green = (byte)(value << 8);//O(1)
                Matrixforimagepath[item.Value.nodeindex1, item.Value.nodeindex2].blue = (byte)(value << 16);//O(1)

            }
            Console.WriteLine("done2");//O(1) 
            return Matrixforimagepath;//O(1) 

        }

    }
}


