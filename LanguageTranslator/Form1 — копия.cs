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
            Stack<string> tokensStack = new Stack<string>();
            for (int i = tokens.Count - 1; i >= 0; i--)
            {

                if (tokens[i].Type == LexicalAnalyser.Token.TokenType.Variables)
                {
                    tokensStack.Push("id");
                }
                else if (tokens[i].Type == LexicalAnalyser.Token.TokenType.Literals)
                {
                    tokensStack.Push("liter");
                }
                else
                {
                    tokensStack.Push(lexicalAnalyser[tokens[i].Type][tokens[i].Index]);
                }
            }
            bool error = false;
            try
            {
                SintaxAnalys sintaxAnalys = new SintaxAnalys(tokensStack);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(),"Ошибка!",MessageBoxButtons.OK, MessageBoxIcon.Error);
                error = true;
            }
            if (!error)
            {
                MessageBox.Show("Успех");
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
        public class SintaxAnalys
        {
            private Stack<int> States = new Stack<int>();
            Stack<string> tokensText;
            public SintaxAnalys(Stack<string> tokensText)
            {
                this.tokensText = tokensText;
                States.Push(0);
                while (OnTop()!="<программа>")
                {
                    switch (States.Peek())
                    {
                        case 0:
                            if (MultipleComparison("var"))
                            {
                                if (OnTop() == "var")
                                {
                                    States.Push(1);
                                }
                            }
                            break;
                        case 1:
                            if (MultipleComparison("var", "<описание>", "id", "<список_переменных>", "<список_описания>"))
                            {
                                switch (OnTop())
                                {
                                    case "var":
                                        Shift();
                                        break;
                                    case "<описание>":
                                        States.Push(3);
                                        break;
                                    case "id":
                                        States.Push(5);
                                        break;
                                    case "<список_переменных>":
                                        States.Push(4);
                                        break;
                                    case "<список_описания>":
                                        States.Push(2);
                                        break;
                                }
                            }
                            break;
                        case 2:
                            if (MultipleComparison("<список_описания>", ";"))
                            {
                                switch (OnTop())
                                {
                                    case "<список_описания>":
                                        Shift();
                                        break;
                                    case ";":
                                        States.Push(6);
                                        break;
                                }
                            }
                            break;
                        case 3:
                            if (MultipleComparison("<описание>"))
                            {
                                Bringing(1, "<список_описания>");
                            }
                            break;
                        case 4:
                            if (MultipleComparison("<список_переменных>", ":", ","))
                            {
                                switch (OnTop())
                                {
                                    case "<список_переменных>":
                                        Shift();
                                        break;
                                    case ":":
                                        States.Push(7);
                                        break;
                                    case ",":
                                        States.Push(8);
                                        break;
                                }
                            }
                            break;
                        case 5:
                            if (MultipleComparison("id"))
                            {
                                Bringing(1, "<список_переменных>");
                            }
                            break;
                        case 6:
                            if (MultipleComparison(";", "begin", "<описание>", "<список_переменных>", "id"))
                            {
                                switch (OnTop())
                                {
                                    case ";":
                                        Shift();
                                        break;
                                    case "begin":
                                        States.Push(9);
                                        break;
                                    case "<описание>":
                                        States.Push(10);
                                        break;
                                    case "<список_переменных>":
                                        States.Push(4);
                                        break;
                                    case "id":
                                        States.Push(5);
                                        break;
                                }
                            }
                            break;
                        case 7:
                            if (MultipleComparison(":", "<тип>", "integer", "Float", "boolean"))
                            {
                                switch (OnTop())
                                {
                                    case ":":
                                        Shift();
                                        break;
                                    case "<тип>":
                                        States.Push(11);
                                        break;
                                    case "integer":
                                        States.Push(12);
                                        break;
                                    case "Float":
                                        States.Push(13);
                                        break;
                                    case "boolean":
                                        States.Push(14);
                                        break;
                                }
                            }
                            break;
                        case 8:
                            if (MultipleComparison(",", "id"))
                            {
                                switch (OnTop())
                                {
                                    case ",":
                                        Shift();
                                        break;
                                    case "id":
                                        States.Push(15);
                                        break;
                                }
                            }
                            break;
                        case 9:
                            if (MultipleComparison("begin", "<список_операторов>", "<оператор>", "<условный_оператор_case>", "<оператор_присваивания>", "<выбор>", "case", "id"))
                            {
                                switch (OnTop())
                                {
                                    case "begin":
                                        Shift();
                                        break;
                                    case "<список_операторов>":
                                        States.Push(16);
                                        break;
                                    case "<оператор>":
                                        States.Push(17);
                                        break;
                                    case "<условный_оператор_case>":
                                        States.Push(18);
                                        break;
                                    case "<оператор_присваивания>":
                                        States.Push(19);
                                        break;
                                    case "<выбор>":
                                        States.Push(20);
                                        break;
                                    case "case":
                                        States.Push(21);
                                        break;
                                    case "id":
                                        States.Push(22);
                                        break;
                                }
                            }
                            break;
                        case 10:
                            if (MultipleComparison("<описание>"))
                            {
                                Bringing(3, "<список_описания>");
                            }
                            break;
                        case 11:
                            if (MultipleComparison("<тип>"))
                            {
                                Bringing(3, "<описание>");
                            }
                            break;
                        case 12:
                            if (MultipleComparison("integer"))
                            {
                                Bringing(1, "<тип>");
                            }
                            break;
                        case 13:
                            if (MultipleComparison("float"))
                            {
                                Bringing(1, "<тип>");
                            }
                            break;
                        case 14:
                            if (MultipleComparison("boolean"))
                            {
                                Bringing(1, "<тип>");
                            }
                            break;
                        case 15:
                            if (MultipleComparison("id"))
                            {
                                Bringing(3, "<список_переменных>");
                            }
                            break;
                        case 16:
                            if (MultipleComparison("<список_операторов>", ";"))
                            {
                                switch (OnTop())
                                {
                                    case "<список_операторов>":
                                        Shift();
                                        break;
                                    case ";":
                                        States.Push(23);
                                        break;
                                }
                            }
                            break;
                        case 17:
                            if (MultipleComparison("<оператор>"))
                            {
                                Bringing(1, "<список_операторов>");
                            }
                            break;
                        case 18:
                            if (MultipleComparison("<условный_оператор_case>"))
                            {
                                Bringing(1, "<оператор>");
                            }
                            break;
                        case 19:
                            if (MultipleComparison("<оператор_присваивания>"))
                            {
                                Bringing(1, "<оператор>");
                            }
                            break;
                        case 20:
                            if (MultipleComparison("<выбор>", "end", ";"))
                            {
                                switch (OnTop())
                                {
                                    case "<выбор>":
                                        Shift();
                                        break;
                                    case "end":
                                        States.Push(24);
                                        break;
                                    case ";":
                                        States.Push(25);
                                        break;
                                }
                            }
                            break;
                        case 21:
                            if (MultipleComparison("case", "id"))
                            {
                                switch (OnTop())
                                {
                                    case "case":
                                        Shift();
                                        break;
                                    case "id":
                                        States.Push(26);
                                        break;
                                }
                            }
                            break;
                        case 22:
                            if (MultipleComparison("id", ":="))
                            {
                                switch (OnTop())
                                {
                                    case "id":
                                        Shift();
                                        break;
                                    case ":=":
                                        States.Push(27);
                                        break;
                                }
                            }
                            break;
                        case 23:
                            if (MultipleComparison(";", "end", "<оператор>", "<условный_оператор_case>", "<выбор>", "case", "<оператор_присваивания>", "id"))
                            {
                                switch (OnTop())
                                {
                                    case ";":
                                        Shift();
                                        break;
                                    case "end":
                                        States.Push(28);
                                        break;
                                    case "<оператор>":
                                        States.Push(54);
                                        break;
                                    case "<условный_оператор_case>":
                                        States.Push(18);
                                        break;
                                    case "<выбор>":
                                        States.Push(20);
                                        break;
                                    case "case":
                                        States.Push(21);
                                        break;
                                    case "<оператор_присваивания>":
                                        States.Push(19);
                                        break;
                                    case "id":
                                        States.Push(22);
                                        break;
                                }
                            }
                            break;
                        case 24:
                            if (MultipleComparison("end"))
                            {
                                Bringing(2, "<условный_оператор_case>");
                            }
                            break;
                        case 25:
                            if (MultipleComparison(";", "else"))
                            {
                                switch (OnTop())
                                {
                                    case ";":
                                        Shift();
                                        break;
                                    case "else":
                                        States.Push(29);
                                        break;
                                }
                            }
                            break;
                        case 26:
                            if (MultipleComparison("id", "of"))
                            {
                                switch (OnTop())
                                {
                                    case "id":
                                        Shift();
                                        break;
                                    case "of":
                                        States.Push(30);
                                        break;
                                }
                            }
                            break;
                        case 27:
                            if (OnTop() == ":=")
                            {
                                Shift();
                            }
                            else
                            {
                                States.Push(31);  //expr
                            }
                            break;
                        case 28:
                            if (MultipleComparison("end", "."))
                            {
                                switch (OnTop())
                                {
                                    case "end":
                                        Shift();
                                        break;
                                    case ".":
                                        States.Push(32);
                                        break;
                                }
                            }
                            break;
                        case 29:
                            if (MultipleComparison("else", "<иначе>", "<оператор>", "begin", "<условный_оператор_case>", "<оператор_присваивания>", "id", "<выбор>", "case"))
                            {
                                switch (OnTop())
                                {
                                    case "else":
                                        Shift();
                                        break;
                                    case "<иначе>":
                                        States.Push(33);
                                        break;
                                    case "<оператор>":
                                        States.Push(34);
                                        break;
                                    case "begin":
                                        States.Push(35);
                                        break;
                                    case "<условный_оператор_case>":
                                        States.Push(18);
                                        break;
                                    case "<оператор_присваивания>":
                                        States.Push(19);
                                        break;
                                    case "id":
                                        States.Push(22);
                                        break;
                                    case "<выбор>":
                                        States.Push(20);
                                        break;
                                    case "case":
                                        States.Push(21);
                                        break;
                                }
                            }
                            break;
                        case 30:
                            if (MultipleComparison("of", "<описание_условия>", "<условие>", "liter", "<список_условий>"))
                            {
                                switch (OnTop())
                                {
                                    case "of":
                                        Shift();
                                        break;
                                    case "<описание_условия>":
                                        States.Push(37);
                                        break;
                                    case "<условие>":
                                        States.Push(38);
                                        break;
                                    case "liter":
                                        States.Push(39);
                                        break;
                                    case "<список_условий>":
                                        States.Push(36);
                                        break;
                                }
                            }
                            break;
                        case 31:
                            Bringing(3, "<оператор_присваивания>"); //expr
                            break;
                        case 32:
                            if (MultipleComparison("."))
                            {
                                Bringing(8, "<программа>");
                            }
                            break;
                        case 33:
                            if (MultipleComparison("<иначе>", "end"))
                            {
                                switch (OnTop())
                                {
                                    case "<иначе>":
                                        Shift();
                                        break;
                                    case "end":
                                        States.Push(40);
                                        break;
                                }
                            }
                            break;
                        case 34:
                            if (MultipleComparison("<оператор>", ";"))
                            {
                                switch (OnTop())
                                {
                                    case "<оператор>":
                                        Shift();
                                        break;
                                    case ";":
                                        States.Push(41);
                                        break;
                                }
                            }
                            break;
                        case 35:
                            if (MultipleComparison("begin", "<список_операторов>", "<оператор>", "<условный_оператор_case>", "<выбор>", "case", "<оператор_присваивания>", "id"))
                            {
                                switch (OnTop())
                                {
                                    case "begin":
                                        Shift();
                                        break;
                                    case "<список_операторов>":
                                        States.Push(42);
                                        break;
                                    case "<оператор>":
                                        States.Push(17);
                                        break;
                                    case "<условный_оператор_case>":
                                        States.Push(18);
                                        break;
                                    case "<выбор>":
                                        States.Push(20);
                                        break;
                                    case "case":
                                        States.Push(21);
                                        break;
                                    case "<оператор_присваивания>":
                                        States.Push(19);
                                        break;
                                    case "id":
                                        States.Push(22);
                                        break;
                                }
                            }
                            break;
                        case 36:
                            if (MultipleComparison("<список_условий>", ";"))
                            {
                                switch (OnTop())
                                {
                                    case "<список_условий>":
                                        if (MultipleComparisonNext(1, "end", ";"))
                                        {
                                            switch (Next(1))
                                            {
                                                case "end":
                                                    Bringing(4, "<выбор>");
                                                    break;
                                                case ";":
                                                    if (MultipleComparisonNext(2, "else", "liter"))
                                                    {
                                                        switch (Next(2))
                                                        {
                                                            case "else":
                                                                Bringing(4, "<выбор>");
                                                                break;
                                                            case "liter":
                                                                Shift();
                                                                break;
                                                        }
                                                    }
                                                    break;
                                            }

                                        }
                                        break;
                                    case ";":
                                        States.Push(43);
                                        break;
                                }
                            }
                            break;
                        case 37:
                            if (MultipleComparison("<описание_условия>"))
                            {
                                Bringing(1, "<список_условий>");
                            }
                            break;
                        case 38:
                            if (MultipleComparison("<условие>", ":"))
                            {
                                switch (OnTop())
                                {
                                    case "<условие>":
                                        Shift();
                                        break;
                                    case ":":
                                        States.Push(44);
                                        break;
                                }
                            }
                            break;
                        case 39:
                            if (MultipleComparison("liter", ".."))
                            {
                                switch (OnTop())
                                {
                                    case "liter":
                                        if (MultipleComparisonNext(1, ":", ".."))
                                        {
                                            switch (Next(1))
                                            {
                                                case ":":
                                                    Bringing(1, "<условие>");
                                                    break;
                                                case "..":
                                                    Shift();
                                                    break;
                                            }
                                        }
                                        break;
                                    case "..":
                                        States.Push(55);
                                        break;
                                }
                            }
                            break;
                        case 40:
                            if (MultipleComparison("end"))
                            {
                                Bringing(5, "<условный_оператор_case>");
                            }
                            break;
                        case 41:
                            if (MultipleComparison(";"))
                            {
                                Bringing(2, "<иначе>");
                            }
                            break;
                        case 42:
                            if (MultipleComparison("<список_операторов>", ";"))
                            {
                                switch (OnTop())
                                {
                                    case "<список_операторов>":
                                        Shift();
                                        break;
                                    case ";":
                                        States.Push(45);
                                        break;
                                }
                            }
                            break;
                        case 43:
                            if (MultipleComparison(";", "<описание_условия>", "<условие>", "liter"))
                            {
                                switch (OnTop())
                                {
                                    case ";":
                                        Shift();
                                        break;
                                    case "<описание_условия>":
                                        States.Push(46);
                                        break;
                                    case "<условие>":
                                        States.Push(38);
                                        break;
                                    case "liter":
                                        States.Push(39);
                                        break;
                                }
                            }
                            break;
                        case 44:
                            if (MultipleComparison(":", "<оператор>", "begin", "<условный_оператор_case>", "<выбор>", "case", "<оператор_присваивания>", "id"))
                            {
                                switch (OnTop())
                                {
                                    case ":":
                                        Shift();
                                        break;
                                    case "<оператор>":
                                        States.Push(47);
                                        break;
                                    case "begin":
                                        States.Push(48);
                                        break;
                                    case "<условный_оператор_case>":
                                        States.Push(18);
                                        break;
                                    case "<выбор>":
                                        States.Push(20);
                                        break;
                                    case "case":
                                        States.Push(21);
                                        break;
                                    case "<оператор_присваивания>":
                                        States.Push(19);
                                        break;
                                    case "id":
                                        States.Push(22);
                                        break;
                                }
                            }
                            break;
                        case 45:
                            if (MultipleComparison(";", "end", "<оператор>", "<условный_оператор_case>", "case", "<выбор>", "<оператор_присваивания>", "id"))
                            {
                                switch (OnTop())
                                {
                                    case ";":
                                        Shift();
                                        break;
                                    case "end":
                                        States.Push(49);
                                        break;
                                    case "<оператор>":
                                        States.Push(57);
                                        break;
                                    case "<условный_оператор_case>":
                                        States.Push(18);
                                        break;
                                    case "case":
                                        States.Push(21);
                                        break;
                                    case "<выбор>":
                                        States.Push(20);
                                        break;
                                    case "<оператор_присваивания>":
                                        States.Push(19);
                                        break;
                                    case "id":
                                        States.Push(22);
                                        break;
                                }
                            }
                            break;
                        case 46:
                            if (MultipleComparison("<описание_условия>"))
                            {
                                Bringing(3, "<список_условий>");
                            }
                            break;
                        case 47:
                            if (MultipleComparison("<оператор>"))
                            {
                                Bringing(3, "<описание_условия>");
                            }
                            break;
                        case 48:
                            if (MultipleComparison("begin", "<список_операторов>", "<оператор>", "<условный_оператор_case>", "case", "<выбор>", "<оператор_присваивания>", "id"))
                            {
                                switch (OnTop())
                                {
                                    case "begin":
                                        Shift();
                                        break;
                                    case "<список_операторов>":
                                        States.Push(50);
                                        break;
                                    case "<оператор>":
                                        States.Push(17);
                                        break;
                                    case "<условный_оператор_case>":
                                        States.Push(18);
                                        break;
                                    case "case":
                                        States.Push(21);
                                        break;
                                    case "<выбор>":
                                        States.Push(20);
                                        break;
                                    case "<оператор_присваивания>":
                                        States.Push(19);
                                        break;
                                    case "id":
                                        States.Push(22);
                                        break;
                                }
                            }
                            break;
                        case 49:
                            if (MultipleComparison("end", ";"))
                            {
                                switch (OnTop())
                                {
                                    case "end":
                                        Shift();
                                        break;
                                    case ";":
                                        States.Push(51);
                                        break;
                                }
                            }
                            break;
                        case 50:
                            if (MultipleComparison("<список_операторов>", ";"))
                            {
                                switch (OnTop())
                                {
                                    case "<список_операторов>":
                                        Shift();
                                        break;
                                    case ";":
                                        States.Push(52);
                                        break;
                                }
                            }
                            break;
                        case 51:
                            if (MultipleComparison(";"))
                            {
                                Bringing(5, "<иначе>");
                            }
                            break;
                        case 52:
                            if (MultipleComparison(";", "end", "<оператор>", "<условный_оператор_case>", "case", "<выбор>", "<оператор_присваивания>", "id"))
                            {
                                switch (OnTop())
                                {
                                    case ";":
                                        Shift();
                                        break;
                                    case "end":
                                        States.Push(53);
                                        break;
                                    case "<оператор>":
                                        States.Push(57);
                                        break;
                                    case "<условный_оператор_case>":
                                        States.Push(18);
                                        break;
                                    case "case":
                                        States.Push(21);
                                        break;
                                    case "<выбор>":
                                        States.Push(20);
                                        break;
                                    case "<оператор_присваивания>":
                                        States.Push(19);
                                        break;
                                    case "id":
                                        States.Push(22);
                                        break;
                                }
                            }
                            break;
                        case 53:
                            if (MultipleComparison("end"))
                            {
                                Bringing(6, "<описание_условия>");
                            }
                            break;
                        case 54:
                            if (MultipleComparison("<оператор>"))
                            {
                                Bringing(3, "<список_операторов>");
                            }
                            break;
                        case 55:
                            if (MultipleComparison("..", "liter"))
                            {
                                switch (OnTop())
                                {
                                    case "..":
                                        Shift();
                                        break;
                                    case "liter":
                                        States.Push(56);
                                        break;
                                }
                            }
                            break;
                        case 56:
                            if (MultipleComparison("liter"))
                            {
                                Bringing(3, "<условие>");
                            }
                            break;
                        case 57:
                            if (MultipleComparison("<оператор>"))
                            {
                                Bringing(3, "<список_операторов>");
                            }
                            break;
                    }
                }

                string OnTop()
                {
                    return tokensText.Peek();
                }
            }

            private string Next(int count) // если индекс с конца(иначе исправить)
            {
                return tokensText.ToArray()[count];
            }
            private bool MultipleComparisonNext(int count, params string[] b)
            {
                string[] tokensTextArray = tokensText.ToArray();
                foreach (var item in b)
                {
                    if (tokensTextArray[count] == item)
                    {
                        return true;
                    }
                }
                string exp = "";
                foreach (var item in b)
                {
                    exp += item + " ";
                }
                throw new Exception($"Ожидалось встретить {exp}  встречено {tokensTextArray[count]}");
            }

            private void Bringing(int count, string name)
            {
                for (int i = 0; i < count; i++)
                {
                    States.Pop();
                }
                tokensText.Pop();
                tokensText.Push(name);
            }

            private void Shift()
            {
                tokensText.Pop();
            }

            private bool MultipleComparison(params string[] b)
            {
                foreach (var item in b)
                {
                    if (tokensText.Peek() == item)
                    {
                        return true;
                    }
                }
                string exp = "";
                foreach (var item in b)
                {
                    exp += item + " ";
                }
                throw new Exception($"Ожидалось встретить {exp}  встречено {tokensText.Peek()}");
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