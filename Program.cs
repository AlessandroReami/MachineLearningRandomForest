using Accord.MachineLearning.DecisionTrees;
using Accord.Math.Optimization.Losses; // Accord Library for evaluation metrics
using System.Diagnostics;



namespace Project
{
    public static class Program
    {

        public static void Main()
        {



            string BasePath = Directory.GetCurrentDirectory().ToString().Substring(0, Directory.GetCurrentDirectory().ToString().IndexOf("bin") - 1);
            string DatasetPath = BasePath + Path.DirectorySeparatorChar  + "Dataset_only_useful_letters.csv";    
            //string DatasetPath = BasePath + Path.DirectorySeparatorChar + "Dataset" + Path.DirectorySeparatorChar + "Dataset_all_letters.csv";    


            byte[] indexFeature = { 6, 7, 13, 15, 17, 23, 19, 20, 31, 34, 24, 25, 26, 27, 28, 29, 4 }, NumbTree = { 10, 20 };
            byte LabelIndex = 5;
            double TrainingRatio = 0.85, Error, BestError = 1, BestRatio = 0, BestCoverage = 0;
            double[] Ratio = { 0.80, 0.6, 0.45 };
            double[] Coverage = { 0.9, 0.7};
            int BestNumbTree=0, numbOfSuddivision = NumbTree.Length * Ratio.Length*Coverage.Length;
            double accuracy = 0, errorRate = 0, sensitivity = 0, specificity = 0;

            Dataset DATASET;

            try
            {

                // FULL DATASET
                Console.WriteLine("Building the DATASET, it can take a while (15 seconds)...");   // on my pc it takes less than 15 seconds
                DATASET = new Dataset(DatasetPath, indexFeature, LabelIndex, TrainingRatio, numbOfSuddivision);
                Console.WriteLine("Buinding DATASET: success" + Environment.NewLine);
                //Console.WriteLine("numb of letters: "+ DATASET.NumbLetter);

                //DATASET.Description();
                int[][] TrainingData = DATASET.ToJagTraining();
                int[] LabelTraining = DATASET.LabelTrainingArray();
                int[][] TestData = DATASET.ToJagTest();
                int[] LabelTest = DATASET.LabelTestArray();

                // cross-validation
                Console.WriteLine("Start CrossValidation...");
                
                for (int i = 0; i < NumbTree.Length; i++)
                {
                    for (int j = 0; j < Ratio.Length; j++)
                    {
                        for (int k = 0; k < Coverage.Length; k++)
                        {
                            //Console.WriteLine((i * Ratio.Length + j) * Coverage.Length + k);
                            int[][] TrainingDataCV = DATASET.ToJagCrossValidationTraining((i * Ratio.Length + j)*Coverage.Length +k);
                            int[] LabelTrainingCV = DATASET.LabelCrossValidationTrainingArray((i * Ratio.Length + j) * Coverage.Length + k);
                            int[][] TestDataCV = DATASET.ToJagCrossValidationTest((i * Ratio.Length + j) * Coverage.Length + k);
                            int[] LabelTestCV = DATASET.LabelCrossValidationTestArray((i * Ratio.Length + j) * Coverage.Length + k);

                            Error = TryRandomForest(NumbTree[i], Ratio[j], Coverage[k], TrainingDataCV, LabelTrainingCV, TestDataCV, LabelTestCV);
                            //Console.WriteLine(BestError + " " + Error);
                            if (Error < BestError)
                            {
                                BestError = Error;
                                BestNumbTree = NumbTree[i];
                                BestRatio = Ratio[j];
                                BestCoverage = Coverage[k];
                                //Console.WriteLine("New best: "+ BestNumbTree+" and "+ BestRatio+Environment.NewLine);
                            }
                        }
                    }
                }
                Console.WriteLine("End CrossValidiation" + Environment.NewLine);




                Console.WriteLine("Using the best values arised from the crossvalidation:");

                var teacher = new RandomForestLearning()
                {
                    NumberOfTrees = BestNumbTree,
                    SampleRatio = BestRatio,
                    CoverageRatio = BestCoverage,
                };
                var forest = teacher.Learn(TrainingData, LabelTraining);

                double errorTraining = new ZeroOneLoss(LabelTraining).Loss(forest.Decide(TrainingData));
                double errorTest = new ZeroOneLoss(LabelTest).Loss(forest.Decide(TestData));

                Console.WriteLine("Number of trees: " + BestNumbTree + " Sample ratio: " + BestRatio+ " Coverage ratio: "+ BestCoverage);
                Console.WriteLine("Misclassified ratio on Training Set: " + errorTraining + ". Misclassified ratio on Test Set: " + errorTest);
                int[] predicted = forest.Decide(TestData);

                //for (int i=0; i<predicted.Length; i++) { Console.WriteLine("true value: " + LabelTest[i] +  " predicted value " + predicted[i]);  }


                //confusion matrix matrix
                int truepositive = 0, falsepositive = 0, truenegative = 0, falsenegative = 0;
                for (int i = 0; i < predicted.Length; i++)
                {
                    if (predicted[i] == LabelTest[i] && predicted[i] == 1)
                        truepositive++;
                    else if (predicted[i] == LabelTest[i] && predicted[i] == 0)
                        truenegative++;
                    else if (predicted[i] != LabelTest[i] && predicted[i] == 1)
                        falsepositive++;
                    else falsenegative++;
                }
                
                accuracy = (double)(truenegative + truepositive)/predicted.Length;
                errorRate = 1-accuracy;
                sensitivity = (double)truepositive / (truepositive + falsenegative);
                specificity = (double)truenegative / (truenegative + falsepositive);
                


                Console.WriteLine(Environment.NewLine+ "Confusion Matrix" + Environment.NewLine +
                                  "true positive: " + truepositive + "    " + "false positive: " + falsepositive + Environment.NewLine +
                                  "false negative: " + falsenegative + "    " + "true negative: " + truenegative+ Environment.NewLine+ 
                                  "accuracy: " + accuracy+ "    "+ "error rate: "+ errorRate + "    "+ "sensitivity: "+ sensitivity + "    "+"specificity: "+ specificity );



                //Run100Forests(BestNumbTree, BestRatio, BestCoverage, TrainingData, LabelTraining, TestData, LabelTest);
            }


            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }





        }

