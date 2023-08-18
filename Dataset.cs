using Accord.Math;


namespace Project
{
    public class Dataset
    {

        // absolute position
        public string Path { get; set; }
        public int NumbLetter { get; set; }     
        public byte NumbFeature { get; }
        public Letter[] MatrixLett{get;set;}

        public Letter[] TrainingSet{get;set;}
        public double TrainingRatio{get;set;}
        public Letter[] TestSet{get;set;}
        public int[] IndexTraining { get; set; }

        public Letter[][] CrossValidation{get;set;}

        public int NumbOfSuddivisions{get;set;}



        public Dataset(string Path, byte[] index, byte LabelIndex, double TrainingRatio, int NumbOfSuddivisions)   //it throw an Exception if index.length is not 17
        {
            this.NumbOfSuddivisions = NumbOfSuddivisions;
            this.Path = Path;
            this.TrainingRatio = TrainingRatio;

            //block for NumbFeature
            if (index.Length != 17)
            {
                throw new System.Exception("The number of indexes provided are not enought or too much, 17 indexes are needed");
            }


            int i = 0;

            //  block for NumbLetter
            try
            {
                StreamReader reader = new StreamReader(Path);
                bool tmpbool = true;
                while (!reader.EndOfStream && tmpbool)
                {
                    string Line = reader.ReadLine();
                    if (Line[0].Equals(';'))       // we don't want to consider the last lines of the file that only contains ";"
                    {
                        tmpbool = false;
                        i--;  //because we are checking the condition after
                    }
                    i++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            this.NumbLetter = i;     // set of dimension (i.e. the number of the letters), this value can be modified in the next lines if some letters are removed
            this.NumbFeature = (byte)index.Length;      // set of the number of feature (i.e. the number of columns)
            MatrixLett = new Letter[i];  //set the matrix length



            //  block for MatrixLett
            try
            {
                int counterEx = 0; // counter of throwed exception

                StreamReader reader = new StreamReader(Path);

                // build MatrixLetter and check correctness of data
                for (int kk = 0; kk < NumbLetter; kk++)    
                {

                    string line = reader.ReadLine();
                    string[] values = line.Split(';');    //notice that our csv file use semicolons ";" instead of commas ","

                    string tmp = "";
                    for (int jj = 0; jj < index.Length; jj++)
                    {
                        if (values[index[jj]].IsEqual(""))
                        {
                            Console.Write("There is a missing value on the " + (kk + 1) + "-th line at the " + (index[jj] + 1) + "-th column, which is needed, so it's been removed all the line. "); 
                            //will be throwed the exception of Letter class
                        }
                        tmp += values[index[jj]] + ",";
                    }
                    tmp += values[LabelIndex];

                    try
                    {
                        MatrixLett[kk] = new Letter(tmp);
                        //if (kk < 10) { MatrixLett[kk].Description(); }
                    }
                    catch (OverflowException ex)
                    {
                        counterEx++;
                        Console.WriteLine(ex.Message + " The " + (kk + counterEx) + "-th  letter has incompatible data, so it's been removed. ");
                        kk--;
                        NumbLetter--;
                    }
                    catch (Exception ex)   // catch the exception on class Letter
                    {
                        counterEx++;
                        Console.WriteLine(ex.Message);
                        kk--;
                        NumbLetter--;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Error during the construction of the dataset, check also if the CSV file uses commas or semicolons");
            }


            //split in test and training set
            IndexTraining = new int[(int)Math.Round(NumbLetter * TrainingRatio)];
            IndexTraining = GenerateRandomIndex(NumbLetter, IndexTraining.Length);
            //for (int pippo = 0; pippo < IndexTraining.Length; pippo++) { Console.WriteLine(IndexTraining[pippo]); }
            TrainingSet = new Letter[IndexTraining.Length];
            TestSet = new Letter[NumbLetter - IndexTraining.Length];


            // build up test and training sets
            i = 0;
            int j = 0;
            for (int ll = 0; ll < NumbLetter; ll++)
            {
                int tmp = Array.IndexOf(IndexTraining, ll);
                //Console.WriteLine("element " + ll + " is in position " + tmp);
                if (tmp>=0) 
                {
                    TrainingSet[tmp] = MatrixLett[ll];
                    i++;
                    
                }
                else
                {
                    TestSet[j] = MatrixLett[ll];
                    j++;
                }
            }

            // build CrossValidation jagged array
            this.CrossValidation = new Letter[NumbOfSuddivisions][]; // notice that the array TrainingSet is already randomized
            int l = TrainingSet.Length;
            int k = l % NumbOfSuddivisions;
            int h = (l - k) / NumbOfSuddivisions;
            //Console.WriteLine("suddivisioni " + numbOfSuddivision + " trainingset lunghezza " + l + " resto: " + k + " quoziente: " + h);
            for (int ii = 0; ii < NumbOfSuddivisions; ii++)
            {
                if (ii < k)
                {
                    CrossValidation[ii] = new Letter[h + 1];
                    Array.Copy(TrainingSet, ii * (h + 1), CrossValidation[ii], 0, h + 1);
                }
                else
                {
                    CrossValidation[ii] = new Letter[h];
                    Array.Copy(TrainingSet, k + (ii * h), CrossValidation[ii], 0, h);
                }
            }
        }

        // private method used to extract an array of features from an array of letters
        private int[][] ToJag(Letter[] array)
        {
            int[][] Matrix = new int[array.Length][];
            for (int i = 0; i < array.Length; i++)
            {
                ushort[] tmp = array[i].ToArrayWithoutLabel();
                Matrix[i] = new int[NumbFeature];

                for (int j = 0; j < NumbFeature; j++)
                {
                    Matrix[i][j] = tmp[j];
                }

            }
            return Matrix;
        }

        public int[][] ToJagTraining()
        {
            return ToJag(TrainingSet);
        }

        public int[][] ToJagTest()
        {
            return ToJag(TestSet);
        }

        public int[][] ToJagCrossValidationTraining(int k)
        {
            int x = TrainingSet.Length - CrossValidation[k].Length; //we want a matrix without the k-th crossValidation set
            //Console.WriteLine("x "+x);    

            int counter = 0; // counts how many entries we already filled of Matrix

            int[][] Matrix = new int[x][];
            for (int ii = 0; ii < NumbOfSuddivisions; ii++)
            {
                if (ii == k) { if (ii == NumbOfSuddivisions - 1) { break; } ii++; } //skip the k-th set
                //Console.WriteLine(ii);   //check if we are avoid the right CrossValidation set


                Array.Copy(ToJag(CrossValidation[ii]), 0, Matrix, counter, CrossValidation[ii].Length);
                counter += CrossValidation[ii].Length;
                //Console.WriteLine(counter);
            }
            return Matrix;
        }

        public int[][] ToJagCrossValidationTest(int k)
        {
            return ToJag(CrossValidation[k]);
        }


        // private method used to extract an array of labels from an array of letters
        private int[] Label(Letter[] array)
        {
            int[] NewArray = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                NewArray[i] = array[i].Label;
            }
            return NewArray;
        }
        public int[] LabelTrainingArray()
        {
            return Label(TrainingSet);
        }
        public int[] LabelTestArray()
        {
            return Label(TestSet);
        }

        public int[] LabelCrossValidationTrainingArray(int k)
        {
            int x = TrainingSet.Length - CrossValidation[k].Length; //we want a matrix without the k-th crossValidation set
            int counter = 0;

            int[] array = new int[x];
            for (int ii = 0; ii < NumbOfSuddivisions; ii++)
            {
                if (ii == k) { if (ii == NumbOfSuddivisions - 1) { break; } ii++; } //skip the k-th set
                //Console.WriteLine(ii);     // as before
                Array.Copy(Label(CrossValidation[ii]), 0, array, counter, Label(CrossValidation[ii]).Length);
                counter += CrossValidation[ii].Length;
            }
            return array;
        }
        public int[] LabelCrossValidationTestArray(int k)
        {
            return Label(CrossValidation[k]);
        }

        public void Description()  // provide a description of all the letters in the dataset, we don't override ToString since the resulting string would be very big
        {
            for (int i = 0; i < NumbLetter; i++)
            {
                MatrixLett[i].Description();
            }
        }

        // method that generates a random permutation of the indexes between 0 and MaxValue, it generates only an array with length= length
        private static int[] GenerateRandomIndex(int MaxValue, int length)
        {

            int[] Array = new int[length];
            int seed = 0;
            Random rand = new Random(seed); //  set the seed
            for (int i = 0; i < Array.Length; i++)
            {
                Array[i] = rand.Next(0, MaxValue);
                for (int j = 0; j < i; j++)
                {
                    if (Array[j] == Array[i])
                    {
                        i--;
                        break;
                    }
                }
            }
            return Array;
        }

    }

}