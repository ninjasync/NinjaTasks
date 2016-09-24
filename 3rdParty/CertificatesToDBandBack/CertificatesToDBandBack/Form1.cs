using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Drawing;
using PemImport;

namespace CertificatesToDBandBack
{
    public partial class Form1 : Form
    {
        private bool _isValid = false;

        public bool IsValid
        {
            get { return _isValid; }
            set { _isValid = value; }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void btnLoadCer_Click(object sender, EventArgs e)
        {
            OpenFileDialog openCer = new OpenFileDialog();
            openCer.Filter = "Security certificate | *.cer";

            if (openCer.ShowDialog() == DialogResult.OK)
            {
                using (TextReader tr = new StreamReader(openCer.FileName))
                {
                    txtCer.Text = tr.ReadToEnd();
                }
            }
        }

        private void btnLoadKey_Click(object sender, EventArgs e)
        {
            OpenFileDialog openKey = new OpenFileDialog();
            openKey.Filter = "Key file | *.key";

            if (openKey.ShowDialog() == DialogResult.OK)
            {
                using (TextReader tr = new StreamReader(openKey.FileName))
                {
                    txtKey.Text = tr.ReadToEnd();
                }
            }
        }

        private void btnSaveXML_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                MessageBox.Show("First you need to create a valid certificate in order to persist it!.");
            }

            Certificate cert = new Certificate(txtCer.Text, txtKey.Text, txtPassword.Text);

            XmlSerializer x = new XmlSerializer(typeof(Certificate));

            try
            {
                using (TextWriter writer = new StreamWriter("certificate.xml", false))
                {
                    x.Serialize(writer, cert);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Couldn't properly serialize the certificate file. Error {0}.", ex.Message));
            }
        }

        private void btnLoadXML_Click(object sender, EventArgs e)
        {
            if (!File.Exists("certificate.xml"))
            {
                MessageBox.Show("Certificate backstore file doesn't exist!");
            }

            Certificate cert = null;

            XmlSerializer x = new XmlSerializer(typeof(Certificate));

            try
            {
                using (StreamReader reader = new StreamReader("certificate.xml"))
                {
                    cert = (Certificate)x.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Couldn't properly deserialize the certificate file. Error {0}.", ex.Message));
                return;
            }

            txtCer.Text = cert.PublicCertificate;
            txtKey.Text = cert.PrivateKey;
            txtPassword.Text = cert.Password;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            IsValid = false;
            Certificate cert = new Certificate(txtCer.Text, txtKey.Text, txtPassword.Text);
            X509Certificate2 xcert = null;

            try
            {
                if (string.IsNullOrEmpty(cert.PrivateKey))
                    xcert = cert.GetCertificateFromPEMstring(true);
                else
                    xcert = cert.GetCertificateFromPEMstring(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An error occure during certificate creation: Error {0}", ex.Message));
                pictureBox1.Image = new Bitmap("red_light.png");
                return;
            }

            if (!string.IsNullOrEmpty(cert.PrivateKey) && !xcert.HasPrivateKey)
                pictureBox1.Image = new Bitmap("red_light.png");
            else
                pictureBox1.Image = new Bitmap("green_light.png");
            IsValid = true;
        }
    }
}