        //static public void Run100Forests(int NumbTree, double Ratio, double Coverage, int[][] TrainingData, int[] LabelTraining, int[][] TestData, int[] LabelTest) {

        //    double MeanAccuracy=0, MeanErrorRate=0, MeanSensitivity=0, MeanSpecificity=0;
        //    double accuracy=0, errorRate=0, sensitivity=0,  specificity=0;
        //    int truepositive = 0, falsepositive = 0, truenegative = 0, falsenegative = 0;
        //    double MeanTP = 0, MeanFP = 0, MeanTN = 0, MeanFN = 0;

        //    for (int j = 0; j < 100; j++) {
        //        truepositive = 0; falsepositive = 0; truenegative = 0; falsenegative = 0;
        //        Console.WriteLine(j);
        //        var teacher = new RandomForestLearning()
        //        {
        //            NumberOfTrees = NumbTree,
        //            SampleRatio = Ratio,
        //            CoverageRatio = Coverage,
        //        };
        //        var forest = teacher.Learn(TrainingData, LabelTraining);

        //        int[] predicted = forest.Decide(TestData);

        //        //confusion matrix matrix
        //        for (int i = 0; i < predicted.Length; i++)
        //        {
        //            if (predicted[i] == LabelTest[i] && predicted[i] == 1)
        //                truepositive++;
        //            else if (predicted[i] == LabelTest[i] && predicted[i] == 0)
        //                truenegative++;
        //            else if (predicted[i] != LabelTest[i] && predicted[i] == 1)
        //                falsepositive++;
        //            else falsenegative++;
        //        }

        //        accuracy = (double)(truenegative + truepositive) / predicted.Length;
        //        errorRate = 1 - accuracy;
        //        sensitivity = (double)truepositive / (truepositive + falsenegative);
        //        specificity = (double)truenegative / (truenegative + falsepositive);

        //        MeanTP += (double)truepositive / 100;
        //        MeanTN+= (double)truenegative / 100;
        //        MeanFP += (double)falsepositive / 100;
        //        MeanFN += (double)falsenegative / 100;

        //        MeanAccuracy += accuracy / 100;
        //        MeanErrorRate += errorRate / 100;
        //        MeanSensitivity += sensitivity / 100;
        //        MeanSpecificity += specificity / 100;
                

        //    }
        //    Console.WriteLine(Environment.NewLine + "Mean values of the Confusion Matrix" + Environment.NewLine +
        //                              "true positive: " + MeanTP + "    " + "false positive: " + MeanFP + Environment.NewLine +
        //                              "false negative: " + MeanFN + "    " + "true negative: " + MeanTN + Environment.NewLine +
        //                              "accuracy: " + MeanAccuracy + "    " + "error rate: " + MeanErrorRate + "    " + "sensitivity: " + MeanSensitivity + "    " + "specificity: " + MeanSpecificity );

        //}

        static public double TryRandomForest(int NumbTree, double Ratio,double Coverage, int[][] TrainingData, int[] LabelTraining, int[][] TestData, int[] LabelTest)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var teacher = new RandomForestLearning()
            {
                NumberOfTrees = NumbTree,
                SampleRatio = Ratio,
                CoverageRatio = Coverage
            };
            var forest = teacher.Learn(TrainingData, LabelTraining);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            //int[] predicted = forest.Decide(TestData);
            double errorTraining = new ZeroOneLoss(LabelTraining).Loss(forest.Decide(TrainingData));
            double errorTest = new ZeroOneLoss(LabelTest).Loss(forest.Decide(TestData));

            Console.WriteLine("Number of trees: " + NumbTree + " Sample ratio: " + Ratio + " Coverage ratio: " + Coverage);
            Console.WriteLine("Misclassified ratio on Training Set: " + errorTraining + ". Misclassified ratio on Test Set: " + errorTest);
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime + Environment.NewLine + Environment.NewLine);

            return errorTest;
        }
    }
}