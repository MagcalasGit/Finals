using atmsystem;
using Newtonsoft;
using Newtonsoft.Json;

namespace updatedATM
{
    public partial class Form1 : Form
    {
        private int loginAttempts = 0;
        private const int maxAttempts = 3;
        public Form1()
        {
            InitializeComponent();
            PnlInsertCard.Visible = true;
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chbShow_CheckedChanged(object sender, EventArgs e)
        {
            if (chbShow.Checked == true)
            {
                txtPIN.UseSystemPasswordChar = false;
            }
            else
            {
                txtPIN.UseSystemPasswordChar = true;
            }
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            loginAttempts++;

            if (loginAttempts > maxAttempts)
            {
                MessageBox.Show("Maximum login attempts exceeded. Account Disabled.");
                return;
            } 

            if (txtCardNum.Text == AccountInfo.accountNumber && txtPIN.Text == AccountInfo.PIN)
            {
                MainForm gotoMain = new MainForm();
                this.Hide();
                gotoMain.Show();
            }
            else
            {
                MessageBox.Show("Invalid Account Number or PIN. Attempts left: " + (maxAttempts - loginAttempts));
                txtCardNum.Text = "";
                txtPIN.Text = "";

                if (loginAttempts == maxAttempts)
                {
                    MessageBox.Show("Maximum login attempts reached. Closing program.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
        }
        

        private void btninsertCard_Click(object sender, EventArgs e)
        {
            if (txtInsertCard.Text == "0000")
            {
                PnlInsertCard.Visible = false;
            }
            else
            {
                MessageBox.Show("Invalid Input, Please enter 0000");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
    
}
