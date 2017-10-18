using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MTGLandBalancer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int testval;
            if(textBox1.Text == "")
            {
                MessageBox.Show("Please select a .txt file containing one card name per line", "No List Selected", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if(!File.Exists(textBox1.Text))
            {
                MessageBox.Show("Could not find deck list", "File Does Not Exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if(Path.GetExtension(textBox1.Text) != ".txt")
            {
                MessageBox.Show("Only .txt files containing one card name per line are accepted", "Wrong File Type", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if(textBox2.Text == "")
            {
                MessageBox.Show("Please enter the total amount of lands in the deck", "No Total", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if(!Int32.TryParse(textBox2.Text, out testval))
            {
                MessageBox.Show("Please enter a valid number of lands", "Non-Numerical Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                List<string> names = new List<string>();

                StreamReader reader = new StreamReader(textBox1.Text);
                int lineNum = 0;
                while(!reader.EndOfStream)
                {
                    lineNum++;
                    string line = reader.ReadLine();
                    line = line.Trim();
                    if (!TitleLine(line))
                    {
                        names.Add(line);
                    }
                }
                reader.Close();

                if(names.Count <= 0)
                {
                    MessageBox.Show("Deck list did not contain any cards", "No Cards Found", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.json");

                    if(files.Length <= 0)
                    {
                        MessageBox.Show("Could not find .JSON reference file. Please place it in the same folder as this program's .exe file", "No JSON Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(files.Length > 1)
                    {
                        MessageBox.Show("Found Multiple .JSON reference files. Please remove all but one", "Multiple JSON Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Dictionary<string, Card> dictionary = LoadCards(files[0]);

                        bool success = true;

                        int whiteCount = 0;
                        int blackCount = 0;
                        int blueCount = 0;
                        int greenCount = 0;
                        int redCount = 0;

                        List<string> failures = new List<string>();
                        
                        foreach(string name in names)
                        {
                            if(dictionary.ContainsKey(name))
                            {
                                int[] colors = ConvertColors(dictionary[name].manaCost);

                                whiteCount += colors[0];
                                blackCount += colors[1];
                                blueCount += colors[2];
                                greenCount += colors[3];
                                redCount += colors[4];
                            }
                            else
                            {
                                failures.Add(name);
                                success = false;                    
                            }
                        }

                        if(success)
                        {
                            int totalDevotion = whiteCount + blackCount + blueCount + greenCount + redCount;
                            int totalLands = Int32.Parse(textBox2.Text);

                            float whiteLands = DetermineLandCount(whiteCount, totalDevotion, totalLands);
                            float blackLands = DetermineLandCount(blackCount, totalDevotion, totalLands);
                            float blueLands = DetermineLandCount(blueCount, totalDevotion, totalLands);
                            float greenLands = DetermineLandCount(greenCount, totalDevotion, totalLands);
                            float redLands = DetermineLandCount(redCount, totalDevotion, totalLands);

                            string results = "Required Lands:\n";

                            if(whiteLands > 0)
                            {
                                results += "White: " + whiteLands + "(" + (whiteLands / totalLands) * 100 + "% of total)\n";
                            }

                            if (blackLands > 0)
                            {
                                results += "Black: " + blackLands + "(" + (blackLands / totalLands) * 100 + "% of total)\n";
                            }

                            if (blueLands > 0)
                            {
                                results += "Blue: " + blueLands + "(" + (blueLands / totalLands) * 100 + "% of total)\n";
                            }

                            if (greenLands > 0)
                            {
                                results += "Green: " + greenLands + "(" + (greenLands / totalLands) * 100 + "% of total)\n";
                            }

                            if (redLands > 0)
                            {
                                results += "Red: " + redLands + "(" + (redLands / totalLands) * 100 + "% of total)\n";
                            }

                            MessageBox.Show(results, "Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            string message = "Could not find the following cards:\n";
                            foreach(string fail in failures)
                            {
                                message += "'" + fail + "'\n";
                            }

                            MessageBox.Show(message, "Could Not Find All Cards", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private bool TitleLine(string line)
        {
            if(Regex.IsMatch(line, "Lands \\("))
            {
                return true;
            }
            if (Regex.IsMatch(line, "Artifact \\("))
            {
                return true;
            }
            if (Regex.IsMatch(line, "Vehicle \\("))
            {
                return true;
            }
            if (Regex.IsMatch(line, "Driver \\("))
            {
                return true;
            }
            if (Regex.IsMatch(line, "Enchantment \\("))
            {
                return true;
            }
            if (Regex.IsMatch(line, "Instant \\("))
            {
                return true;
            }
            if (Regex.IsMatch(line, "Sorcery \\("))
            {
                return true;
            }
            if (Regex.IsMatch(line, "Large Creature \\("))
            {
                return true;
            }
            if (Regex.IsMatch(line, "Medium Creature \\("))
            {
                return true;
            }
            if (Regex.IsMatch(line, "Small Creature \\("))
            {
                return true;
            }
            if (Regex.IsMatch(line, "Removal \\("))
            {
                return true;
            }

            return false;
        }

        private float DetermineLandCount(int devotionCount, int totalDevotion, int totalLands)
        {
            float value1 = devotionCount * totalLands;

            float retValue = value1 / totalDevotion;

            return retValue;
        }

        private int[] ConvertColors(string input)
        {
            int whiteCount = 0;
            int blackCount = 0;
            int blueCount = 0;
            int greenCount = 0;
            int redCount = 0;

            MatchCollection matches = Regex.Matches(input, "{(.+?)}");

            foreach(Match match in matches)
            {
                switch(match.Groups[1].Value)
                {
                    case "W":
                        {
                            whiteCount++;
                            break;
                        }
                    case "B":
                        {
                            blackCount++;
                            break;
                        }
                    case "U":
                        {
                            blueCount++;
                            break;
                        }
                    case "G":
                        {
                            greenCount++;
                            break;
                        }
                    case "R":
                        {
                            redCount++;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return new int[] {whiteCount, blackCount, blueCount, greenCount, redCount };
        }

        private Dictionary<string, Card> LoadCards(string path)
        {
            Dictionary<string, Card> ret = new Dictionary<string, Card>();
            StreamReader reader = new StreamReader(path);
            string currentName = "";
            string currentManaCost = "";

            while(!reader.EndOfStream)
            {
                string currentLine = reader.ReadLine();

                if (Regex.IsMatch(currentLine, "\"name\":\\s*\"(.+?)\"", RegexOptions.IgnoreCase))
                {
                    currentName = Regex.Match(currentLine, "\"name\":\\s*\"(.+?)\"", RegexOptions.IgnoreCase).Groups[1].Value;
                }
                else if (Regex.IsMatch(currentLine, "\"manaCost\":\\s*\"(.+?)\"", RegexOptions.IgnoreCase))
                {
                    currentManaCost = Regex.Match(currentLine, "\"manaCost\":\\s*\"(.+?)\"", RegexOptions.IgnoreCase).Groups[1].Value;

                    if(currentName != "" && currentManaCost != "")
                    {
                        Card card = new Card();
                        card.name = currentName;
                        card.manaCost = currentManaCost;
                        ret.Add(currentName, card);
                    }

                    currentName = "";
                    currentManaCost = "";
                }
            }
            reader.Close();
            return ret;
        }
    }
}
