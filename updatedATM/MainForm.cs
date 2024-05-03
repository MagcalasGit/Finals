using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;

namespace updatedATM
{
    public partial class MainForm : Form
    {
        private const string transactionsFilePath = "TransacHistory.json";
        public MainForm()
        {
            InitializeComponent();
            TransactionPanel.Visible = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //late
        }

        private void balanceBtn_Click(object sender, EventArgs e)
        {
            //later
        }

        private void transactionsBtn_Click(object sender, EventArgs e)
        {
            TransactionPanel.Visible = true;
            LoadTransactionHistory();   
        }
        //functions
        private void LoadTransactionHistory()
        {
            try
            {
                if (File.Exists(transactionsFilePath))
                {
                    string jsonData = File.ReadAllText(transactionsFilePath);
                    var transactions = JsonConvert.DeserializeObject<Transactions[]>(jsonData);

                    dgvTransaction.DataSource = ToDataTable(transactions);
                }
                else
                {
                    MessageBox.Show("Transaction history file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading transaction history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable ToDataTable(Transactions[] transactions)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Type");
            dataTable.Columns.Add("Time", typeof(DateTime));
            dataTable.Columns.Add("Amount", typeof(decimal));
            dataTable.Columns.Add("Balance", typeof(decimal));

            foreach (var transaction in transactions)
            {
                dataTable.Rows.Add(transaction.Type, transaction.Time, transaction.Amount, transaction.Balance);
            }

            return dataTable;
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Form1 goback = new Form1();
            this.Hide();
            goback.Show();
        }

        private void wtihdrawBtn_Click(object sender, EventArgs e)
        {
            withdrawsection gotoWithdraw = new withdrawsection();
            this.Hide();
            gotoWithdraw.Show();
        }

        private void depositBtn_Click(object sender, EventArgs e)
        {
            depositsection gotoDeposit = new depositsection();
            this.Hide();
            gotoDeposit.Show();
        }

        private void paybillsBtn_Click(object sender, EventArgs e)
        {
            paybillssection gotoPaybills = new paybillssection();
            this.Hide();
            gotoPaybills.Show();
        }

        private void hideTransactionBtn_Click(object sender, EventArgs e)
        {
            TransactionPanel.Visible = false;
        }

        private void TransactionPanel_Paint(object sender, PaintEventArgs e)
        {

        }


    }
    public class Transaction
    {
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }
}
