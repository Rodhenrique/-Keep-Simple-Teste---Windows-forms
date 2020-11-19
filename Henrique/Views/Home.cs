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
using System.Data.SQLite;

namespace Henrique
{
    public partial class Home : Form
    {
        private const string PATH = "acme.sqlite";
        private bool status = true;
        private bool excluirItem = false;
        private static SQLiteConnection connection;
        public static List<DataRow> listItens { get; set; }
        public Home()
        {
            InitializeComponent();
        }

        private static SQLiteConnection DbConnection()
        {
            connection = new SQLiteConnection("Data Source=acme.sqlite; Version=3;");
            connection.Open();
            return connection;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "ACME FLIGHT MANAGER";


            //CRIAR UM ARQUIVO SQLITE CASO NÃO EXISTAM
            if (!File.Exists(PATH)) {
                SQLiteConnection.CreateFile(PATH);
                using (var cmd = DbConnection().CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS TB_VOO(ID_VOO INTEGER PRIMARY KEY, DATA_VOO Datetime,CUSTO numeric (10,2), DISTANCIA Int,CAPTURA Char(1), NIVEL_DOR Int)";
                    cmd.ExecuteNonQuery();
                }
            }

            Listar();
        }
        //MÉTODO GET PARA LISTAR TODOS REGISTROS DO BANCO DE DADOS 
        public void Listar()
        {
            SQLiteDataAdapter da = null;
            DataSet ds = new DataSet();
            List<DataRow> objs = new List<DataRow>();

            try
            {
                using (var cmd = DbConnection().CreateCommand())
                {
                    //DATA_VOO,CAPTURA,NIVEL_DOR
                    cmd.CommandText = "SELECT * FROM TB_VOO";
                    da = new SQLiteDataAdapter(cmd.CommandText, DbConnection());
                    da.Fill(ds);

                    foreach (DataRow dtrow in ds.Tables[0].Rows)
                    {
                        objs.Add(dtrow);
                        string[] row = { dtrow.ItemArray[1].ToString(), dtrow.ItemArray[4].ToString(), dtrow.ItemArray[5].ToString() };
                        var listViewItem = new ListViewItem(row);
                        tabela.Items.Add(listViewItem);
                    }
                    listItens = objs;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //MÉTODO POST DE REGISTRO ADICONANDO NO ARQUIVO SQLITE
        private void salvar_Click(object sender, EventArgs e)
        {
            if (verifyCampos() == true)
            {
            if (status == true)
            {
                try
                {
                    using (var cmd = DbConnection().CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO TB_VOO(DATA_VOO, CUSTO,DISTANCIA,CAPTURA,NIVEL_DOR) values (@data, @custo, @distancia,@captura,@nivel)";
                        cmd.Parameters.AddWithValue("@data", DateTime.Parse(TbData.Text));
                        cmd.Parameters.AddWithValue("@custo", TbCusto.Text);
                        cmd.Parameters.AddWithValue("@distancia", TbDistancia.Text);
                        cmd.Parameters.AddWithValue("@captura", (checkBox2.Checked) ? "N" : "S");
                        cmd.Parameters.AddWithValue("@nivel", TbNivel.Text);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                try
                {
                    int select = tabela.SelectedItems[0].Index;
                    var item = listItens[select].ItemArray;
                    int id = int.Parse(item[0].ToString());
                    using (var cmd = DbConnection().CreateCommand())
                    {
                        cmd.CommandText = "UPDATE TB_VOO SET DATA_VOO = @data, CUSTO = @custo ,DISTANCIA = @distancia,CAPTURA = @captura,NIVEL_DOR = @nivel WHERE ID_VOO=@Id";
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@data", DateTime.Parse(TbData.Text));
                        cmd.Parameters.AddWithValue("@custo", TbCusto.Text);
                        cmd.Parameters.AddWithValue("@distancia", TbDistancia.Text);
                        cmd.Parameters.AddWithValue("@captura", (checkBox2.Checked) ? "N" : "S");
                        cmd.Parameters.AddWithValue("@nivel", TbNivel.Text);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
                excluirItem = false;
                limparCampos();
                tabela.Items.Clear();
                habilitar(false);
                Listar();
            }
            else
            {
                MessageBox.Show("Preenchar os campos!!!");
            }
        }

        //VERIFICAR SE OS CAMPOS ESTÃO VAZIOS PARA FAZER UMA REQUEST
        private bool verifyCampos()
        {
            DateTime hora = DateTime.Now;
            if (TbData.Text == null || TbData.Text == "" || Convert.ToDateTime(TbData.Text) > hora)
            {
                return false;
            }else if (TbCusto.Text == "")
            {
                return false;
            }else if (TbDistancia.Text == "")
            {
                return false;
            }
            else if (TbNivel.Text == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //EXCLUIR UM REGISTRO DA TABELA 
        private void excluir_Click(object sender, EventArgs e)
        {
            if (excluirItem == true)
            {
                int select = tabela.SelectedItems[0].Index;
                var item = listItens[select].ItemArray;
                try
                {
                    using (var cmd = new SQLiteCommand(DbConnection()))
                    {
                        cmd.CommandText = "DELETE FROM TB_VOO Where ID_VOO=@Id";
                        cmd.Parameters.AddWithValue("@Id", int.Parse(item[0].ToString()));
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            limparCampos();
            habilitar(false);
            tabela.Items.Clear();
            Listar();
        }

        //BOTÃO DE CANCELAR 
        private void cancelar_Click(object sender, EventArgs e)
        {
            limparCampos();
            habilitar(false);
            excluirItem = false;
            tabela.Items.Clear();
            Listar();
        }

        private void checkBox_Click(object sender, EventArgs e)
        {
            checkBox2.CheckState = CheckState.Unchecked;
        }

        //HABILITAR E DESABILITAR OS CAMPOS DO FORMULARIO
        private void habilitar(bool stats)
        {
            if (stats == true)
            {
                button3.Enabled = true;
                button4.Enabled = true;
                TbData.Enabled = true;
                TbCusto.Enabled = true;
                TbNivel.Enabled = true;
                TbDistancia.Enabled = true;
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
                button4.Enabled = false;
                TbData.Enabled = false;
                TbCusto.Enabled = false;
                TbNivel.Enabled = false;
                TbDistancia.Enabled = false;
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
            }
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            checkBox1.CheckState = CheckState.Unchecked;
        }

        //SELECIONAR UM ITEM DA LIST VIEW
        private void tabela_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (tabela.SelectedItems.Count > 0)
            {
              int select = tabela.SelectedItems[0].Index;

                var item = listItens[select].ItemArray;

                TbData.Text = item[1].ToString();
                TbCusto.Text = item[2].ToString();
                TbDistancia.Text = item[3].ToString();
                bool state = (item[4].ToString() == "N") ? false : true;
                checkBox1.CheckState = (state == true) ? CheckState.Checked : CheckState.Unchecked;
                checkBox2.CheckState = (state == false) ? CheckState.Checked : CheckState.Unchecked;
                TbNivel.Text = item[5].ToString();

                habilitar(true);
                status = false;
                excluirItem = true;
            }
        }

        //BOTÃO INCLUIR 
        private void incluir_Click(object sender, EventArgs e)
        {
            limparCampos();
            status = true;
            habilitar(true);
        }

        //LIMPAR TODOS CAMPOS DO FORMULARIO
        private void limparCampos()
        {
            TbData.Text = "";
            TbCusto.Text = "";
            TbDistancia.Text = "";
            TbNivel.Text = "";
            checkBox1.CheckState = CheckState.Unchecked;
            checkBox2.CheckState = CheckState.Unchecked;
            excluirItem = false;
        }


        //MASK PARA SOMENTE NÚMEROS NO CAMPO NIVEL
        private void TbNivel_TextChanged_1(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(TbNivel.Text, "  ^ [0-9]"))
            {
                TbNivel.Text = "";
            }
        }
        //MASK PARA SOMENTE NÚMEROS NO CAMPO NIVEL
        private void TbNivel_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }
    }
}
