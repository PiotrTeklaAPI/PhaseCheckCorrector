using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;
using TSG = Tekla.Structures.Geometry3d;
using System.Collections;
using Tekla.Structures.Analysis;
using System.Globalization;



namespace PhaseCheckCorrector
{
    public partial class Form1 : Form
    {
        private readonly Model model = new Model();
        List<Part> modelObjects = new List<Part>();
        List<Reinforcement> modelObjectsRebar = new List<Reinforcement>();
        List<Reinforcement> modelObjectsRebarUnattached = new List<Reinforcement>();
        List<Assembly> modelObjectsAssembly = new List<Assembly>();
        public Form1()
        {
            InitializeComponent();
        }
        public List<Part> getModelObjectsPart(Model model)
        {
            // metoda tworząca listę partów, których Assembly Prefix jest równy textBox1
            System.Type[] TypePart = new System.Type[1];
            TypePart.SetValue(typeof(Part), 0);
            ModelObjectEnumerator enumerator = model.GetModelObjectSelector().GetAllObjectsWithType(TypePart);
            while (enumerator.MoveNext())
            {
                Part modelObject = enumerator.Current as Part;
                if (modelObject != null)
                {
                    if (modelObject.GetAssembly().AssemblyNumber.Prefix == textBox1.Text)
                    {
                        modelObjects.Add(modelObject);
                    }
                }
            }
            return modelObjects;
        }
        public List<Reinforcement> getModelObjectsRebar(Model model)
        {
            // metoda tworząca listę prętów dołączonych do castunitu, których fatherpartprefix zaczyna się od tego co jest w textBox1
            System.Type[] TypeReinforcement = new System.Type[1];
            TypeReinforcement.SetValue(typeof(Reinforcement), 0);
            ModelObjectEnumerator enumerator = model.GetModelObjectSelector().GetAllObjectsWithType(TypeReinforcement);
            while (enumerator.MoveNext())
            {
            Reinforcement modelObjectRebar = enumerator.Current as Reinforcement;
                if (modelObjectRebar != null)
                {
                int ValuePartID = -1;
                modelObjectRebar.GetReportProperty("PART.ID", ref ValuePartID);
                string FatherPrefix = "A";
                modelObjectRebar.GetReportProperty("CAST_UNIT_POS", ref FatherPrefix);
                    if (ValuePartID > 0 &&  FatherPrefix.ToString().StartsWith(textBox1.Text) == true)
                        {
                            modelObjectsRebar.Add(modelObjectRebar);
                        }
                }
            }
            return modelObjectsRebar;
        }
        public List<Reinforcement> getModelObjectsRebarUnattached(Model model)
        {
            // metoda tworząca listę prętów nie dołączonych do żadnego cast unitu.
            System.Type[] TypeReinforcementUn = new System.Type[1];
            TypeReinforcementUn.SetValue(typeof(Reinforcement), 0);
            ModelObjectEnumerator enumerator = model.GetModelObjectSelector().GetAllObjectsWithType(TypeReinforcementUn);
            while (enumerator.MoveNext())
            { 
                Reinforcement modelObjectRebar = enumerator.Current as Reinforcement;
                if (modelObjectRebar != null)
                {
                    int ValuePartID = -1;
                    modelObjectRebar.GetReportProperty("PART.ID", ref ValuePartID);
                    if (ValuePartID > 0)
                    {
                        continue;  
                    }
                    else
                    {
                        modelObjectsRebarUnattached.Add(modelObjectRebar);
                    }
                }
            }
            return modelObjectsRebarUnattached;
        }
        public List<Assembly> getModelObjectsAssembly(Model model)
        {
            // metoda tworząca listę subassembly, których Assembly Prefix jest równy textBox1
            System.Type[] TypeAssembly = new System.Type[1];
            TypeAssembly.SetValue(typeof(Assembly), 0);
            ModelObjectEnumerator enumerator = model.GetModelObjectSelector().GetAllObjectsWithType(TypeAssembly);
            modelObjectsAssembly.Clear();
            while (enumerator.MoveNext())
            {
                Assembly modelObjectAssembly = enumerator.Current as Assembly;
                if (modelObjectAssembly != null && String.IsNullOrEmpty(textBox1.Text) && modelObjectAssembly.GetAssemblyType() == Assembly.AssemblyTypeEnum.PRECAST_ASSEMBLY)
                {
                    modelObjectsAssembly.Add(modelObjectAssembly);
                    continue;
                }
                if (modelObjectAssembly != null && modelObjectAssembly.AssemblyNumber.Prefix == textBox1.Text)
                {
                        modelObjectsAssembly.Add(modelObjectAssembly);
                }
            }
            if (modelObjectsAssembly.Count == 0)
            {
                Operation.DisplayPrompt("No elements selected, please try again.");
            }
            return modelObjectsAssembly;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // Sprawdzam połączenie z programem.
            if (!model.GetConnectionStatus())
            {
                Operation.DisplayPrompt("Tekla Structures not connected!");
                return;
            }
            else
            {
                Operation.DisplayPrompt((string.Format("Connected to Tekla!")));
                return;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            modelObjectsAssembly.Clear();
            modelObjects.Clear();
            modelObjectsRebar.Clear();
            Operation.DisplayPrompt((string.Format("Please wait, selecting objects...")));
            if (listBox1.SelectedItem == null)
            {
                Operation.DisplayPrompt((string.Format("No type selected.")));
            }
            if(listBox1.GetSelected(0) == true)
            {
                List<Part> modelObjects = getModelObjectsPart(model);
                Tekla.Structures.Model.UI.ModelObjectSelector selector = new Tekla.Structures.Model.UI.ModelObjectSelector();
                selector.Select(new System.Collections.ArrayList(modelObjects));
            }
            if(listBox1.GetSelected(1) == true)
            {
                List<Reinforcement> modelObjectsRebar = getModelObjectsRebar(model);
                Tekla.Structures.Model.UI.ModelObjectSelector selector = new Tekla.Structures.Model.UI.ModelObjectSelector();
                selector.Select(new System.Collections.ArrayList(modelObjectsRebar));
            }
            if (listBox1.GetSelected(3) == true)
            {
                List<Assembly> modelObjectsAssembly = getModelObjectsAssembly(model);
                Tekla.Structures.Model.UI.ModelObjectSelector selector = new Tekla.Structures.Model.UI.ModelObjectSelector();
                selector.Select(new System.Collections.ArrayList(modelObjectsAssembly));
            }
            Operation.DisplayPrompt((string.Format("Objects selected!")));
        }
        //button1 służy do zaznaczenia w programie Listy obiektów dodanych w metodach.
        private void button4_Click(object sender, EventArgs e)
        {
            modelObjectsRebarUnattached.Clear();
            Operation.DisplayPrompt((string.Format("Please wait, selecting objects...")));
            if (listBox1.SelectedItem == null)
            {
                Operation.DisplayPrompt((string.Format("No type selected.")));
            }
            if (listBox1.GetSelected(2) == true)
            {
                List<Reinforcement> modelObjectsRebarUnattached = getModelObjectsRebarUnattached(model);
                Tekla.Structures.Model.UI.ModelObjectSelector selector = new Tekla.Structures.Model.UI.ModelObjectSelector();
                selector.Select(new System.Collections.ArrayList(modelObjectsRebarUnattached));
            }
            Operation.DisplayPrompt((string.Format("Objects selected!")));
        }
        //button4 służy do zaznaczenia prętów nie dołączonych do żadnego cast unitu.
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            if (listBox1.GetSelected(0) == true)
            {
                foreach (Part part in modelObjects)
                {
                    part.GetPhase(out Phase phase1);
                    if (!listBox2.Items.Contains(phase1.PhaseName))
                    {
                        listBox2.Items.Add(phase1.PhaseName);
                    }
                }
                listBox2.Sorted = true;
            }
            if (listBox1.GetSelected(2) == true)
            {
                    foreach (Reinforcement rebarunattached in modelObjectsRebarUnattached)
                    {
                        rebarunattached.GetPhase(out Phase phase2);
                        if (!listBox2.Items.Contains(phase2.PhaseName))
                        {
                            listBox2.Items.Add(phase2.PhaseName);
                        }
                    }
                    listBox2.Sorted = true;
            }
            if (listBox1.GetSelected(1) == true)
                {
                    foreach (Reinforcement rebar in modelObjectsRebar)
                    {
                        rebar.GetPhase(out Phase phase3);
                        if (!listBox2.Items.Contains(phase3.PhaseName))
                        {
                            listBox2.Items.Add(phase3.PhaseName);
                        }
                    }
                    listBox2.Sorted = true;
                }
            if (listBox1.GetSelected(3) == true)
            {
                foreach (Assembly assembly in modelObjectsAssembly)
                {
                    foreach (Assembly subassembly in assembly.GetSubAssemblies())
                    {
                        subassembly.GetPhase(out Phase phase4);
                        if (!listBox2.Items.Contains(phase4.PhaseName))
                        {
                            listBox2.Items.Add(phase4.PhaseName);
                        }
                    }
                }
                listBox2.Sorted = true;
            }
        }
        //button2 służy do odszukania faz poszczególnych obiektów i wyświetlenia w listboxie.
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.GetSelected(0) == true)
            {
                foreach (Part part in modelObjects)
                {
                    Assembly Mainpart = part.GetAssembly();
                    Mainpart.GetPhase(out Phase AssemblyPhase);
                    part.GetPhase(out Phase PartPhase);
                    if(AssemblyPhase != PartPhase)
                    {
                        part.SetPhase(AssemblyPhase);
                    }
                }
                model.CommitChanges();
            }
            if (listBox1.GetSelected(1) == true)
            {
                foreach (Reinforcement rebar in modelObjectsRebar)
                {
                    rebar.Father.GetPhase(out Phase MainPartPhase);
                    rebar.GetPhase(out Phase RebarPhase);
                    if (MainPartPhase != RebarPhase)
                    {
                        rebar.SetPhase(MainPartPhase);
                    }
                }
                model.CommitChanges();
            }
            if(listBox1.GetSelected(3) == true)
            {
                foreach (Assembly Assembly in modelObjectsAssembly)
                {
                    foreach(Assembly subassembly in Assembly.GetSubAssemblies())
                    {
                        Assembly.GetPhase(out Phase AssemblyPhase);
                        subassembly.GetPhase(out Phase SubPhase);
                        if(SubPhase != AssemblyPhase)
                        {
                            subassembly.SetPhase(AssemblyPhase);
                        }
                    }
                }
                model.CommitChanges();
            }
            //button3 służy do automatycznej korekty faz poszczególnie zaznaczonych i dodanych do listy obiektów.
        }   
    }
}
