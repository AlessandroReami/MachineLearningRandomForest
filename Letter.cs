using Accord.Math;


namespace Project
{
    public class Letter
    {
        public byte Sex { get; set; }
        public byte Age { get; set; }
        public byte Work { get; set; }
        public byte Family { get; set; }
        public byte School { get; set; }
        public byte ExitDia { get; set; }
        public byte Drug { get; set; }    // called "Doppia diagnosi" in medicine
        public byte Tso { get; set; }     // also known as "Trattamento Sanitario Obbligatorio"
        public byte AfterDim { get; set; }
        public ushort Days { get; set; }    // notice: there are person with MORE THAN 255 days at SPDC
        public byte Nl { get; set; }
        public byte Lai { get; set; }
        public byte Ad { get; set; }
        public byte Stabil { get; set; }
        public byte Bdz { get; set; }
        public byte OtherMedicine { get; set; }

        public byte OtherLet { get; set; }     // number of dimission letter

        public byte Dim {get;} // number of fields

        public byte Label { get; set; } // Label: Revolving=1,  not revolving=0



        public Letter(string sample)    //constructor: it throw an Exception if there is a missing value
        {
            char delimiter = ',';
            string[] values = sample.Split(delimiter);
            Dim = (byte)values.Length;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].IsEqual("")) { throw new Exception("There is a missing value."); }
            }

            Sex = Byte.Parse(values[0]);
            Age = Byte.Parse(values[1]);
            Work = Byte.Parse(values[2]);
            Family = Byte.Parse(values[3]);
            School = Byte.Parse(values[4]);
            ExitDia = Byte.Parse(values[5]);
            Drug = Byte.Parse(values[6]);
            Tso = Byte.Parse(values[7]);
            AfterDim = Byte.Parse(values[8]);
            Days = ushort.Parse(values[9]);
            Nl = Byte.Parse(values[10]);
            Lai = Byte.Parse(values[11]);
            Ad = Byte.Parse(values[12]);
            Stabil = Byte.Parse(values[13]);
            Bdz = Byte.Parse(values[14]);
            OtherMedicine = Byte.Parse(values[15]);
            OtherLet = (byte)(Byte.Parse(values[16]) - 1);   //in the csv file it's counted the total number
            Label = Byte.Parse(values[17]);

        }

        public ushort[] ToArrayWithoutLabel()
        {
            ushort[] array = new ushort[Dim - 1];
            array[0] = Sex;
            array[1] = Age;
            array[2] = Work;
            array[3] = Family;
            array[4] = School;
            array[5] = ExitDia;
            array[6] = Drug;
            array[7] = Tso;
            array[8] = AfterDim;
            array[9] = Days;
            array[10] = Nl;
            array[11] = Lai;
            array[12] = Ad;
            array[13] = Stabil;
            array[14] = Bdz;
            array[15] = OtherMedicine;
            array[16] = OtherLet;
            return array;

        }

        public void Description()
        {
            Console.WriteLine("The data inside the letter are:" + Environment.NewLine +
                               "Sex= " + Sex + Environment.NewLine +
                               "Age= " + Age + Environment.NewLine +
                               "Work condition= " + Work + Environment.NewLine +
                               "Family condition= " + Family + Environment.NewLine +
                               "Schooling= " + School + Environment.NewLine +
                               "Exit diagnosis= " + ExitDia + Environment.NewLine +
                               "Drug= " + Drug + Environment.NewLine +
                               "Tso= " + Tso + Environment.NewLine +
                               "After dimission= " + AfterDim + Environment.NewLine +
                               "Days at SPDC= " + Days + Environment.NewLine +
                               "NL as therapy= " + Nl + Environment.NewLine +
                               "LAI as therapy= " + Lai + Environment.NewLine +
                               "AD as therapy= " + Ad + Environment.NewLine +
                               "STABILIZERS as therapy= " + Stabil + Environment.NewLine +
                               "BENZODIAZEPINES as therapy= " + Bdz + Environment.NewLine +
                               "OTHER MEDICINE as therapy= " + OtherMedicine + Environment.NewLine +
                               "Other letters= " + OtherLet + Environment.NewLine +
                               "will he/she be revolving? " + Label + Environment.NewLine);
        }

    }
}