using atmsystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace updatedATM
{
    public partial class withdrawsection : Form
    {
        private string jsonFilePath = "accountBalances.json";
        private dynamic accountBalances;
        public withdrawsection()
        {
            InitializeComponent();
            loadAccountBalances();
            receiptPanel.Visible = false;
        }

        private void withdrawBtn_Click(object sender, EventArgs e)
        {
            if (cbxsaviOrCheq.SelectedItem == null)
            {
                MessageBox.Show("Please select Savings or Cheque.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedAccount = cbxsaviOrCheq.SelectedItem.ToString();

            if (!decimal.TryParse(txtWithdrawAmount.Text, out decimal withdrawAmount))
            {
                MessageBox.Show("Please enter a valid withdrawal amount.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            loadAccountBalances();

            if (accountBalances != null && accountBalances.Count > 0)
            {
                bool accountFound = false;

                foreach (var accountBalance in accountBalances)
                {
                    if (accountBalance.ContainsKey(selectedAccount))
                    {
                        decimal currentBalance = accountBalance[selectedAccount];

                        if (withdrawAmount > currentBalance)
                        {
                            MessageBox.Show("Withdrawal amount exceeds available balance.", "Insufficient Funds", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        accountBalance[selectedAccount] -= withdrawAmount;
                        SaveAccountBalances();
                        LogTransaction(selectedAccount, "Withdraw", withdrawAmount);

                        Dictionary<int, int> billDispenseResult = DispenseBills((double)withdrawAmount);

                        if (billDispenseResult != null)
                        {
                            MessageBox.Show($"Withdrawal of ₱{withdrawAmount:N2} from {selectedAccount} account successful.", "Withdrawal Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadReceipt();
                            receiptPanel.Visible = true;
                        }
                        else
                        {
                            MessageBox.Show("Unable to dispense bills for the withdrawal amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            accountBalance[selectedAccount] += withdrawAmount;
                            SaveAccountBalances();
                        }

                        loadAccountBalances();
                        accountFound = true;
                        break;
                    }
                }

                if (!accountFound)
                {
                    MessageBox.Show($"{selectedAccount} account not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Account balances not found or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void LogTransaction(string accountType, string transactionType, decimal amount)
        {
            try
            {
                Dictionary<string, decimal> targetDictionary = null;

                foreach (var dictionary in accountBalances)
                {
                    if (dictionary.ContainsKey(accountType))
                    {
                        targetDictionary = dictionary;
                        break;
                    }
                }

                if (targetDictionary != null)
                {
                    Transactions transaction = new Transactions
                    {
                        Type = $"{transactionType} in {accountType}",
                        Time = DateTime.Now,
                        Amount = amount,
                        Balance = targetDictionary[accountType]
                    };

                    string formattedTime = transaction.Time.ToString("MM/dd/yyyy hh:mm tt");
                    transaction.Time = DateTime.ParseExact(formattedTime, "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture);

                    string transactionJson = JsonConvert.SerializeObject(transaction);
                    string transactionsFilePath = "Transactions.json";

                    if (File.Exists(transactionsFilePath))
                    {
                        File.AppendAllText(transactionsFilePath, transactionJson + Environment.NewLine);
                    }
                    else
                    {
                        File.WriteAllText(transactionsFilePath, transactionJson + Environment.NewLine);
                    }
                }
                else
                {
                    MessageBox.Show($"Account '{accountType}' not found in account balances.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error logging transaction: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadAccountBalances()
        {
            try
            {
                if (File.Exists(jsonFilePath))
                {
                    string jsonData = File.ReadAllText(jsonFilePath);
                    accountBalances = JsonConvert.DeserializeObject<List<Dictionary<string, decimal>>>(jsonData);
                }
                else
                {
                    accountBalances = new List<Dictionary<string, decimal>>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading account balances: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveAccountBalances()
        {
            string jsonData = JsonConvert.SerializeObject(accountBalances);
            File.WriteAllText("accountBalances.json", jsonData);
        }

        private Dictionary<int, int> DispenseBills(double withdrawalAmount)
        {
            int[] availableBills = { 1000, 500, 200, 100, 50, 20 };
            Dictionary<int, int> billCounts = new Dictionary<int, int>();
            double remainingAmount = withdrawalAmount;

            foreach (int billValue in availableBills)
            {
                if (remainingAmount <= 0)
                    break;

                if (billValue > remainingAmount)
                    continue;

                int billCount = (int)(remainingAmount / billValue);

                if (billCount > 0)
                {
                    billCounts.Add(billValue, billCount);
                    remainingAmount -= billCount * billValue;
                }
            }

            const double epsilon = 0.01;
            if (Math.Abs(remainingAmount) < epsilon)
                return billCounts;
            else
                return null;
        }

        private void ConvertLastTransactionToJsonArray()
        {
            try
            {
                string transactionsFilePath = "Transactions.json";

                if (File.Exists(transactionsFilePath))
                {
                    string[] transactionLines = File.ReadAllLines(transactionsFilePath);

                    if (transactionLines.Length > 0)
                    {
                        List<Transactions> transactionList = new List<Transactions>();

                        foreach (string transactionLine in transactionLines)
                        {
                            Transactions transaction = JsonConvert.DeserializeObject<Transactions>(transactionLine);
                            transactionList.Add(transaction);
                        }

                        string jsonArray = JsonConvert.SerializeObject(transactionList, Formatting.Indented);

                        string outputFilePath = "TransacHistory.json";
                        File.WriteAllText(outputFilePath, jsonArray);
                    }
                    else
                    {
                        MessageBox.Show("No transactions found in the input file.", "Empty File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Transactions input file not found.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting transactions to JSON array: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void continueBtn_Click(object sender, EventArgs e)
        {
            MainForm goBack = new MainForm();
            this.Hide();
            goBack.Show();
        }
        private void LoadReceipt()
        {
            if (accountBalances != null && accountBalances.Count > 0)
            {
                string transactionsFilePath = "Transactions.json";
                if (File.Exists(transactionsFilePath))
                {
                    string[] transactionLines = File.ReadAllLines(transactionsFilePath);

                    if (transactionLines.Length > 0)
                    {
                        string lastTransactionJson = transactionLines[transactionLines.Length - 1];
                        Transactions lastTransaction = JsonConvert.DeserializeObject<Transactions>(lastTransactionJson);
                        ConvertLastTransactionToJsonArray();

                        lblAccountNum.Text = lastTransaction.Type.Contains("Savings") ? "Savings Account" : "Cheque Account";
                        lblAmountAdd.Text = lastTransaction.Amount.ToString("N2");
                        lblCurrentBal.Text = lastTransaction.Balance.ToString("N2");
                        lblAccNum.Text = AccountInfo.accountNumber.ToString();
                    }
                }
            }
        }
    }
}
