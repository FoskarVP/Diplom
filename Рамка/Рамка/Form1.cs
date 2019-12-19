using MySql.Data.MySqlClient;
using System;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace Рамка
{
    public partial class Form1 : Form
    {
        SerialPort Serial;

        public Form1()
        {
            InitializeComponent();

            string[] ports = SerialPort.GetPortNames();     //Чтение портов доступных в системе
            COMports.Items.Clear();                         //Очистка содержимого бокса
            COMports.Items.AddRange(ports);                 //Добавление найденных портов в бокс    
            Serial = new SerialPort();
            Serial.BaudRate = 9600;
            Serial.DataBits = 8;
            Serial.StopBits = StopBits.One;
            Serial.Parity = Parity.None;
            Serial.DataReceived += new SerialDataReceivedEventHandler(Read);

        }

        ~Form1()
        {
            if (Serial.IsOpen) Serial.Close();
        }

        private void start_Click(object sender, EventArgs e)
        {
            if (!Serial.IsOpen)
            {
                Serial.PortName = (string)COMports.SelectedItem;
                Serial.Open();
                textBox1.Text += "Подключен к " + Serial.PortName + "\r\n";
                COMports.Enabled = false;
                start.Text = "Disconnect";
            }
            else
            {
                start.Text = "Connect";
                Serial.Close();
                textBox1.Text += Serial.PortName + " отчключен" + "\r\n";
                COMports.Enabled = true;
            }
        }

        private void Read(object sender, SerialDataReceivedEventArgs e)
        {
            if (!Serial.IsOpen) return;
            System.Threading.Thread.Sleep(100);
            string buf = Serial.ReadLine();
            GetBytes(buf, out string[] bytes);
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action(() => textBox1.Text += buf + ": "));
                textBox1.Invoke(new Action(() => textBox1.ScrollToCaret()));

                string Connect = "Server=127.0.0.1;Port=3306;Database=diplom;User Id=root";
                MySqlConnection myConnection = new MySqlConnection(Connect);

                string QueryText = "Select Name From Employees WHERE thirdByte=" + bytes[2] + " and fourthByte=" + bytes[3] + "";
                string getID = "Select ID From Employees WHERE thirdByte=" + bytes[2] + " and fourthByte=" + bytes[3] + "";
                try
                {
                    myConnection.Open();
                    MySqlCommand myCommand = new MySqlCommand(QueryText, myConnection);
                    string text = myCommand.ExecuteScalar().ToString() + "\r\n";
                    MySqlCommand getCommand = new MySqlCommand(getID, myConnection);
                    string id = getCommand.ExecuteScalar().ToString() + "\r\n";
                    string insertStatistic = "INSERT INTO `statistic`(`Employee`, `Place`, `Time`) VALUES (" + id[0] + ", " + bytes[4] + ", '" +
                        DateTime.Now.Year + "-" + DateTime.Now.Month+"-" + DateTime.Now.Day + " " + DateTime.Now.TimeOfDay+ "')";
                    MySqlCommand insertCommand = new MySqlCommand(insertStatistic, myConnection);
                    MySqlDataReader reader;
                    reader = insertCommand.ExecuteReader(); 
                    textBox1.Invoke(new Action(() => textBox1.Text += text));

                }
                catch { }
            }
            else
            {
                textBox1.Text += buf + "\r\n";
                textBox1.ScrollToCaret();
            }
        }
        

        void GetBytes(string id, out string[] bytes) 
        {
           bytes = id.Split(new char[] { ' ' });
        }
    }
}
