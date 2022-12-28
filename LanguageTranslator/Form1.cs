using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LanguageTranslator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void button2_Click(object sender, EventArgs e)
        {

            bool error = false;
            try
            {
                LexicalAnalyser lexicalAnalyser = new LexicalAnalyser(textBox1.Text);
                dataGridView1.Columns.Clear();
                dataGridView1.Columns.Add("Lexeme", "Лексема");
                dataGridView1.Columns.Add("Text", "Предварительный тип");
                List<LexicalAnalyser.Lexeme> lexemes = lexicalAnalyser.Analys();
                foreach (var item in lexemes)
                {
                    dataGridView1.Rows.Add(item.Text, item.StateToString());
                }
                List<LexicalAnalyser.Token> tokens = lexicalAnalyser.CompilingTokens(lexemes);
                PrintListToTable(lexicalAnalyser.Keys, dataGridView2);
                PrintListToTable(lexicalAnalyser.Separators, dataGridView3);
                PrintListToTable(lexicalAnalyser.Literals, dataGridView4);
                PrintListToTable(lexicalAnalyser.Variables, dataGridView5);

                dataGridView6.Columns.Clear();
                dataGridView6.Columns.Add("firstColumn", "Таблица");
                dataGridView6.Columns.Add("secondColumn", "Индекс");
                for (int i = 0; i < tokens.Count; i++)
                {
                    dataGridView6.Rows.Add(tokens[i].Type, tokens[i].Index);
                }
                SyntaxAnalys sintaxAnalys = new SyntaxAnalys(tokens, lexicalAnalyser);
                listBox1.Items.Clear();
                foreach (var item in sintaxAnalys.Matrix)
                {
                    listBox1.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                error = true;
            }
            if (!error)
            {
                MessageBox.Show("Ошибок не найдено","Успех",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }

        }
        private void PrintListToTable(List<string> keys, DataGridView dataGridView)
        {
            dataGridView.Columns.Clear();
            dataGridView.Columns.Add("firstColumn", "Индекс");
            dataGridView.Columns.Add("secondColumn", "Значение");
            for (int i = 0; i < keys.Count; i++)
            {
                dataGridView.Rows.Add(i, keys[i]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }
    }
}