using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Beanstream.Api.SDK;
using Beanstream.Api.SDK.Domain;
using Beanstream.Api.SDK.Requests;
using Beanstream.Api.SDK.Data;
using Beanstream.Api.SDK.Exceptions;

namespace PaymentManager
{
    public partial class paymentManagerForm : Form
    {
        const string paymentsAPIKeyString = "b9950D37FE0c4f92A1177F7C935CcBeA";
        const int merchantIDInt = 300201147;
        const string APIVersionInt = "1";

        public paymentManagerForm()
        {
            InitializeComponent();
        }

        private void paymentManagerForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'paymentsDataSet.Payments' table. You can move, or remove it, as needed.
            this.paymentsTableAdapter.Fill(this.paymentsDataSet.Payments);

            //set to the first item of the dropdown list
            monthComboBox.SelectedIndex = 0;
            yearComboBox.SelectedIndex = 0;
        }

        //to clear all fields
        private void ClearFields()
        {
            amountTextBox.Clear();
            nameTextBox.Clear();
            cardNumberTextBox.Clear();
            monthComboBox.SelectedIndex = 0;
            yearComboBox.SelectedIndex = 0;
            cvvTextBox.Clear();
        }

        private void submitPaymentButton_Click(object sender, EventArgs e)
        {
            try {
                //validate if user has input all the required data
                if (string.IsNullOrWhiteSpace(amountTextBox.Text) || string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(cardNumberTextBox.Text))
                {
                    MessageBox.Show("Please fill in the empty fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    //check if amount is greater than 0 and less than 1000
                    if (Convert.ToDouble(amountTextBox.Text) > 0.00 && Convert.ToDouble(amountTextBox.Text) < 1000.00)
                    {
                        // Initialize Gateway connection
                        Gateway bsGateway = new Gateway();
                        bsGateway.MerchantId = merchantIDInt;
                        bsGateway.PaymentsApiKey = paymentsAPIKeyString;
                        bsGateway.ApiVersion = APIVersionInt;

                        // Setup the Credit Card details
                        Card ccCard = new Card();
                        ccCard.Name = nameTextBox.Text;
                        ccCard.Number = cardNumberTextBox.Text;
                        ccCard.ExpiryMonth = monthComboBox.Text;
                        ccCard.ExpiryYear = yearComboBox.Text;
                        ccCard.Cvd = cvvTextBox.Text;

                        //random number generator for order number
                        Random r = new Random();
                        int number = r.Next();

                        // Setup the payment request
                        CardPaymentRequest reqCardPaymentRequest = new CardPaymentRequest();
                        reqCardPaymentRequest.Amount = Convert.ToDouble(amountTextBox.Text);
                        reqCardPaymentRequest.OrderNumber = number.ToString(); // Change this to a unique number
                        reqCardPaymentRequest.Card = ccCard;

                        // Process the payment and get the response from their servers 
                        PaymentResponse response = bsGateway.Payments.MakePayment(reqCardPaymentRequest);

                        PaymentRequest payment = new PaymentRequest();

                        DataRow newPaymentsRow = paymentsDataSet.Tables["Payments"].NewRow();
                        
                        newPaymentsRow["PaymentDate"] = response.Created.ToShortDateString();
                        newPaymentsRow["PaymentStatus"] = "-";
                        newPaymentsRow["CardType"] = response.Card.CardType;
                        newPaymentsRow["LastFourDigits"] = response.Card.LastFour;
                        newPaymentsRow["NameOnCard"] = nameTextBox.Text;
                        newPaymentsRow["PaymentAmount"] = amountTextBox.Text;
                        newPaymentsRow["OrderNumber"] = response.OrderNumber;
                        newPaymentsRow["TransactionID"] = response.TransactionId;

                        // Add the row to the Region table
                        paymentsDataSet.Tables["Payments"].Rows.Add(newPaymentsRow);

                        // Save the new row to the database
                        this.paymentsTableAdapter.Update(this.paymentsDataSet.Payments);
                        MessageBox.Show("Payment Successful");

                        //clear the fields
                        ClearFields();
                    }
                    else
                    {
                        MessageBox.Show("Please enter the amount between $0 - $1000!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
             catch(Exception ex){
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }//catch
        }

        private void clearFieldsButton_Click(object sender, EventArgs e)
        {
            //clear all the fields
            ClearFields();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //closes the application
            this.Close();
        }

        private void backupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { 
            //Build the CSV file data as a Comma separated string.
            string csv = string.Empty;

            //Add the Header row for CSV file.
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                csv += column.HeaderText + ',';
            }

            //Add new line.
            csv += "\r\n";

            //Adding the Rows
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    //Add the Data rows.
                    csv += cell.Value + ",";
                }
                //Add new line.
                csv += "\r\n";
            }

            saveFileDialog.Filter = "CSV Files|*.csv";
            saveFileDialog.Title = "Save file as";
            saveFileDialog.FileName = "Payments.csv";
            saveFileDialog.ShowDialog();

            //Exporting to CSV.
            System.IO.File.WriteAllText(saveFileDialog.FileName, csv);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }//catch
        }
    }
}
